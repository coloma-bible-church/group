# The configuration for the `remote` backend.
terraform {
  backend "remote" {
    organization = "coloma-bible-church"

    workspaces {
      name = "group"
    }
  }
}

# An example resource that does nothing.
resource "null_resource" "example" {
  triggers = {
    value = "A example resource that does nothing!"
  }
}