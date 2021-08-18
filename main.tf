terraform {
  backend "azurerm" {
    resource_group_name  = "rg-tf-cbc-group"
    storage_account_name = "satfcbcgroup"
    container_name       = "terraform-state"
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

resource "azurerm_storage_container" "app" {
  name                 = "${var.project}-${var.environment}-storage-container-app"
  storage_account_name = azurerm_storage_account.default.name
}

resource "azurerm_storage_container" "dead" {
  name                 = "${var.project}-${var.environment}-storage-container-dead"
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
  kind                = "Linux"
  reserved            = true

  sku {
    tier = "Basic"
    size = "B1"
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
  storage_container_name = azurerm_storage_container.app.name
  type                   = "Block"
  source                 = data.archive_file.default.output_path
}

data "azurerm_storage_account_blob_container_sas" "default" {
  connection_string = azurerm_storage_account.default.primary_connection_string
  container_name    = azurerm_storage_container.app.name

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

resource "azurerm_cosmosdb_account" "default" {
  name                = "${var.project}-${var.environment}-cosmosodb-account"
  resource_group_name = azurerm_resource_group.default.name
  location            = var.location
  offer_type          = "Standard"

  capabilities {
    name = "EnableServerless"
  }

  consistency_policy {
    consistency_level = "ConsistentPrefix"
  }

  geo_location {
    location          = var.location
    failover_priority = 0
  }
}

resource "azurerm_cosmosdb_sql_database" "default" {
  name                = "${var.project}-${var.environment}-cosmosodb-sql-database"
  resource_group_name = azurerm_resource_group.default.name
  account_name        = azurerm_cosmosdb_account.default.name
}

resource "azurerm_cosmosdb_sql_container" "users" {
  name                = "${var.project}-${var.environment}-cosmosdb-sql-container-users"
  resource_group_name = azurerm_resource_group.default.name
  account_name        = azurerm_cosmosdb_account.default.name
  database_name       = azurerm_cosmosdb_sql_database.default.name
  partition_key_path  = "/id"
}

resource "azurerm_app_service" "default" {
  name                = "${var.project}-${var.environment}-app-service"
  location            = var.location
  resource_group_name = azurerm_resource_group.default.name
  app_service_plan_id = azurerm_app_service_plan.default.id

  app_settings = {
    "WEBSITE_RUN_FROM_PACKAGE"       = "https://${azurerm_storage_account.default.name}.blob.core.windows.net/${azurerm_storage_container.app.name}/${azurerm_storage_blob.default.name}${data.azurerm_storage_account_blob_container_sas.default.sas}",
    "APPINSIGHTS_INSTRUMENTATIONKEY" = azurerm_application_insights.default.instrumentation_key,
    "FUNCTIONS_WORKER_RUNTIME"       = "dotnet",
    "DB_CONNECTION_STRING"           = element(azurerm_cosmosdb_account.default.connection_strings, 0),
    "DB_DB_ID"                       = azurerm_cosmosdb_sql_database.default.id,
    "DB_USERS_CONTAINER_ID"          = azurerm_cosmosdb_sql_container.users.id
  }

  site_config {
    dotnet_framework_version = "v5.0"
    linux_fx_version         = "DOTNETCORE|5.0"
  }
}
