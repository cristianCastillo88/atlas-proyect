const API_BASE = 'http://localhost:5044/api';

export interface SucursalResumen {
  id: number;
  nombre: string;
  direccion: string;
  telefono: string;
  slug: string;
}

export interface NegocioPublico {
  id: number;
  nombre: string;
  slug: string;
  urlLogo: string | null;
  sucursales: SucursalResumen[];
}

export async function getNegocioPublico(slug: string): Promise<NegocioPublico> {
  const response = await fetch(`${API_BASE}/Negocios/public/${slug}`);
  
  if (!response.ok) {
    throw new Error('Negocio no encontrado');
  }
  
  return response.json();
}

export interface ProductoPublico {
  id: number;
  nombre: string;
  descripcion: string;
  precio: number;
  urlImagen: string | null;
  stock: number;
  categoriaNombre: string;
}

export interface MetodoPago {
  id: number;
  nombre: string;
}

export interface TipoEntrega {
  id: number;
  nombre: string;
}

export interface SucursalPublica {
  id: number;
  nombre: string;
  direccion: string;
  telefono: string;
  slug: string;
  horario?: string | null;
  productos: ProductoPublico[];
  metodosPago: MetodoPago[];
  tiposEntrega: TipoEntrega[];
}

export async function getSucursalPublica(slug: string): Promise<SucursalPublica> {
  const response = await fetch(`${API_BASE}/Sucursales/public/${slug}`);
  
  if (!response.ok) {
    throw new Error('Sucursal no encontrada');
  }
  
  return response.json();
}

export interface PedidoPublicoRequest {
  sucursalId: number;
  nombreCliente: string;
  telefonoCliente: string;
  direccionCliente: string | null;
  tipoEntregaId: number;
  metodoPagoId: number;
  observaciones: string | null;
  items: {
    productoId: number;
    cantidad: number;
  }[];
}

export async function crearPedidoPublico(pedido: PedidoPublicoRequest): Promise<any> {
  const response = await fetch(`${API_BASE}/Pedidos`, {
    method: 'POST',
    headers: {
      'Content-Type': 'application/json',
    },
    body: JSON.stringify(pedido),
  });
  
  if (!response.ok) {
    const error = await response.text();
    throw new Error(error || 'Error al crear el pedido');
  }
  
  return response.json();
}
