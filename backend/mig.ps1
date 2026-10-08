# Migration üretir ve uygular.
# Kullanım:  .\mig.ps1 AddSportsSyncFields
param(
    [Parameter(Mandatory = $true, Position = 0)]
    [string]$Name
)

$ErrorActionPreference = "Stop"
Set-Location -Path $PSScriptRoot

$project = "RandaSports.Persistence"
$startup = "RandaSports.Api"

Write-Host "-> Migration üretiliyor: $Name" -ForegroundColor Cyan
dotnet ef migrations add $Name -p $project -s $startup

if ($LASTEXITCODE -ne 0) {
    Write-Host "Migration üretilemedi, veritabanına dokunulmadı." -ForegroundColor Red
    exit $LASTEXITCODE
}

Write-Host "-> Veritabanına uygulanıyor" -ForegroundColor Cyan
dotnet ef database update -p $project -s $startup

if ($LASTEXITCODE -ne 0) {
    Write-Host "Uygulama başarısız. Geri almak için: dotnet ef migrations remove -p $project -s $startup" -ForegroundColor Red
    exit $LASTEXITCODE
}

Write-Host "Tamam: $Name üretildi ve uygulandı." -ForegroundColor Green
