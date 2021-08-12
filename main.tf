terraform {
  backend "azurerm" {
    resource_group_name  = "rg-tf-cbc-group"
    storage_account_name = "satfcbcgroup"
    container_name       = "terraform-state"
    key                  = "terraform.tfstate"
  }

  required_providers {
    azurerm = {
      source  = "hashicorp/azurerm"
      version = "~> 2.72"
    }
  }
}

provider "azurerm" {
  features {}
}

resource "azurerm_resource_group" "default" {
  name     = "${var.project}-${var.environment}-resource-group"
  location = var.location
}

resource "azurerm_storage_account" "default" {
  name                     = "${var.project}${var.environment}"
  resource_group_name      = azurerm_resource_group.default.name
  location                 = azurerm_resource_group.default.location
  account_tier             = "Standard"
  account_replication_type = "LRS"
}

resource "azurerm_storage_container" "default" {
  name                 = "${var.project}-${var.environment}-storage-container"
  storage_account_name = azurerm_storage_account.default.name
}

resource "azurerm_application_insights" "default" {
  name                = "${var.project}-${var.environment}-application-insights"
  location            = var.location
  resource_group_name = azurerm_resource_group.default.name
  application_type    = "other"
}

resource "azurerm_app_service_plan" "default" {
  name                = "${var.project}-${var.environment}-app-service-plan"
  resource_group_name = azurerm_resource_group.default.name
  location            = var.location
  kind                = "FunctionApp"

  sku {
    tier = "Dynamic"
    size = "Y1"
  }
}

data "archive_file" "default" {
  type        = "zip"
  source_dir  = "dist"
  output_path = "archive/app.zip"
}

resource "azurerm_storage_blob" "default" {
  name                   = "${filesha256(data.archive_file.default.output_path)}.zip"
  storage_account_name   = azurerm_storage_account.default.name
  storage_container_name = azurerm_storage_container.default.name
  type                   = "Block"
  source                 = data.archive_file.default.output_path
}

data "azurerm_storage_account_blob_container_sas" "default" {
  connection_string = azurerm_storage_account.default.primary_connection_string
  container_name    = azurerm_storage_container.default.name

  start  = timeadd(timestamp(), "-1h")
  expiry = timeadd(timestamp(), "87600h")

  permissions {
    read   = true
    add    = false
    create = false
    write  = false
    delete = false
    list   = false
  }
}

resource "azurerm_function_app" "default" {
  name                       = "${var.project}-${var.environment}-function-app"
  location                   = var.location
  resource_group_name        = azurerm_resource_group.default.name
  app_service_plan_id        = azurerm_app_service_plan.default.id
  storage_account_name       = azurerm_storage_account.default.name
  storage_account_access_key = azurerm_storage_account.default.primary_access_key

  app_settings = {
    "WEBSITE_RUN_FROM_PACKAGE" = "https://${azurerm_storage_account.default.name}.blob.core.windows.net/${azurerm_storage_container.default.name}/${azurerm_storage_blob.default.name}${data.azurerm_storage_account_blob_container_sas.default.sas}"
  }
}

resource "azurerm_storage_queue" "default" {
  name                 = "${var.project}-${var.environment}-storage-queue"
  storage_account_name = azurerm_storage_account.default.name
}

resource "azurerm_eventgrid_event_subscription" "default" {
  name                 = "${var.project}-${var.environment}-event-subscription"
  scope                = azurerm_resource_group.default.id
  included_event_types = ["SMS Received"]

  azure_function_endpoint {
    function_id = azurerm_function_app.default.id
  }

  storage_queue_endpoint {
    storage_account_id = azurerm_storage_account.default.id
    queue_name         = azurerm_storage_queue.default.name
  }
}

resource "azurerm_communication_service" "default" {
  name                = "${var.project}-${var.environment}-communication-service"
  resource_group_name = azurerm_resource_group.default.name
  # Manually manage the SMS number
}