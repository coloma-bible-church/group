output "function_app_name" {
  value       = azurerm_function_app.default.name
  description = "Deployed function app name"
}

output "function_app_default_hostname" {
  value       = azurerm_function_app.default.default_hostname
  description = "Deployed function app host name"
}