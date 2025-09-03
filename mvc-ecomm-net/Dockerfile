FROM mcr.microsoft.com/dotnet/runtime:8.0 AS base
USER $APP_UID
WORKDIR /app

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["mvc-ecomm-net/mvc-ecomm-net.csproj", "mvc-ecomm-net/"]
RUN dotnet restore "mvc-ecomm-net/mvc-ecomm-net.csproj"
COPY . .
WORKDIR "/src/mvc-ecomm-net"
RUN dotnet build "./mvc-ecomm-net.csproj" -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "./mvc-ecomm-net.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "mvc-ecomm-net.dll"]
