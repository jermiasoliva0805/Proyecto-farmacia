# Ejemplo de Funcionamiento: Etiqueta Logística Cliente-Producto

## Escenario 1: Cliente Frecuente con Historial

### Cliente: María Ramírez
- **Historial de compras:**
  - Pedido #001: 5x Pañales Adulto (Cat: Incontinencia)
  - Pedido #002: 3x Pañales Adulto + 2x Cremas (Cat: Incontinencia, Cuidado Personal)
  - Pedido #003: 10x Pañales Adulto (Cat: Incontinencia)
  - Pedido #004: 2x Alcohol + 1x Pañales Adulto (Cat: Desinfectantes, Incontinencia)

- **Categorías encontradas:**
  - Incontinencia: 4 pedidos (aparece en todos)
  - Cuidado Personal: 1 pedido
  - Desinfectantes: 1 pedido

- **Etiqueta generada:** `Ramirez - Incontinencia`

### Email enviado:
```
ASUNTO: Confirmación de Pedido #005 [Relación: Ramirez - Incontinencia]

CUERPO:
┌─────────────────────────────────┐
│ Relación Cliente-Producto:      │
│ Ramirez - Incontinencia         │
└─────────────────────────────────┘
```

---

## Escenario 2: Institución Hospitalaria

### Cliente: Hospital Privado San José
- **Historial de compras:**
  - Pedido #045: Guantes Quirúrgicos (Cat: Insumos Quirúrgicos)
  - Pedido #046: Mascarillas N95 + Batas (Cat: Protección Médica, Insumos Quirúrgicos)
  - Pedido #047: Sueros + Jeringas (Cat: Insumos Médicos, Insumos Quirúrgicos)

- **Categorías encontradas:**
  - Insumos Quirúrgicos: 3 pedidos
  - Protección Médica: 1 pedido
  - Insumos Médicos: 1 pedido

- **Etiqueta generada:** `San José - Insumos Quirúrgicos + Insumos Médicos`

### Email enviado:
```
ASUNTO: Confirmación de Pedido #048 [Relación: San José - Insumos Quirúrgicos + Insumos Médicos]

CUERPO:
┌──────────────────────────────────────────────────────┐
│ Relación Cliente-Producto:                           │
│ San José - Insumos Quirúrgicos + Insumos Médicos    │
└──────────────────────────────────────────────────────┘
```

---

## Escenario 3: Cliente Nuevo (Sin Historial)

### Cliente: Juan López (Nuevo)
- **Historial de compras:** (Ninguno)
- **Etiqueta generada:** `Lopez - Consumidor General`

### Email enviado:
```
ASUNTO: Confirmación de Pedido #100 [Relación: Lopez - Consumidor General]

CUERPO:
┌──────────────────────────────┐
│ Relación Cliente-Producto:   │
│ Lopez - Consumidor General   │
└──────────────────────────────┘
```

---

## Escenario 4: Cliente con Compras Variadas

### Cliente: Farmacia Los Arcos
- **Historial de compras:**
  - Pedido #200: Medicamentos (Cat: Farmacéuticos)
  - Pedido #201: Vitaminas + Suplementos (Cat: Suplementos, Farmacéuticos)
  - Pedido #202: Cosméticos (Cat: Cosméticos)
  - Pedido #203: Medicamentos (Cat: Farmacéuticos)

- **Categorías encontradas:**
  - Farmacéuticos: 3 pedidos
  - Suplementos: 1 pedido
  - Cosméticos: 1 pedido

- **Etiqueta generada:** `Los Arcos - Farmacéuticos + Suplementos`

### Email enviado:
```
ASUNTO: Confirmación de Pedido #204 [Relación: Los Arcos - Farmacéuticos + Suplementos]

CUERPO:
┌──────────────────────────────────────────┐
│ Relación Cliente-Producto:               │
│ Los Arcos - Farmacéuticos + Suplementos  │
└──────────────────────────────────────────┘
```

---

## Datos que usa la lógica

### Base de Datos
```csharp
Cliente (IDCliente=1)
├── Nombre: "María"
├── Apellido: "Ramírez"
└── Pedidos[]
    ├── Pedido #001 → Detalles[]
    │   └── DetalleDePedido
    │       └── Producto.Categoria = "Incontinencia"
    ├── Pedido #002 → Detalles[]
    │   ├── DetalleDePedido → Producto.Categoria = "Incontinencia"
    │   └── DetalleDePedido → Producto.Categoria = "Cuidado Personal"
    └── ...
```

### Procesamiento
```
1. Obtener cliente (IDCliente = 1)
2. Obtener todos los Pedidos del cliente
3. Para cada Pedido:
   - Obtener los Detalles
   - Para cada Detalle:
     - Extraer Producto.Categoria
4. Contar ocurrencias de cada categoría
5. Ordenar por frecuencia
6. Tomar Top 2
7. Construir: "{Apellido} - {Top2_Categorias}"
```

---

## Cambios de Estado donde se Incluye

1. ✅ **Admin asigna Operario** (1→2)
   - Email: "Preparando tu Pedido [Relación: ...]"

2. ✅ **Admin asigna Cadete** (4→5)
   - Email: "Listo para Despachar [Relación: ...]"

3. ✅ **Pedido en Camino** (5→6)
   - Email: "En Ruta [Relación: ...]"

4. ✅ **Pedido Entregado** (6→7)
   - Email: "¡Entregado! [Relación: ...]"

5. ✅ **Intento Fallido** (6→8)
   - Email: "Intento Fallido [Relación: ...]"

6. ✅ **Cancelación Manual** (cualquier→9)
   - Email: "Pedido Cancelado [Relación: ...]"

---

## Ventajas para Logística

- **Rápida identificación del tipo de cliente**
- **Optimización de rutas**: Sabe qué esperar (medicinas, insumos, cosméticos, etc.)
- **Mejor comunicación**: Personalizada según relación comercial
- **Tracking mejorado**: El repartidor sabe la naturaleza del pedido
- **Seguridad**: Insumos médicos requieren cuidado especial

