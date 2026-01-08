import { $userStore } from '../store/auth';

const API_URL = import.meta.env.PUBLIC_API_URL;

function getToken(): string | null {
  const store = $userStore.get();
  if (store?.token) return store.token;
  const fromStorage = localStorage.getItem('userStore') || sessionStorage.getItem('userStore');
  if (fromStorage) {
    try {
      const parsed = JSON.parse(fromStorage);
      return parsed.token ?? null;
    } catch {
      return null;
    }
  }
  return null;
}

async function request<T>(path: string, options: RequestInit = {}): Promise<T> {
  const token = getToken();
  if (!token) throw new Error('No hay token disponible. Inicia sesión nuevamente.');
  const res = await fetch(`${API_URL}${path}`, {
    ...options,
    headers: {
      'Content-Type': 'application/json',
      Authorization: `Bearer ${token}`,
      ...(options.headers || {}),
    },
  });
  if (!res.ok) {
    const text = await res.text();
    throw new Error(text || 'Error en la petición');
  }
  if (res.status === 204) return {} as T;
  
  // Check if response has content and is JSON
  const contentType = res.headers.get('content-type');
  if (contentType && contentType.includes('application/json')) {
    return (await res.json()) as T;
  }
  
  // For non-JSON responses (like plain text), return empty object
  return {} as T;
}

// Tipos
export type Rol = 'SuperAdmin' | 'AdminNegocio' | 'Empleado';

export interface EmpleadoDto {
  id: number;
  nombre: string;
  email: string;
  sucursalId: number;
}

export interface CrearEmpleadoDto {
  nombre: string;
  email: string;
  password: string;
  sucursalId: number;
}

export interface ProductoDto {
  id: number;
  nombre: string;
  descripcion: string;
  precio: number;
  urlImagen?: string | null;
  activo: boolean;
  stock: number;
  categoriaNombre: string;
}

export interface CategoriaDto {
  id: number;
  nombre: string;
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

export interface PedidoDto {
  id: number;
  fechaCreacion: string;
  clienteNombre: string;
  total: number;
  estadoPedidoNombre: string;
  tipoEntregaNombre: string;
  metodoPagoNombre: string;
  resumenItems: string;
}

// Empleados
export function getEmpleadosPorSucursal(sucursalId: number) {
  return request<EmpleadoDto[]>(`/api/empleados/sucursal/${sucursalId}`);
}

export function crearEmpleado(payload: CrearEmpleadoDto) {
  return request(`/api/empleados`, {
    method: 'POST',
    body: JSON.stringify(payload),
  });
}

export function darDeBajaEmpleado(empleadoId: number) {
  return request(`/api/empleados/${empleadoId}`, {
    method: 'DELETE',
  });
}

// Pedidos
export function getPedidosPorSucursal(sucursalId: number, estado?: string) {
  const qs = estado ? `?estado=${encodeURIComponent(estado)}` : '';
  return request<PedidoDto[]>(`/api/pedidos/sucursal/${sucursalId}${qs}`);
}

export function cambiarEstadoPedido(pedidoId: number, nuevoEstadoId: number) {
  return request(`/api/pedidos/${pedidoId}/estado`, {
    method: 'PATCH',
    body: JSON.stringify({ nuevoEstadoId }),
  });
}

// Productos (usa endpoints existentes)
export function getProductos() {
  return request<ProductoDto[]>(`/api/productos`);
}

export function getProductosPorSucursal(sucursalId: number) {
  return request<ProductoDto[]>(`/api/productos/sucursal/${sucursalId}`);
}

export function crearProducto(payload: ProductoCreateReq) {
  return request<ProductoDto>(`/api/productos`, {
    method: 'POST',
    body: JSON.stringify(payload),
  });
}

export function actualizarProducto(payload: ProductoUpdateReq) {
  return request<ProductoDto>(`/api/productos/${payload.id}`, {
    method: 'PUT',
    body: JSON.stringify(payload),
  });
}

export function borrarProducto(id: number) {
  return request(`/api/productos/${id}`, { method: 'DELETE' });
}

export interface SucursalDto {
  id: number;
  negocioId: number;
  nombre: string;
  slug: string;
  direccion: string;
  telefono: string;
  horario?: string | null;
  activo: boolean;
}

export function getSucursal(id: number) {
  return request<SucursalDto>(`/api/sucursales/${id}`);
}

export function actualizarSucursal(id: number, data: { direccion: string; telefono: string; horario?: string | null }) {
  return request(`/api/sucursales/${id}`, {
    method: 'PUT',
    body: JSON.stringify(data),
  });
}

// Categorias
export function getCategoriasBySucursal(sucursalId: number) {
  return request<CategoriaDto[]>(`/api/categorias/sucursal/${sucursalId}`);
}

export function crearCategoria(nombre: string, sucursalId: number) {
  return request<CategoriaDto>(`/api/categorias`, {
    method: 'POST',
    body: JSON.stringify({ nombre, sucursalId, id: 0 }),
  });
}

// Maestros
export interface MetodoPagoDto {
  id: number;
  nombre: string;
  descripcion: string;
}

export interface TipoEntregaDto {
  id: number;
  nombre: string;
  precioBase: number;
}

export function getMetodosPago() {
  return request<MetodoPagoDto[]>(`/api/maestros/pagos`);
}

export function getTiposEntrega() {
  return request<TipoEntregaDto[]>(`/api/maestros/entregas`);
}

// Pedidos
export interface CrearPedidoDto {
  nombreCliente: string;
  telefonoCliente: string;
  direccionCliente: string;
  metodoPagoId: number;
  tipoEntregaId: number;
  sucursalId: number;
  observaciones?: string;
  items: { productoId: number; cantidad: number }[];
}

export function crearPedido(payload: CrearPedidoDto) {
  return request(`/api/pedidos`, {
    method: 'POST',
    body: JSON.stringify(payload),
  });
}
