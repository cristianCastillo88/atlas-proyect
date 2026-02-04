import React, { useState } from 'react';
import { agregarItem } from '../../stores/cart';
import type { SucursalPublica } from '../../services/public';
import { toast } from 'sonner';
import CartDrawer from './CartDrawer';

interface SucursalPublicViewProps {
    sucursal: SucursalPublica;
}

type View = 'categories' | 'products' | 'info' | 'location';

export default function SucursalPublicView({ sucursal }: SucursalPublicViewProps) {
    const [activeView, setActiveView] = useState<View>('categories');
    const [selectedCategory, setSelectedCategory] = useState<string | null>(null);
    const [isMobileMenuOpen, setIsMobileMenuOpen] = useState(false);

    // Group products by category
    const productsByCategory = React.useMemo(() => {
        return sucursal.productos.reduce((acc, producto) => {
            const categoria = producto.categoriaNombre || 'Otros';
            if (!acc[categoria]) {
                acc[categoria] = [];
            }
            acc[categoria].push(producto);
            return acc;
        }, {} as Record<string, typeof sucursal.productos>);
    }, [sucursal.productos]);

    const categorias = Object.keys(productsByCategory);

    const handleCategoryClick = (category: string) => {
        setSelectedCategory(category);
        setActiveView('products');
        window.scrollTo({ top: 0, behavior: 'smooth' });
    };

    const handleBackToCategories = () => {
        setSelectedCategory(null);
        setActiveView('categories');
        window.scrollTo({ top: 0, behavior: 'smooth' });
    };

    const handleAddToCart = (product: any) => {
        try {
            agregarItem({
                id: product.id,
                nombre: product.nombre,
                precio: product.precio,
                stock: product.stock
            }, sucursal.id);
            toast.success(`${product.nombre} agregado`);
        } catch (err: any) {
            toast.error(err.message || 'Error al agregar');
        }
    };

    const NavButton = ({ target, label, icon }: { target: View; label: string; icon: React.ReactNode }) => {
        const isActive = activeView === target || (activeView === 'products' && target === 'categories');
        return (
            <button
                onClick={() => { setActiveView(target); setIsMobileMenuOpen(false); }}
                className={`w-full flex items-center gap-3 px-4 py-3 rounded-lg transition-all group ${isActive ? 'bg-gray-800 text-white shadow-sm' : 'text-gray-400 hover:bg-gray-800 hover:text-white'
                    }`}
            >
                <div className={`${isActive ? 'text-primary-400' : 'text-gray-500 group-hover:text-primary-400'}`}>
                    {icon}
                </div>
                <span className="font-medium">{label}</span>
            </button>
        );
    };

    return (
        <div className="flex h-screen bg-white font-sans overflow-hidden">

            {/* Sidebar (Desktop) */}
            <aside className="w-64 bg-gray-900 text-white flex-col hidden md:flex h-full shadow-xl z-20">
                <div className="p-6 flex items-center gap-3 border-b border-gray-800">
                    <div className="w-10 h-10 bg-primary-600 rounded-lg flex items-center justify-center font-bold text-xl font-display shadow-lg shadow-primary-900/50">
                        {sucursal.nombre.substring(0, 1)}
                    </div>
                    <h1 className="font-display font-bold text-xl tracking-tight leading-none">
                        {sucursal.nombre}
                    </h1>
                </div>

                <nav className="flex-1 py-6 px-3 space-y-1">
                    <NavButton
                        target="categories"
                        label="Inicio"
                        icon={<svg className="w-5 h-5" fill="none" stroke="currentColor" viewBox="0 0 24 24"><path strokeLinecap="round" strokeLinejoin="round" strokeWidth="2" d="M3 12l2-2m0 0l7-7 7 7M5 10v10a1 1 0 001 1h3m10-11l2 2m-2-2v10a1 1 0 01-1 1h-3m-6 0a1 1 0 001-1v-4a1 1 0 011-1h2a1 1 0 011 1v4a1 1 0 001 1m-6 0h6"></path></svg>}
                    />
                    <NavButton
                        target="info"
                        label="Información"
                        icon={<svg className="w-5 h-5" fill="none" stroke="currentColor" viewBox="0 0 24 24"><path strokeLinecap="round" strokeLinejoin="round" strokeWidth="2" d="M13 16h-1v-4h-1m1-4h.01M21 12a9 9 0 11-18 0 9 9 0 0118 0z"></path></svg>}
                    />
                    <NavButton
                        target="location"
                        label="Ubicación"
                        icon={<svg className="w-5 h-5" fill="none" stroke="currentColor" viewBox="0 0 24 24"><path strokeLinecap="round" strokeLinejoin="round" strokeWidth="2" d="M17.657 16.657L13.414 20.9a1.998 1.998 0 01-2.827 0l-4.244-4.243a8 8 0 1111.314 0z"></path><path strokeLinecap="round" strokeLinejoin="round" strokeWidth="2" d="M15 11a3 3 0 11-6 0 3 3 0 016 0z"></path></svg>}
                    />
                </nav>

                <div className="p-6 border-t border-gray-800 space-y-3">
                    {!!sucursal.urlInstagram || !!sucursal.urlFacebook ? (
                        <p className="text-xs text-gray-500 uppercase tracking-wider font-semibold">Síguenos</p>
                    ) : null}

                    {sucursal.urlInstagram && (
                        <a href={sucursal.urlInstagram} target="_blank" rel="noopener noreferrer" className="flex items-center gap-3 text-gray-400 hover:text-pink-500 transition-colors">
                            <svg className="w-5 h-5" fill="none" stroke="currentColor" viewBox="0 0 24 24"><path strokeLinecap="round" strokeLinejoin="round" strokeWidth="2" d="M16 11.37A4 4 0 1112.63 8 4 4 0 0116 11.37zm1.5-4.87h.01"></path><rect width="20" height="20" x="2" y="2" rx="5" ry="5" strokeWidth="2"></rect></svg>
                            Instagram
                        </a>
                    )}

                    {sucursal.urlFacebook && (
                        <a href={sucursal.urlFacebook} target="_blank" rel="noopener noreferrer" className="flex items-center gap-3 text-gray-400 hover:text-blue-500 transition-colors">
                            <svg className="w-5 h-5" fill="none" stroke="currentColor" viewBox="0 0 24 24"><path strokeLinecap="round" strokeLinejoin="round" strokeWidth="2" d="M18 2h-3a5 5 0 00-5 5v3H7v4h3v8h4v-8h3l1-4h-4V7a1 1 0 011-1h3z"></path></svg>
                            Facebook
                        </a>
                    )}
                </div>
            </aside>

            {/* Main Content */}
            <main className="flex-1 flex flex-col h-full bg-gray-50 overflow-hidden relative">
                {/* Top Bar */}
                <header className="bg-white px-4 md:px-8 py-4 shadow-sm flex justify-between items-center z-10 shrink-0">
                    <div className="flex items-center gap-3 md:hidden">
                        <button onClick={() => setIsMobileMenuOpen(true)} className="p-2 -ml-2 text-gray-600">
                            <svg className="w-6 h-6" fill="none" stroke="currentColor" viewBox="0 0 24 24"><path strokeLinecap="round" strokeLinejoin="round" strokeWidth="2" d="M4 6h16M4 12h16M4 18h16"></path></svg>
                        </button>
                        <span className="font-bold text-lg">{sucursal.nombre}</span>
                    </div>

                    <div className="hidden md:block text-2xl font-display font-bold text-gray-800">
                        {activeView === 'categories' ? 'Nuestro Menú' :
                            activeView === 'products' ? selectedCategory :
                                activeView === 'info' ? 'Información' : 'Ubicación'}
                    </div>
                </header>

                {/* Scrollable Content */}
                <div className="flex-1 overflow-y-auto p-4 md:p-8 relative scroll-smooth">

                    {/* Categories View */}
                    {activeView === 'categories' && (
                        <div className="animate-fadeIn">
                            {/* Status Banner */}
                            {(() => {
                                // Simple logic to check open/closed status based on current time
                                // Expecting string like "09:00 - 23:00" or similar. Robust parsing needed for real world.
                                // For now, let's assume if we can parse two times, we use them. Defaults to Open if unknown.
                                let isOpen = false;
                                let statusText = "Cerrado Ahora";

                                const now = new Date();
                                const currentHour = now.getHours();
                                const currentMinutes = now.getMinutes();
                                const currentTime = currentHour * 60 + currentMinutes;

                                if (sucursal.horario) {
                                    // Match all time pairs: "12:00 - 15:00", "20:00 - 23:30"
                                    // Pattern: find HH:MM sequences. We expect pairs.
                                    const allTimes = sucursal.horario.match(/(\d{1,2}):(\d{2})/g);

                                    if (allTimes && allTimes.length >= 2) {
                                        // Process in pairs of 2
                                        for (let i = 0; i < allTimes.length; i += 2) {
                                            if (i + 1 >= allTimes.length) break;

                                            const startStr = allTimes[i]!;
                                            const endStr = allTimes[i + 1]!;

                                            const [startHStr, startMStr] = startStr.split(':');
                                            const [endHStr, endMStr] = endStr.split(':');

                                            const startTime = Number(startHStr) * 60 + Number(startMStr);
                                            const endTime = Number(endHStr) * 60 + Number(endMStr);

                                            let isIntervalOpen = false;

                                            if (endTime < startTime) {
                                                // Crosses midnight
                                                isIntervalOpen = currentTime >= startTime || currentTime <= endTime;
                                            } else {
                                                isIntervalOpen = currentTime >= startTime && currentTime <= endTime;
                                            }

                                            if (isIntervalOpen) {
                                                isOpen = true;
                                                break;
                                            }
                                        }
                                    } else {
                                        // Fallback if no valid times found but string exists: assume open? or closed?
                                        // Safer to assume closed but let user know.
                                        // For MVP if no parsable time, maybe just ignore 'isOpen' logic or default to true?
                                        // Let's stick to closed if we can't parse, but statusText might be "Consultar".
                                        // But we initialized isOpen = false.
                                    }
                                }

                                statusText = isOpen ? "Abierto Ahora" : "Cerrado Ahora";

                                return (
                                    <div className={`border-l-4 p-4 rounded-r-lg mb-8 flex items-center gap-4 shadow-sm transition-colors ${isOpen
                                        ? 'bg-green-50 border-green-500 text-green-900'
                                        : 'bg-red-50 border-red-500 text-red-900'
                                        }`}>
                                        <div className={`w-12 h-12 rounded-full flex items-center justify-center shrink-0 ${isOpen ? 'bg-green-100 text-green-600' : 'bg-red-100 text-red-600'
                                            }`}>
                                            {isOpen ? (
                                                <svg className="w-6 h-6" fill="none" stroke="currentColor" viewBox="0 0 24 24"><path strokeLinecap="round" strokeLinejoin="round" strokeWidth="2" d="M12 8v4l3 3m6-3a9 9 0 11-18 0 9 9 0 0118 0z"></path></svg>
                                            ) : (
                                                <svg className="w-6 h-6" fill="none" stroke="currentColor" viewBox="0 0 24 24"><path strokeLinecap="round" strokeLinejoin="round" strokeWidth="2" d="M12 15v2m-6 4h12a2 2 0 002-2v-6a2 2 0 00-2-2H6a2 2 0 00-2 2v6a2 2 0 002 2zm10-10V7a4 4 0 00-8 0v4h8z"></path></svg>
                                            )}
                                        </div>
                                        <div>
                                            <h3 className="font-bold text-lg">{statusText}</h3>
                                            <p className={`text-sm ${isOpen ? 'text-green-700' : 'text-red-700'}`}>
                                                {isOpen
                                                    ? `¡Haz tu pedido! Atendemos: ${sucursal.horario}`
                                                    : `Lo sentimos, estamos cerrados. Horario de atención: ${sucursal.horario}`
                                                }
                                            </p>
                                        </div>
                                    </div>
                                );
                            })()}

                            <h2 className="text-xl font-bold text-gray-800 mb-6 flex items-center gap-2">
                                <span className="w-2 h-8 bg-gray-800 rounded-full inline-block"></span>
                                Categorías
                            </h2>

                            <div className="grid grid-cols-1 sm:grid-cols-2 lg:grid-cols-3 gap-6">
                                {categorias.map(cat => (
                                    <button
                                        key={cat}
                                        onClick={() => handleCategoryClick(cat)}
                                        className="group relative h-40 rounded-xl overflow-hidden shadow-md hover:shadow-xl transition-all hover:-translate-y-1 cursor-pointer bg-gray-800 w-full text-left"
                                    >
                                        <div className="absolute inset-0 bg-gradient-to-t from-black/90 via-black/50 to-transparent z-10"></div>
                                        <div className="absolute inset-0 z-20 flex items-center justify-center p-4">
                                            <h3 className="text-2xl font-display font-bold text-white uppercase tracking-wider text-center drop-shadow-lg group-hover:text-primary-200 transition-colors">
                                                {cat}
                                            </h3>
                                        </div>
                                    </button>
                                ))}
                            </div>
                        </div>
                    )}

                    {/* Products View */}
                    {activeView === 'products' && selectedCategory && (
                        <div className="animate-fadeIn">
                            <div className="mb-6 flex items-center gap-4 sticky top-0 bg-gray-50/95 backdrop-blur z-10 py-4 border-b border-gray-200">
                                <button onClick={handleBackToCategories} className="p-2 hover:bg-gray-200 rounded-full transition-colors text-gray-700">
                                    <svg className="w-6 h-6" fill="none" stroke="currentColor" viewBox="0 0 24 24"><path strokeLinecap="round" strokeLinejoin="round" strokeWidth="2" d="M10 19l-7-7m0 0l7-7m-7 7h18"></path></svg>
                                </button>
                                <h2 className="text-2xl font-bold text-gray-900 uppercase">{selectedCategory}</h2>
                            </div>

                            <div className="grid grid-cols-1 xl:grid-cols-2 gap-4">
                                {productsByCategory[selectedCategory]?.map(p => (
                                    <div key={p.id} className="bg-white p-4 rounded-xl shadow-sm border border-gray-100 flex gap-4 hover:shadow-md transition-all group">
                                        {/* Image */}
                                        <div className="w-24 h-24 sm:w-32 sm:h-32 shrink-0 bg-gray-100 rounded-lg overflow-hidden relative">
                                            {p.urlImagen ? (
                                                <img src={p.urlImagen} className="w-full h-full object-cover group-hover:scale-105 transition-transform duration-500" alt={p.nombre} />
                                            ) : (
                                                <div className="w-full h-full flex items-center justify-center text-gray-300">
                                                    <svg className="w-8 h-8" fill="none" stroke="currentColor" viewBox="0 0 24 24"><path strokeLinecap="round" strokeLinejoin="round" strokeWidth="2" d="M4 16l4.586-4.586a2 2 0 012.828 0L16 16m-2-2l1.586-1.586a2 2 0 012.828 0L20 14m-6-6h.01M6 20h12a2 2 0 002-2V6a2 2 0 00-2-2H6a2 2 0 00-2 2v12a2 2 0 002 2z"></path></svg>
                                                </div>
                                            )}
                                            {p.stock === 0 && (
                                                <div className="absolute inset-0 bg-black/60 flex items-center justify-center">
                                                    <span className="text-white text-xs font-bold uppercase tracking-wider">Agotado</span>
                                                </div>
                                            )}
                                        </div>

                                        {/* Content */}
                                        <div className="flex-1 flex flex-col justify-between">
                                            <div>
                                                <h3 className="font-bold text-gray-900 text-lg leading-tight mb-1">{p.nombre}</h3>
                                                <p className="text-sm text-gray-500 line-clamp-2">{p.descripcion}</p>
                                            </div>

                                            <div className="flex items-center justify-between mt-3">
                                                <div className="flex flex-col">
                                                    <span className="text-xs text-gray-400 uppercase font-semibold">Precio</span>
                                                    <span className="text-xl font-bold text-gray-900">${p.precio.toFixed(2)}</span>
                                                </div>

                                                <button
                                                    onClick={() => handleAddToCart(p)}
                                                    disabled={p.stock === 0}
                                                    className={`w-10 h-10 rounded-full flex items-center justify-center transition-all shadow-sm ${p.stock === 0
                                                        ? 'bg-gray-100 text-gray-400 cursor-not-allowed'
                                                        : 'bg-gray-900 text-white hover:bg-primary-600 hover:scale-110'
                                                        }`}
                                                >
                                                    {p.stock === 0 ? '✕' : '+'}
                                                </button>
                                            </div>
                                        </div>
                                    </div>
                                ))}
                            </div>
                        </div>
                    )}

                    {/* Info View */}
                    {activeView === 'info' && (
                        <div className="animate-fadeIn p-2">
                            <h2 className="text-2xl font-bold mb-6 text-gray-800">Información del Local</h2>
                            <div className="bg-white p-6 rounded-xl shadow-sm border border-gray-100 max-w-2xl">
                                <div className="space-y-6">
                                    <div>
                                        <h3 className="text-sm font-bold text-gray-400 uppercase mb-2">Dirección</h3>
                                        <p className="text-lg text-gray-800">{sucursal.direccion}</p>
                                    </div>
                                    <div>
                                        <h3 className="text-sm font-bold text-gray-400 uppercase mb-2">Teléfono</h3>
                                        <p className="text-lg text-gray-800">{sucursal.telefono}</p>
                                    </div>
                                    <div>
                                        <h3 className="text-sm font-bold text-gray-400 uppercase mb-2">Horarios de Atención</h3>
                                        <p className="text-lg text-gray-800 whitespace-pre-line">{sucursal.horario || 'No especificado'}</p>
                                    </div>
                                </div>
                            </div>
                        </div>
                    )}

                    {/* Location View */}
                    {activeView === 'location' && (
                        <div className="animate-fadeIn p-2">
                            <h2 className="text-2xl font-bold mb-6 text-gray-800">Ubicación</h2>
                            <div className="bg-white p-4 rounded-xl shadow-sm border border-gray-100 h-96 flex items-center justify-center bg-gray-100 text-gray-400">
                                <div className="text-center">
                                    <svg className="w-16 h-16 mx-auto mb-2" fill="none" stroke="currentColor" viewBox="0 0 24 24"><path strokeLinecap="round" strokeLinejoin="round" strokeWidth="2" d="M17.657 16.657L13.414 20.9a1.998 1.998 0 01-2.827 0l-4.244-4.243a8 8 0 1111.314 0z"></path><path strokeLinecap="round" strokeLinejoin="round" strokeWidth="2" d="M15 11a3 3 0 11-6 0 3 3 0 016 0z"></path></svg>
                                    <p>Mapa no disponible por el momento</p>
                                    <p className="text-sm mt-2 text-gray-500">{sucursal.direccion}</p>
                                </div>
                            </div>
                        </div>
                    )}
                </div>
            </main>

            <CartDrawer
                metodosPago={sucursal.metodosPago}
                tiposEntrega={sucursal.tiposEntrega}
                sucursalTelefono={sucursal.telefono}
                sucursalNombre={sucursal.nombre}
                precioDelivery={sucursal.precioDelivery || 0}
            />

            {/* Mobile Sidebar */}
            {isMobileMenuOpen && (
                <div className="fixed inset-0 z-50 md:hidden">
                    <div className="absolute inset-0 bg-black/50 backdrop-blur-sm" onClick={() => setIsMobileMenuOpen(false)}></div>
                    <div className="absolute left-0 top-0 h-full w-64 bg-gray-900 text-white flex flex-col shadow-2xl animate-slide-right">
                        <div className="p-6 flex items-center justify-between border-b border-gray-800">
                            <span className="font-bold text-xl">{sucursal.nombre}</span>
                            <button onClick={() => setIsMobileMenuOpen(false)} className="text-gray-400 hover:text-white">✕</button>
                        </div>
                        <nav className="flex-1 p-4 space-y-2">
                            <NavButton
                                target="categories"
                                label="Inicio"
                                icon={<svg className="w-5 h-5" fill="none" stroke="currentColor" viewBox="0 0 24 24"><path strokeLinecap="round" strokeLinejoin="round" strokeWidth="2" d="M3 12l2-2m0 0l7-7 7 7M5 10v10a1 1 0 001 1h3m10-11l2 2m-2-2v10a1 1 0 01-1 1h-3m-6 0a1 1 0 001-1v-4a1 1 0 011-1h2a1 1 0 011 1v4a1 1 0 001 1m-6 0h6"></path></svg>}
                            />
                            <NavButton
                                target="info"
                                label="Información"
                                icon={<svg className="w-5 h-5" fill="none" stroke="currentColor" viewBox="0 0 24 24"><path strokeLinecap="round" strokeLinejoin="round" strokeWidth="2" d="M13 16h-1v-4h-1m1-4h.01M21 12a9 9 0 11-18 0 9 9 0 0118 0z"></path></svg>}
                            />
                            <NavButton
                                target="location"
                                label="Ubicación"
                                icon={<svg className="w-5 h-5" fill="none" stroke="currentColor" viewBox="0 0 24 24"><path strokeLinecap="round" strokeLinejoin="round" strokeWidth="2" d="M17.657 16.657L13.414 20.9a1.998 1.998 0 01-2.827 0l-4.244-4.243a8 8 0 1111.314 0z"></path><path strokeLinecap="round" strokeLinejoin="round" strokeWidth="2" d="M15 11a3 3 0 11-6 0 3 3 0 016 0z"></path></svg>}
                            />
                        </nav>
                    </div>
                </div>
            )}
        </div>
    );
}
