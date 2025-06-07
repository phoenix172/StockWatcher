FROM mcr.microsoft.com/dotnet/runtime:9.0 AS base
USER $APP_UID
WORKDIR /app

FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["StockWatcher.Core/StockWatcher.Core.csproj", "StockWatcher.Core/"]
RUN dotnet restore "StockWatcher.Core/StockWatcher.Core.csproj"
COPY . .
WORKDIR "/src/StockWatcher.Core"
RUN dotnet build "StockWatcher.Core.csproj" -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "StockWatcher.Core.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "StockWatcher.Core.dll"]
