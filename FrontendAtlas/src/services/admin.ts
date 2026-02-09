import { api } from '../lib/api';


export interface TenantDto {
  id: number;
  nombre: string;
  slug: string;
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

export interface UpdateNegocioPayload {
  nombre: string;
  slug: string;
}

export interface SucursalDto {
  id: number;
  negocioId: number;
  nombre: string;
  slug: string;
  direccion: string;
  activo: boolean;
  telefono: string;
}

export async function obtenerTodosLosNegocios(): Promise<TenantDto[]> {
  const { data } = await api.get<TenantDto[]>('/Admin/tenants');
  return data;
}

export async function crearNegocio(payload: CrearNegocioPayload) {
  const { data } = await api.post('/Admin/tenants/registrar-negocio', {
    nombreNegocio: payload.nombreNegocio,
    direccionCentral: payload.direccionCentral,
    telefono: payload.telefono,
    datosDueno: {
      nombre: payload.nombreDueno,
      email: payload.email,
      password: payload.password,
    },
  });
  return data;
}

export async function crearSucursal(payload: CrearSucursalPayload) {
  const { data } = await api.post('/Sucursales', {
    negocioId: payload.negocioId,
    nombre: payload.nombre,
    direccion: payload.direccion,
    telefono: payload.telefono,
  });
  return data;
}

export async function alternarEstadoNegocio(id: number) {
  const { data } = await api.patch(`/Admin/tenants/${id}/toggle-status`);
  return data;
}

export async function actualizarNegocio(id: number, payload: UpdateNegocioPayload) {
  const { data } = await api.put(`/Admin/tenants/${id}`, payload);
  return data;
}

export async function obtenerSucursalesPorNegocio(negocioId: number): Promise<SucursalDto[]> {
  const { data } = await api.get<SucursalDto[]>(`/Sucursales/negocios/${negocioId}`);
  return data;
}

export async function alternarEstadoSucursal(id: number) {
  const { data } = await api.patch(`/Sucursales/${id}/toggle-status`);
  return data;
}

// ============ QR CODE FUNCTIONS ============

/**
 * Obtiene la URL del endpoint de QR code para preview
 */
export function getQRCodeUrl(sucursalId: number, size: number = 20): string {
  const baseUrl = import.meta.env.PUBLIC_API_URL || 'http://localhost:5000';
  return `${baseUrl}/api/Sucursales/${sucursalId}/qr?size=${size}`;
}

/**
 * Obtiene la URL del endpoint de descarga de QR
 */
export function getQRCodeDownloadUrl(sucursalId: number, size: number = 20): string {
  const baseUrl = import.meta.env.PUBLIC_API_URL || 'http://localhost:5000';
  return `${baseUrl}/api/Sucursales/${sucursalId}/qr/download?size=${size}`;
}

/**
 * Obtiene la URL del endpoint SVG de QR
 */
export function getQRCodeSvgUrl(sucursalId: number, size: number = 20): string {
  const baseUrl = import.meta.env.PUBLIC_API_URL || 'http://localhost:5000';
  return `${baseUrl}/api/Sucursales/${sucursalId}/qr/svg?size=${size}`;
}

export async function getQRCodeBlobUrl(sucursalId: number, size: number = 20): Promise<string> {
  const { data } = await api.get(`/Sucursales/${sucursalId}/qr`, {
    params: { size, t: Date.now() },
    responseType: 'blob'
  });
  return URL.createObjectURL(data);
}

/**
 * Descarga el QR code como archivo PNG
 * Abre en nueva ventana con autenticación
 */
export async function downloadQRCode(sucursalId: number, size: number = 20): Promise<void> {
  // Usar instancia api para manejar base URL y Auth headers automáticamente
  const response = await api.get(`/Sucursales/${sucursalId}/qr/download`, {
    params: { size, t: Date.now() },
    responseType: 'blob'
  });

  // Crear blob y descargar
  const blob = response.data;
  const downloadUrl = window.URL.createObjectURL(blob);
  const a = document.createElement('a');
  a.href = downloadUrl;

  // Intentar obtener nombre de archivo del header content-disposition si existe
  // O generar uno fallback
  const contentDisposition = response.headers['content-disposition'];
  let fileName = `QR_Sucursal_${sucursalId}.png`;

  if (contentDisposition) {
    const fileNameMatch = contentDisposition.match(/filename="?([^"]+)"?/);
    if (fileNameMatch && fileNameMatch.length === 2) {
      fileName = fileNameMatch[1];
    }
  }

  a.download = fileName;
  document.body.appendChild(a);
  a.click();
  document.body.removeChild(a);
  window.URL.revokeObjectURL(downloadUrl);
}
