FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS base
WORKDIR /app
ENV ASPNETCORE_HTTP_PORTS=8080
EXPOSE 8080
USER $APP_UID

FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src
COPY ["src/Oficina.Api/Oficina.Api.csproj", "src/Oficina.Api/"]
COPY ["src/Oficina.Application/Oficina.Application.csproj", "src/Oficina.Application/"]
COPY ["src/Oficina.Domain/Oficina.Domain.csproj", "src/Oficina.Domain/"]
COPY ["src/Oficina.Infrastructure/Oficina.Infrastructure.csproj", "src/Oficina.Infrastructure/"]
RUN dotnet restore "src/Oficina.Api/Oficina.Api.csproj"
COPY . .
WORKDIR /src/src/Oficina.Api
RUN dotnet publish "Oficina.Api.csproj" --configuration Release --no-restore --output /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "Oficina.Api.dll"]
