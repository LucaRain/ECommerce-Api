# Build stage
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /app

# copy solution and project files first (better layer caching)
COPY ECommerce.slnx .
COPY src/ECommerce.API/ECommerce.API.csproj src/ECommerce.API/
COPY src/ECommerce.Application/ECommerce.Application.csproj src/ECommerce.Application/
COPY src/ECommerce.Infrastructure/ECommerce.Infrastructure.csproj src/ECommerce.Infrastructure/

# restore dependencies
RUN dotnet restore

# copy everything else
COPY src/ src/

# build and publish
RUN dotnet publish src/ECommerce.API/ECommerce.API.csproj -c Release -o /app/publish

# Runtime stage
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS runtime
WORKDIR /app

# copy published output
COPY --from=build /app/publish .

# create images folder
RUN mkdir -p wwwroot/images

EXPOSE 8080

ENTRYPOINT ["dotnet", "ECommerce.API.dll"]