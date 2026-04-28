# Script de prueba para crear un pedido y verificar fechas

$baseUrl = "http://localhost:5000/api"

# 1. Obtener un cliente
Write-Host "Obteniendo cliente..." -ForegroundColor Cyan
$clientesResponse = Invoke-WebRequest -Uri "$baseUrl/clientes" -Method Get -UseBasicParsing
$clientesData = $clientesResponse.Content | ConvertFrom-Json
$clientes = $clientesData.value
$clienteId = $clientes[0].id
Write-Host "Cliente obtenido: ID=$clienteId, Nombre=$($clientes[0].nombre)" -ForegroundColor Green

# 2. Obtener un producto
Write-Host "`nObteniendo producto..." -ForegroundColor Cyan
$productosResponse = Invoke-WebRequest -Uri "$baseUrl/productos" -Method Get -UseBasicParsing
$productosData = $productosResponse.Content | ConvertFrom-Json
$productos = $productosData[0..5] | Where-Object { $_.stock -gt 0 } | Select-Object -First 1
$productoId = $productos.id
$precioPorUnidad = $productos.precio
Write-Host "Producto obtenido: ID=$productoId, Nombre=$($productos.nombre), Precio=$precioPorUnidad" -ForegroundColor Green

# 3. Crear pedido
Write-Host "`nCreando pedido..." -ForegroundColor Cyan
$orderBody = @{
    idCliente = $clienteId
    idSucursal = 1
    idUsuario = 1
    formaDePago = "Efectivo"
    zonaId = 1
    detalles = @(
        @{
            idProducto = $productoId
            cantidad = 1
            precioUnitario = $precioPorUnidad
        }
    )
} | ConvertTo-Json

$orderResponse = Invoke-WebRequest -Uri "$baseUrl/orders" -Method Post -Body $orderBody -ContentType "application/json" -UseBasicParsing
$orderData = $orderResponse.Content | ConvertFrom-Json
$pedidoId = $orderData.pedidoId
Write-Host "Pedido creado: ID=$pedidoId" -ForegroundColor Green

# 4. Obtener detalles del pedido
Write-Host "`nObteniendo detalles del pedido..." -ForegroundColor Cyan
$pedidoResponse = Invoke-WebRequest -Uri "$baseUrl/orders/$pedidoId" -Method Get -UseBasicParsing
$pedidoDetails = $pedidoResponse.Content | ConvertFrom-Json

Write-Host "`n========== RESULTADO ===========" -ForegroundColor Yellow
Write-Host "ID Pedido: $($pedidoDetails.idPedido)" -ForegroundColor White
Write-Host "Fecha: $($pedidoDetails.fecha)" -ForegroundColor Cyan
Write-Host "Hora Entrega Estimada: $($pedidoDetails.horaEntregaEstimada)" -ForegroundColor Cyan
Write-Host "Fecha Entrega Estimada: $($pedidoDetails.fechaEntregaEstimada)" -ForegroundColor Cyan
Write-Host "Estado: $($pedidoDetails.estadoActual)" -ForegroundColor Cyan

# 5. Ver el historial
Write-Host "`n========== HISTORIAL ===========" -ForegroundColor Yellow
if ($pedidoDetails.historial -and $pedidoDetails.historial.Count -gt 0) {
    foreach ($item in $pedidoDetails.historial) {
        Write-Host "Estado: $($item.nombreEstado)" -ForegroundColor White
        Write-Host "Fecha/Hora: $($item.fechaHora)" -ForegroundColor Green
        Write-Host "Responsable: $($item.responsable)" -ForegroundColor White
        Write-Host "Observaciones: $($item.observaciones)" -ForegroundColor Gray
        Write-Host "---"
    }
} else {
    Write-Host "No hay historial" -ForegroundColor Yellow
}

Write-Host "`n✅ Prueba completada" -ForegroundColor Green
