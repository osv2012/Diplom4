# Create a resource groups
resource "azurerm_resource_group" "rg_diplom4" {
  name     = "rg-diplom4"
  location = var.location
}

