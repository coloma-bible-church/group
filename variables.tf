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

variable "twilio_auth_token" {
  type        = string
  description = "Twilio auth token"
  sensitive   = true
}