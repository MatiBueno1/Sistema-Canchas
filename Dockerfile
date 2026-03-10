# ── Etapa 1: Build ───────────────────────────────────────────────────────
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

# Copiar archivos de proyecto
COPY ["FutbolComplejo.csproj", "./"]
RUN dotnet restore

# Copiar todo el código
COPY . .
RUN dotnet publish -c Release -o /app/publish

# ── Etapa 2: Runtime ─────────────────────────────────────────────────────
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS final
WORKDIR /app

# Copiar el build
COPY --from=build /app/publish .

# Copiar el frontend (index.html y admin.html se sirven estáticos)
COPY frontend/ ./wwwroot/

# Puerto que Railway expone
ENV ASPNETCORE_URLS=http://+:8080
ENV ASPNETCORE_ENVIRONMENT=Production

EXPOSE 8080

ENTRYPOINT ["dotnet", "FutbolComplejo.dll"]
