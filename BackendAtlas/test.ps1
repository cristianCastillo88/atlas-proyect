$baseUrl = "http://localhost:5044"

# Test weatherforecast
try {
    $response = Invoke-WebRequest -Uri "$baseUrl/weatherforecast" -Method GET
    Write-Host "weatherforecast: $($response.StatusCode) - OK"
} catch {
    Write-Host "weatherforecast: Error - $($_.Exception.Message)"
}

# Test categorias
try {
    $response = Invoke-WebRequest -Uri "$baseUrl/api/categorias" -Method GET
    Write-Host "api/categorias: $($response.StatusCode) - OK"
} catch {
    Write-Host "api/categorias: Error - $($_.Exception.Message)"
}

# Test maestros/pagos
try {
    $response = Invoke-WebRequest -Uri "$baseUrl/api/maestros/pagos" -Method GET
    Write-Host "api/maestros/pagos: $($response.StatusCode) - OK"
} catch {
    Write-Host "api/maestros/pagos: Error - $($_.Exception.Message)"
}

# Test maestros/entregas
try {
    $response = Invoke-WebRequest -Uri "$baseUrl/api/maestros/entregas" -Method GET
    Write-Host "api/maestros/entregas: $($response.StatusCode) - OK"
} catch {
    Write-Host "api/maestros/entregas: Error - $($_.Exception.Message)"
}

# Test pedidos/dashboard
try {
    $response = Invoke-WebRequest -Uri "$baseUrl/api/pedidos/dashboard" -Method GET
    Write-Host "api/pedidos/dashboard: $($response.StatusCode) - OK"
} catch {
    Write-Host "api/pedidos/dashboard: Error - $($_.Exception.Message)"
}

# Test productos
try {
    $response = Invoke-WebRequest -Uri "$baseUrl/api/productos" -Method GET
    Write-Host "api/productos: $($response.StatusCode) - OK"
} catch {
    Write-Host "api/productos: Error - $($_.Exception.Message)"
}

# Test productos/{id} - assume id=1
try {
    $response = Invoke-WebRequest -Uri "$baseUrl/api/productos/1" -Method GET
    Write-Host "api/productos/1: $($response.StatusCode) - OK"
} catch {
    Write-Host "api/productos/1: Error - $($_.Exception.Message)"
}