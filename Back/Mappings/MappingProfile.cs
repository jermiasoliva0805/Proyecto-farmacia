using AutoMapper;
using Back.DTOs; // Unificado (a veces aparece como DTOS, asegúrate que coincida con tu carpeta)
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

            // --- Mapeos de Creación de Pedidos ---
            CreateMap<CreateOrderDTO, Pedido>();
            CreateMap<OrderDetailDTO, DetalleDePedido>();

            // ==========================================================
            //       SECCIÓN DE USUARIOS (GESTIÓN COMPLETA)
            // ==========================================================

            // 1. Lectura (Entity -> DTO)
            CreateMap<Usuario, UserDTO>()
                .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.IDUsuario))
                .ForMember(dest => dest.Usuario, opt => opt.MapFrom(src => src.UsuarioNombre))
                .ForMember(dest => dest.Email, opt => opt.MapFrom(src => src.Mail))
                .ForMember(dest => dest.NombreCompleto, opt => opt.MapFrom(src => $"{src.Nombre} {src.Apellido}"))
                .ForMember(dest => dest.Rol, opt => opt.MapFrom(src => src.Rol))
                .ForMember(dest => dest.NombreSucursal, opt => opt.MapFrom(src => src.Sucursal != null ? src.Sucursal.NombreSucursal : "Sin Sucursal"));

            // 2. Registro (RegisterDTO -> Entity)
            CreateMap<RegisterDTO, Usuario>()
                .ForMember(dest => dest.UsuarioNombre, opt => opt.MapFrom(src => src.UsuarioNombre))
                .ForMember(dest => dest.Contraseña, opt => opt.Ignore()) // ¡Vital! Se hashea en el Service
                .ForMember(dest => dest.Nombre, opt => opt.MapFrom(src => src.Nombre));

            // 3. Edición (UpdateUserDTO -> Entity)
            CreateMap<UpdateUserDTO, Usuario>()
                .ForMember(dest => dest.IDUsuario, opt => opt.Ignore()) // No tocar ID
                .ForMember(dest => dest.Nombre, opt => opt.MapFrom(src => src.Nombre))
                .ForMember(dest => dest.UsuarioNombre, opt => opt.MapFrom(src => src.UsuarioNombre))
                .ForMember(dest => dest.Mail, opt => opt.MapFrom(src => src.Mail))
                .ForMember(dest => dest.Rol, opt => opt.MapFrom(src => src.Rol))
                .ForMember(dest => dest.IDSucursal, opt => opt.MapFrom(src => src.IDSucursal));


            // ==========================================================
            //       SECCIÓN DE PEDIDOS Y TRACKING
            // ==========================================================

            // Cambio de Estado
            CreateMap<ChangeOrderStatusDTO, HistorialDeEstados>()
                .ForMember(dest => dest.IDPedido, opt => opt.MapFrom(src => src.IDPedido))
                .ForMember(dest => dest.IDEstadoDePedido, opt => opt.MapFrom(src => src.IDNuevoEstado))
                .ForMember(dest => dest.IDUsuario, opt => opt.MapFrom(src => src.IDUsuario))
                .ForMember(dest => dest.fecha_hora_inicio, opt => opt.MapFrom(src => DateTime.Now));

            // Tracking
            CreateMap<Pedido, OrderTrackingDTO>()
                .ForMember(dest => dest.IDPedido, opt => opt.MapFrom(src => src.IDPedido))
                .ForMember(dest => dest.EstadoActual, opt => opt.MapFrom(src => src.EstadoDePedido.NombreEstado))
                .ForMember(dest => dest.UltimaActualizacion, opt => opt.MapFrom(src => DateTime.Now))
                .ForMember(dest => dest.Historial, opt => opt.MapFrom(src => src.HistorialDeEstados));

            // Items del Historial
            CreateMap<HistorialDeEstados, TrackingHistoryItemDTO>()
                .ForMember(dest => dest.NombreEstado, opt => opt.MapFrom(src => src.EstadoDePedido.NombreEstado))
                .ForMember(dest => dest.FechaHora, opt => opt.MapFrom(src => src.fecha_hora_inicio))
                .ForMember(dest => dest.Responsable, opt => opt.MapFrom(src => src.Usuario != null ? $"{src.Usuario.Nombre} {src.Usuario.Apellido}" : "Sistema"))
                .ForMember(dest => dest.MotivoCancelacion, opt => opt.MapFrom(src => src.EstadoDePedido.motivo_cancelacion))
                .ForMember(dest => dest.Observaciones, opt => opt.MapFrom(src => src.Observaciones));

            // Resumen
            CreateMap<Pedido, OrderSummaryDTO>()
                .ForMember(dest => dest.IDEstadoDePedido, opt => opt.MapFrom(src => src.IDEstadoDePedido))
                .ForMember(dest => dest.EstadoNombre, opt => opt.MapFrom(src => src.EstadoDePedido.NombreEstado));
        }
    }
}