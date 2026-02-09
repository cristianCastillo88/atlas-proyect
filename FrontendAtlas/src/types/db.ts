export interface Sucursal {
    id: number;
    nombre: string;
    direccion: string;
    telefono: string;
    slug: string;
    horario?: string; // Present in [id].astro usage
    urlInstagram?: string;
    urlFacebook?: string;
    precioDelivery?: number;
    esCentral: boolean;
    negocioId: number;
    activo?: boolean; // Present in [id].astro usage
}

export interface Categoria {
    id: number;
    nombre: string;
    orden?: number;
    sucursalId?: number;
}

export interface Producto {
    id: number;
    nombre: string;
    descripcion: string;
    precio: number;
    urlImagen?: string | null;
    activo: boolean;
    stock: number;
    categoriaNombre?: string;
    categoriaId?: number;
    sucursalId?: number;
}

export interface Empleado {
    id: number;
    nombre: string;
    email: string;
    sucursalId: number;
}

export interface Pedido {
    id: number;
    fechaCreacion: string;
    clienteNombre: string;
    clienteTelefono?: string;
    direccionCliente?: string;
    total: number;
    estadoPedidoNombre: string;
    tipoEntregaNombre: string;
    metodoPagoNombre: string;
    resumenItems: string;
    items?: {
        productoId: number;
        cantidad: number;
        productoNombre: string; // From backend DTO
        precioUnitario: number;
        aclaraciones?: string;
    }[];
}

export interface MetodoPago {
    id: number;
    nombre: string;
    descripcion: string;
}

export interface TipoEntrega {
    id: number;
    nombre: string;
    precioBase: number;
}

// DTOs for Creation/Updates
export interface CrearEmpleadoDto {
    nombre: string;
    email: string;
    password: string;
    sucursalId: number;
}

export interface ProductoCreateReq {
    nombre: string;
    descripcion: string;
    precio: number;
    stock: number;
    urlImagen?: string;
    categoriaId: number;
    sucursalId: number;
}

export interface ProductoUpdateReq extends ProductoCreateReq {
    id: number;
}

export interface CrearPedidoDto {
    nombreCliente: string;
    telefonoCliente: string;
    direccionCliente: string;
    metodoPagoId: number;
    tipoEntregaId: number;
    sucursalId: number;
    observaciones?: string;
    items: { productoId: number; cantidad: number; aclaraciones?: string | undefined }[];
}
