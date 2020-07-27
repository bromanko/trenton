# Project Planning

This project is a personal data pipeline. It aggregates information
across a variety of sources and persists that data for analysis.

The webhooks will store data in it's raw format in CloudStorage. Any
further manipulation can be done downstream.

So, this project will be a simple webhook -> CloudFunctions mapping.
It should be very simple to create a new endpoint.

## Anatomy of a webhook

A hook should
* have a form of schema validation
* support some form of authentication
* sink the raw data in a cloud storage bucket
* have a standard response json

## Fitbit

### Creating a Subscriber
There's no way to do this automatically. So, I'll have to do it manually.
I'll make the verification code configurable per env.

###  Creating a Subscription
This is the binding of the user to the subscriber. This should be done
on authentication. I will need to pass the X-Fitbit-Subscriber-Id for
the environment

### Webhook
We need to respond immediately. So,


Webhook call will need to ensure an access token exists and that we can
hit the API. I think the service will need to have a persistence
mechanism in order to retain the access token between restarts.

One option would be to use a persistent volume claim and just store
it on the filesystem. Another option would be to use some sort of GCP
database. Another option would be to store in Cloud Storage. That latter
is basically the same as persistent volumes.

When the webhook hits it needs to grab the last access token we have.
If there's none, fail. If there's one but it's expired, refresh it.
If the refresh fails, also need to fail.
