# Bağlam: backend/ klasörü.

FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

COPY RandaSports.Domain/RandaSports.Domain.csproj           RandaSports.Domain/
COPY RandaSports.Application/RandaSports.Application.csproj RandaSports.Application/
COPY RandaSports.Persistence/RandaSports.Persistence.csproj RandaSports.Persistence/
COPY RandaSports.Infrastructure/RandaSports.Infrastructure.csproj RandaSports.Infrastructure/
COPY RandaSports.Worker/RandaSports.Worker.csproj           RandaSports.Worker/
RUN dotnet restore RandaSports.Worker/RandaSports.Worker.csproj

COPY . .
RUN dotnet publish RandaSports.Worker/RandaSports.Worker.csproj -c Release -o /app --no-restore

# Worker bir web sunucusu değil; ASP.NET katmanı olmayan küçük çalışma zamanı yetiyor.
FROM mcr.microsoft.com/dotnet/runtime:10.0 AS runtime
WORKDIR /app
COPY --from=build /app .

ENV DOTNET_ENVIRONMENT=Production

USER $APP_UID

ENTRYPOINT ["dotnet", "RandaSports.Worker.dll"]
