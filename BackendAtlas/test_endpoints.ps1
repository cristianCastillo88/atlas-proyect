$baseUrl = "http://localhost:5044"

# Function to test GET
function Test-Get($endpoint) {
    try {
        $response = Invoke-WebRequest -Uri "$baseUrl$endpoint" -Method GET
        Write-Host "$endpoint GET: $($response.StatusCode)"
    } catch {
        Write-Host "$endpoint GET: $($_.Exception.Response.StatusCode.value__)"
    }
}

# Test GET endpoints
Test-Get "/weatherforecast"
Test-Get "/api/categorias"
Test-Get "/api/maestros/pagos"
Test-Get "/api/maestros/entregas"
Test-Get "/api/pedidos/dashboard"
Test-Get "/api/productos"
Test-Get "/api/productos/1"

# Test POST /api/pedidos
try {
    $json = '{"NombreCliente":"Test","DireccionCliente":"Test","TelefonoCliente":"123","MetodoPagoId":1,"TipoEntregaId":1,"Items":[{"ProductoId":1,"Cantidad":1}]}'
    $response = Invoke-WebRequest -Uri "$baseUrl/api/pedidos" -Method POST -Body $json -ContentType "application/json"
    Write-Host "/api/pedidos POST: $($response.StatusCode)"
} catch {
    Write-Host "/api/pedidos POST: $($_.Exception.Response.StatusCode.value__)"
}

# Test POST /api/productos
try {
    $json = '{"Nombre":"Test","Descripcion":"Test","Precio":10.0,"CategoriaId":1}'
    $response = Invoke-WebRequest -Uri "$baseUrl/api/productos" -Method POST -Body $json -ContentType "application/json"
    Write-Host "/api/productos POST: $($response.StatusCode)"
} catch {
    Write-Host "/api/productos POST: $($_.Exception.Response.StatusCode.value__)"
}

# Test PUT /api/productos/1
try {
    $json = '{"Id":1,"Nombre":"Test","Descripcion":"Test","Precio":10.0,"CategoriaId":1}'
    $response = Invoke-WebRequest -Uri "$baseUrl/api/productos/1" -Method PUT -Body $json -ContentType "application/json"
    Write-Host "/api/productos/1 PUT: $($response.StatusCode)"
} catch {
    Write-Host "/api/productos/1 PUT: $($_.Exception.Response.StatusCode.value__)"
}

# Test PATCH /api/pedidos/1/estado
try {
    $json = '{"NuevoEstadoId":2}'
    $response = Invoke-WebRequest -Uri "$baseUrl/api/pedidos/1/estado" -Method PATCH -Body $json -ContentType "application/json"
    Write-Host "/api/pedidos/1/estado PATCH: $($response.StatusCode)"
} catch {
    Write-Host "/api/pedidos/1/estado PATCH: $($_.Exception.Response.StatusCode.value__)"
}

# Test DELETE /api/productos/1
try {
    $response = Invoke-WebRequest -Uri "$baseUrl/api/productos/1" -Method DELETE
    Write-Host "/api/productos/1 DELETE: $($response.StatusCode)"
} catch {
    Write-Host "/api/productos/1 DELETE: $($_.Exception.Response.StatusCode.value__)"
}