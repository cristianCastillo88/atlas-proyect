using AutoMapper;
using BackendAtlas.Domain;
using BackendAtlas.DTOs;

namespace BackendAtlas.Mapping
{
    public class MappingProfile : Profile
    {
        public MappingProfile()
        {
            CreateMap<Categoria, CategoriaDto>()
                .ForMember(dest => dest.SucursalId, opt => opt.MapFrom(src => src.SucursalId));
            CreateMap<MetodoPago, MetodoPagoDto>();
            CreateMap<TipoEntrega, TipoEntregaDto>();

            CreateMap<Producto, ProductoResponseDto>()
                .ForMember(dest => dest.CategoriaNombre, opt => opt.MapFrom(src => src.Categoria!.Nombre));

            CreateMap<ProductoCreateRequestDto, Producto>();
            CreateMap<ProductoUpdateRequestDto, Producto>();

            CreateMap<PedidoCreateDto, Pedido>();
            CreateMap<Pedido, PedidoResponseDto>()
                .ForMember(dest => dest.EstadoPedido, opt => opt.MapFrom(src => src.EstadoPedido!.Nombre));

            CreateMap<Pedido, PedidoAdminListDto>()
                .ForMember(dest => dest.ClienteNombre, opt => opt.MapFrom(src => src.NombreCliente))
                .ForMember(dest => dest.ClienteTelefono, opt => opt.MapFrom(src => src.TelefonoCliente))
                .ForMember(dest => dest.DireccionCliente, opt => opt.MapFrom(src => src.DireccionCliente))
                .ForMember(dest => dest.EstadoPedidoNombre, opt => opt.MapFrom(src => src.EstadoPedido!.Nombre))
                .ForMember(dest => dest.TipoEntregaNombre, opt => opt.MapFrom(src => src.TipoEntrega!.Nombre))
                .ForMember(dest => dest.MetodoPagoNombre, opt => opt.MapFrom(src => src.MetodoPago!.Nombre))
                .ForMember(dest => dest.ResumenItems, opt => opt.MapFrom(src => 
                    src.DetallesPedido != null 
                        ? string.Join(", ", src.DetallesPedido.Select(d => 
                            d.Cantidad + "x " + (d.Producto != null ? d.Producto.Nombre : "Producto") + (string.IsNullOrEmpty(d.Aclaraciones) ? "" : $" ({d.Aclaraciones})")
                        ))
                        : string.Empty))
                .ForMember(dest => dest.Items, opt => opt.MapFrom(src => src.DetallesPedido));

            CreateMap<DetallePedido, DetallePedidoAdminDto>()
                .ForMember(dest => dest.ProductoNombre, opt => opt.MapFrom(src => src.Producto!.Nombre))
                .ForMember(dest => dest.PrecioUnitario, opt => opt.MapFrom(src => src.PrecioUnitario)) // Redundante si nombre coincide, pero explicito
                .ForMember(dest => dest.Aclaraciones, opt => opt.MapFrom(src => src.Aclaraciones));

            CreateMap<Usuario, EmpleadoDto>();
        }
    }
}