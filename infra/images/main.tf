terraform {
  backend "remote" {
    organization = "bromanko"

    workspaces {
      name = "trenton-images"
    }
  }
}

provider "google" {
  version     = "~> 2.17.0"
  credentials = var.gcp_credentials
  project     = var.project_id
}

locals {
  url         = data.google_container_registry_repository.main.repository_url
}

resource "google_project_service" "cloudresourcemanager" {
  service = "cloudresourcemanager.googleapis.com"
  project = var.project_id
}

resource "google_project_service" "containerregistry" {
  service = "containerregistry.googleapis.com"
  project = var.project_id
}

resource "google_project_service" "storage" {
  service = "storage-api.googleapis.com"
  project = var.project_id
}

resource "google_project_service" "iam" {
  service = "iam.googleapis.com"
}

resource "google_service_account" "rw" {
  project      = var.project_id
  account_id   = "gcr-read-write"
  display_name = "GCR read/write service account"
}

resource "google_service_account" "ro" {
  project      = var.project_id
  account_id   = "gcr-read-only"
  display_name = "GCR read-only service account"
}

data "google_container_registry_repository" "main" {
  project = var.project_id
}

resource "google_service_account_key" "ro_key" {
  service_account_id = google_service_account.ro.account_id
}
