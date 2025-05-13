FROM kong/kong-gateway:3.8.0.0

COPY ./plugins/jwt-to-claims /usr/local/share/lua/5.1/kong/plugins/jwt-to-claims

ENV KONG_PLUGINS=bundled,jwt-to-claims

USER kong
