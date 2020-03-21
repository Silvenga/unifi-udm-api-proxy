ARG BUILD_IMAGE=mcr.microsoft.com/dotnet/core/aspnet:3.1-bionic

FROM ${BUILD_IMAGE} AS base
WORKDIR /app
EXPOSE 80

ENV ASPNETCORE_URLS=https://+;http://+
ENV ASPNETCORE_Kestrel__Certificates__Default__Path=/app/self-signed.pfx
ENV ASPNETCORE_Kestrel__Certificates__Default__Password=password

HEALTHCHECK --interval=5s --timeout=10s --retries=3 CMD curl --fail http://localhost:80/healthcheck || exit 1

FROM mcr.microsoft.com/dotnet/core/sdk:3.1-bionic AS build

RUN dotnet dev-certs https --clean \
    && dotnet dev-certs https -ep /app/publish/self-signed.pfx -p password

WORKDIR /src
COPY ["src/UdmApi.Proxy/UdmApi.Proxy.csproj", "src/UdmApi.Proxy/"]
RUN dotnet restore "src/UdmApi.Proxy/UdmApi.Proxy.csproj"

COPY . .
WORKDIR "/src/src/UdmApi.Proxy"
RUN dotnet build "UdmApi.Proxy.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "UdmApi.Proxy.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "UdmApi.Proxy.dll"]