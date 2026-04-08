using Back.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using CsvHelper;
using System.Globalization;
using CsvHelper.Configuration;

namespace Back.Data
{
    public static class DbInitializer
    {
        public static void Initialize(AppDbContext context)
        {
            // IMPORTANTE (Producción/Azure):
            // No borrar ni recrear la base en el startup.
            // En su lugar: asegurar esquema existente (migraciones) y sembrar solo si falta.
            context.Database.Migrate();

            // Si ya hay usuarios (o cualquier tabla clave), asumimos que ya está sembrado.
            // Ajustá esta condición si preferís otra tabla.
            if (context.Usuarios.Any())
            {
                Console.WriteLine("ℹ️ Base ya inicializada. Seed omitido.");
                return;
            }

            // 1. MOTIVOS DE CANCELACIÓN
            context.MotivosCancelacion.AddRange(
                new MotivoCancelacion { Nombre = "Arrepentimiento", Activo = true },
                new MotivoCancelacion { Nombre = "Falta de stock", Activo = true },
                new MotivoCancelacion { Nombre = "Error en el pago", Activo = true },
                new MotivoCancelacion { Nombre = "Dirección incorrecta", Activo = true }
            );
            context.SaveChanges();

            // 2. ESTADOS DE PEDIDO
            context.EstadosDePedidos.AddRange(
                new EstadoDePedido { NombreEstado = "Sin preparar", motivo_cancelacion = "N/A" },        // 1
                new EstadoDePedido { NombreEstado = "Preparar pedido", motivo_cancelacion = "N/A" },    // 2
                new EstadoDePedido { NombreEstado = "Demorado", motivo_cancelacion = "N/A" },           // 3
                new EstadoDePedido { NombreEstado = "Listo para despachar", motivo_cancelacion = "N/A" }, // 4
                new EstadoDePedido { NombreEstado = "Despachando", motivo_cancelacion = "N/A" },        // 5
                new EstadoDePedido { NombreEstado = "En camino", motivo_cancelacion = "N/A" },          // 6
                new EstadoDePedido { NombreEstado = "Entregado", motivo_cancelacion = "N/A" },          // 7
                new EstadoDePedido { NombreEstado = "Entrega fallida", motivo_cancelacion = "N/A" },    // 8
                new EstadoDePedido { NombreEstado = "Cancelado", motivo_cancelacion = "Arrepentimiento" } // 9
            );
            context.SaveChanges();

            // 3. LOCALIDADES Y BARRIOS
            var cordoba = new Localidad { Ciudad = "Córdoba", Provincia = "Córdoba", CodigoPostal = "5000" };
            context.Localidades.Add(cordoba);
            context.SaveChanges();

            var barrioGral = new Barrio { Nombre = "Nueva Córdoba", IDLocalidad = cordoba.IDLocalidad };
            context.Barrios.Add(barrioGral);
            context.SaveChanges();

            // 4. SUCURSALES
            var suc = new Sucursal { NombreSucursal = "Farmacia General Paz Centro", Dirección = "Av. Colon 123", Teléfono = "3514445566" };
            context.Sucursales.Add(suc);
            context.SaveChanges();

            // 4.1 PRODUCTOS - Cargando desde CSV
            var rutaCSV = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Data", "SeedData", "catalogo_perfumeria - Perfumeria.csv");
            var listaProductos = CargarProductosDesdeCSV(rutaCSV);

            if (listaProductos.Count == 0)
            {
                // Fallback: productos hardcodeados si no encuentra el CSV
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
                Console.WriteLine("⚠️ CSV no encontrado. Usando productos hardcodeados.");
            }

            context.Productos.AddRange(listaProductos);
            context.SaveChanges();
            Console.WriteLine($"✅ {listaProductos.Count} productos cargados exitosamente.");

            // 5. USUARIOS
            var userAdmin = new Usuario { Nombre = "Encargado", Apellido = "Sistema", UsuarioNombre = "encargado", Contraseña = "123", Rol = "Encargado", IDSucursal = suc.IDSucursal, Mail = "admin@test.com" };
            var opAna = new Usuario { Nombre = "Ana", Apellido = "Lopez", UsuarioNombre = "anaLop", Contraseña = "123", Rol = "Operario", IDSucursal = suc.IDSucursal, Mail = "ana@test.com" };
            var opLuis = new Usuario { Nombre = "Luis", Apellido = "Gomez", UsuarioNombre = "luis", Contraseña = "123", Rol = "Operario", IDSucursal = suc.IDSucursal, Mail = "luis@test.com" };
            var opMarta = new Usuario { Nombre = "Marta", Apellido = "Sosa", UsuarioNombre = "marta", Contraseña = "123", Rol = "Operario", IDSucursal = suc.IDSucursal, Mail = "marta@test.com" };
            var cadete1 = new Usuario { Nombre = "Carlos", Apellido = "Martinez", UsuarioNombre = "carlos", Contraseña = "123", Rol = "Cadete", IDSucursal = suc.IDSucursal, Mail = "carlos@test.com" };
            var cadete2 = new Usuario { Nombre = "Pedro", Apellido = "Romero", UsuarioNombre = "pedro", Contraseña = "123", Rol = "Cadete", IDSucursal = suc.IDSucursal, Mail = "pedro@test.com" };
            var cadete3 = new Usuario { Nombre = "Sofia", Apellido = "Garcia", UsuarioNombre = "sofia", Contraseña = "123", Rol = "Cadete", IDSucursal = suc.IDSucursal, Mail = "sofia@test.com" };

            context.Usuarios.AddRange(userAdmin, opAna, opLuis, opMarta, cadete1, cadete2, cadete3);
            context.SaveChanges();

            // 6. CLIENTES (Ampliados a 10 para reportes detallados)
            var clientes = new List<Cliente>
            {
                new Cliente { Nombre = "Juan", Apellido = "Perez", DNI = "30123456", IDBarrio = barrioGral.IDBarrio, IDLocalidad = cordoba.IDLocalidad, Direccion = "Belgrano 800", Mail = "juan@test.com" },
                new Cliente { Nombre = "Maria", Apellido = "Gonzalez", DNI = "32654321", IDBarrio = barrioGral.IDBarrio, IDLocalidad = cordoba.IDLocalidad, Direccion = "San Martin 500", Mail = "maria@test.com" },
                new Cliente { Nombre = "Carlos", Apellido = "Rodriguez", DNI = "31789456", IDBarrio = barrioGral.IDBarrio, IDLocalidad = cordoba.IDLocalidad, Direccion = "Av. Velez Sarsfield 1200", Mail = "carlos@test.com" },
                new Cliente { Nombre = "Ana", Apellido = "Martinez", DNI = "33456789", IDBarrio = barrioGral.IDBarrio, IDLocalidad = cordoba.IDLocalidad, Direccion = "Calle Ituzaingo 450", Mail = "ana@test.com" },
                new Cliente { Nombre = "Roberto", Apellido = "Lopez", DNI = "34567890", IDBarrio = barrioGral.IDBarrio, IDLocalidad = cordoba.IDLocalidad, Direccion = "Avenida Colon 600", Mail = "roberto@test.com" },
                new Cliente { Nombre = "Gabriela", Apellido = "Sanchez", DNI = "35678901", IDBarrio = barrioGral.IDBarrio, IDLocalidad = cordoba.IDLocalidad, Direccion = "Ruta Nacional 9 Km 10", Mail = "gabriela@test.com" },
                new Cliente { Nombre = "Fernando", Apellido = "Diaz", DNI = "36789012", IDBarrio = barrioGral.IDBarrio, IDLocalidad = cordoba.IDLocalidad, Direccion = "Calle Hipólito Irigoyen 800", Mail = "fernando@test.com" },
                new Cliente { Nombre = "Alejandra", Apellido = "Torres", DNI = "37890123", IDBarrio = barrioGral.IDBarrio, IDLocalidad = cordoba.IDLocalidad, Direccion = "Paseo Sobremonte 350", Mail = "alejandra@test.com" },
                new Cliente { Nombre = "Jorge", Apellido = "Castro", DNI = "38901234", IDBarrio = barrioGral.IDBarrio, IDLocalidad = cordoba.IDLocalidad, Direccion = "Calle General Paz 950", Mail = "jorge@test.com" },
                new Cliente { Nombre = "Patricia", Apellido = "Flores", DNI = "39012345", IDBarrio = barrioGral.IDBarrio, IDLocalidad = cordoba.IDLocalidad, Direccion = "Avenida Maipú 1100", Mail = "patricia@test.com" },
            };

            context.Clientes.AddRange(clientes);
            context.SaveChanges();

            Console.WriteLine("✅ Semillado inicial completado (sin reset de base).");
        }

