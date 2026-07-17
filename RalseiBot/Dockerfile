FROM mcr.microsoft.com/dotnet/runtime:10.0 AS base
FROM python
USER $APP_UID
WORKDIR /app

FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["RalseiBot/RalseiBot.csproj", "RalseiBot/"]
COPY ["RalseiBot.Backend/RalseiBot.Backend.csproj", "RalseiBot.Backend/"]
COPY ["RalseiBot.ServiceDefaults/RalseiBot.ServiceDefaults.csproj", "RalseiBot.ServiceDefaults/"]
COPY ["RalseiBot.Web/RalseiBot.Web.csproj", "RalseiBot.Web/"]
RUN dotnet restore "RalseiBot/RalseiBot.csproj"
COPY . .
WORKDIR "/src/RalseiBot"
RUN dotnet build "./RalseiBot.csproj" -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "./RalseiBot.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
EXPOSE 8000
ENTRYPOINT ["dotnet", "RalseiBot.dll"]
