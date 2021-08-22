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

variable "server_secret" {
  type        = string
  description = "Secret key for the Group.Hub app"
  sensitive   = true
}

variable "twilio_account_sid" {
  type        = string
  description = "Twilio account SID"
  sensitive   = true
}

variable "twilio_messaging_service_id" {
  type        = string
  description = "Twilio messaging service ID"
  sensitive   = true
}

variable "twilio_auth_token" {
  type        = string
  description = "Twilio auth token"
  sensitive   = true
}

variable "twilio_connection_secret" {
  type        = string
  description = "Connection secret for Group.Twilio app"
  sensitive   = true
}

variable "twilio_server_secret" {
  type        = string
  description = "Server secret for Group.Twilio app"
  sensitive   = true
}