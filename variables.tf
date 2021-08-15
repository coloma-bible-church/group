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

variable "communication_service_id" {
  type        = string
  description = "Azure ID for the communication service holding the phone number"
  sensitive   = true
}

variable "sms_connection_string" {
  type        = string
  description = "Connection string for SMS client in app"
  sensitive   = true
}

variable "sms_service_number" {
  type        = string
  description = "Phone number from which SMS client sends messages in app"
  sensitive   = true
}