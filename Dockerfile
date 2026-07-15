# =========================================================
# Etapa 1: Build - compila el proyecto usando el SDK de .NET
# =========================================================
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

# Copiamos solo el .csproj primero para aprovechar el cache de capas de Docker
COPY SakilaApp.csproj ./
RUN dotnet restore "SakilaApp.csproj"

# Copiamos el resto del código fuente
COPY . .

# Publicamos la aplicación en modo Release
RUN dotnet publish "SakilaApp.csproj" -c Release -o /app/publish /p:UseAppHost=false

# =========================================================
# Etapa 2: Runtime - imagen final, liviana, solo con el runtime de ASP.NET
# =========================================================
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS final
WORKDIR /app

# Copiamos únicamente los binarios publicados desde la etapa de build
COPY --from=build /app/publish .

# Puerto interno que expone Kestrel dentro del contenedor
EXPOSE 8080

# Variable de entorno para que Kestrel escuche en el puerto 8080
ENV ASPNETCORE_URLS=http://+:8080

ENTRYPOINT ["dotnet", "SakilaApp.dll"]