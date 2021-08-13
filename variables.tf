variable "project" {
  type        = string
  description = "Project name"
}

variable "environment" {
  type        = string
  description = "Environment (dev / stage / prod)"
}

variable "location" {
  type        = string
  description = "Azure region for deployment"
}

variable "SMS_CONNECTION_STRING" {
  type        = string
  description = "Connection string for SMS client in app"
  sensitive   = true
}

variable "SMS_SERVICE_NUMBER" {
  type        = string
  description = "Phone number from which SMS client sends messages in app"
  sensitive   = true
}