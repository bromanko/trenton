FROM google/cloud-sdk:304.0.0-alpine

RUN apk --no-cache --update add openjdk8-jre
RUN gcloud components install --quiet beta cloud-datastore-emulator \
  && rm -rf /google-cloud-sdk/.install/.backup

ENTRYPOINT ["gcloud", "beta", "emulators", "datastore"],
