output "gcr-url" {
  description = "Fully-qualified registry URL."
  value       = local.url
}

output "gcr-images-ro-service-account-key" {
  description = "Service account key for read only access to GCR images"
  value       = google_service_account_key.ro_key.private_key
  sensitive   = true
}
