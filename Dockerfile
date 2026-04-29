# Stage 1: Build
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

# Копируем全体 структуру (включая Directory.Build.props)
COPY . .

# Восстанавливаем зависимости из QuestsApi.csproj (Directory.Build.props применится)
WORKDIR "/src/QuestsApi"
RUN dotnet restore "QuestsApi.csproj"

# Собираем
RUN dotnet build "QuestsApi.csproj" -c Release -o /app/build

# Stage 2: Publish
FROM build AS publish
RUN dotnet publish "QuestsApi.csproj" -c Release -o /app/publish /p:UseAppHost=true

# Stage 3: Final runtime image
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS final
WORKDIR /app
EXPOSE 80
EXPOSE 7240

# Создаём non-root пользователя для безопасности (Debian-based)
RUN groupadd -r appuser && useradd -r -g appuser -d /app -s /sbin/nologin appuser \
    && chown -R appuser:appuser /app
USER appuser

COPY --from=publish --chown=appuser:appuser /app/publish .
ENTRYPOINT ["dotnet", "QuestsApi.dll"]
