# Dockerfile para ContactDelete.API
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 8080
EXPOSE 8081

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src

# Copiar arquivos de projeto e restaurar dependências
COPY ["src/ContactDelete.API/ContactDelete.API.csproj", "src/ContactDelete.API/"]
COPY ["src/Shared/ContactModels/ContactModels.csproj", "src/Shared/ContactModels/"]
COPY ["src/Shared/Common/Common.csproj", "src/Shared/Common/"]
COPY ["src/Shared/Infrastructure/Infrastructure.csproj", "src/Shared/Infrastructure/"]

RUN dotnet restore "src/ContactDelete.API/ContactDelete.API.csproj"

# Copiar todo o código fonte
COPY . .

# Build da aplicação
WORKDIR "/src/src/ContactDelete.API"
RUN dotnet build "ContactDelete.API.csproj" -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "ContactDelete.API.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .

# Criar usuário não-root para segurança
RUN adduser --disabled-password --gecos '' appuser && chown -R appuser /app
USER appuser

# Configurar variáveis de ambiente
ENV ASPNETCORE_URLS=http://+:8080
ENV ASPNETCORE_ENVIRONMENT=Production

# Health check
HEALTHCHECK --interval=30s --timeout=3s --start-period=5s --retries=3 \
  CMD curl -f http://localhost:8080/api/contact/health || exit 1

ENTRYPOINT ["dotnet", "ContactDelete.API.dll"]

