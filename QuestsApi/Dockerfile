# Stage 1: Build
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

# Копируем csproj файлы и восстанавливаем зависимости
COPY ["QuestsApi/QuestsApi.csproj", "QuestsApi/"]
COPY ["Application/Application.csproj", "Application/"]
COPY ["Data/Data.csproj", "Data/"]
COPY ["Domain/Domain.csproj", "Domain/"]
WORKDIR "/src/QuestsApi"
RUN dotnet restore "QuestsApi.csproj"

# Копируем исходный код и собираем
COPY . .
WORKDIR "/src/QuestsApi"
RUN dotnet build "QuestsApi.csproj" -c Release -o /app/build

# Stage 2: Publish
FROM build AS publish
RUN dotnet publish "QuestsApi.csproj" -c Release -o /app/publish /p:UseAppHost=true

# Stage 3: Final runtime image
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS final
WORKDIR /app	
EXPOSE 80
EXPOSE 7240

# Создаём non-root пользователя для безопасности
RUN adduser --disabled-password --gecos "" appuser && chown -R appuser /app
USER appuser

COPY --from=publish --chown=appuser:appuser /app/publish .
ENTRYPOINT ["dotnet", "QuestsApi.dll"]
