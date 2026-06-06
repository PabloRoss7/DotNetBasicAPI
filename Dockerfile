# Build stage: SDK image, compiles and publishes the app.
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

# Restore first (cached layer if dependencies don't change).
COPY DotNetBasicAPI/*.csproj DotNetBasicAPI/
RUN dotnet restore DotNetBasicAPI/DotNetBasicAPI.csproj

# Copy the rest and publish.
COPY DotNetBasicAPI/ DotNetBasicAPI/
RUN dotnet publish DotNetBasicAPI/DotNetBasicAPI.csproj -c Release -o /app

# Runtime stage: smaller image with only the ASP.NET runtime.
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS final
WORKDIR /app
COPY --from=build /app .

ENV ASPNETCORE_ENVIRONMENT=Production
ENV ASPNETCORE_HTTP_PORTS=8080
EXPOSE 8080

# The JWT signing key must be provided at runtime, e.g.:
#   docker run -p 8080:8080 -e Jwt__Key="..." usermgmt-api
ENTRYPOINT ["dotnet", "DotNetBasicAPI.dll"]
