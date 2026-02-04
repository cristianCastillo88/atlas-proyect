import React, { useState, useMemo } from 'react';
import { toast } from 'sonner';
import type { Producto, Categoria, MetodoPago, TipoEntrega } from '../../../types/db';
import { sucursalService } from '../../../services/sucursal';

interface PosTerminalProps {
    sucursalId: number;
    initialProductos: Producto[];
    categorias: Categoria[];
    metodosPago: MetodoPago[];
    tiposEntrega: TipoEntrega[];
    precioDelivery: number;
}

interface CartItem {
    uuid: string;
    producto: Producto;
    cantidad: number;
    aclaraciones?: string | undefined;
}

export default function PosTerminal({
    sucursalId,
    initialProductos,
    categorias,
    metodosPago,
    tiposEntrega,
    precioDelivery
}: PosTerminalProps) {
    const [productos, setProductos] = useState<Producto[]>(initialProductos);
    const [selectedCategory, setSelectedCategory] = useState<number | 'all'>('all');
    const [cart, setCart] = useState<CartItem[]>([]);
    const [loading, setLoading] = useState(false);

    // Form State
    const [clientData, setClientData] = useState({
        nombre: '',
        telefono: '',
        direccion: '',
        metodoPagoId: metodosPago[0]?.id || 0,
        tipoEntregaId: tiposEntrega[0]?.id || 0,
        observaciones: ''
    });

    const filteredProducts = useMemo(() => {
        if (selectedCategory === 'all') return productos;

        return productos.filter(p => {
            // 1. Try direct ID match (most reliable if ID is present)
            if (p.categoriaId === selectedCategory) return true;

            // 2. Fallback: Match by Name (if ID is missing but Name is populated)
            if (!p.categoriaId && p.categoriaNombre) {
                const categoryName = categorias.find(c => c.id === selectedCategory)?.nombre;
                return p.categoriaNombre === categoryName;
            }

            return false;
        });
    }, [productos, selectedCategory, categorias]);

    const generateId = () => Date.now().toString(36) + Math.random().toString(36).substr(2);

    const addToCart = (producto: Producto) => {
        // POS Logic: By default add new line or group? 
        // Usually POS groups identical items. 
        // We will group by ID + Empty Note.

        const existing = cart.find(item => item.producto.id === producto.id && !item.aclaraciones);

        if (existing) {
            if (existing.cantidad + 1 > producto.stock) {
                toast.error(`No hay suficiente stock. Máximo: ${producto.stock}`);
                return;
            }

            setCart(cart.map(item =>
                item.uuid === existing.uuid
                    ? { ...item, cantidad: item.cantidad + 1 }
                    : item
            ));
        } else {
            setCart([...cart, {
                uuid: generateId(),
                producto,
                cantidad: 1,
                aclaraciones: ''
            }]);
        }
    };

    const removeFromCart = (uuid: string) => {
        setCart(cart.filter(item => item.uuid !== uuid));
    };

    const updateQuantity = (uuid: string, delta: number) => {
        const item = cart.find(i => i.uuid === uuid);
        if (!item) return;

        const newQty = item.cantidad + delta;
        if (newQty <= 0) {
            removeFromCart(uuid);
            return;
        }

        // Validate total stock for this product across all lines
        const currentTotal = cart.filter(i => i.producto.id === item.producto.id).reduce((acc, curr) => acc + curr.cantidad, 0);
        // We are changing by delta (1 or -1). If delta is 1, check if we exceed stock.
        if (delta > 0 && currentTotal + 1 > item.producto.stock) {
            toast.error('Stock insuficiente');
            return;
        }

        setCart(cart.map(i =>
            i.uuid === uuid ? { ...i, cantidad: newQty } : i
        ));
    };

    const updateAclaraciones = (uuid: string, text: string) => {
        setCart(cart.map(i =>
            i.uuid === uuid ? { ...i, aclaraciones: text } : i
        ));
    };

    const calculateTotal = () => {
        const subtotal = cart.reduce((sum, item) => sum + (item.producto.precio * item.cantidad), 0);
        const selectedType = tiposEntrega.find(t => t.id === Number(clientData.tipoEntregaId));
        const isDelivery = selectedType && (selectedType.nombre.toLowerCase().includes('delivery') || selectedType.nombre.toLowerCase().includes('envío') || selectedType.nombre.toLowerCase().includes('envio'));
        const deliveryCost = isDelivery ? precioDelivery : 0;
        return subtotal + deliveryCost;
    };

    const handleSubmit = async (e: React.FormEvent) => {
        e.preventDefault();
        if (cart.length === 0) {
            toast.error('El carrito está vacío');
            return;
        }

        setLoading(true);
        try {
            await sucursalService.createPedido({
                sucursalId,
                nombreCliente: clientData.nombre,
                telefonoCliente: clientData.telefono,
                direccionCliente: clientData.direccion,
                metodoPagoId: Number(clientData.metodoPagoId),
                tipoEntregaId: Number(clientData.tipoEntregaId),
                observaciones: clientData.observaciones,
                items: cart.map(i => ({
                    productoId: i.producto.id,
                    cantidad: i.cantidad,
                    aclaraciones: i.aclaraciones
                }))
            });
            toast.success('Pedido creado exitosamente');
            setCart([]);
            setClientData(prev => ({ ...prev, nombre: '', telefono: '', direccion: '', observaciones: '' }));
            // Optionally refresh products to update stock
            const updatedProducts = await sucursalService.getProductos(sucursalId);
            setProductos(updatedProducts);
        } catch (error) {
            console.error(error);
            toast.error('Error al crear el pedido');
        } finally {
            setLoading(false);
        }
    };

    return (
        <div className="flex flex-col lg:flex-row gap-6 h-full p-1 animate-fadeIn">
            {/* Product Section */}
            <div className="flex-1 flex flex-col gap-4">
                {/* Category Filter */}
                <div className="flex flex-wrap gap-2 pb-2 overflow-x-auto no-scrollbar">
                    <button
                        onClick={() => setSelectedCategory('all')}
                        className={`px-4 py-2 rounded-full text-sm font-medium transition-all ${selectedCategory === 'all'
                            ? 'bg-admin-primary text-white shadow-lg shadow-admin-primary/25'
                            : 'bg-white text-admin-text-secondary hover:bg-admin-surface hover:text-admin-text'
                            }`}
                    >
                        Todos
                    </button>
                    {categorias.map(cat => (
                        <button
                            key={cat.id}
                            onClick={() => setSelectedCategory(cat.id)}
                            className={`px-4 py-2 rounded-full text-sm font-medium transition-all ${selectedCategory === cat.id
                                ? 'bg-admin-primary text-white shadow-lg shadow-admin-primary/25'
                                : 'bg-white text-admin-text-secondary hover:bg-admin-surface hover:text-admin-text'
                                }`}
                        >
                            {cat.nombre}
                        </button>
                    ))}
                </div>

                {/* Grid */}
                <div className="grid grid-cols-2 md:grid-cols-3 xl:grid-cols-4 gap-4 overflow-y-auto pr-2 pb-20 lg:pb-0" style={{ maxHeight: 'calc(100vh - 250px)' }}>
                    {filteredProducts.map(product => (
                        <div
                            key={product.id}
                            onClick={() => product.stock > 0 && addToCart(product)}
                            className={`group relative bg-white rounded-xl shadow-sm border border-admin-border overflow-hidden cursor-pointer transition-all hover:shadow-md hover:border-admin-primary/50 flex flex-col ${product.stock === 0 ? 'opacity-60 grayscale cursor-not-allowed' : ''
                                }`}
                        >
                            <div className="aspect-video w-full bg-gray-100 relative overflow-hidden">
                                {product.urlImagen ? (
                                    <img
                                        src={product.urlImagen}
                                        alt={product.nombre}
                                        className="w-full h-full object-cover group-hover:scale-105 transition-transform duration-500"
                                    />
                                ) : (
                                    <div className="w-full h-full flex items-center justify-center text-gray-300">
                                        <svg className="w-10 h-10" fill="none" viewBox="0 0 24 24" stroke="currentColor">
                                            <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={1.5} d="M4 16l4.586-4.586a2 2 0 012.828 0L16 16m-2-2l1.586-1.586a2 2 0 012.828 0L20 14m-6-6h.01M6 20h12a2 2 0 002-2V6a2 2 0 00-2-2H6a2 2 0 00-2 2v12a2 2 0 002 2z" />
                                        </svg>
                                    </div>
                                )}
                                <div className="absolute top-2 right-2 bg-black/70 text-white text-xs px-2 py-1 rounded-full font-medium backdrop-blur-sm">
                                    ${product.precio}
                                </div>
                            </div>
                            <div className="p-3 flex-1 flex flex-col gap-1">
                                <h3 className="font-semibold text-admin-text line-clamp-1 group-hover:text-admin-primary transition-colors">
                                    {product.nombre}
                                </h3>
                                <p className="text-xs text-admin-text-secondary line-clamp-2">{product.descripcion}</p>
                                <div className="mt-auto pt-2 flex justify-between items-center text-xs">
                                    <span className={product.stock < 5 ? 'text-amber-600 font-medium' : 'text-green-600'}>
                                        {product.stock} en stock
                                    </span>
                                </div>
                            </div>
                        </div>
                    ))}
                </div>
            </div>

            {/* Cart Section */}
            <div className="w-full lg:w-96 bg-white rounded-2xl shadow-xl shadow-admin-shadow overflow-hidden flex flex-col border border-admin-border h-fit sticky top-4">
                <div className="p-4 bg-admin-surface border-b border-admin-border flex justify-between items-center">
                    <h2 className="font-bold text-lg text-admin-text flex items-center gap-2">
                        <svg className="w-5 h-5 text-admin-primary" fill="none" viewBox="0 0 24 24" stroke="currentColor">
                            <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M3 3h2l.4 2M7 13h10l4-8H5.4M7 13L5.4 5M7 13l-2.293 2.293c-.63.63-.184 1.707.707 1.707H17m0 0a2 2 0 100 4 2 2 0 000-4zm-8 2a2 2 0 11-4 0 2 2 0 014 0z" />
                        </svg>
                        Nuevo Pedido
                    </h2>
                    <span className="bg-admin-primary/10 text-admin-primary text-xs px-2 py-1 rounded-full font-bold">
                        {cart.reduce((acc, item) => acc + item.cantidad, 0)} Items
                    </span>
                </div>

                <div className="flex-1 overflow-y-auto min-h-[200px] max-h-[400px] p-4 flex flex-col gap-3">
                    {cart.length === 0 ? (
                        <div className="flex flex-col items-center justify-center h-48 text-admin-text-secondary gap-2">
                            <svg className="w-12 h-12 opacity-20" fill="none" viewBox="0 0 24 24" stroke="currentColor">
                                <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={1} d="M16 11V7a4 4 0 00-8 0v4M5 9h14l1 12H4L5 9z" />
                            </svg>
                            <p>El carrito está vacío</p>
                        </div>
                    ) : (
                        cart.map((item) => (
                            <div key={item.uuid} className="flex flex-col gap-2 bg-white rounded-lg p-2 border border-admin-border/50 shadow-sm">
                                <div className="flex gap-3 items-center group">
                                    <div className="w-12 h-12 bg-gray-100 rounded-lg overflow-hidden shrink-0">
                                        {item.producto.urlImagen && (
                                            <img src={item.producto.urlImagen} alt="" className="w-full h-full object-cover" />
                                        )}
                                    </div>
                                    <div className="flex-1 min-w-0">
                                        <h4 className="text-sm font-medium text-admin-text truncate">{item.producto.nombre}</h4>
                                        <p className="text-xs text-admin-text-secondary">${item.producto.precio} x {item.cantidad}</p>
                                    </div>
                                    <div className="flex items-center gap-2 bg-admin-surface rounded-lg p-1">
                                        <button
                                            onClick={() => updateQuantity(item.uuid, -1)}
                                            className="w-6 h-6 flex items-center justify-center rounded bg-white shadow-sm text-admin-text hover:text-red-500 hover:bg-red-50 transition-colors"
                                        >
                                            -
                                        </button>
                                        <span className="text-sm font-medium w-4 text-center">{item.cantidad}</span>
                                        <button
                                            onClick={() => updateQuantity(item.uuid, 1)}
                                            className="w-6 h-6 flex items-center justify-center rounded bg-white shadow-sm text-admin-text hover:text-green-500 hover:bg-green-50 transition-colors"
                                        >
                                            +
                                        </button>
                                    </div>
                                </div>
                                <input
                                    type="text"
                                    placeholder="Nota/Aclaración..."
                                    className="w-full text-xs px-2 py-1.5 bg-gray-50 border border-gray-200 rounded outline-none focus:border-admin-primary focus:bg-white transition-all"
                                    value={item.aclaraciones || ''}
                                    onChange={(e) => updateAclaraciones(item.uuid, e.target.value)}
                                />
                            </div>
                        ))
                    )}
                </div>

                <form onSubmit={handleSubmit} className="border-t border-admin-border p-4 bg-gray-50/50 flex flex-col gap-3">
                    <input
                        required
                        type="text"
                        placeholder="Nombre Cliente"
                        className="w-full px-3 py-2 bg-white border border-admin-border rounded-lg text-sm focus:ring-2 focus:ring-admin-primary/20 focus:border-admin-primary outline-none transition-all"
                        value={clientData.nombre}
                        onChange={e => setClientData({ ...clientData, nombre: e.target.value })}
                    />
                    <div className="grid grid-cols-2 gap-3">
                        <input
                            required
                            type="tel"
                            placeholder="Teléfono"
                            className="w-full px-3 py-2 bg-white border border-admin-border rounded-lg text-sm focus:ring-2 focus:ring-admin-primary/20 focus:border-admin-primary outline-none transition-all"
                            value={clientData.telefono}
                            onChange={e => setClientData({ ...clientData, telefono: e.target.value })}
                        />
                        <select
                            className="w-full px-3 py-2 bg-white border border-admin-border rounded-lg text-sm focus:ring-2 focus:ring-admin-primary/20 focus:border-admin-primary outline-none transition-all appearance-none"
                            value={clientData.metodoPagoId}
                            onChange={e => setClientData({ ...clientData, metodoPagoId: Number(e.target.value) })}
                        >
                            <option disabled value={0}>Metodo Pago</option>
                            {metodosPago.map(m => <option key={m.id} value={m.id}>{m.nombre}</option>)}
                        </select>
                    </div>
                    <input
                        required
                        type="text"
                        placeholder="Dirección"
                        className="w-full px-3 py-2 bg-white border border-admin-border rounded-lg text-sm focus:ring-2 focus:ring-admin-primary/20 focus:border-admin-primary outline-none transition-all"
                        value={clientData.direccion}
                        onChange={e => setClientData({ ...clientData, direccion: e.target.value })}
                    />
                    <select
                        className="w-full px-3 py-2 bg-white border border-admin-border rounded-lg text-sm focus:ring-2 focus:ring-admin-primary/20 focus:border-admin-primary outline-none transition-all appearance-none"
                        value={clientData.tipoEntregaId}
                        onChange={e => setClientData({ ...clientData, tipoEntregaId: Number(e.target.value) })}
                    >
                        <option disabled value={0}>Tipo Entrega</option>
                        {tiposEntrega.map(t => {
                            const isDelivery = t.nombre.toLowerCase().includes('delivery') || t.nombre.toLowerCase().includes('envío') || t.nombre.toLowerCase().includes('envio');
                            const price = isDelivery ? precioDelivery : 0;
                            return (
                                <option key={t.id} value={t.id}>
                                    {t.nombre} {price > 0 ? `(+$${price})` : ''}
                                </option>
                            );
                        })}
                    </select>
                    <textarea
                        placeholder="Observaciones..."
                        className="w-full px-3 py-2 bg-white border border-admin-border rounded-lg text-sm focus:ring-2 focus:ring-admin-primary/20 focus:border-admin-primary outline-none transition-all resize-none h-16"
                        value={clientData.observaciones}
                        onChange={e => setClientData({ ...clientData, observaciones: e.target.value })}
                    />

                    <div className="mt-2 pt-3 border-t border-admin-border/50 flex justify-between items-end">
                        <div>
                            <p className="text-xs text-admin-text-secondary">Total a Pagar</p>
                            <p className="text-2xl font-bold text-admin-text">${calculateTotal().toLocaleString()}</p>
                        </div>
                        <button
                            disabled={loading || cart.length === 0}
                            className={`px-6 py-3 rounded-xl font-semibold shadow-lg shadow-admin-primary/25 transition-all flex items-center gap-2 ${loading || cart.length === 0
                                ? 'bg-gray-300 text-gray-500 cursor-not-allowed shadow-none'
                                : 'bg-admin-primary text-white hover:bg-admin-primary-dark hover:scale-[1.02] active:scale-[0.98]'
                                }`}
                        >
                            {loading ? (
                                <span className="animate-spin h-5 w-5 border-2 border-white border-t-transparent rounded-full" />
                            ) : (
                                <>
                                    <span>Confirmar</span>
                                    <svg className="w-5 h-5" fill="none" viewBox="0 0 24 24" stroke="currentColor">
                                        <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M14 5l7 7m0 0l-7 7m7-7H3" />
                                    </svg>
                                </>
                            )}
                        </button>
                    </div>
                </form>
            </div>
        </div>
    );
}
