output "function_app_name" {
  value       = azurerm_app_service.default.name
  description = "Deployed function app name"
}

output "function_app_default_hostname" {
  value       = azurerm_app_service.default.default_site_hostname
  description = "Deployed function app host name"
}