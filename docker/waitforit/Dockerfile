FROM alpine:3.12.0 AS build

ENV WAITFORIT_VERSION="v2.4.1"

RUN apk --no-cache add curl
RUN curl -o /usr/local/bin/waitforit -sSL https://github.com/maxcnunes/waitforit/releases/download/$WAITFORIT_VERSION/waitforit-linux_amd64 && \
    chmod +x /usr/local/bin/waitforit

FROM scratch

COPY --from=build /usr/local/bin/waitforit /waitforit

ENTRYPOINT ["/waitforit"]
