import axios from 'axios';
import { $authToken, logout } from '../stores/auth';

// Crear instancia base de Axios con config por defecto
export const api = axios.create({
  baseURL: import.meta.env.PUBLIC_API_URL || 'http://localhost:5044/api',
  headers: {
    'Content-Type': 'application/json',
  },
});

// Interceptor de Request: Inyectar token automáticamente
api.interceptors.request.use(
  (config) => {
    // Obtener token del store de NanoStores
    const token = $authToken.get();

    if (token) {
      config.headers.Authorization = `Bearer ${token}`;
    }

    return config;
  },
  (error) => {
    return Promise.reject(error);
  }
);

// Interceptor de Response: Manejo global de errores (ej: 401 Unauthorized)
api.interceptors.response.use(
  (response) => response,
  (error) => {
    if (error.response?.status === 401) {
      // Token expirado o inválido -> Logout
      console.warn('Sesión expirada o no autorizada. Redirigiendo a login...');
      logout();
      // Opcional: Redirigir usando window.location si estamos en el cliente
      if (typeof window !== 'undefined' && !window.location.pathname.includes('/login')) {
        window.location.href = '/login';
      }
    }
    return Promise.reject(error);
  }
);
