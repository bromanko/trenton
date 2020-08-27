##################################
FROM fsharp:10.7.0-netcore

# Install Dockerize and requirements
RUN apt-get update && \
    apt-get --no-install-recommends install -y \
    musl
COPY --from=jwilder/dockerize:0.6.0 /usr/local/bin/dockerize /usr/local/bin

# Install Node.Js
RUN apt-get update --allow-releaseinfo-change && \
    curl -sL https://deb.nodesource.com/setup_14.x | bash - && \
    apt-get install -y nodejs && \
    node -v

WORKDIR /usr/src/app/
