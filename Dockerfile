FROM alpine:3

# Update package repos.
RUN apk update --no-cache

# Install dependencies.
RUN apk add --no-cache ca-certificates tzdata wget openssl

# Setup dockerize.
RUN wget -O - https://github.com/jwilder/dockerize/releases/download/v0.9.9/dockerize-alpine-linux-amd64-v0.9.9.tar.gz | tar xzf - -C /usr/local/bin && \
    apk del wget

# Copy Temporal binaries.
COPY --from=temporalio/server:1.29.2 --chmod=755 /usr/local/bin/temporal-server /usr/local/bin/
COPY --from=temporalio/server:1.29.2 --chmod=755 /etc/temporal/entrypoint.sh /etc/temporal/entrypoint.sh
COPY --from=temporalio/server:1.29.2 --chmod=755 /etc/temporal/config/config_template.yaml /etc/temporal/config/config_template.yaml
COPY --from=temporalio/server:1.29.2 --chmod=755 /etc/temporal/config/dynamicconfig/docker.yaml /etc/temporal/config/dynamicconfig/docker.yaml

# Update config folder permissions to allow dockerize to create the required config.
RUN chmod 777 /etc/temporal/config/

# Setup Temporal user.
RUN addgroup -g 1000 temporal && \
    adduser -u 1000 -G temporal -D temporal 

# Copy admin-tools binaries.
COPY --from=temporalio/admin-tools:1.29.1-tctl-1.18.4-cli-1.5.0 --chmod=755 \
	/usr/local/bin/temporal \
	/usr/local/bin/temporal-sql-tool \
	/usr/local/bin/tdbg \
	/usr/local/bin/
COPY --from=temporalio/admin-tools:1.29.1-tctl-1.18.4-cli-1.5.0 /etc/temporal/schema /etc/temporal/schema

# Copy the container's entry point & namespace creation script.
COPY --chmod=755 ./entrypoint.sh /etc/entrypoint.sh
COPY --chmod=755 ./create-namespace.sh /etc/create-namespace.sh

WORKDIR /etc/temporal
USER temporal

ENTRYPOINT [ "/etc/entrypoint.sh" ]
