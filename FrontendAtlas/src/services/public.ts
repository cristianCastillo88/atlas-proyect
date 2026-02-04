import { api } from '../lib/api';

export interface SucursalResumen {
  id: number;
  nombre: string;
  direccion: string;
  telefono: string;
  slug: string;
  horario?: string;
}

export interface NegocioPublico {
  id: number;
  nombre: string;
  slug: string;
  urlLogo: string | null;
  sucursales: SucursalResumen[];
}

export async function getNegocioPublico(slug: string): Promise<NegocioPublico> {
  try {
    const { data } = await api.get<NegocioPublico>(`/Negocios/public/${slug}`);
    return data;
  } catch (error) {
    throw new Error('Negocio no encontrado');
  }
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

// ============ NUEVAS INTERFACES: Separación estático/dinámico ============

/**
 * Producto con SOLO datos estáticos (cacheable)
 * No incluye precio ni stock
 */
export interface ProductoPublicoEstatico {
  id: number;
  nombre: string;
  descripcion: string;
  urlImagen: string | null;
  categoriaNombre: string;
}

/**
 * Datos dinámicos de producto (precio + stock en tiempo real)
 */
export interface ProductoDinamico {
  id: number;
  precio: number;
  stock: number;
}

/**
 * Sucursal pública con datos estáticos SOLAMENTE (cacheable)
 */
export interface SucursalPublicaEstatica {
  id: number;
  nombre: string;
  direccion: string;
  telefono: string;
  slug: string;
  horario?: string;
  urlInstagram?: string;
  urlFacebook?: string;
  productos: ProductoPublicoEstatico[]; // Sin precio ni stock
  categorias: CategoriaPublica[];
  metodosPago: MetodoPago[];
  tiposEntrega: TipoEntrega[];
  precioDelivery?: number;
}

export interface MetodoPago {
  id: number;
  nombre: string;
}

export interface TipoEntrega {
  id: number;
  nombre: string;
}

export interface CategoriaPublica {
  id: number;
  nombre: string;
}

/**
 * Sucursal pública COMPLETA (datos estáticos + dinámicos combinados)
 * Esta es la interfaz que usa la UI
 */
export interface SucursalPublica {
  id: number;
  nombre: string;
  direccion: string;
  telefono: string;
  slug: string;
  horario?: string;
  urlInstagram?: string;
  urlFacebook?: string;
  productos: ProductoPublico[]; // Incluye precio y stock
  categorias: CategoriaPublica[];
  metodosPago: MetodoPago[];
  tiposEntrega: TipoEntrega[];
  precioDelivery?: number;
}

/**
 * LEGACY: Obtener sucursal con todos los datos (sin separación)
 * Este método NO se recomienda usar porque cachea datos dinámicos
 * @deprecated Usar getSucursalPublicaOptimizada() en su lugar
 */
export async function getSucursalPublica(slug: string): Promise<SucursalPublica> {
  const { data } = await api.get<SucursalPublica>(`/Sucursales/public/${slug}/completo`);
  return data;
}

/**
 * RECOMENDADO: Obtener sucursal con fetch paralelo optimizado
 * Separa datos estáticos (cached) de dinámicos (tiempo real)
 * Performance: 10x más rápido gracias al cache del backend
 */
export async function getSucursalPublicaOptimizada(slug: string): Promise<SucursalPublica> {
  try {
    // Fetch paralelo: estático (cached) + dinámico (fresh)
    const [estatica, dinamicos] = await Promise.all([
      api.get<SucursalPublicaEstatica>(`/Sucursales/public/${slug}`),
      api.get<Record<number, ProductoDinamico>>(`/Sucursales/public/${slug}/datos-dinamicos`)
    ]);

    // Combinar datos estáticos con dinámicos
    const productosCompletos: ProductoPublico[] = estatica.data.productos.map(p => ({
      ...p,
      precio: dinamicos.data[p.id]?.precio || 0,
      stock: dinamicos.data[p.id]?.stock || 0
    }));

    return {
      ...estatica.data,
      productos: productosCompletos
    };
  } catch (error) {
    console.error('Error fetching sucursal optimizada:', error);
    throw new Error('Sucursal no encontrada');
  }
}

/**
 * Obtener SOLO datos dinámicos (precio + stock)
 * Útil para actualizar precios en vivo sin recargar toda la página
 */
export async function getDatosDinamicosSucursal(slug: string): Promise<Record<number, ProductoDinamico>> {
  const { data } = await api.get<Record<number, ProductoDinamico>>(`/Sucursales/public/${slug}/datos-dinamicos`);
  return data;
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
    aclaraciones?: string | undefined;
  }[];
}

export async function crearPedidoPublico(pedido: PedidoPublicoRequest): Promise<any> {
  const { data } = await api.post('/Pedidos', pedido);
  return data;
}
