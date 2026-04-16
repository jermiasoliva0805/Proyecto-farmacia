using AutoMapper;
using Back.DTOs;
using Back.DTOS;
using Back.Models;

namespace Back.Mappings
{
    public class MappingProfile : Profile
    {
        public MappingProfile()
        {
            // --- Mapeos de Localidades ---
            CreateMap<Localidad, LocalityDTO>()
                .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.IDLocalidad))
                .ForMember(dest => dest.Nombre, opt => opt.MapFrom(src => src.Ciudad));

            // --- Mapeos de Clientes (para combos del frontend) ---
            CreateMap<Cliente, ClientDTO>()
                .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.IDCliente))
                .ForMember(dest => dest.Nombre, opt => opt.MapFrom(src => src.Nombre))
                .ForMember(dest => dest.Apellido, opt => opt.MapFrom(src => src.Apellido))
                .ForMember(dest => dest.DNI, opt => opt.MapFrom(src => src.DNI))
                .ForMember(dest => dest.Email, opt => opt.MapFrom(src => src.Mail))
                .ForMember(dest => dest.Telefono, opt => opt.MapFrom(src => src.Telefono))
                .ForMember(dest => dest.IDLocalidad, opt => opt.MapFrom(src => src.IDLocalidad))
                .ForMember(dest => dest.NombreLocalidad, opt => opt.MapFrom(src => src.Localidad != null ? src.Localidad.Ciudad : "Sin localidad"));

            // --- Mapeos de Productos (CSV a DTO) ---
            CreateMap<Producto, ProductDTO>()
                .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.IDProducto))
                .ForMember(dest => dest.Nombre, opt => opt.MapFrom(src => src.NombreProducto))
                .ForMember(dest => dest.Descripcion, opt => opt.MapFrom(src => src.Descripcion))
                .ForMember(dest => dest.Precio, opt => opt.MapFrom(src => src.PrecioProducto))
                .ForMember(dest => dest.Stock, opt => opt.MapFrom(src => src.CantidadProducto))
                .ForMember(dest => dest.Categoria, opt => opt.MapFrom(src => src.Categoria));

            // --- Mapeos de Sucursales ---
            CreateMap<Sucursal, SucursalDTO>()
                .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.IDSucursal))
                .ForMember(dest => dest.Nombre, opt => opt.MapFrom(src => src.NombreSucursal))
                .ForMember(dest => dest.Direccion, opt => opt.MapFrom(src => src.Dirección))
                .ForMember(dest => dest.Telefono, opt => opt.MapFrom(src => src.Teléfono));

            // --- Mapeos de Creación de Pedidos ---
            CreateMap<CreateOrderDTO, Pedido>();
            CreateMap<OrderDetailDTO, DetalleDePedido>();

            // ==========================================================
            //       SECCIÓN DE USUARIOS (LECTURA)
            // ==========================================================
            CreateMap<Usuario, UserDTO>()
                .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.IDUsuario))
                .ForMember(dest => dest.Usuario, opt => opt.MapFrom(src => src.UsuarioNombre))
                .ForMember(dest => dest.Email, opt => opt.MapFrom(src => src.Mail))
                .ForMember(dest => dest.NombreCompleto, opt => opt.MapFrom(src => $"{src.Nombre} {src.Apellido}"))
                .ForMember(dest => dest.Rol, opt => opt.MapFrom(src => src.Rol))
                .ForMember(dest => dest.NombreSucursal, opt => opt.MapFrom(src => src.Sucursal != null ? src.Sucursal.NombreSucursal : "Sin Sucursal"));

            // Mapeo de creación de usuarios (RegisterDTO -> Usuario)
            CreateMap<Back.DTOS.RegisterDTO, Usuario>()
                .ForMember(dest => dest.Nombre, opt => opt.MapFrom(src => src.Nombre))
                .ForMember(dest => dest.Apellido, opt => opt.MapFrom(src => src.Apellido))
                .ForMember(dest => dest.UsuarioNombre, opt => opt.MapFrom(src => src.UsuarioNombre))
                .ForMember(dest => dest.Mail, opt => opt.MapFrom(src => src.Mail))
                .ForMember(dest => dest.Contraseña, opt => opt.MapFrom(src => src.Contraseña))
                .ForMember(dest => dest.Rol, opt => opt.MapFrom(src => src.Rol))
                .ForMember(dest => dest.IDSucursal, opt => opt.MapFrom(src => src.IDSucursal))
                .ForMember(dest => dest.ZonaId, opt => opt.MapFrom(src => src.ZonaId));

            // Mapeo de actualización de usuarios (UpdateUserDTO -> Usuario)
            CreateMap<UpdateUserDTO, Usuario>()
                .ForMember(dest => dest.Nombre, opt => opt.Condition(src => src.Nombre != null))
                .ForMember(dest => dest.Apellido, opt => opt.Condition(src => src.Apellido != null))
                .ForMember(dest => dest.UsuarioNombre, opt => opt.Condition(src => src.UsuarioNombre != null))
                .ForMember(dest => dest.Rol, opt => opt.Condition(src => src.Rol != null))
                .ForMember(dest => dest.Mail, opt => opt.Condition(src => src.Mail != null))
                .ForMember(dest => dest.IDSucursal, opt => opt.Condition(src => src.IDSucursal.HasValue))
                .ForMember(dest => dest.ZonaId, opt => opt.MapFrom(src => src.ZonaId)) // Sin condición: mapea siempre
                .ForMember(dest => dest.Contraseña, opt => opt.Condition(src => src.Contraseña != null));

            // ==========================================================
            //       SECCIÓN DE PEDIDOS Y TRACKING
            // ==========================================================
            CreateMap<ChangeOrderStatusDTO, HistorialDeEstados>()
                .ForMember(dest => dest.IDPedido, opt => opt.MapFrom(src => src.IDPedido))
                .ForMember(dest => dest.IDEstadoDePedido, opt => opt.MapFrom(src => src.IDNuevoEstado))
                .ForMember(dest => dest.IDUsuario, opt => opt.MapFrom(src => src.IDUsuario))
                .ForMember(dest => dest.fecha_hora_inicio, opt => opt.MapFrom(src => DateTime.UtcNow));

            // --- Tracking principal ---
            CreateMap<Pedido, OrderTrackingDTO>()
                .ForMember(dest => dest.IDPedido, opt => opt.MapFrom(src => src.IDPedido))
                .ForMember(dest => dest.EstadoActual, opt => opt.MapFrom(src => src.EstadoDePedido.NombreEstado))
                .ForMember(dest => dest.UltimaActualizacion, opt => opt.MapFrom(src => ConvertToArgentinaTime(src.HistorialDeEstados.Max(h => h.fecha_hora_inicio))))
                .ForMember(dest => dest.Historial, opt => opt.MapFrom(src => src.HistorialDeEstados));

            // --- Items del historial ---
            CreateMap<HistorialDeEstados, TrackingHistoryItemDTO>()
                .ForMember(dest => dest.NombreEstado, opt => opt.MapFrom(src => src.EstadoDePedido.NombreEstado))
                .ForMember(dest => dest.FechaHora, opt => opt.MapFrom(src => ConvertToArgentinaTime(src.fecha_hora_inicio)))
                .ForMember(dest => dest.Responsable, opt => opt.MapFrom(src => src.Usuario != null ? $"{src.Usuario.Nombre} {src.Usuario.Apellido}" : "Sistema"))
                .ForMember(dest => dest.Observaciones, opt => opt.MapFrom(src => src.Observaciones))
                .ForMember(dest => dest.MotivoCancelacion, opt => opt.MapFrom(src => src.EstadoDePedido.motivo_cancelacion))
                .ForMember(dest => dest.IntentosEntregaFallida, opt => opt.MapFrom(src => src.IntentosEntregaFallida))
                .ForMember(dest => dest.IntentosMax, opt => opt.MapFrom(src => src.IntentosMax));

            // --- Resumen ---
            CreateMap<Pedido, OrderSummaryDTO>()
                .ForMember(dest => dest.IDEstadoDePedido, opt => opt.MapFrom(src => src.IDEstadoDePedido))
                .ForMember(dest => dest.EstadoNombre, opt => opt.MapFrom(src => src.EstadoDePedido.NombreEstado));

            // --- Mapeos de Creación de Pedidos ---
            CreateMap<CreateOrderDTO, Pedido>()
                .ForMember(dest => dest.IDLocalidad, opt => opt.Ignore()) // Se obtiene del Cliente o del DTO
                .ForMember(dest => dest.Detalles, opt => opt.MapFrom(src => src.Detalles));
            
            CreateMap<OrderDetailDTO, DetalleDePedido>();
        }

        // Método para convertir fechas UTC a hora de Argentina (UTC-3 / UTC-2)
        private DateTime ConvertToArgentinaTime(DateTime utcTime)
        {
            try
            {
                // Asegurar que la fecha sea tratada como UTC
                if (utcTime.Kind != DateTimeKind.Utc)
                {
                    utcTime = DateTime.SpecifyKind(utcTime, DateTimeKind.Utc);
                }
                
                // Argentina Standard Time es el identificador de la zona horaria
                TimeZoneInfo argentinaTimeZone = TimeZoneInfo.FindSystemTimeZoneById("Argentina Standard Time");
                return TimeZoneInfo.ConvertTimeFromUtc(utcTime, argentinaTimeZone);
            }
            catch
            {
                // Fallback: si "Argentina Standard Time" no existe, restar 3 horas directamente
                return utcTime.AddHours(-3);
            }
        }
    }
}
