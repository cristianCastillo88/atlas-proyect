import React, { useState, useEffect } from 'react';
import { sucursalService } from '../../../services/sucursal';
import { logout } from '../../../stores/auth';
import PosTerminal from './PosTerminal';
import ProductManager from './ProductManager';
import StaffManager from './StaffManager';
import KitchenDisplay from './KitchenDisplay';
import StoreConfigForm from './StoreConfigForm';
import QRCodeSection from '../sucursales/QRCodeSection';
import { Toaster } from 'sonner';
import { signalRService } from '../../../services/signalr';

interface SucursalAdminViewProps {
    sucursalId: number;
}

export default function SucursalAdminView({ sucursalId }: SucursalAdminViewProps) {
    const [activeTab, setActiveTab] = useState('nuevo-pedido');
    const [loading, setLoading] = useState(true);
    const [error, setError] = useState('');

    // Data State
    const [sucursal, setSucursal] = useState<any>(null);
    const [productos, setProductos] = useState<any[]>([]);
    const [categorias, setCategorias] = useState<any[]>([]);
    const [empleados, setEmpleados] = useState<any[]>([]);
    const [pedidos, setPedidos] = useState<any[]>([]);
    const [metodosPago, setMetodosPago] = useState<any[]>([]);
    const [tiposEntrega, setTiposEntrega] = useState<any[]>([]);

    const [role, setRole] = useState<number | string>(0);

    useEffect(() => {
        loadData();
        checkRole();
    }, [sucursalId]);

    const checkRole = () => {
        const userData = sessionStorage.getItem("userStore") || localStorage.getItem("userStore");
        if (userData) {
            try {
                const user = JSON.parse(userData);
                const userObj = user.state?.user || user;
                const rawRole = userObj.role || userObj.rol || userObj.Role;
                setRole(rawRole);
            } catch (e) {
                console.error(e);
            }
        }
    };

    // ============ SIGNALR INTEGRATION ============
    useEffect(() => {
        // Obtenemos token del store para conectar
        const token = localStorage.getItem('token') || (sessionStorage.getItem('userStore') ? JSON.parse(sessionStorage.getItem('userStore')!).state?.token : null);

        // Mejor intento de obtener token (la lógica Auth varía en el proyecto parece)
        // El servicio signalRService se encarga de la lógica de conexión
        if (token) {
            signalRService.connect(sucursalId, token);
        }

        signalRService.onNuevoPedido((pedido) => {
            // Actualizar lista de pedidos si estamos en tab pedidos
            // Add to state
            setPedidos((prev) => [pedido, ...prev]);
        });

        return () => {
            signalRService.disconnect();
        };
    }, [sucursalId]);

    const loadData = async () => {
        try {
            setLoading(true);

            // 1. Basic Public/Shared Data (Sucursal, Products, Metados)
            const [
                sucData,
                prodData,
                catData,
                pagoData,
                entregaData
            ] = await Promise.all([
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

            // 2. Transaccional Data (Pedidos) - usually allowed for authorized staff
            try {
                const pedData = await sucursalService.getPedidosPorSucursal(sucursalId);
                setPedidos(pedData);
            } catch (e) {
                console.warn("Failed to load Orders", e);
            }

            // 3. Admin-Only Data (Empleados)
            // If the user is just an 'Empleado', this endpoint might return 403 Forbidden.
            // We should treat this failure gracefully.
            try {
                const empData = await sucursalService.getEmpleadosPorSucursal(sucursalId);
                setEmpleados(empData);
            } catch (e) {
                console.warn("Failed to load Staff (insufficient permissions?)", e);
                // Don't fail the whole view, just set empty staff list
                setEmpleados([]);
            }

        } catch (err: any) {
            console.error('Error loading sucursal data', err);
            // Critical error only if basic data fails
            setError('Error al cargar datos del negocio. Verifique su conexión.');
        } finally {
            setLoading(false);
        }
    };

    // Determine sensitive tabs visibility
    const isEmpleado = role === 'Empleado' || role === 3;

    if (loading) {
        return (
            <div className="flex items-center justify-center h-full">
                <div className="animate-spin rounded-full h-12 w-12 border-b-2 border-admin-primary"></div>
            </div>
        );
    }

    if (error) {
        return (
            <div className="flex flex-col items-center justify-center h-full gap-4 text-center p-6">
                <div className="text-red-500 text-xl font-bold">Error</div>
                <p className="text-gray-600">{error}</p>
                <button onClick={() => window.location.reload()} className="px-4 py-2 bg-admin-primary text-white rounded-lg">Reintentar</button>
            </div>
        );
    }

    return (
        <div className="flex h-screen bg-gray-50 overflow-hidden">
            {/* Sidebar Navigation */}
            <aside className="w-64 bg-admin-sidebar text-white flex flex-col shadow-xl z-20">
                <div className="p-6 flex items-center gap-3 border-b border-white/10">
                    <div className="w-10 h-10 bg-admin-primary rounded-lg flex items-center justify-center font-bold text-xl shadow-lg">
                        {sucursal?.nombre?.substring(0, 1) || 'S'}
                    </div>
                    <h1 className="font-bold text-lg leading-tight truncate">
                        {sucursal?.nombre || 'Sucursal'}
                    </h1>
                </div>

                <nav className="flex-1 py-6 px-3 space-y-1 overflow-y-auto">
                    <NavButton
                        active={activeTab === 'nuevo-pedido'}
                        onClick={() => setActiveTab('nuevo-pedido')}
                        label="Nuevo Pedido"
                        icon={<svg className="w-5 h-5" fill="none" viewBox="0 0 24 24" stroke="currentColor"><path strokeLinecap="round" strokeLinejoin="round" strokeWidth="2" d="M12 4v16m8-8H4"></path></svg>}
                    />
                    <NavButton
                        active={activeTab === 'carta'}
                        onClick={() => setActiveTab('carta')}
                        label="Carta"
                        icon={<svg className="w-5 h-5" fill="none" viewBox="0 0 24 24" stroke="currentColor"><path strokeLinecap="round" strokeLinejoin="round" strokeWidth="2" d="M9 5H7a2 2 0 00-2 2v12a2 2 0 002 2h10a2 2 0 002-2V7a2 2 0 00-2-2h-2M9 5a2 2 0 002 2h2a2 2 0 002-2M9 5a2 2 0 012-2h2a2 2 0 012 2"></path></svg>}
                    />
                    {!isEmpleado && (
                        <NavButton
                            active={activeTab === 'personal'}
                            onClick={() => setActiveTab('personal')}
                            label="Personal"
                            icon={<svg className="w-5 h-5" fill="none" viewBox="0 0 24 24" stroke="currentColor"><path strokeLinecap="round" strokeLinejoin="round" strokeWidth="2" d="M12 4.354a4 4 0 110 5.292M15 21H3v-1a6 6 0 0112 0v1zm0 0h6v-1a6 6 0 00-9-5.197M13 7a4 4 0 11-8 0 4 4 0 018 0z"></path></svg>}
                        />
                    )}
                    <NavButton
                        active={activeTab === 'pedidos'}
                        onClick={() => setActiveTab('pedidos')}
                        label="Pedidos"
                        icon={<svg className="w-5 h-5" fill="none" viewBox="0 0 24 24" stroke="currentColor"><path strokeLinecap="round" strokeLinejoin="round" strokeWidth="2" d="M9 5H7a2 2 0 00-2 2v12a2 2 0 002 2h10a2 2 0 002-2V7a2 2 0 00-2-2h-2M9 5a2 2 0 002 2h2a2 2 0 002-2M9 5a2 2 0 012-2h2a2 2 0 012 2m-3 7h3m-3 4h3m-6-4h.01M9 16h.01"></path></svg>}
                    />
                    {!isEmpleado && (
                        <NavButton
                            active={activeTab === 'configuracion'}
                            onClick={() => setActiveTab('configuracion')}
                            label="Configuración"
                            icon={<svg className="w-5 h-5" fill="none" viewBox="0 0 24 24" stroke="currentColor"><path strokeLinecap="round" strokeLinejoin="round" strokeWidth="2" d="M10.325 4.317c.426-1.756 2.924-1.756 3.35 0a1.724 1.724 0 002.573 1.066c1.543-.94 3.31.826 2.37 2.37a1.724 1.724 0 001.065 2.572c1.756.426 1.756 2.924 0 3.35a1.724 1.724 0 00-1.066 2.573c.94 1.543-.826 3.31-2.37 2.37a1.724 1.724 0 00-2.572 1.065c-.426 1.756-2.924 1.756-3.35 0a1.724 1.724 0 00-2.573-1.066c-1.543.94-3.31-.826-2.37-2.37a1.724 1.724 0 00-1.065-2.572c-1.756-.426-1.756-2.924 0-3.35a1.724 1.724 0 001.066-2.573c-.94-1.543.826-3.31 2.37-2.37.996.608 2.296.07 2.572-1.065z"></path><path strokeLinecap="round" strokeLinejoin="round" strokeWidth="2" d="M15 12a3 3 0 11-6 0 3 3 0 016 0z"></path></svg>}
                        />
                    )}
                    {!isEmpleado && (
                        <NavButton
                            active={activeTab === 'qr-code'}
                            onClick={() => setActiveTab('qr-code')}
                            label="Código QR"
                            icon={<svg className="w-5 h-5" fill="none" viewBox="0 0 24 24" stroke="currentColor"><path strokeLinecap="round" strokeLinejoin="round" strokeWidth="2" d="M12 4v1m6 11h2m-6 0h-2v4m0-11v3m0 0h.01M12 12h4.01M16 20h4M4 12h4m12 0h.01M5 8h2a1 1 0 001-1V5a1 1 0 00-1-1H5a1 1 0 00-1 1v2a1 1 0 001 1zm12 0h2a1 1 0 001-1V5a1 1 0 00-1-1h-2a1 1 0 00-1 1v2a1 1 0 001 1zM5 20h2a1 1 0 001-1v-2a1 1 0 00-1-1H5a1 1 0 00-1 1v2a1 1 0 001 1z"></path></svg>}
                        />
                    )}

                </nav>

                <div className="p-4 border-t border-white/10">
                    <button
                        onClick={() => {
                            logout();
                            window.location.href = '/login';
                        }}
                        className="w-full flex items-center gap-3 px-4 py-3 rounded-xl text-left transition-all text-red-300 hover:bg-red-500/10 hover:text-red-200 font-medium group"
                    >
                        <svg className="w-5 h-5" fill="none" viewBox="0 0 24 24" stroke="currentColor">
                            <path strokeLinecap="round" strokeLinejoin="round" strokeWidth="2" d="M17 16l4-4m0 0l-4-4m4 4H7m6 4v1a3 3 0 01-3 3H6a3 3 0 01-3-3V7a3 3 0 013-3h4a3 3 0 013 3v1"></path>
                        </svg>
                        <span>Cerrar Sesión</span>
                    </button>
                    <div className="mt-2">
                        <a href="/admin/negocios" className="w-full flex items-center gap-3 px-4 py-3 rounded-xl text-left transition-all text-white/70 hover:bg-white/5 hover:text-white font-medium group text-sm">
                            <svg className="w-5 h-5" fill="none" viewBox="0 0 24 24" stroke="currentColor"><path strokeLinecap="round" strokeLinejoin="round" strokeWidth="2" d="M10 19l-7-7m0 0l7-7m-7 7h18"></path></svg>
                            <span>Volver</span>
                        </a>
                    </div>
                </div>
            </aside>

            {/* Content Area */}
            <div className="flex-1 flex flex-col h-full overflow-hidden">
                {/* Header */}
                <header className="bg-white border-b border-gray-200 h-16 flex items-center justify-between px-6 shrink-0 z-10">
                    <h2 className="text-xl font-bold text-admin-text">
                        {activeTab === 'nuevo-pedido' && 'Nuevo Pedido'}
                        {activeTab === 'carta' && 'Gestión de Carta'}
                        {activeTab === 'personal' && 'Gestión de Personal'}
                        {activeTab === 'pedidos' && 'Pedidos en Cocina'}
                        {activeTab === 'configuracion' && ' Configuración'}
                        {activeTab === 'qr-code' && 'Código QR'}
                    </h2>
                    <div className="flex items-center gap-3">
                        <span className={`px-3 py-1 rounded-full text-sm font-medium ${sucursal?.activo ? "bg-green-100 text-green-700" : "bg-red-100 text-red-700"}`}>
                            {sucursal?.activo ? "Operativo" : "Inactivo"}
                        </span>
                    </div>
                </header>

                {/* Tab Panels */}
                <div className="flex-1 overflow-hidden relative">
                    {activeTab === 'nuevo-pedido' && (
                        <PosTerminal
                            sucursalId={sucursalId}
                            initialProductos={productos}
                            categorias={categorias}
                            metodosPago={metodosPago}
                            tiposEntrega={tiposEntrega}
                            precioDelivery={sucursal?.precioDelivery || 0}
                        />
                    )}
                    {activeTab === 'carta' && (
                        <ProductManager
                            sucursalId={sucursalId}
                            initialProductos={productos}
                            categorias={categorias}
                        />
                    )}
                    {activeTab === 'personal' && !isEmpleado && (
                        <StaffManager
                            sucursalId={sucursalId}
                            initialEmpleados={empleados}
                        />
                    )}
                    {activeTab === 'pedidos' && (
                        <KitchenDisplay
                            sucursalId={sucursalId}
                            initialPedidos={pedidos}
                            sucursalConfig={sucursal}
                        />
                    )}
                    {activeTab === 'configuracion' && !isEmpleado && (
                        <div className="h-full overflow-y-auto p-6 md:p-8">
                            <StoreConfigForm
                                sucursalId={sucursalId}
                                initialData={sucursal}
                            />
                        </div>
                    )}
                    {activeTab === 'qr-code' && !isEmpleado && (
                        <div className="h-full overflow-y-auto p-6 md:p-8">
                            <QRCodeSection
                                sucursalId={sucursalId}
                                sucursalNombre={sucursal?.nombre || 'Sucursal'}
                                sucursalSlug={sucursal?.slug || ''}
                            />
                        </div>
                    )}
                </div>
            </div>
            <Toaster />
        </div>
    );
}

function NavButton({ active, onClick, label, icon }: any) {
    return (
        <button
            onClick={onClick}
            className={`w-full flex items-center gap-3 px-4 py-3 rounded-xl text-left transition-all mb-1 font-medium group ${active
                ? 'bg-admin-primary text-white shadow-lg shadow-admin-primary/25'
                : 'text-white/70 hover:bg-white/5 hover:text-white'
                }`}
        >
            <div className={`transition-transform duration-300 ${active ? 'scale-110' : 'group-hover:scale-110'}`}>
                {icon}
            </div>
            <span>{label}</span>
        </button>
    );
}
