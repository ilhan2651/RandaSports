# Bağlam: backend/ klasörü.

FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

# Önce yalnızca proje dosyaları kopyalanıyor: kaynak kodu değiştiğinde restore
# katmanı önbellekten geliyor, her derlemede paketler yeniden indirilmiyor.
COPY RandaSports.Domain/RandaSports.Domain.csproj           RandaSports.Domain/
COPY RandaSports.Application/RandaSports.Application.csproj RandaSports.Application/
COPY RandaSports.Persistence/RandaSports.Persistence.csproj RandaSports.Persistence/
COPY RandaSports.Infrastructure/RandaSports.Infrastructure.csproj RandaSports.Infrastructure/
COPY RandaSports.Api/RandaSports.Api.csproj                 RandaSports.Api/
RUN dotnet restore RandaSports.Api/RandaSports.Api.csproj

COPY . .
RUN dotnet publish RandaSports.Api/RandaSports.Api.csproj -c Release -o /app --no-restore

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS runtime
WORKDIR /app
COPY --from=build /app .

ENV ASPNETCORE_URLS=http://+:8080 \
    ASPNETCORE_ENVIRONMENT=Production \
    DOTNET_gcServer=0

EXPOSE 8080

# Hazır gelen kök dışı kullanıcı; imaj root olarak çalışmıyor.
USER $APP_UID

ENTRYPOINT ["dotnet", "RandaSports.Api.dll"]
