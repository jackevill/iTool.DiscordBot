FROM debian:jessie-slim

COPY docker-build ./publish

RUN apt-get update \
    && apt-get install -y --no-install-recommends \
        libc6 \
        libcurl3 \
        libgcc1 \
        libgssapi-krb5-2 \
        libicu52 \
        liblttng-ust0 \
        libssl1.0.0 \
        libstdc++6 \
        libunwind8 \
        libuuid1 \
        zlib1g \
    && apt-get autoremove -y \
    && apt-get clean -y \
    && rm -rf /var/lib/apt/lists/*

ENTRYPOINT ["publish/iTool.DiscordBot"]
