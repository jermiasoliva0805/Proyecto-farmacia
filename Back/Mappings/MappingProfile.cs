using AutoMapper;
using Back.DTOs;
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
            //       SECCIÓN DE USUARIOS (LECTURA)
            // ==========================================================
            CreateMap<Usuario, UserDTO>()
                .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.IDUsuario))
                .ForMember(dest => dest.Usuario, opt => opt.MapFrom(src => src.UsuarioNombre))
                .ForMember(dest => dest.Email, opt => opt.MapFrom(src => src.Mail))
                .ForMember(dest => dest.NombreCompleto, opt => opt.MapFrom(src => $"{src.Nombre} {src.Apellido}"))
                .ForMember(dest => dest.Rol, opt => opt.MapFrom(src => src.Rol))
                .ForMember(dest => dest.NombreSucursal, opt => opt.MapFrom(src => src.Sucursal != null ? src.Sucursal.NombreSucursal : "Sin Sucursal"));

            // ==========================================================
            //       SECCIÓN DE PEDIDOS Y TRACKING
            // ==========================================================
            CreateMap<ChangeOrderStatusDTO, HistorialDeEstados>()
                .ForMember(dest => dest.IDPedido, opt => opt.MapFrom(src => src.IDPedido))
                .ForMember(dest => dest.IDEstadoDePedido, opt => opt.MapFrom(src => src.IDNuevoEstado))
                .ForMember(dest => dest.IDUsuario, opt => opt.MapFrom(src => src.IDUsuario))
                .ForMember(dest => dest.fecha_hora_inicio, opt => opt.MapFrom(src => DateTime.Now));

            // --- Tracking principal ---
            CreateMap<Pedido, OrderTrackingDTO>()
                .ForMember(dest => dest.IDPedido, opt => opt.MapFrom(src => src.IDPedido))
                .ForMember(dest => dest.EstadoActual, opt => opt.MapFrom(src => src.EstadoDePedido.NombreEstado))
                .ForMember(dest => dest.UltimaActualizacion, opt => opt.MapFrom(src => DateTime.Now))
                .ForMember(dest => dest.Historial, opt => opt.MapFrom(src => src.HistorialDeEstados));

            // --- Items del historial ---
            CreateMap<HistorialDeEstados, TrackingHistoryItemDTO>()
                .ForMember(dest => dest.NombreEstado, opt => opt.MapFrom(src => src.EstadoDePedido.NombreEstado))
                .ForMember(dest => dest.FechaHora, opt => opt.MapFrom(src => src.fecha_hora_inicio))
                .ForMember(dest => dest.Responsable, opt => opt.MapFrom(src => src.Usuario != null ? $"{src.Usuario.Nombre} {src.Usuario.Apellido}" : "Sistema"))
                .ForMember(dest => dest.Observaciones, opt => opt.MapFrom(src => src.Observaciones))
                .ForMember(dest => dest.MotivoCancelacion, opt => opt.MapFrom(src => src.EstadoDePedido.motivo_cancelacion));

            // --- Resumen ---
            CreateMap<Pedido, OrderSummaryDTO>()
                .ForMember(dest => dest.IDEstadoDePedido, opt => opt.MapFrom(src => src.IDEstadoDePedido))
                .ForMember(dest => dest.EstadoNombre, opt => opt.MapFrom(src => src.EstadoDePedido.NombreEstado));
        }
    }
}