        // --- MÉTODOS AUXILIARES ---
        private static List<Producto> CargarProductosDesdeCSV(string rutaCSVInicial)
        {
            var productos = new List<Producto>();
            string rutaCSV = null;

            var rutasPosibles = new[]
            {
                rutaCSVInicial,
                Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Data", "SeedData", "catalogo_perfumeria - Perfumeria.csv"),
                Path.Combine(Directory.GetCurrentDirectory(), "Data", "SeedData", "catalogo_perfumeria - Perfumeria.csv"),
                Path.Combine(Directory.GetCurrentDirectory(), "..", "Back", "Data", "SeedData", "catalogo_perfumeria - Perfumeria.csv"),
                "Data/SeedData/catalogo_perfumeria - Perfumeria.csv",
                "./Data/SeedData/catalogo_perfumeria - Perfumeria.csv",
            };

            Console.WriteLine($"🔍 Buscando CSV. BaseDirectory: {AppDomain.CurrentDomain.BaseDirectory}");
            Console.WriteLine($"🔍 CurrentDirectory: {Directory.GetCurrentDirectory()}");

            foreach (var ruta in rutasPosibles)
            {
                if (File.Exists(ruta))
                {
                    rutaCSV = ruta;
                    Console.WriteLine($"✅ CSV encontrado en: {ruta}");
                    break;
                }
            }

            if (rutaCSV == null)
            {
                Console.WriteLine($"❌ CSV no encontrado en ninguna de las rutas buscadas");
                return productos;
            }

            try
            {
                using (var reader = new StreamReader(rutaCSV, System.Text.Encoding.UTF8))
                using (var csv = new CsvReader(reader, CultureInfo.InvariantCulture))
                {
                    csv.Context.RegisterClassMap<ProductoCSVMap>();
                    var records = csv.GetRecords<ProductoCSV>().ToList();
                    Console.WriteLine($"📄 Archivo leído: {records.Count} registros");

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

                    Console.WriteLine($"✅ Se cargaron {productos.Count} productos desde CSV");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Error al cargar CSV: {ex.Message}");
                Console.WriteLine($"❌ Stack: {ex.StackTrace}");
            }

            return productos;
        }

        // Clase auxiliar para mapear el CSV
        private class ProductoCSV
        {
            public string IDProducto { get; set; }
            public string NombreProducto { get; set; }
            public string Descripcion { get; set; }
            public string Categoria { get; set; }
            public string PrecioProducto { get; set; }
        }

        // Map para CsvHelper
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
