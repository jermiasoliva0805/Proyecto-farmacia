using Back.Models;
using BCrypt.Net;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using CsvHelper;
using CsvHelper.Configuration;

namespace Back.Data
{
    /// <summary>
    /// DbInitializer: Semillado robusto, idempotente y tolerante a fallos.
    /// - Ejecuta etapas independientes, cada una protegida con try-catch
    /// - Si una etapa falla, continúa con las siguientes
    /// - Idempotencia: solo inserta datos si no existen
    /// - Contraseñas hasheadas con BCrypt
    /// - Logging detallado para debuggeo en Azure
    /// </summary>
    public static class DbInitializer
    {
        public static void Initialize(AppDbContext context)
        {
            try
            {
                Console.WriteLine("\n═══════════════════════════════════════════════════════════");
                Console.WriteLine($"▶️ Iniciando DbInitializer (Seed de BD)");
                Console.WriteLine($"📍 BaseDirectory: {AppDomain.CurrentDomain.BaseDirectory}");
                Console.WriteLine($"📍 CurrentDirectory: {Directory.GetCurrentDirectory()}");
                Console.WriteLine("═══════════════════════════════════════════════════════════");

                // PASO 1: EJECUTAR MIGRACIONES
                Console.WriteLine("\n▶️ Ejecutando migraciones con context.Database.Migrate()...");
                context.Database.Migrate();
                Console.WriteLine("✅ Migraciones ejecutadas correctamente");

                // PASO 2: SEMBRAMOS POR ETAPAS (cada una independiente y protegida)
                SeedMotivosCancelacion(context);
                SeedEstadosDePedidos(context);
                var (localidad, barrio) = SeedLocalidadYBarrio(context);
                var sucursal = SeedSucursal(context);
                var zonas = SeedZonas(context);
                SeedMapeoBarrios(context, zonas);
                SeedProductos(context);
                SeedUsuarios(context, sucursal);
                SeedClientes(context, localidad, barrio);

                // RESUMEN FINAL
                Console.WriteLine("\n═══════════════════════════════════════════════════════════");
                Console.WriteLine("✅ SEMILLADO COMPLETADO EXITOSAMENTE");
                Console.WriteLine("═══════════════════════════════════════════════════════════\n");
            }
            catch (Exception ex)
            {
                // Este catch solo se ejecuta si algo falla ANTES de las etapas
                Console.WriteLine($"\n❌ ERROR CRÍTICO EN DBINITIALIZER: {ex.Message}");
                Console.WriteLine($"❌ StackTrace: {ex.StackTrace}");
                throw;
            }
        }

