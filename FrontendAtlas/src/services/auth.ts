import { api } from '../lib/api';
import { setLoginData, type UserData } from '../stores/auth';
import type { AxiosError } from 'axios';

// --- Interfaces (Contratos Estrictos) ---

export interface LoginPayload {
    email: string;
    password: string;
}

export interface ChangePasswordPayload {
    passwordActual: string;
    passwordNueva: string;
    confirmarPasswordNueva: string;
}

export interface ForgotPasswordPayload {
    email: string;
}

export interface ResetPasswordPayload {
    token: string;
    nuevaPassword: string;
    confirmarPassword: string;
}

export interface AuthResponse {
    message: string;
    [key: string]: any;
}

// --- Service Logic ---

export async function login(credentials: LoginPayload): Promise<UserData> {
    try {
        const { data } = await api.post('/auth/login', credentials);

        // Defensive Programming: Normalize response keys
        const userData: UserData = {
            token: data.token || data.Token || '',
            role: data.role || data.Role || '',
            name: data.name || data.Name || '',
            negocioId: data.negocioId || data.NegocioId || undefined,
            sucursalId: data.sucursalId || data.SucursalId || undefined,
        };

        setLoginData(userData);
        return userData;
    } catch (error) {
        throw handleAuthError(error);
    }
}

export async function forgotPassword(email: string): Promise<string> {
    try {
        const payload: ForgotPasswordPayload = { email };
        const { data } = await api.post<AuthResponse>('/auth/solicitar-recuperacion', payload);
        return data.message;
    } catch (error) {
        // Security best practice: Even if it fails, we might want to show a generic message 
        // depending on the error type, but here we let the component decide.
        throw handleAuthError(error);
    }
}

export async function resetPassword(payload: ResetPasswordPayload): Promise<string> {
    try {
        const { data } = await api.post<AuthResponse>('/auth/restablecer-con-token', payload);
        return data.message;
    } catch (error) {
        throw handleAuthError(error);
    }
}

export async function changePassword(payload: ChangePasswordPayload): Promise<string> {
    try {
        const { data } = await api.post<AuthResponse>('/auth/cambiar-password', payload);
        return data.message;
    } catch (error) {
        throw handleAuthError(error);
    }
}

export function logout() {
    // Clear sensitive data from memory/storage immediately
    setLoginData(null as any);

    // Optional: Redirect to login or invalidate session on server if applicable
    window.location.href = '/login';
}

// --- Internal Helpers ---

function handleAuthError(error: any): Error {
    if (error && error.response && error.response.data) {
        // Extract server-side validation message
        const serverMessage = error.response.data.message || error.response.data.title || 'Error de autenticación';
        return new Error(serverMessage);
    }
    return new Error('Error de conexión o problema inesperado.');
}
