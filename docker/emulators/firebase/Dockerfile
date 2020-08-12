FROM node:12.18.3-alpine3.9

ENV FIREBASE_TOOLS_VERSION="8.7.0"
ENV HOME=/home/node

RUN apk --no-cache --update add openjdk8-jre && \
    yarn global add firebase-tools@${FIREBASE_TOOLS_VERSION} && \
    yarn cache clean && \
    firebase setup:emulators:database && \
    firebase setup:emulators:firestore && \
    firebase setup:emulators:pubsub && \
    firebase -V && \
    java -version && \
    chown -R node:node $HOME

USER node
VOLUME $HOME/.cache
WORKDIR $HOME

ENTRYPOINT ["firebase"]
