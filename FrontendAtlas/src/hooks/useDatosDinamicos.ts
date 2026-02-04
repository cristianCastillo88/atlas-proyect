import { useState, useEffect, useCallback } from 'react';
import { getDatosDinamicosSucursal, type ProductoDinamico } from '../services/public';

/**
 * Custom hook para actualizar datos dinámicos (precio + stock) en tiempo real
 * 
 * Casos de uso:
 * - Actualización automática cada X segundos
 * - Refresh manual cuando el usuario vuelve a la pestaña
 * - Sincronización en tiempo real con WebSockets (futuro)
 * 
 * @param slug - Slug de la sucursal
 * @param options - Opciones de configuración
 * @returns Datos dinámicos y función de refresh manual
 * 
 * @example
 * ```tsx
 * const { datosDinamicos, isLoading, refresh } = useDatosDinamicos('pizzeria-centro', {
 *   autoRefresh: true,
 *   intervalMs: 30000 // 30 segundos
 * });
 * ```
 */
export function useDatosDinamicos(
    slug: string,
    options: {
        autoRefresh?: boolean;
        intervalMs?: number;
        onError?: (error: Error) => void;
    } = {}
) {
    const {
        autoRefresh = false,
        intervalMs = 30000, // Default: 30 segundos
        onError
    } = options;

    const [datosDinamicos, setDatosDinamicos] = useState<Record<number, ProductoDinamico>>({});
    const [isLoading, setIsLoading] = useState(false);
    const [error, setError] = useState<Error | null>(null);

    const fetchDatos = useCallback(async () => {
        if (!slug) return;

        setIsLoading(true);
        setError(null);

        try {
            const datos = await getDatosDinamicosSucursal(slug);
            setDatosDinamicos(datos);
        } catch (err) {
            const error = err instanceof Error ? err : new Error('Error desconocido');
            setError(error);
            onError?.(error);
        } finally {
            setIsLoading(false);
        }
    }, [slug, onError]);

    // Auto-refresh con intervalo
    useEffect(() => {
        if (!autoRefresh) return;

        fetchDatos(); // Fetch inicial

        const interval = setInterval(fetchDatos, intervalMs);
        return () => clearInterval(interval);
    }, [autoRefresh, intervalMs, fetchDatos]);

    // Refresh cuando la página vuelve a estar visible
    useEffect(() => {
        const handleVisibilityChange = () => {
            if (document.visibilityState === 'visible') {
                fetchDatos();
            }
        };

        document.addEventListener('visibilitychange', handleVisibilityChange);
        return () => document.removeEventListener('visibilitychange', handleVisibilityChange);
    }, [fetchDatos]);

    return {
        datosDinamicos,
        isLoading,
        error,
        refresh: fetchDatos
    };
}

/**
 * Helper para combinar productos estáticos con datos dinámicos
 * 
 * @example
 * ```tsx
 * const productosActualizados = mergeProductosDinamicos(
 *   sucursal.productos,
 *   datosDinamicos
 * );
 * ```
 */
export function mergeProductosDinamicos<T extends { id: number }>(
    productosEstaticos: T[],
    datosDinamicos: Record<number, ProductoDinamico>
): (T & { precio: number; stock: number })[] {
    return productosEstaticos.map(p => ({
        ...p,
        precio: datosDinamicos[p.id]?.precio ?? 0,
        stock: datosDinamicos[p.id]?.stock ?? 0
    }));
}
