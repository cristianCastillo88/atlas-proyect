import { useEffect, useCallback } from 'react';
import { signalRService } from '../services/signalr';
import { getUserFromStorage } from '../utils/authUtils';

export function useSucursalSignalR(sucursalId: number, onNuevoPedido?: (pedido: any) => void) {

    // Función estable para conectar
    const connect = useCallback(() => {
        const user = getUserFromStorage();
        const token = user?.token;

        if (token && sucursalId) {
            signalRService.connect(sucursalId, token);
        }
    }, [sucursalId]);

    // Efecto de conexión y limpieza
    useEffect(() => {
        connect();

        // Suscripción a eventos
        if (onNuevoPedido) {
            signalRService.onNuevoPedido((pedido) => {
                // console.log("🔔 Nuevo pedido recibido via SignalR:", pedido);
                onNuevoPedido(pedido);
            });
        }

        return () => {
            // console.log("🔕 Desconectando SignalR...");
            signalRService.disconnect();
        };
    }, [connect, onNuevoPedido]);

    return { reconnect: connect };
}