        // ════════════════════════════════════════════════════════════════════════════════
        // ETAPA 1: MOTIVOS DE CANCELACIÓN
        // ════════════════════════════════════════════════════════════════════════════════
        private static void SeedMotivosCancelacion(AppDbContext context)
        {
            try
            {
                Console.WriteLine("\n▶️ Iniciando SeedMotivosCancelacion...");

                var motivosACrear = new[]
                {
                    new { Nombre = "Arrepentimiento", Activo = true },
                    new { Nombre = "Falta de stock", Activo = true },
                    new { Nombre = "Error en el pago", Activo = true },
                    new { Nombre = "Dirección incorrecta", Activo = true }
                };

                int insertados = 0;
                foreach (var motivo in motivosACrear)
                {
                    // IDEMPOTENCIA: chequear si existe por nombre
                    if (!context.MotivosCancelacion.Any(m => m.Nombre == motivo.Nombre))
                    {
                        context.MotivosCancelacion.Add(new MotivoCancelacion
                        {
                            Nombre = motivo.Nombre,
                            Activo = motivo.Activo
                        });
                        insertados++;
                    }
                }

                if (insertados > 0)
                {
                    context.SaveChanges();
                    Console.WriteLine($"✅ SeedMotivosCancelacion OK: {insertados} registros insertados");
                }
                else
                {
                    Console.WriteLine($"ℹ️ SeedMotivosCancelacion omitido: todos los motivos ya existen");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Error en SeedMotivosCancelacion: {ex.Message}");
                Console.WriteLine($"❌ StackTrace: {ex.StackTrace}");
                // NO relanzar excepción, continuar con la siguiente etapa
            }
        }

        // ════════════════════════════════════════════════════════════════════════════════
        // ETAPA 2: ESTADOS DE PEDIDO
        // ════════════════════════════════════════════════════════════════════════════════
        private static void SeedEstadosDePedidos(AppDbContext context)
        {
            try
            {
                Console.WriteLine("\n▶️ Iniciando SeedEstadosDePedidos...");

                var estadosACrear = new[]
                {
                    new { NombreEstado = "Sin preparar", motivo_cancelacion = "N/A" },
                    new { NombreEstado = "Preparar pedido", motivo_cancelacion = "N/A" },
                    new { NombreEstado = "Demorado", motivo_cancelacion = "N/A" },
                    new { NombreEstado = "Listo para despachar", motivo_cancelacion = "N/A" },
                    new { NombreEstado = "Despachando", motivo_cancelacion = "N/A" },
                    new { NombreEstado = "En camino", motivo_cancelacion = "N/A" },
                    new { NombreEstado = "Entregado", motivo_cancelacion = "N/A" },
                    new { NombreEstado = "Entrega fallida", motivo_cancelacion = "N/A" },
                    new { NombreEstado = "Cancelado", motivo_cancelacion = "Arrepentimiento" }
                };

                int insertados = 0;
                foreach (var estado in estadosACrear)
                {
                    // IDEMPOTENCIA: chequear si existe por nombre
                    if (!context.EstadosDePedidos.Any(e => e.NombreEstado == estado.NombreEstado))
                    {
                        context.EstadosDePedidos.Add(new EstadoDePedido
                        {
                            NombreEstado = estado.NombreEstado,
                            motivo_cancelacion = estado.motivo_cancelacion
                        });
                        insertados++;
                    }
                }

                if (insertados > 0)
                {
                    context.SaveChanges();
                    Console.WriteLine($"✅ SeedEstadosDePedidos OK: {insertados} registros insertados");
                }
                else
                {
                    Console.WriteLine($"ℹ️ SeedEstadosDePedidos omitido: todos los estados ya existen");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Error en SeedEstadosDePedidos: {ex.Message}");
                Console.WriteLine($"❌ StackTrace: {ex.StackTrace}");
            }
        }

        // ════════════════════════════════════════════════════════════════════════════════
        // ETAPA 3: LOCALIDADES Y BARRIOS
        // ════════════════════════════════════════════════════════════════════════════════
        private static (Localidad localidad, Barrio barrio) SeedLocalidadYBarrio(AppDbContext context)
        {
            Localidad localidad = null;
            Barrio barrio = null;

            try
            {
                Console.WriteLine("\n▶️ Iniciando SeedLocalidadYBarrio...");

                // Buscar Córdoba, si no existe crearla
                localidad = context.Localidades.FirstOrDefault(l => l.Ciudad == "Córdoba");
                if (localidad == null)
                {
                    localidad = new Localidad
                    {
                        Ciudad = "Córdoba",
                        Provincia = "Córdoba",
                        CodigoPostal = "5000"
                    };
                    context.Localidades.Add(localidad);
                    context.SaveChanges();
                    Console.WriteLine($"✅ Localidad 'Córdoba' creada (ID: {localidad.IDLocalidad})");
                }
                else
                {
                    Console.WriteLine($"ℹ️ Localidad 'Córdoba' ya existe (ID: {localidad.IDLocalidad})");
                }

                // Buscar barrio, si no existe crearlo
                barrio = context.Barrios.FirstOrDefault(b =>
                    b.Nombre == "Nueva Córdoba" && b.IDLocalidad == localidad.IDLocalidad);
                if (barrio == null)
                {
                    barrio = new Barrio
                    {
                        Nombre = "Nueva Córdoba",
                        IDLocalidad = localidad.IDLocalidad
                    };
                    context.Barrios.Add(barrio);
                    context.SaveChanges();
                    Console.WriteLine($"✅ Barrio 'Nueva Córdoba' creado (ID: {barrio.IDBarrio})");
                }
                else
                {
                    Console.WriteLine($"ℹ️ Barrio 'Nueva Córdoba' ya existe (ID: {barrio.IDBarrio})");
                }

                Console.WriteLine($"✅ SeedLocalidadYBarrio OK");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Error en SeedLocalidadYBarrio: {ex.Message}");
                Console.WriteLine($"❌ StackTrace: {ex.StackTrace}");
            }

            return (localidad, barrio);
        }

        // ════════════════════════════════════════════════════════════════════════════════
        // ETAPA 4: SUCURSAL
        // ════════════════════════════════════════════════════════════════════════════════
        private static Sucursal SeedSucursal(AppDbContext context)
        {
            Sucursal sucursal = null;

            try
            {
                Console.WriteLine("\n▶️ Iniciando SeedSucursal...");

                // IDEMPOTENCIA: buscar por nombre
                sucursal = context.Sucursales.FirstOrDefault(s =>
                    s.NombreSucursal == "Farmacia General Paz Centro");

                if (sucursal == null)
                {
                    sucursal = new Sucursal
                    {
                        NombreSucursal = "Farmacia General Paz Centro",
                        Dirección = "Av. Colon 123",
                        Teléfono = "3514445566"
                    };
                    context.Sucursales.Add(sucursal);
                    context.SaveChanges();
                    Console.WriteLine($"✅ Sucursal 'Farmacia General Paz Centro' creada (ID: {sucursal.IDSucursal})");
                }
                else
                {
                    Console.WriteLine($"ℹ️ Sucursal 'Farmacia General Paz Centro' ya existe (ID: {sucursal.IDSucursal})");
                }

                Console.WriteLine($"✅ SeedSucursal OK");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Error en SeedSucursal: {ex.Message}");
                Console.WriteLine($"❌ StackTrace: {ex.StackTrace}");
            }

            return sucursal;
        }

        // ════════════════════════════════════════════════════════════════════════════════
        // ETAPA 5: ZONAS DE REPARTO
        // ════════════════════════════════════════════════════════════════════════════════
        private static Dictionary<int, Zona> SeedZonas(AppDbContext context)
        {
            var zonas = new Dictionary<int, Zona>();

            try
            {
                Console.WriteLine("\n▶️ Iniciando SeedZonas...");

                var zonasACrear = new[]
                {
                    new { Id = 1, Nombre = "Centro / N. Córdoba" },
                    new { Id = 2, Nombre = "Norte / Noroeste" },
                    new { Id = 3, Nombre = "Sur" },
                    new { Id = 4, Nombre = "Este / Gral. Paz" },
                    new { Id = 5, Nombre = "Oeste" }
                };

                int insertados = 0;
                foreach (var zona in zonasACrear)
                {
                    // IDEMPOTENCIA: chequear si existe por nombre
                    var zonaExistente = context.Zonas.FirstOrDefault(z => z.Nombre == zona.Nombre);
                    if (zonaExistente == null)
                    {
                        var nuevaZona = new Zona { Nombre = zona.Nombre };
                        context.Zonas.Add(nuevaZona);
                        zonas[zona.Id] = nuevaZona;
                        insertados++;
                    }
                    else
                    {
                        zonas[zona.Id] = zonaExistente;
                    }
                }

                if (insertados > 0)
                {
                    context.SaveChanges();
                    Console.WriteLine($"✅ SeedZonas OK: {insertados} zonas insertadas");
                }
                else
                {
                    Console.WriteLine($"ℹ️ SeedZonas omitido: todas las zonas ya existen");
                }

                // Recargar todas las zonas para usarlas en mapeo
                zonas.Clear();
                var allZonas = context.Zonas.ToList();
                for (int i = 0; i < allZonas.Count; i++)
                {
                    zonas[i + 1] = allZonas[i];
                }

                Console.WriteLine($"✅ SeedZonas completado con {zonas.Count} zonas cargadas");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Error en SeedZonas: {ex.Message}");
                Console.WriteLine($"❌ StackTrace: {ex.StackTrace}");
            }

            return zonas;
        }

        // ════════════════════════════════════════════════════════════════════════════════
        // ETAPA 5B: MAPEO DE BARRIOS A ZONAS
        // ════════════════════════════════════════════════════════════════════════════════
        private static void SeedMapeoBarrios(AppDbContext context, Dictionary<int, Zona> zonas)
        {
            try
            {
                Console.WriteLine("\n▶️ Iniciando SeedMapeoBarrios (Mapeo de Barrios a Zonas)...");

                // Obtener localidad Córdoba
                var localidad = context.Localidades.FirstOrDefault(l => l.Ciudad == "Córdoba");
                if (localidad == null)
                {
                    Console.WriteLine("ℹ️ SeedMapeoBarrios omitido: Localidad 'Córdoba' no existe");
                    return;
                }

                // Mapeo de barrios a zonas
                var mapeoBarrios = new Dictionary<string, int>
                {
                    // Zona 1: Centro / N. Córdoba
                    { "Centro", 1 },
                    { "Nueva Córdoba", 1 },
                    { "Güemes", 1 },
                    { "Alberdi", 1 },

                    // Zona 2: Norte / Noroeste
                    { "Alta Córdoba", 2 },
                    { "Cerro de las Rosas", 2 },
                    { "Argüello", 2 },
                    { "Villa Belgrano", 2 },
                    { "Cofico", 2 },

                    // Zona 3: Sur
                    { "Barrio Jardín", 3 },
                    { "San Carlos", 3 },
                    { "Villa El Libertador", 3 },
                    { "Tejas del Sur", 3 },

                    // Zona 4: Este / Gral. Paz
                    { "General Paz", 4 },
                    { "San Vicente", 4 },
                    { "Pueyrredón", 4 },
                    { "Juniors", 4 },

                    // Zona 5: Oeste
                    { "Alto Alberdi", 5 },
                    { "Los Plátanos", 5 },
                    { "Las Palmas", 5 },
                    { "Villa Cabrera", 5 }
                };

                int actualizados = 0;
                foreach (var mapeo in mapeoBarrios)
                {
                    var nombreBarrio = mapeo.Key;
                    var idZona = mapeo.Value;

                    // Obtener la zona
                    if (!zonas.ContainsKey(idZona))
                    {
                        Console.WriteLine($"⚠️ Zona {idZona} no encontrada para barrio '{nombreBarrio}'");
                        continue;
                    }

                    var zona = zonas[idZona];

                    // Buscar o crear el barrio
                    var barrio = context.Barrios.FirstOrDefault(b =>
                        b.Nombre == nombreBarrio && b.IDLocalidad == localidad.IDLocalidad);

                    if (barrio != null)
                    {
                        // Si el barrio ya existe pero no tiene zona asignada, actualizar
                        if (barrio.ZonaId == null)
                        {
                            barrio.ZonaId = zona.Id;
                            context.Barrios.Update(barrio);
                            actualizados++;
                        }
                    }
                    else
                    {
                        // Crear nuevo barrio con zona
                        var nuevoBarrio = new Barrio
                        {
                            Nombre = nombreBarrio,
                            IDLocalidad = localidad.IDLocalidad,
                            ZonaId = zona.Id
                        };
                        context.Barrios.Add(nuevoBarrio);
                        actualizados++;
                    }
                }

                if (actualizados > 0)
                {
                    context.SaveChanges();
                    Console.WriteLine($"✅ SeedMapeoBarrios OK: {actualizados} barrios procesados");
                }
                else
                {
                    Console.WriteLine($"ℹ️ SeedMapeoBarrios omitido: todos los barrios ya están mapeados");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Error en SeedMapeoBarrios: {ex.Message}");
                Console.WriteLine($"❌ StackTrace: {ex.StackTrace}");
            }
        }

        // ════════════════════════════════════════════════════════════════════════════════
        // ETAPA 6: PRODUCTOS (desde CSV con fallback hardcodeado)
        // ════════════════════════════════════════════════════════════════════════════════
        private static void SeedProductos(AppDbContext context)
        {
            try
            {
                Console.WriteLine("\n▶️ Iniciando SeedProductos...");

                // IDEMPOTENCIA: si ya hay productos, omitir todo
                if (context.Productos.Any())
                {
                    int count = context.Productos.Count();
                    Console.WriteLine($"ℹ️ SeedProductos omitido: la tabla ya contiene {count} productos");
                    return;
                }

                // Cargar desde CSV
                var listaProductos = CargarProductosDesdeCSV();

                // Si CSV no devuelve datos (no encontrado o error), usar fallback
                if (listaProductos.Count == 0)
                {
                    Console.WriteLine("⚠️ CSV sin resultados. Using fallback hardcodeado...");
                    listaProductos = new List<Producto>
                    {
                        new Producto { NombreProducto = "EDP Balance By Dadatina", Descripcion = "70ml, Dadatina", Categoria = "Perfumeria", CantidadProducto = 50, PrecioProducto = 47900m },
                        new Producto { NombreProducto = "Boos Intense Black EDP", Descripcion = "90ml, fragancia masculina", Categoria = "Perfumeria", CantidadProducto = 100, PrecioProducto = 52927m },
                        new Producto { NombreProducto = "Shakira Amarillo EDP", Descripcion = "80ml, fragancia femenina", Categoria = "Perfumeria", CantidadProducto = 30, PrecioProducto = 45045m },
                        new Producto { NombreProducto = "Oneblade Face+Body", Descripcion = "Philips QP2824 - Afeitadora", Categoria = "Electro", CantidadProducto = 30, PrecioProducto = 110932.79m },
                        new Producto { NombreProducto = "Nebulizador Pistón", Descripcion = "ASPEN Nbb02-A-50 Silencioso", Categoria = "Electro", CantidadProducto = 20, PrecioProducto = 80330.58m },
                        new Producto { NombreProducto = "Tensiometro Aneroide", Descripcion = "FEMMTO Kit con Estetoscopio", Categoria = "Electro", CantidadProducto = 25, PrecioProducto = 42274.33m },
                        new Producto { NombreProducto = "Planchita Pelo Bellissima", Descripcion = "Ceramic Long Plates", Categoria = "Electro", CantidadProducto = 15, PrecioProducto = 75199m },
                        new Producto { NombreProducto = "Alcohol en Gel 500ml", Descripcion = "Antibacterial con dosificador", Categoria = "Salud", CantidadProducto = 200, PrecioProducto = 3500m }
                    };
                }

                context.Productos.AddRange(listaProductos);
                context.SaveChanges();
                Console.WriteLine($"✅ SeedProductos OK: {listaProductos.Count} productos cargados");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Error en SeedProductos: {ex.Message}");
                Console.WriteLine($"❌ StackTrace: {ex.StackTrace}");
            }
        }

        // ════════════════════════════════════════════════════════════════════════════════
        // ETAPA 7: USUARIOS
        // ════════════════════════════════════════════════════════════════════════════════
        private static void SeedUsuarios(AppDbContext context, Sucursal sucursal)
        {
            try
            {
                Console.WriteLine("\n▶️ Iniciando SeedUsuarios...");

                if (sucursal == null)
                {
                    Console.WriteLine("⚠️ SeedUsuarios omitido: Sucursal no disponible");
                    return;
                }

                var usuariosACrear = new[]
                {
                    new { Nombre = "Encargado", Apellido = "Sistema", UsuarioNombre = "encargado", Contraseña = "123", Rol = "Encargado", Mail = "admin@test.com" },
                    new { Nombre = "Ana", Apellido = "Lopez", UsuarioNombre = "anaLop", Contraseña = "123", Rol = "Operario", Mail = "ana@test.com" },
                    new { Nombre = "Luis", Apellido = "Gomez", UsuarioNombre = "luis", Contraseña = "123", Rol = "Operario", Mail = "luis@test.com" },
                    new { Nombre = "Marta", Apellido = "Sosa", UsuarioNombre = "marta", Contraseña = "123", Rol = "Operario", Mail = "marta@test.com" },
                    new { Nombre = "Carlos", Apellido = "Martinez", UsuarioNombre = "carlos", Contraseña = "123", Rol = "Cadete", Mail = "carlos@test.com" },
                    new { Nombre = "Pedro", Apellido = "Romero", UsuarioNombre = "pedro", Contraseña = "123", Rol = "Cadete", Mail = "pedro@test.com" },
                    new { Nombre = "Sofia", Apellido = "Garcia", UsuarioNombre = "sofia", Contraseña = "123", Rol = "Cadete", Mail = "sofia@test.com" },
                };

                int insertados = 0;
                foreach (var usuarioData in usuariosACrear)
                {
                    // IDEMPOTENCIA: chequear si existe por UsuarioNombre
                    if (!context.Usuarios.Any(u => u.UsuarioNombre == usuarioData.UsuarioNombre))
                    {
                        var usuario = new Usuario
                        {
                            Nombre = usuarioData.Nombre,
                            Apellido = usuarioData.Apellido,
                            UsuarioNombre = usuarioData.UsuarioNombre,
                            // HASHEAR CON BCRYPT (no texto plano)
                            Contraseña = BCrypt.Net.BCrypt.HashPassword(usuarioData.Contraseña),
                            Rol = usuarioData.Rol,
                            IDSucursal = sucursal.IDSucursal,
                            Mail = usuarioData.Mail
                        };
                        context.Usuarios.Add(usuario);
                        insertados++;
                    }
                }

                if (insertados > 0)
                {
                    context.SaveChanges();
                    Console.WriteLine($"✅ SeedUsuarios OK: {insertados} usuarios creados (contraseñas hasheadas con BCrypt)");
                }
                else
                {
                    Console.WriteLine($"ℹ️ SeedUsuarios omitido: todos los usuarios ya existen");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Error en SeedUsuarios: {ex.Message}");
                Console.WriteLine($"❌ StackTrace: {ex.StackTrace}");
            }
        }

        // ════════════════════════════════════════════════════════════════════════════════
        // ETAPA 8: CLIENTES
        // ════════════════════════════════════════════════════════════════════════════════
        private static void SeedClientes(AppDbContext context, Localidad localidad, Barrio barrio)
        {
            try
            {
                Console.WriteLine("\n▶️ Iniciando SeedClientes...");

                if (localidad == null || barrio == null)
                {
                    Console.WriteLine("⚠️ SeedClientes omitido: Localidad o Barrio no disponibles");
                    return;
                }

                // IDEMPOTENCIA: si ya hay clientes, omitir todo
                if (context.Clientes.Any())
                {
                    int count = context.Clientes.Count();
                    Console.WriteLine($"ℹ️ SeedClientes omitido: la tabla ya contiene {count} clientes");
                    return;
                }

                var clientes = new List<Cliente>
                {
                    new Cliente { Nombre = "Juan", Apellido = "Perez", DNI = "30123456", IDBarrio = barrio.IDBarrio, IDLocalidad = localidad.IDLocalidad, Direccion = "Belgrano 800", Mail = "juan@test.com" },
                    new Cliente { Nombre = "Maria", Apellido = "Gonzalez", DNI = "32654321", IDBarrio = barrio.IDBarrio, IDLocalidad = localidad.IDLocalidad, Direccion = "San Martin 500", Mail = "maria@test.com" },
                    new Cliente { Nombre = "Carlos", Apellido = "Rodriguez", DNI = "31789456", IDBarrio = barrio.IDBarrio, IDLocalidad = localidad.IDLocalidad, Direccion = "Av. Velez Sarsfield 1200", Mail = "carlos@test.com" },
                    new Cliente { Nombre = "Ana", Apellido = "Martinez", DNI = "33456789", IDBarrio = barrio.IDBarrio, IDLocalidad = localidad.IDLocalidad, Direccion = "Calle Ituzaingo 450", Mail = "ana@test.com" },
                    new Cliente { Nombre = "Roberto", Apellido = "Lopez", DNI = "34567890", IDBarrio = barrio.IDBarrio, IDLocalidad = localidad.IDLocalidad, Direccion = "Avenida Colon 600", Mail = "roberto@test.com" },
                    new Cliente { Nombre = "Gabriela", Apellido = "Sanchez", DNI = "35678901", IDBarrio = barrio.IDBarrio, IDLocalidad = localidad.IDLocalidad, Direccion = "Ruta Nacional 9 Km 10", Mail = "gabriela@test.com" },
                    new Cliente { Nombre = "Fernando", Apellido = "Diaz", DNI = "36789012", IDBarrio = barrio.IDBarrio, IDLocalidad = localidad.IDLocalidad, Direccion = "Calle Hipólito Irigoyen 800", Mail = "fernando@test.com" },
                    new Cliente { Nombre = "Alejandra", Apellido = "Torres", DNI = "37890123", IDBarrio = barrio.IDBarrio, IDLocalidad = localidad.IDLocalidad, Direccion = "Paseo Sobremonte 350", Mail = "alejandra@test.com" },
                    new Cliente { Nombre = "Jorge", Apellido = "Castro", DNI = "38901234", IDBarrio = barrio.IDBarrio, IDLocalidad = localidad.IDLocalidad, Direccion = "Calle General Paz 950", Mail = "jorge@test.com" },
                    new Cliente { Nombre = "Patricia", Apellido = "Flores", DNI = "39012345", IDBarrio = barrio.IDBarrio, IDLocalidad = localidad.IDLocalidad, Direccion = "Avenida Maipú 1100", Mail = "patricia@test.com" },
                };

                context.Clientes.AddRange(clientes);
                context.SaveChanges();
                Console.WriteLine($"✅ SeedClientes OK: {clientes.Count} clientes creados");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Error en SeedClientes: {ex.Message}");
                Console.WriteLine($"❌ StackTrace: {ex.StackTrace}");
            }
        }

        // ════════════════════════════════════════════════════════════════════════════════
        // MÉTODOS AUXILIARES
        // ════════════════════════════════════════════════════════════════════════════════

        /// <summary>
        /// Carga productos desde CSV. Completamente protegido: si falla, devuelve lista vacía.
        /// No relanza excepciones.
        /// </summary>
        private static List<Producto> CargarProductosDesdeCSV()
        {
            var productos = new List<Producto>();

            try
            {
                var rutasPosibles = new[]
                {
                    Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Data", "SeedData", "catalogo_perfumeria - Perfumeria.csv"),
                    Path.Combine(Directory.GetCurrentDirectory(), "Data", "SeedData", "catalogo_perfumeria - Perfumeria.csv"),
                    Path.Combine(Directory.GetCurrentDirectory(), "..", "Back", "Data", "SeedData", "catalogo_perfumeria - Perfumeria.csv"),
                    "Data/SeedData/catalogo_perfumeria - Perfumeria.csv",
                    "./Data/SeedData/catalogo_perfumeria - Perfumeria.csv",
                };

                string rutaCSV = null;

                // Buscar CSV en todas las rutas posibles
                Console.WriteLine("  🔍 Buscando CSV de productos...");
                foreach (var ruta in rutasPosibles)
                {
                    bool existe = File.Exists(ruta);
                    Console.WriteLine($"    - {ruta} -> {(existe ? "✅ EXISTE" : "❌ no existe")}");
                    if (existe)
                    {
                        rutaCSV = ruta;
                        Console.WriteLine($"    ✅ CSV seleccionado: {ruta}");
                        break;
                    }
                }

                // Si no encuentra CSV, devolver lista vacía (sin lanzar excepción)
                if (rutaCSV == null)
                {
                    Console.WriteLine("  ⚠️ CSV no encontrado en ninguna ruta");
                    return productos;
                }

                // Leer CSV de forma segura
                using (var reader = new StreamReader(rutaCSV, System.Text.Encoding.UTF8))
                using (var csv = new CsvReader(reader, CultureInfo.InvariantCulture))
                {
                    csv.Context.RegisterClassMap<ProductoCSVMap>();
                    var records = csv.GetRecords<ProductoCSV>().ToList();
                    Console.WriteLine($"  📄 CSV leído: {records.Count} registros totales");

                    foreach (var record in records)
                    {
                        if (string.IsNullOrWhiteSpace(record.NombreProducto))
                            continue;

                        decimal precio = 0;
                        if (!string.IsNullOrWhiteSpace(record.PrecioProducto))
                        {
                            var precioStr = record.PrecioProducto.Trim()
                                .Replace("\"", "")
                                .Replace(",", ".");

                            if (!decimal.TryParse(precioStr, CultureInfo.InvariantCulture, out precio))
                                precio = 0;
                        }

                        productos.Add(new Producto
                        {
                            NombreProducto = record.NombreProducto.Trim(),
                            Descripcion = record.Descripcion?.Trim() ?? "",
                            Categoria = record.Categoria?.Trim() ?? "General",
                            CantidadProducto = 100,
                            PrecioProducto = precio
                        });
                    }

                    Console.WriteLine($"  ✅ Se parsearon {productos.Count} productos del CSV");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"  ❌ Excepción en CargarProductosDesdeCSV: {ex.Message}");
                Console.WriteLine($"  ❌ StackTrace: {ex.StackTrace}");
                Console.WriteLine("  ⚠️ Se devolverá lista vacía (el caller usará fallback)");
                // NO relanzar, solo devolver lista vacía
            }

            return productos;
        }

        // ════════════════════════════════════════════════════════════════════════════════
        // CLASES AUXILIARES PARA CSV
        // ════════════════════════════════════════════════════════════════════════════════

        private class ProductoCSV
        {
            public string IDProducto { get; set; }
            public string NombreProducto { get; set; }
            public string Descripcion { get; set; }
            public string Categoria { get; set; }
            public string PrecioProducto { get; set; }
        }

        private sealed class ProductoCSVMap : ClassMap<ProductoCSV>
        {
            public ProductoCSVMap()
            {
                Map(m => m.IDProducto).Index(0);
                Map(m => m.NombreProducto).Index(1);
                Map(m => m.Descripcion).Index(2);
                Map(m => m.Categoria).Index(3);
                Map(m => m.PrecioProducto).Index(4);
            }
        }
    }
}
