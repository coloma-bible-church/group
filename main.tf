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

# An example resource that does nothing.
resource "null_resource" "example" {
  triggers = {
    value = "A example resource that does nothing!"
  }
}