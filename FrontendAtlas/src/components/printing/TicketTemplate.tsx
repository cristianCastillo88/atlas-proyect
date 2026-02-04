import React, { forwardRef } from 'react';
import type { Pedido, Sucursal } from '../../types/db';

interface TicketTemplateProps {
    order: Pedido;
    sucursal: Sucursal;
    width?: '58mm' | '80mm';
}

export const TicketTemplate = forwardRef<HTMLDivElement, TicketTemplateProps>(({ order, sucursal, width = '80mm' }, ref) => {
    // Determine CSS width class based on prop
    const widthClass = width === '80mm' ? 'w-[80mm]' : 'w-[58mm]';

    return (
        <div ref={ref} className={`${widthClass} bg-white text-black font-mono p-2 text-xs leading-tight mx-auto`}>
            {/* Header */}
            <div className="flex flex-col items-center justify-center mb-4 text-center border-b border-black pb-2 border-dashed">
                {/* Logo Placeholder - If you have a logo, render it here with grayscale filter */}
                {/* <img src="/logo.png" className="w-16 h-16 grayscale mb-2" /> */}

                <h1 className="font-bold text-lg uppercase tracking-wider">{sucursal.nombre}</h1>
                <p className="text-[10px]">{sucursal.direccion}</p>
                {sucursal.telefono && <p className="text-[10px]">Tel: {sucursal.telefono}</p>}

                <div className="mt-2 w-full flex justify-between text-[10px]">
                    <span>Fecha: {new Date(order.fechaCreacion).toLocaleDateString()}</span>
                    <span>Hora: {new Date(order.fechaCreacion).toLocaleTimeString([], { hour: '2-digit', minute: '2-digit' })}</span>
                </div>
                <div className="font-bold text-[12px] mt-1">
                    ORDEN #{order.id}
                </div>
            </div>

            {/* Customer Info */}
            <div className="mb-3 pb-2 border-b border-black border-dashed">
                <p className="font-bold uppercase">Cliente:</p>
                <div className="flex justify-between">
                    <span>{order.clienteNombre}</span>
                    {/* Add Phone if available */}
                    {order.clienteTelefono && <span>{order.clienteTelefono}</span>}
                </div>

                {/* Check for Delivery and Address */}
                {(order.tipoEntregaNombre === 'Delivery' || order.tipoEntregaNombre.includes('Delivery') || order.tipoEntregaNombre.includes('Envio')) && (
                    <div className="mt-1">
                        <span className="font-bold">Dirección:</span>
                        {/* We rely on proper DTO mapping now */}
                        <p className="whitespace-pre-wrap">{order.direccionCliente || 'N/A'}</p>
                    </div>
                )}

                <div className="flex justify-between mt-2 text-[10px] uppercase">
                    <span className="font-bold">{order.tipoEntregaNombre}</span>
                    <span>{order.metodoPagoNombre}</span>
                </div>
            </div>

            {/* Items */}
            <table className="w-full mb-3 text-left">
                <thead>
                    <tr className="border-b border-black text-[10px]">
                        <th className="w-6 py-1">Cant</th>
                        <th className="py-1">Producto</th>
                        <th className="w-12 text-right py-1">Total</th>
                    </tr>
                </thead>
                <tbody>
                    {order.items?.map((item, idx) => (
                        <tr key={idx} className="align-top">
                            <td className="py-1 font-bold">{item.cantidad}</td>
                            <td className="py-1">
                                <div>{item.productoNombre}</div>
                                {item.aclaraciones && (
                                    <div className="text-[10px] italic mt-0.5">({item.aclaraciones})</div>
                                )}
                            </td>
                            <td className="text-right py-1">${(item.precioUnitario * item.cantidad).toFixed(2)}</td>
                        </tr>
                    ))}
                </tbody>
            </table>

            {/* Totals */}
            <div className="border-t border-black border-dashed pt-2 mb-6">
                {/* Breakdown Calculation */}
                {(() => {
                    const isDelivery = order.tipoEntregaNombre === 'Delivery' || order.tipoEntregaNombre.includes('Delivery');
                    const deliveryCost = isDelivery ? (sucursal.precioDelivery || 0) : 0;
                    const subtotal = order.total - deliveryCost;

                    return (
                        <>
                            <div className="flex justify-between text-xs mb-1">
                                <span>Subtotal</span>
                                <span>${subtotal.toFixed(2)}</span>
                            </div>
                            {isDelivery && (
                                <div className="flex justify-between text-xs mb-1">
                                    <span>Envío</span>
                                    <span>${deliveryCost.toFixed(2)}</span>
                                </div>
                            )}
                            <div className="flex justify-between font-bold text-sm mt-1 border-t border-dotted border-gray-400 pt-1">
                                <span>TOTAL</span>
                                <span>${order.total.toFixed(2)}</span>
                            </div>
                        </>
                    );
                })()}
            </div>

            {/* Footer */}
            <div className="text-center text-[10px] space-y-1">
                <p>¡Gracias por su compra!</p>
                <p>*** COPIA DEL CLIENTE ***</p>
                {/* Cut Line Visualization for Preview (Hidden in print if needed, but usually good to have padding) */}
                <div className="pt-8 text-[8px] opacity-0">.</div>
            </div>

            {/* Styles specific associated with this component to ensure print works well */}
            {/* react-to-print handles the @page margins via pageStyle prop, so we only need local visual styles if any. */}
            {/* Tailwind classes handle the layout. */}
        </div>
    );
});

TicketTemplate.displayName = 'TicketTemplate';
