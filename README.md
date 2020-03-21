# unifi-udm-api-proxy

[![Docker Image Version (latest semver)](https://img.shields.io/docker/v/silvenga/unifi-udm-api-proxy?label=docker%20hub&sort=semver&cacheSeconds=3600&logo=Docker&logoColor=WhiteSmoke)](https://hub.docker.com/r/silvenga/unifi-udm-api-proxy)
[![Build](https://github.com/Silvenga/unifi-udm-api-proxy/workflows/Build/badge.svg)](https://github.com/Silvenga/unifi-udm-api-proxy/actions)
[![GitHub](https://img.shields.io/github/license/Silvenga/unifi-udm-api-proxy?cacheSeconds=3600)](https://github.com/Silvenga/unifi-udm-api-proxy/blob/master/LICENSE)

A compatibility shim to support accessing the new UnifiOs API.

The `unifi-udm-api-proxy` can be used to provide backwards compatibility between projects that utilize the unofficial Unifi/Protect api's on newer UnifiOs based devices (the UDM/UDMP) - without modifying existing clients.

### Example

The [unifiprotect](https://github.com/briis/unifiprotect) project is a Home Assistant integration that utilizes the Unifi Protect API to provide Home Assistant with data directly from Unifi Protect.

The normal network model look like this:

```
unifiprotect <-> protect api (192.168.0.2:7443)
```

With `unifi-udm-api-proxy` introduced, the network model changes to:

```
unifiprotect <-> unifi-udm-api-proxy (192.168.0.2:5000) <-> protect api (192.168.0.1:443)
```

In this model, [unifiprotect](https://github.com/briis/unifiprotect) communicates with unifi-udm-api-proxy as if it was talking to the Protect api.

### Setup

`docker-compose` is easiest way to deploy an instance:

```yaml
version: '2.1'
services:
  unifi-udm-api-proxy:
    image: silvenga/unifi-udm-api-proxy:1
    ports:
      - 5000:443
    environment:
      UDM__URI: https://192.168.0.1
    restart: always
```

The container will listen on http and https (ports 80 and 443 respectively). Ensure that the environment variable `UDM__URI` is pointed to the UDM/UDMP. Currently, when https is specified in `UDM__URI`, no server verification will occur.

### Compatibility

Since the Unifi API is undocumented, reverse engineering is required. The following projects have been lightly tested to work with the `unifi-udm-api-proxy` in-place.

- [unifiprotect](https://github.com/briis/unifiprotect)
