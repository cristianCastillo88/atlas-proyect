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
  return (await res.json()) as T;
}

export interface TenantDto {
  id: number;
  nombre: string;
  dueñoEmail: string;
  cantidadSucursales: number;
  activo: boolean;
  fechaRegistro: string;
}

export interface CrearNegocioPayload {
  nombreNegocio: string;
  direccionCentral: string;
  telefono: string;
  nombreDueno: string;
  email: string;
  password: string;
}

export interface CrearSucursalPayload {
  negocioId: number;
  nombre: string;
  direccion: string;
  telefono: string;
}

export interface SucursalDto {
  id: number;
  negocioId: number;
  nombre: string;
  slug: string;
  direccion: string;
  activo: boolean;
}

export async function obtenerTodosLosNegocios(): Promise<TenantDto[]> {
  return request<TenantDto[]>('/api/admin/tenants');
}

export async function crearNegocio(payload: CrearNegocioPayload) {
  return request('/api/admin/tenants/registrar-negocio', {
    method: 'POST',
    body: JSON.stringify({
      nombreNegocio: payload.nombreNegocio,
      direccionCentral: payload.direccionCentral,
      telefono: payload.telefono,
      datosDueno: {
        nombre: payload.nombreDueno,
        email: payload.email,
        password: payload.password,
      },
    }),
  });
}

export async function crearSucursal(payload: CrearSucursalPayload) {
  return request('/api/sucursales', {
    method: 'POST',
    body: JSON.stringify({
      negocioId: payload.negocioId,
      nombre: payload.nombre,
      direccion: payload.direccion,
      telefono: payload.telefono,
    }),
  });
}

export async function alternarEstadoNegocio(id: number) {
  return request(`/api/admin/tenants/${id}/toggle-status`, {
    method: 'PATCH',
  });
}

export async function obtenerSucursalesPorNegocio(negocioId: number): Promise<SucursalDto[]> {
  return request<SucursalDto[]>(`/api/sucursales/negocios/${negocioId}`);
}

export async function alternarEstadoSucursal(id: number) {
  return request(`/api/sucursales/${id}/toggle-status`, {
    method: 'PATCH',
  });
}
