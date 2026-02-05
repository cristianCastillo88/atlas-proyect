import React, { useState, useEffect } from 'react';
import { sucursalService } from '../../../services/sucursal';
import { logout } from '../../../stores/auth';
// import type { Pedido } from '../../../types/sucursal';
import KitchenDisplay from './KitchenDisplay';
import PosTerminal from './PosTerminal';
// import { signalRService } from '../../../services/signalr';
import { Toaster } from 'sonner';
import { useSucursalSignalR } from '../../../hooks/useSucursalSignalR';

interface Props {
    sucursalId: number;
}

export default function SucursalEmployeeView({ sucursalId }: Props) {
    const [activeTab, setActiveTab] = useState('pedidos'); // Default para empleados es ver pedidos
    const [loading, setLoading] = useState(true);
    const [error, setError] = useState('');

    // Data mínima necesaria
    const [sucursal, setSucursal] = useState<any>(null);
    const [pedidos, setPedidos] = useState<any[]>([]);
    const [productos, setProductos] = useState<any[]>([]);
    const [categorias, setCategorias] = useState<any[]>([]);
    const [metodosPago, setMetodosPago] = useState<any[]>([]);
    const [tiposEntrega, setTiposEntrega] = useState<any[]>([]);

    useEffect(() => {
        loadEmployeeData();
    }, [sucursalId]);

    // Hook personalizado de SignalR
    useSucursalSignalR(sucursalId, (pedido) => {
        setPedidos((prev) => [pedido, ...prev]);
    });

    const loadEmployeeData = async () => {
        try {
            setLoading(true);

            // Carga paralela de datos operativos
            const [sucData, prodData, catData, pagoData, entregaData] = await Promise.all([
                sucursalService.getSucursal(sucursalId),
                sucursalService.getProductos(sucursalId),
                sucursalService.getCategoriasBySucursal(sucursalId),
                sucursalService.getMetodosPago(),
                sucursalService.getTiposEntrega()
            ]);

            setSucursal(sucData);
            setProductos(prodData);
            setCategorias(catData);
            setMetodosPago(pagoData);
            setTiposEntrega(entregaData);

            // Intentar cargar pedidos (puede fallar si backend issue persiste, pero manejamos error local)
            try {
                const pedData = await sucursalService.getPedidosPorSucursal(sucursalId);
                setPedidos(pedData);
            } catch (e) {
                console.error("Error cargando pedidos:", e);
                // No bloqueamos la UI completa, pero mostramos alerta visual en el componente de pedidos
            }

        } catch (err: any) {
            console.error('Error loading employee data', err);
            setError('Error de conexión. Intente recargar.');
        } finally {
            setLoading(false);
        }
    };

    if (loading) {
        return (
            <div className="flex items-center justify-center h-screen bg-gray-50">
                <div className="animate-spin rounded-full h-12 w-12 border-b-2 border-primary"></div>
            </div>
        );
    }

    if (error) {
        return (
            <div className="flex flex-col items-center justify-center h-screen gap-4">
                <h2 className="text-xl font-bold text-red-600">Error</h2>
                <p>{error}</p>
                <button onClick={() => window.location.reload()} className="px-4 py-2 bg-primary text-white rounded">
                    Reintentar
                </button>
            </div>
        );
    }

    return (
        <div className="flex h-screen bg-gray-50 overflow-hidden">
            {/* Sidebar Simplificado para Empleado */}
            <aside className="w-20 lg:w-64 bg-slate-900 text-white flex flex-col shadow-xl z-20 transition-all duration-300">
                <div className="p-4 flex items-center justify-center lg:justify-start gap-3 border-b border-white/10 h-16">
                    <div className="w-8 h-8 bg-green-500 rounded flex items-center justify-center font-bold text-lg shrink-0">
                        {sucursal?.nombre?.substring(0, 1) || 'S'}
                    </div>
                    <span className="font-bold text-lg truncate hidden lg:block">
                        {sucursal?.nombre || 'Panel'}
                    </span>
                </div>

                <nav className="flex-1 py-6 px-2 space-y-2 overflow-y-auto">
                    <NavButton
                        active={activeTab === 'pedidos'}
                        onClick={() => setActiveTab('pedidos')}
                        icon={<svg className="w-6 h-6" fill="none" viewBox="0 0 24 24" stroke="currentColor"><path strokeLinecap="round" strokeLinejoin="round" strokeWidth="2" d="M9 5H7a2 2 0 00-2 2v12a2 2 0 002 2h10a2 2 0 002-2V7a2 2 0 00-2-2h-2M9 5a2 2 0 002 2h2a2 2 0 002-2M9 5a2 2 0 012-2h2a2 2 0 012 2m-3 7h3m-3 4h3m-6-4h.01M9 16h.01"></path></svg>}
                        label="Pedidos"
                    />
                    <NavButton
                        active={activeTab === 'nuevo'}
                        onClick={() => setActiveTab('nuevo')}
                        icon={<svg className="w-6 h-6" fill="none" viewBox="0 0 24 24" stroke="currentColor"><path strokeLinecap="round" strokeLinejoin="round" strokeWidth="2" d="M12 4v16m8-8H4"></path></svg>}
                        label="Nuevo Pedido"
                    />
                </nav>

                <div className="p-4 border-t border-white/10">
                    <button
                        onClick={() => {
                            logout();
                            window.location.href = '/login';
                        }}
                        className="w-full flex items-center justify-center lg:justify-start gap-3 px-2 py-2 rounded-lg text-red-300 hover:bg-red-500/10 transition-colors"
                        title="Cerrar Sesión"
                    >
                        <svg className="w-6 h-6" fill="none" viewBox="0 0 24 24" stroke="currentColor">
                            <path strokeLinecap="round" strokeLinejoin="round" strokeWidth="2" d="M17 16l4-4m0 0l-4-4m4 4H7m6 4v1a3 3 0 01-3 3H6a3 3 0 01-3-3V7a3 3 0 013-3h4a3 3 0 013 3v1"></path>
                        </svg>
                        <span className="hidden lg:inline font-medium">Salir</span>
                    </button>
                </div>
            </aside>

            {/* Content Area */}
            <main className="flex-1 flex flex-col h-full overflow-hidden w-full">
                {/* Mobile/Tablet Header */}
                <header className="bg-white border-b border-gray-200 h-16 flex items-center justify-between px-6 shrink-0 z-10">
                    <h2 className="text-xl font-bold text-gray-800">
                        {activeTab === 'pedidos' ? 'Pedidos en Cocina' : 'Nuevo Pedido'}
                    </h2>
                    <div className="flex items-center gap-3">
                        <span className={`px-3 py-1 rounded-full text-xs font-bold uppercase tracking-wider ${sucursal?.activo ? "bg-green-100 text-green-700" : "bg-red-100 text-red-700"}`}>
                            {sucursal?.activo ? "Operativo" : "Cerrado"}
                        </span>
                    </div>
                </header>

                <div className="flex-1 overflow-hidden relative bg-gray-100 p-0 lg:p-4">
                    {activeTab === 'pedidos' && (
                        <div className="h-full rounded-xl overflow-hidden bg-white shadow-sm border border-gray-200">
                            <KitchenDisplay
                                sucursalId={sucursalId}
                                initialPedidos={pedidos}
                                sucursalConfig={sucursal}
                            />
                        </div>
                    )}

                    {activeTab === 'nuevo' && (
                        <div className="h-full rounded-xl overflow-hidden bg-white shadow-sm border border-gray-200">
                            <PosTerminal
                                sucursalId={sucursalId}
                                initialProductos={productos}
                                categorias={categorias}
                                metodosPago={metodosPago}
                                tiposEntrega={tiposEntrega}
                                precioDelivery={sucursal?.precioDelivery || 0}
                            />
                        </div>
                    )}
                </div>
            </main>
            <Toaster />
        </div>
    );
}

function NavButton({ active, onClick, icon, label }: any) {
    return (
        <button
            onClick={onClick}
            className={`w-full flex items-center justify-center lg:justify-start gap-3 px-3 py-3 rounded-lg transition-all group relative ${active
                ? 'bg-blue-600 text-white shadow-lg shadow-blue-500/30'
                : 'text-gray-400 hover:bg-white/5 hover:text-white'
                }`}
            title={label}
        >
            <div className={`transition-transform duration-200 ${active ? 'scale-110' : 'group-hover:scale-110'}`}>
                {icon}
            </div>
            <span className="hidden lg:inline font-medium">{label}</span>

            {/* Tooltip for collapsed state */}
            {!active && (
                <div className="absolute left-14 bg-gray-900 text-white text-xs px-2 py-1 rounded opacity-0 group-hover:opacity-100 transition-opacity lg:hidden z-50 whitespace-nowrap pointer-events-none">
                    {label}
                </div>
            )}
        </button>
    );
}
