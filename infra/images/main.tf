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
  url = data.google_container_registry_repository.main.repository_url
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

resource "google_storage_bucket" "gcr-bucket" {
  name       = "artifacts.${var.project_id}.appspot.com"
  depends_on = [google_project_service.containerregistry, data.google_container_registry_repository.main]
}


# google_storage_bucket_iam_binding resources are authoritative for their respective roles
# if an entity isn't in its list of members their access will be revoked

# storage.objectAdmin provides complete access to objects in a bucket, but no access to the bucket itself
# this is one of two roles required to grant docker push access
resource "google_storage_bucket_iam_member" "object_admin" {
  bucket     = google_storage_bucket.gcr-bucket.self_link
  role       = "roles/storage.objectAdmin"
  member     = "serviceAccount:${google_service_account.rw.email}"
  depends_on = [google_project_service.containerregistry]
}

# docker push also requires storage.buckets.get in order to work properly
# storage.legacyBucketReader is the least permissive role that provides this
resource "google_storage_bucket_iam_member" "legacy_bucket_reader" {
  bucket     = google_storage_bucket.gcr-bucket.self_link
  role       = "roles/storage.legacyBucketReader"
  member     = "serviceAccount:${google_service_account.rw.email}"
  depends_on = [google_project_service.containerregistry]
}

# google_storage_bucket_iam_member resources are not authoritative for their roles
# additional members can be configured outside this module

# storage.objectViewer enables an entity to docker pull from the repository
resource "google_storage_bucket_iam_member" "object_viewer" {
  bucket     = google_storage_bucket.gcr-bucket.self_link
  role       = "roles/storage.objectViewer"
  member     = "serviceAccount:${google_service_account.ro.email}"
  depends_on = [google_project_service.containerregistry]
}

resource "google_service_account_key" "ro_key" {
  service_account_id = google_service_account.ro.account_id
}

resource "google_service_account_key" "rw_key" {
  service_account_id = google_service_account.rw.account_id
}
