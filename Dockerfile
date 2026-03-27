# Этап 1 — сборка
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build

# # Фикс ошибки с fallback-папкой Visual Studio (Windows-артефакт в Linux-контейнере)
# RUN mkdir -p "/Program Files (x86)/Microsoft Visual Studio/Shared/NuGetPackages" \
#     && chmod -R 777 "/Program Files (x86)"


ENV DOTNET_NUGET_FALLBACK_PACKAGES=""

ENV DOTNET_CLI_TELEMETRY_OPTOUT=1
ENV DOTNET_NOLOGO=1

WORKDIR /src

# Копируем только csproj-файлы для кэширования restore
COPY ["API-sport-training-program/API-sport-training-program.csproj", "API-sport-training-program/"]

RUN dotnet restore "API-sport-training-program/API-sport-training-program.csproj"

# Копируем весь исходный код
COPY API-sport-training-program/ API-sport-training-program/

# Переходим в папку проекта
WORKDIR "/src/API-sport-training-program"

# Собираем (без --no-restore пока — для надёжности; потом можно вернуть)
RUN dotnet build "API-sport-training-program.csproj" -c Release -o /app/build

# Этап 2 — публикация
FROM build AS publish
RUN dotnet publish "API-sport-training-program.csproj" -c Release -o /app/publish

# Этап 3 — финальный runtime-образ
FROM mcr.microsoft.com/dotnet/aspnet:10.0
WORKDIR /app
COPY --from=publish /app/publish .

# Порт (можно поменять на 80, если хочешь стандартный)
EXPOSE 8080

# Важно для контейнера
ENV ASPNETCORE_URLS=http://+:8080
ENV ASPNETCORE_ENVIRONMENT=Production

ENTRYPOINT ["dotnet", "API-sport-training-program.dll"]