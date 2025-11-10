# Script para ejecutar tests con cobertura de codigo
Write-Host "============================================" -ForegroundColor Cyan
Write-Host "   COBERTURA DE CODIGO - FORARIA" -ForegroundColor Cyan
Write-Host "============================================" -ForegroundColor Cyan

# Limpiar resultados anteriores
Write-Host ""
Write-Host "[1/4] Limpiando resultados anteriores..." -ForegroundColor Yellow
$testResultsPath = "./ForariaTest/TestResults"
$reportPath = "./TestResults/CoverageReport"

if (Test-Path $testResultsPath) {
    Remove-Item -Path $testResultsPath -Recurse -Force -ErrorAction SilentlyContinue
    Write-Host "      Limpieza completada" -ForegroundColor Green
}

if (Test-Path $reportPath) {
    Remove-Item -Path $reportPath -Recurse -Force -ErrorAction SilentlyContinue
}

# Ejecutar tests con cobertura
Write-Host ""
Write-Host "[2/4] Ejecutando tests con cobertura..." -ForegroundColor Yellow
dotnet test ForariaTest/ForariaTest.csproj /p:CollectCoverage=true /p:CoverletOutputFormat=cobertura /p:CoverletOutput="./TestResults/" --verbosity:minimal

if ($LASTEXITCODE -ne 0) {
    Write-Host ""
    Write-Host "ERROR: Los tests fallaron" -ForegroundColor Red
    exit 1
}

# Verificar archivo de cobertura
Write-Host ""
Write-Host "[3/4] Verificando archivo de cobertura..." -ForegroundColor Yellow
$coverageFile = "./ForariaTest/TestResults/coverage.cobertura.xml"

if (-not (Test-Path $coverageFile)) {
    Write-Host "      ERROR: No se genero el archivo de cobertura" -ForegroundColor Red
    exit 1
}

Write-Host "      Archivo encontrado correctamente" -ForegroundColor Green

# Generar reporte HTML
Write-Host ""
Write-Host "[4/4] Generando reporte HTML..." -ForegroundColor Yellow
reportgenerator -reports:"$coverageFile" -targetdir:"$reportPath" -reporttypes:"Html;Badges" -verbosity:Error

if ($LASTEXITCODE -ne 0) {
    Write-Host "      ERROR: No se pudo generar el reporte" -ForegroundColor Red
    exit 1
}

# Verificar que el reporte se genero
$indexFile = "$reportPath/index.html"
if (-not (Test-Path $indexFile)) {
    Write-Host "      ERROR: No se genero el archivo index.html" -ForegroundColor Red
    exit 1
}

Write-Host "      Reporte generado correctamente" -ForegroundColor Green

# Mostrar resumen
Write-Host ""
Write-Host "============================================" -ForegroundColor Cyan
Write-Host "   RESUMEN DE COBERTURA" -ForegroundColor Cyan
Write-Host "============================================" -ForegroundColor Cyan

try {
    $xml = [xml](Get-Content $coverageFile)
    $lineRate = [math]::Round([double]$xml.coverage.'line-rate' * 100, 2)
    $branchRate = [math]::Round([double]$xml.coverage.'branch-rate' * 100, 2)
    
    Write-Host ""
    Write-Host "Line Coverage:   $lineRate%" -ForegroundColor $(if ($lineRate -gt 80) { "Green" } elseif ($lineRate -gt 50) { "Yellow" } else { "Red" })
    Write-Host "Branch Coverage: $branchRate%" -ForegroundColor $(if ($branchRate -gt 80) { "Green" } elseif ($branchRate -gt 50) { "Yellow" } else { "Red" })
} catch {
    Write-Host ""
    Write-Host "No se pudo leer el resumen" -ForegroundColor Yellow
}

Write-Host ""
Write-Host "============================================" -ForegroundColor Cyan
Write-Host "Ubicacion: $reportPath/index.html" -ForegroundColor White
Write-Host "============================================" -ForegroundColor Cyan

# Abrir navegador
Write-Host ""
Write-Host "Abriendo reporte en el navegador..." -ForegroundColor Cyan
Start-Sleep -Seconds 1

try {
    Start-Process $indexFile
    Write-Host ""
    Write-Host "PROCESO COMPLETADO!" -ForegroundColor Green
    Write-Host ""
} catch {
    Write-Host ""
    Write-Host "Abre manualmente: $indexFile" -ForegroundColor Yellow
    Write-Host ""
}