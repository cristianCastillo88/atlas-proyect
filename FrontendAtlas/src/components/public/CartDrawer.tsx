import React, { useState } from 'react';
import { useStore } from '@nanostores/react';
import { $cart, $cantidadTotal, $precioTotal, restarItem, agregarItem, eliminarItem, limpiarCarrito, actualizarAclaraciones } from '../../stores/cart';
import type { MetodoPago, TipoEntrega } from '../../services/public';
import { crearPedidoPublico } from '../../services/public';
import { toast } from 'sonner';

interface CartDrawerProps {
    metodosPago: MetodoPago[];
    tiposEntrega: TipoEntrega[];
    sucursalTelefono: string;
    sucursalNombre: string;
    precioDelivery: number;
}

export default function CartDrawer({
    metodosPago,
    tiposEntrega,
    sucursalTelefono,
    sucursalNombre,
    precioDelivery
}: CartDrawerProps) {
    const [isOpen, setIsOpen] = useState(false);
    const cartItems = useStore($cart).items;
    const count = useStore($cantidadTotal);
    const subtotal = useStore($precioTotal); // Base items price

    const [formData, setFormData] = useState({
        nombre: '',
        telefono: '',
        tipoEntrega: '',
        direccion: '',
        metodoPago: '',
        observaciones: ''
    });

    const [loading, setLoading] = useState(false);

    const handleInputChange = (e: React.ChangeEvent<HTMLInputElement | HTMLSelectElement | HTMLTextAreaElement>) => {
        setFormData({
            ...formData,
            [e.target.name]: e.target.value
        });
    };

    const isDelivery = () => {
        const selectedType = tiposEntrega.find(t => t.id === Number(formData.tipoEntrega));
        if (!selectedType) return false;
        const nombre = selectedType.nombre.toLowerCase();
        return nombre.includes('delivery') || nombre.includes('envío') || nombre.includes('envio');
    };

    const finalTotal = isDelivery() ? subtotal + precioDelivery : subtotal;

    const handleCheckout = async (e: React.FormEvent) => {
        e.preventDefault();
        if (cartItems.length === 0) {
            toast.error('Tu carrito está vacío');
            return;
        }

        // Basic validation
        if (!formData.tipoEntrega) { toast.error('Selecciona tipo de entrega'); return; }
        if (!formData.metodoPago) { toast.error('Selecciona método de pago'); return; }
        if (isDelivery() && !formData.direccion) { toast.error('La dirección es obligatoria'); return; }

        setLoading(true);

        try {
            const firstItem = cartItems[0];
            if (!firstItem) return;

            const pedido = {
                sucursalId: firstItem.sucursalId,
                nombreCliente: formData.nombre,
                telefonoCliente: formData.telefono,
                direccionCliente: formData.direccion || null,
                tipoEntregaId: Number(formData.tipoEntrega),
                metodoPagoId: Number(formData.metodoPago),
                items: cartItems.map(item => ({
                    productoId: item.id,
                    cantidad: item.cantidad,
                    aclaraciones: item.aclaraciones
                })),
                observaciones: formData.observaciones || null
            };

            const response = await crearPedidoPublico(pedido);

            // Build WhatsApp message
            const tipoEntregaNombre = tiposEntrega.find(t => t.id === Number(formData.tipoEntrega))?.nombre || 'Retiro';
            const metodoPagoNombre = metodosPago.find(m => m.id === Number(formData.metodoPago))?.nombre || 'Efectivo';

            let mensaje = `🍕 *Nuevo Pedido - ${sucursalNombre}*%0A%0A`;
            mensaje += `👤 *Cliente:* ${formData.nombre}%0A`;
            mensaje += `📞 *Teléfono:* ${formData.telefono}%0A`;
            mensaje += `📦 *Tipo:* ${tipoEntregaNombre}%0A`;
            if (formData.direccion) mensaje += `📍 *Dirección:* ${formData.direccion}%0A`;
            mensaje += `💳 *Pago:* ${metodoPagoNombre}%0A%0A`;

            mensaje += `🛒 *Detalle del Pedido:*%0A`;
            cartItems.forEach(item => {
                mensaje += `• ${item.cantidad}x ${item.nombre}`;
                if (item.aclaraciones) {
                    mensaje += ` (%5BNota: ${item.aclaraciones}%5D)`; // URL Encoded brackets []
                }
                mensaje += ` - $${(item.precio * item.cantidad).toFixed(2)}%0A`;
            });

            mensaje += `%0A-------------------------%0A`;
            mensaje += `Subtotal: $${subtotal.toFixed(2)}%0A`;
            if (isDelivery()) {
                mensaje += `Envío: $${precioDelivery.toFixed(2)}%0A`;
            }
            mensaje += `💰 *TOTAL: $${finalTotal.toFixed(2)}*%0A`;

            if (formData.observaciones) mensaje += `%0A📝 *Observaciones:* ${formData.observaciones}`;
            mensaje += `%0A%0A_Pedido #${response.id}_`;

            limpiarCarrito();
            setIsOpen(false);
            setFormData({ nombre: '', telefono: '', tipoEntrega: '', direccion: '', metodoPago: '', observaciones: '' });

            const whatsappUrl = `https://wa.me/${sucursalTelefono}?text=${mensaje}`;
            window.open(whatsappUrl, '_blank');
            toast.success('¡Pedido enviado! Te redirigimos a WhatsApp.');

        } catch (error) {
            console.error(error);
            toast.error('Error al procesar el pedido');
        } finally {
            setLoading(false);
        }
    };

    return (
        <>
            {/* Floating Button */}
            <button
                onClick={() => setIsOpen(true)}
                className="fixed bottom-6 right-6 w-16 h-16 bg-green-600 hover:bg-green-700 text-white rounded-full shadow-lg flex items-center justify-center z-40 transition-transform hover:scale-110"
            >
                <svg className="w-8 h-8" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                    <path strokeLinecap="round" strokeLinejoin="round" strokeWidth="2" d="M3 3h2l.4 2M7 13h10l4-8H5.4M7 13L5.4 5M7 13l-2.293 2.293c-.63.63-.184 1.707.707 1.707H17m0 0a2 2 0 100 4 2 2 0 000-4zm-8 2a2 2 0 11-4 0 2 2 0 014 0z"></path>
                </svg>
                {count > 0 && (
                    <span className="absolute -top-1 -right-1 bg-red-600 text-white text-xs font-bold rounded-full w-6 h-6 flex items-center justify-center">
                        {count}
                    </span>
                )}
            </button>

            {/* Drawer */}
            <div className={`fixed inset-0 z-50 transform transition-all duration-300 ease-in-out ${isOpen ? 'visible' : 'invisible'}`}>
                {/* Overlay */}
                <div
                    className={`absolute inset-0 bg-black/50 backdrop-blur-sm transition-opacity duration-300 ${isOpen ? 'opacity-100' : 'opacity-0'}`}
                    onClick={() => setIsOpen(false)}
                />

                {/* Panel */}
                <div className={`absolute right-0 top-0 h-full w-full md:w-[480px] bg-white shadow-2xl flex flex-col transition-transform duration-300 ${isOpen ? 'translate-x-0' : 'translate-x-full'}`}>
                    {/* Header */}
                    <div className="bg-green-600 text-white p-4 flex items-center justify-between">
                        <h2 className="text-xl font-bold">Tu Pedido</h2>
                        <button onClick={() => setIsOpen(false)} className="text-white hover:text-gray-200 text-2xl">✕</button>
                    </div>

                    <div className="flex-1 overflow-y-auto p-4">
                        {/* Items */}
                        <div className="space-y-3 mb-6">
                            {cartItems.length === 0 ? (
                                <p className="text-gray-500 text-center py-8">Tu carrito está vacío</p>
                            ) : (
                                cartItems.map(item => (
                                    <div key={item.uuid} className="bg-gray-50 rounded-lg p-3 flex flex-col gap-2">
                                        <div className="flex items-center gap-3">
                                            <div className="flex-1">
                                                <h4 className="font-semibold text-gray-800">{item.nombre}</h4>
                                                <p className="text-sm text-gray-600">${item.precio.toFixed(2)} × {item.cantidad}</p>
                                            </div>
                                            <div className="flex items-center gap-2">
                                                <button
                                                    onClick={() => restarItem(item.uuid)}
                                                    className="w-8 h-8 border border-gray-300 rounded flex items-center justify-center hover:bg-gray-200"
                                                >−</button>
                                                <span className="font-semibold w-8 text-center">{item.cantidad}</span>
                                                <button
                                                    onClick={() => {
                                                        try {
                                                            agregarItem({ id: item.id, nombre: item.nombre, precio: item.precio, stock: item.stock }, item.sucursalId, item.aclaraciones);
                                                        } catch (e: any) {
                                                            toast.error(e.message);
                                                        }
                                                    }}
                                                    className="w-8 h-8 border border-gray-300 rounded flex items-center justify-center hover:bg-gray-200"
                                                >+</button>
                                                <button
                                                    onClick={() => eliminarItem(item.uuid)}
                                                    className="w-8 h-8 text-red-600 hover:bg-red-50 rounded flex items-center justify-center"
                                                >×</button>
                                            </div>
                                        </div>
                                        <input
                                            type="text"
                                            placeholder="Aclaraciones (ej: sin mayonesa)"
                                            className="w-full text-xs px-2 py-1 border border-gray-200 rounded bg-white focus:border-green-500 outline-none text-gray-600"
                                            value={item.aclaraciones || ''}
                                            onChange={(e) => actualizarAclaraciones(item.uuid, e.target.value)}
                                        />
                                    </div>
                                ))
                            )}
                        </div>

                        {/* Total Section with Delivery Breakdown */}
                        <div className="border-t pt-4 mb-6">
                            <div className="space-y-2 mb-3">
                                <div className="flex justify-between text-gray-600">
                                    <span>Subtotal</span>
                                    <span>${subtotal.toFixed(2)}</span>
                                </div>
                                {isDelivery() && (
                                    <div className="flex justify-between text-gray-800 font-medium">
                                        <span>Costo de Envío</span>
                                        <span>${precioDelivery.toFixed(2)}</span>
                                    </div>
                                )}
                            </div>
                            <div className="flex justify-between items-center text-xl font-bold border-t pt-2 border-dashed">
                                <span>Total:</span>
                                <span className="text-green-600">${finalTotal.toFixed(2)}</span>
                            </div>
                        </div>

                        {/* Form */}
                        <form onSubmit={handleCheckout} className="space-y-4">
                            <div>
                                <label className="block text-sm font-semibold text-gray-700 mb-1">Nombre</label>
                                <input required type="text" name="nombre" value={formData.nombre} onChange={handleInputChange}
                                    className="w-full px-4 py-2 border border-gray-300 rounded-lg focus:ring-2 focus:ring-green-500 outline-none" placeholder="Tu nombre completo" />
                            </div>
                            <div>
                                <label className="block text-sm font-semibold text-gray-700 mb-1">Teléfono</label>
                                <input required type="tel" name="telefono" value={formData.telefono} onChange={handleInputChange}
                                    className="w-full px-4 py-2 border border-gray-300 rounded-lg focus:ring-2 focus:ring-green-500 outline-none" placeholder="11 1234-5678" />
                            </div>
                            <div>
                                <label className="block text-sm font-semibold text-gray-700 mb-1">Tipo de Entrega</label>
                                <select required name="tipoEntrega" value={formData.tipoEntrega} onChange={handleInputChange}
                                    className="w-full px-4 py-2 border border-gray-300 rounded-lg focus:ring-2 focus:ring-green-500 outline-none">
                                    <option value="">Seleccionar...</option>
                                    {tiposEntrega.map(t => <option key={t.id} value={t.id}>{t.nombre}</option>)}
                                </select>
                            </div>

                            {isDelivery() && (
                                <div className="animate-fadeIn">
                                    <label className="block text-sm font-semibold text-gray-700 mb-1">Dirección</label>
                                    <input required type="text" name="direccion" value={formData.direccion} onChange={handleInputChange}
                                        className="w-full px-4 py-2 border border-gray-300 rounded-lg focus:ring-2 focus:ring-green-500 outline-none" placeholder="Calle, número, piso" />
                                </div>
                            )}

                            <div>
                                <label className="block text-sm font-semibold text-gray-700 mb-1">Método de Pago</label>
                                <select required name="metodoPago" value={formData.metodoPago} onChange={handleInputChange}
                                    className="w-full px-4 py-2 border border-gray-300 rounded-lg focus:ring-2 focus:ring-green-500 outline-none">
                                    <option value="">Seleccionar...</option>
                                    {metodosPago.map(m => <option key={m.id} value={m.id}>{m.nombre}</option>)}
                                </select>
                            </div>
                            <div>
                                <label className="block text-sm font-semibold text-gray-700 mb-1">Observaciones</label>
                                <textarea name="observaciones" rows={2} value={formData.observaciones} onChange={handleInputChange}
                                    className="w-full px-4 py-2 border border-gray-300 rounded-lg focus:ring-2 focus:ring-green-500 outline-none" placeholder="Sin cebolla, etc." />
                            </div>

                            <button
                                type="submit"
                                disabled={loading || cartItems.length === 0}
                                className={`w-full bg-green-600 hover:bg-green-700 text-white font-bold py-3 rounded-lg flex items-center justify-center gap-2 transition-colors ${loading ? 'opacity-70 cursor-not-allowed' : ''}`}
                            >
                                {loading ? 'Enviando...' : (
                                    <>
                                        <svg className="w-6 h-6" fill="currentColor" viewBox="0 0 24 24"><path d="M17.472 14.382c-.297-.149-1.758-.867-2.03-.967-.273-.099-.471-.148-.67.15-.197.297-.767.966-.94 1.164-.173.199-.347.223-.644.075-.297-.15-1.255-.463-2.39-1.475-.883-.788-1.48-1.761-1.653-2.059-.173-.297-.018-.458.13-.606.134-.133.298-.347.446-.52.149-.174.198-.298.298-.497.099-.198.05-.371-.025-.52-.075-.149-.669-1.612-.916-2.207-.242-.579-.487-.5-.669-.51-.173-.008-.371-.01-.57-.01-.198 0-.52.074-.792.372-.272.297-1.04 1.016-1.04 2.479 0 1.462 1.065 2.875 1.213 3.074.149.198 2.096 3.2 5.077 4.487.709.306 1.262.489 1.694.625.712.227 1.36.195 1.871.118.571-.085 1.758-.719 2.006-1.413.248-.694.248-1.289.173-1.413-.074-.124-.272-.198-.57-.347m-5.421 7.403h-.004a9.87 9.87 0 01-5.031-1.378l-.361-.214-3.741.982.998-3.648-.235-.374a9.86 9.86 0 01-1.51-5.26c.001-5.45 4.436-9.884 9.888-9.884 2.64 0 5.122 1.03 6.988 2.898a9.825 9.825 0 012.893 6.994c-.003 5.45-4.437 9.884-9.885 9.884m8.413-18.297A11.815 11.815 0 0012.05 0C5.495 0 .16 5.335.157 11.892c0 2.096.547 4.142 1.588 5.945L.057 24l6.305-1.654a11.882 11.882 0 005.683 1.448h.005c6.554 0 11.89-5.335 11.893-11.893a11.821 11.821 0 00-3.48-8.413Z" /></svg>
                                        Pedir por WhatsApp
                                    </>
                                )}
                            </button>
                        </form>
                    </div>
                </div>
            </div>
        </>
    );
}
