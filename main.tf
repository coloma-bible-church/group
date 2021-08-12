# The configuration for the `remote` backend.
terraform {
  backend "remote" {
    organization = "coloma-bible-church"

    workspaces {
      name = "group"
    }
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

resource "azurerm_resource_group" "rg-hello-azure" {
  name     = "rg-hello-azure"
  location = "northcentralus"
}