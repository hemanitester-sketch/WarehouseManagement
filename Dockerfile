FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

# Copy common.props and project files
COPY ["aspnet-core/common.props", "aspnet-core/"]
COPY ["aspnet-core/src/WarehouseManagement.Domain.Shared/WarehouseManagement.Domain.Shared.csproj", "aspnet-core/src/WarehouseManagement.Domain.Shared/"]
COPY ["aspnet-core/src/WarehouseManagement.Domain/WarehouseManagement.Domain.csproj", "aspnet-core/src/WarehouseManagement.Domain/"]
COPY ["aspnet-core/src/WarehouseManagement.Application.Contracts/WarehouseManagement.Application.Contracts.csproj", "aspnet-core/src/WarehouseManagement.Application.Contracts/"]
COPY ["aspnet-core/src/WarehouseManagement.Application/WarehouseManagement.Application.csproj", "aspnet-core/src/WarehouseManagement.Application/"]
COPY ["aspnet-core/src/WarehouseManagement.EntityFrameworkCore/WarehouseManagement.EntityFrameworkCore.csproj", "aspnet-core/src/WarehouseManagement.EntityFrameworkCore/"]
COPY ["aspnet-core/src/WarehouseManagement.HttpApi/WarehouseManagement.HttpApi.csproj", "aspnet-core/src/WarehouseManagement.HttpApi/"]
COPY ["aspnet-core/src/WarehouseManagement.HttpApi.Client/WarehouseManagement.HttpApi.Client.csproj", "aspnet-core/src/WarehouseManagement.HttpApi.Client/"]
COPY ["aspnet-core/src/WarehouseManagement.HttpApi.Host/WarehouseManagement.HttpApi.Host.csproj", "aspnet-core/src/WarehouseManagement.HttpApi.Host/"]
COPY ["aspnet-core/src/WarehouseManagement.DbMigrator/WarehouseManagement.DbMigrator.csproj", "aspnet-core/src/WarehouseManagement.DbMigrator/"]

# Restore packages directly for Host project
RUN dotnet restore "aspnet-core/src/WarehouseManagement.HttpApi.Host/WarehouseManagement.HttpApi.Host.csproj"

# Copy full source and publish
COPY aspnet-core/ aspnet-core/
WORKDIR "/src/aspnet-core/src/WarehouseManagement.HttpApi.Host"
RUN dotnet publish "WarehouseManagement.HttpApi.Host.csproj" -c Release -o /app/publish /p:UseAppHost=false

# Runtime Image
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS final
WORKDIR /app
COPY --from=build /app/publish .

ENV ASPNETCORE_URLS=http://+:8080
EXPOSE 8080

ENTRYPOINT ["dotnet", "WarehouseManagement.HttpApi.Host.dll"]
