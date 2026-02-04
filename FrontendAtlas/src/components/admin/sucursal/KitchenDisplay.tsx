import React, { useState, useEffect } from 'react';
import { toast } from 'sonner';
import type { Pedido, Sucursal } from '../../../types/db';
import { sucursalService } from '../../../services/sucursal';
import { usePrintTicket } from '../../../hooks/usePrintTicket';
import { TicketTemplate } from '../../printing/TicketTemplate';

interface KitchenDisplayProps {
    sucursalId: number;
    initialPedidos: Pedido[];
    sucursalConfig?: Sucursal;
}

// Status Constants - Adjust based on backend IDs if fixed, or rely on names
// Based on typical flows. 
const STATUS_COLUMNS = [
    { id: 1, name: 'Pendiente', color: 'bg-yellow-100 text-yellow-800 border-yellow-200' },
    { id: 2, name: 'En Preparacion', color: 'bg-blue-100 text-blue-800 border-blue-200' },
    { id: 3, name: 'Listo', color: 'bg-green-100 text-green-800 border-green-200' },
];

export default function KitchenDisplay({ sucursalId, initialPedidos, sucursalConfig }: KitchenDisplayProps) {
    const [pedidos, setPedidos] = useState<Pedido[]>(initialPedidos);
    const [loading, setLoading] = useState(false);
    const [ticketSize, setTicketSize] = useState<'80mm' | '58mm'>('80mm'); // Default standard
    const { printRef, handlePrint, printingOrder, printingSucursal } = usePrintTicket();

    const refreshPedidos = async () => {
        setLoading(true);
        try {
            const data = await sucursalService.getPedidosPorSucursal(sucursalId);
            setPedidos(data);
        } catch (error) {
            console.error(error);
        } finally {
            setLoading(false);
        }
    };

    useEffect(() => {
        // Optional: Auto refresh every 30s
        const interval = setInterval(refreshPedidos, 30000);
        return () => clearInterval(interval);
    }, [sucursalId]);

    const handleStatusChange = async (pedidoId: number, currentStatusName: string, action: 'advance' | 'cancel') => {
        // Map status name to ID. This is brittle if names change.
        // Ideally we use IDs from backend.
        // Assuming 1->2->3->4(Entregado)
        let nextStatusId = 0;

        // Simple logic based on names seen in [id].astro
        const statusMap: Record<string, number> = {
            'Pendiente': 1,
            'En Preparacion': 2,
            'Listo': 3,
            'Entregado': 4,
            'Cancelado': 5
        };

        const currentId = statusMap[currentStatusName] || 0;

        if (action === 'cancel') {
            if (!confirm('¿Cancelar pedido?')) return;
            nextStatusId = 5;
        } else {
            if (currentId >= 3) {
                // Complete
                if (!confirm('¿Marcar como Entregado?')) return;
                nextStatusId = 4;
            } else {
                nextStatusId = currentId + 1;
            }
        }

        try {
            await sucursalService.cambiarEstadoPedido(pedidoId, nextStatusId);
            toast.success(action === 'cancel' ? 'Pedido cancelado' : 'Estado actualizado');
            refreshPedidos();
        } catch (error) {
            console.error(error);
            toast.error('Error al actualizar estado');
        }
    };

    const getPedidosByStatus = (statusName: string) => {
        return pedidos.filter(p => p.estadoPedidoNombre === statusName);
    };

    return (
        <div className="flex flex-col h-full space-y-4 animate-fadeIn relative">
            {/* Hidden Print Container */}
            <div style={{ display: 'none' }}>
                <div ref={printRef}>
                    {printingOrder && printingSucursal && (
                        <TicketTemplate
                            order={printingOrder}
                            sucursal={printingSucursal}
                            width={ticketSize}
                        />
                    )}
                </div>
            </div>

            <div className="flex justify-between items-center px-2">
                <h2 className="text-xl font-bold text-admin-text">Tablero de Pedidos</h2>

                <div className="flex items-center gap-2">
                    <select
                        value={ticketSize}
                        onChange={(e) => setTicketSize(e.target.value as '80mm' | '58mm')}
                        className="text-sm bg-white border border-admin-border rounded-lg px-2 py-1.5 outline-none focus:ring-2 focus:ring-admin-primary/20"
                        title="Tamaño de papel de la impresora"
                    >
                        <option value="80mm">Ticket 80mm (Estándar)</option>
                        <option value="58mm">Ticket 58mm (Chico)</option>
                    </select>

                    <button
                        onClick={refreshPedidos}
                        className="p-2 bg-white border border-admin-border rounded-lg shadow-sm hover:bg-gray-50 text-admin-text-secondary"
                        title="Actualizar"
                    >
                        <svg className={`w-5 h-5 ${loading ? 'animate-spin' : ''}`} fill="none" viewBox="0 0 24 24" stroke="currentColor">
                            <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M4 4v5h.582m15.356 2A8.001 8.001 0 004.582 9m0 0H9m11 11v-5h-.581m0 0a8.003 8.003 0 01-15.357-2m15.357 2H15" />
                        </svg>
                    </button>
                </div>
            </div>

            <div className="flex-1 overflow-x-auto overflow-y-hidden pb-4">
                <div className="flex gap-4 h-full min-w-[1000px]">
                    {STATUS_COLUMNS.map(col => (
                        <div key={col.id} className="flex-1 flex flex-col bg-gray-50/50 rounded-xl border border-admin-border h-full max-h-full">
                            <div className={`p-3 border-b border-admin-border rounded-t-xl font-bold flex justify-between items-center ${col.color.split(' ')[0]} ${col.color.split(' ')[1]}`}>
                                <span>{col.name}</span>
                                <span className="bg-white/50 px-2 py-0.5 rounded-full text-xs">
                                    {getPedidosByStatus(col.name).length}
                                </span>
                            </div>
                            <div className="p-3 overflow-y-auto flex-1 flex flex-col gap-3">
                                {getPedidosByStatus(col.name).map(pedido => (
                                    <div key={pedido.id} className="bg-white p-4 rounded-xl shadow-sm border border-admin-border hover:shadow-md transition-shadow group relative">
                                        <div className="flex justify-between items-start mb-2">
                                            <span className="font-bold text-lg text-admin-text">#{pedido.id}</span>
                                            <div className="flex items-center gap-2">
                                                {/* Print Button */}
                                                <button
                                                    onClick={() => sucursalConfig && handlePrint(pedido, sucursalConfig)}
                                                    className="p-1.5 text-gray-400 hover:text-admin-primary hover:bg-gray-100 rounded-lg transition-colors"
                                                    title="Imprimir Ticket"
                                                    disabled={!sucursalConfig}
                                                >
                                                    <svg className="w-5 h-5" fill="none" viewBox="0 0 24 24" stroke="currentColor">
                                                        <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M17 17h2a2 2 0 002-2v-4a2 2 0 00-2-2H5a2 2 0 00-2 2v4a2 2 0 002 2h2m2 4h6a2 2 0 002-2v-4a2 2 0 00-2-2H9a2 2 0 00-2 2v4a2 2 0 002 2zm8-12V5a2 2 0 00-2-2H9a2 2 0 00-2 2v4h10z" />
                                                    </svg>
                                                </button>
                                                <span className="text-xs text-admin-text-secondary bg-gray-100 px-2 py-1 rounded-full">
                                                    {new Date(pedido.fechaCreacion).toLocaleTimeString([], { hour: '2-digit', minute: '2-digit' })}
                                                </span>
                                            </div>
                                        </div>
                                        <div className="mb-3">
                                            <p className="font-medium text-admin-text line-clamp-1 mb-1">{pedido.clienteNombre}</p>

                                            {/* Detailed Items View */}
                                            {pedido.items && pedido.items.length > 0 ? (
                                                <ul className="text-sm text-gray-600 space-y-1 bg-gray-50 p-2 rounded-lg border border-gray-100">
                                                    {pedido.items.map((item, idx) => (
                                                        <li key={idx} className="flex items-start gap-1">
                                                            <span className="font-bold min-w-[20px]">{item.cantidad}x</span>
                                                            <div className="flex flex-col">
                                                                <span>{item.productoNombre}</span>
                                                                {item.aclaraciones && (
                                                                    <span className="text-amber-600 text-xs font-medium italic bg-amber-50 px-1 rounded w-fit">
                                                                        Nota: {item.aclaraciones}
                                                                    </span>
                                                                )}
                                                            </div>
                                                        </li>
                                                    ))}
                                                </ul>
                                            ) : (
                                                <p className="text-xs text-admin-text-secondary line-clamp-2">{pedido.resumenItems}</p>
                                            )}
                                        </div>
                                        <div className="flex justify-between items-center pt-2 border-t border-dashed border-gray-200 mt-2">
                                            <span className="font-bold text-admin-primary">${pedido.total}</span>
                                            <div className="flex gap-2">
                                                <button
                                                    onClick={() => handleStatusChange(pedido.id, pedido.estadoPedidoNombre, 'cancel')}
                                                    className="p-1.5 text-red-500 hover:bg-red-50 rounded-lg transition-colors"
                                                    title="Cancelar"
                                                >
                                                    <svg className="w-4 h-4" fill="none" viewBox="0 0 24 24" stroke="currentColor"><path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M6 18L18 6M6 6l12 12" /></svg>
                                                </button>
                                                <button
                                                    onClick={() => handleStatusChange(pedido.id, pedido.estadoPedidoNombre, 'advance')}
                                                    className="p-1.5 text-green-600 hover:bg-green-50 rounded-lg transition-colors border border-green-200 shadow-sm"
                                                    title="Avanzar Estado"
                                                >
                                                    <svg className="w-4 h-4" fill="none" viewBox="0 0 24 24" stroke="currentColor"><path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M5 13l4 4L19 7" /></svg>
                                                </button>
                                            </div>
                                        </div>
                                    </div>
                                ))}
                                {getPedidosByStatus(col.name).length === 0 && (
                                    <div className="text-center py-10 text-gray-300 text-sm">
                                        Sin pedidos
                                    </div>
                                )}
                            </div>
                        </div>
                    ))}

                    <div className="flex flex-col gap-4 w-80 shrink-0 h-full">
                        {/* Delivered Section */}
                        <div className="flex-1 flex flex-col bg-gray-50/50 rounded-xl border border-admin-border overflow-hidden opacity-70">
                            <div className="p-3 border-b border-admin-border rounded-t-xl font-bold text-gray-600 bg-gray-100 flex justify-between items-center">
                                <span>Terminados</span>
                                <span className="bg-gray-200 px-2 py-0.5 rounded-full text-xs">
                                    {getPedidosByStatus('Entregado').length}
                                </span>
                            </div>
                            <div className="p-3 overflow-y-auto flex-1 flex flex-col gap-3">
                                {getPedidosByStatus('Entregado').slice(0, 10).map(pedido => (
                                    <div key={pedido.id} className="bg-white p-3 rounded-lg border border-gray-200 grayscale opacity-80 hover:opacity-100 transition-opacity">
                                        <div className="flex justify-between">
                                            <span className="font-bold text-sm">#{pedido.id}</span>
                                            <button
                                                onClick={() => sucursalConfig && handlePrint(pedido, sucursalConfig)}
                                                className="p-1 text-gray-400 hover:text-admin-primary transition-colors"
                                                title="Reimprimir"
                                            >
                                                <svg className="w-4 h-4" fill="none" viewBox="0 0 24 24" stroke="currentColor">
                                                    <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M17 17h2a2 2 0 002-2v-4a2 2 0 00-2-2H5a2 2 0 00-2 2v4a2 2 0 002 2h2m2 4h6a2 2 0 002-2v-4a2 2 0 00-2-2H9a2 2 0 00-2 2v4a2 2 0 002 2zm8-12V5a2 2 0 00-2-2H9a2 2 0 00-2 2v4h10z" />
                                                </svg>
                                            </button>
                                            <span className="text-xs text-green-600 font-medium">Entregado</span>
                                        </div>
                                        <div className="flex justify-between mt-1 items-end">
                                            <span className="text-xs text-gray-500 truncate max-w-[120px]" title={pedido.clienteNombre}>{pedido.clienteNombre}</span>
                                            <span className="font-bold text-sm text-gray-700">${pedido.total}</span>
                                        </div>
                                    </div>
                                ))}
                                {getPedidosByStatus('Entregado').length === 0 && <div className="text-center text-xs text-gray-400 py-4">Vacío</div>}
                            </div>
                        </div>

                        {/* Cancelled Section */}
                        <div className="flex-1 flex flex-col bg-red-50/30 rounded-xl border border-red-100 overflow-hidden opacity-70">
                            <div className="p-3 border-b border-red-100 rounded-t-xl font-bold text-red-800 bg-red-50 flex justify-between items-center">
                                <span>Cancelados</span>
                                <span className="bg-red-200/50 px-2 py-0.5 rounded-full text-xs text-red-700">
                                    {getPedidosByStatus('Cancelado').length}
                                </span>
                            </div>
                            <div className="p-3 overflow-y-auto flex-1 flex flex-col gap-3">
                                {getPedidosByStatus('Cancelado').slice(0, 10).map(pedido => (
                                    <div key={pedido.id} className="bg-white p-3 rounded-lg border border-red-100 grayscale-[0.5] hover:grayscale-0 transition-all">
                                        <div className="flex justify-between">
                                            <span className="font-bold text-sm text-red-900">#{pedido.id}</span>
                                            <span className="text-xs text-red-600 font-medium">Cancelado</span>
                                        </div>
                                        <div className="flex justify-between mt-1 items-end">
                                            <span className="text-xs text-gray-500 truncate max-w-[120px]" title={pedido.clienteNombre}>{pedido.clienteNombre}</span>
                                            <span className="font-bold text-sm text-gray-700 line-through">${pedido.total}</span>
                                        </div>
                                    </div>
                                ))}
                                {getPedidosByStatus('Cancelado').length === 0 && <div className="text-center text-xs text-gray-400 py-4">Vacío</div>}
                            </div>
                        </div>
                    </div>
                </div>
            </div>
        </div>
    );
}
