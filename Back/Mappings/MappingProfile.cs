using AutoMapper;
using Back.DTOs;
using Back.Models;
using Back.DTOS;
using Proyecto_farmacia.DTOs;

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
            CreateMap<CreateOrderDTO, Pedido>()
                .ForMember(dest => dest.ZonaId, opt => opt.MapFrom(src => src.ZonaId));
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
                .ForMember(dest => dest.NombreSucursal, opt => opt.MapFrom(src => src.Sucursal != null ? src.Sucursal.NombreSucursal : "Sin Sucursal"))
                .ForMember(dest => dest.ZonaId, opt => opt.MapFrom(src => src.ZonaId));

            // Mapeo de creación de usuarios (RegisterDTO -> Usuario)
            CreateMap<Back.DTOS.RegisterDTO, Usuario>()
                .ForMember(dest => dest.Nombre, opt => opt.MapFrom(src => src.Nombre))
                .ForMember(dest => dest.Apellido, opt => opt.MapFrom(src => src.Apellido))
                .ForMember(dest => dest.UsuarioNombre, opt => opt.MapFrom(src => src.UsuarioNombre))
                .ForMember(dest => dest.Mail, opt => opt.MapFrom(src => src.Mail))
                .ForMember(dest => dest.Contraseña, opt => opt.MapFrom(src => src.Contraseña))
                .ForMember(dest => dest.Rol, opt => opt.MapFrom(src => src.Rol));
        }
    }
}
