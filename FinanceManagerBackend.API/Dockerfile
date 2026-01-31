FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS base
ENV APP_UID=1000
USER $APP_UID
WORKDIR /app
EXPOSE 8080
EXPOSE 8081

FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["FinanceManagerBackend.API.csproj", "FinanceManagerBackend.API/"]
RUN dotnet restore "FinanceManagerBackend.API/FinanceManagerBackend.API.csproj"
WORKDIR "/src/FinanceManagerBackend.API"
COPY . .
RUN dotnet build "./FinanceManagerBackend.API.csproj" -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "./FinanceManagerBackend.API.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "FinanceManagerBackend.API.dll"]
