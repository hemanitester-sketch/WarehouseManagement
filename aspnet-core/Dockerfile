FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

# Copy common.props and project files
COPY ["common.props", "./"]
COPY ["src/WarehouseManagement.Domain.Shared/WarehouseManagement.Domain.Shared.csproj", "src/WarehouseManagement.Domain.Shared/"]
COPY ["src/WarehouseManagement.Domain/WarehouseManagement.Domain.csproj", "src/WarehouseManagement.Domain/"]
COPY ["src/WarehouseManagement.Application.Contracts/WarehouseManagement.Application.Contracts.csproj", "src/WarehouseManagement.Application.Contracts/"]
COPY ["src/WarehouseManagement.Application/WarehouseManagement.Application.csproj", "src/WarehouseManagement.Application/"]
COPY ["src/WarehouseManagement.EntityFrameworkCore/WarehouseManagement.EntityFrameworkCore.csproj", "src/WarehouseManagement.EntityFrameworkCore/"]
COPY ["src/WarehouseManagement.HttpApi/WarehouseManagement.HttpApi.csproj", "src/WarehouseManagement.HttpApi/"]
COPY ["src/WarehouseManagement.HttpApi.Client/WarehouseManagement.HttpApi.Client.csproj", "src/WarehouseManagement.HttpApi.Client/"]
COPY ["src/WarehouseManagement.HttpApi.Host/WarehouseManagement.HttpApi.Host.csproj", "src/WarehouseManagement.HttpApi.Host/"]
COPY ["src/WarehouseManagement.DbMigrator/WarehouseManagement.DbMigrator.csproj", "src/WarehouseManagement.DbMigrator/"]

# Restore packages directly for Host project
RUN dotnet restore "src/WarehouseManagement.HttpApi.Host/WarehouseManagement.HttpApi.Host.csproj"

# Copy full source and publish
COPY . .
WORKDIR "/src/src/WarehouseManagement.HttpApi.Host"
RUN dotnet publish "WarehouseManagement.HttpApi.Host.csproj" -c Release -o /app/publish /p:UseAppHost=false

# Runtime Image
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS final
WORKDIR /app
COPY --from=build /app/publish .

ENV ASPNETCORE_URLS=http://+:8080
EXPOSE 8080

ENTRYPOINT ["dotnet", "WarehouseManagement.HttpApi.Host.dll"]
