import { HubConnectionBuilder, HubConnection, LogLevel } from "@microsoft/signalr";
import { toast } from "sonner"; // Usamos sonner para notificaciones elegantes

// Definir interfaz para el payload del evento
export interface NuevoPedidoEvent {
    id: number;
    nombreCliente: string;
    total: number;
    fecha: string;
    estado: string;
}

class SignalRService {
    private connection: HubConnection | null = null;
    private sucursalId: string | null = null;
    private token: string | null = null;
    private isConnected = false;
    private reconnectInterval = 5000;

    // Callbacks events
    private onNuevoPedidoCallback: ((pedido: NuevoPedidoEvent) => void) | null = null;

    constructor() {
        // Singleton initialization logic if needed
    }

    public async connect(sucursalId: number, token: string) {
        if (this.connection && this.isConnected && this.sucursalId === sucursalId.toString()) {
            console.log("SignalR ya conectado a esta sucursal.");
            return;
        }

        // Si hay una conexión previa, desconectar limpiamente
        if (this.connection) {
            await this.disconnect();
        }

        this.sucursalId = sucursalId.toString();
        this.token = token;

        const baseUrl = import.meta.env.PUBLIC_API_URL || 'https://localhost:7029/api';
        const hubUrl = baseUrl.replace('/api', '/hubs/pedidos'); // Asumiendo estructura estándar

        // Importamos HttpTransportType para mayor control
        const { HttpTransportType } = await import("@microsoft/signalr");

        this.connection = new HubConnectionBuilder()
            .withUrl(hubUrl, {
                // Enviar token como query string porque WebSockets no soporta headers estándar en browser API
                accessTokenFactory: () => this.token || '',
                // FORZAR WebSockets para evitar el error de negociación en Railway/Vercel
                skipNegotiation: true,
                transport: HttpTransportType.WebSockets
            })
            .withAutomaticReconnect()
            .configureLogging(LogLevel.Error)
            .build();

        // Listeners globales del hub
        this.connection.on("NuevoPedido", (pedido: NuevoPedidoEvent) => {
            // Audio Alert
            this.playNotificationSound();

            // Visual Alert
            toast.success(`Nuevo Pedido #${pedido.id}`, {
                description: `${pedido.nombreCliente} - $${pedido.total}`,
                duration: 5000,
                action: {
                    label: 'Ver',
                    onClick: () => window.location.reload() // O mejor, actualizar estado sin reload
                }
            });

            // Invocar callback de UI si existe
            if (this.onNuevoPedidoCallback) {
                this.onNuevoPedidoCallback(pedido);
            }
        });

        try {
            await this.connection.start();
            this.isConnected = true;

            // Unirse al grupo de la sucursal
            await this.connection.invoke("JoinSucursalGroup", this.sucursalId);

        } catch (err) {
            console.error("❌ Error conectando SignalR:", err);
            this.isConnected = false;
            // Retry logic podría ir aquí si AutomaticReconnect falla en el start inicial
            setTimeout(() => this.connect(sucursalId, token), this.reconnectInterval);
        }
    }

    public onNuevoPedido(callback: (pedido: NuevoPedidoEvent) => void) {
        this.onNuevoPedidoCallback = callback;
    }

    public async disconnect() {
        if (this.connection) {
            this.connection.off("NuevoPedido"); // Remover listeners para evitar duplicados
            await this.connection.stop();
            this.connection = null;
            this.isConnected = false;
        }
    }

    private playNotificationSound() {
        try {
            // Usar Web Audio API para generar un sonido "Ding" agradable sin dependencias externas
            const AudioContext = window.AudioContext || (window as any).webkitAudioContext;
            if (!AudioContext) return;

            const ctx = new AudioContext();

            // Crear oscilador (tono)
            const osc = ctx.createOscillator();
            // Crear control de volumen
            const gain = ctx.createGain();

            osc.connect(gain);
            gain.connect(ctx.destination);

            // Configurar tono: Empezar en 523.25Hz (Do) y subir un poco (efecto "Ding")
            osc.type = 'sine';
            osc.frequency.setValueAtTime(523.25, ctx.currentTime);
            osc.frequency.exponentialRampToValueAtTime(880, ctx.currentTime + 0.1); // Subir a La

            // Configurar volumen: Fade out suave
            gain.gain.setValueAtTime(0.5, ctx.currentTime);
            gain.gain.exponentialRampToValueAtTime(0.01, ctx.currentTime + 0.6);

            osc.start();
            osc.stop(ctx.currentTime + 0.6);

        } catch (e) {
            console.error("Error playing sound", e);
        }
    }
}

// Export singleton
export const signalRService = new SignalRService();
