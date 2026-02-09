import React, { useState } from 'react';
import { toast } from 'sonner';
import {
    obtenerTodosLosNegocios,
    crearNegocio,
    crearSucursal,
    alternarEstadoNegocio,
    obtenerSucursalesPorNegocio,
    alternarEstadoSucursal,
    actualizarNegocio
} from '../../../services/admin';
import type { TenantDto, SucursalDto, CrearNegocioPayload, CrearSucursalPayload } from '../../../services/admin';
import { sucursalService } from '../../../services/sucursal';

interface NegociosManagerProps {
    initialNegocios: TenantDto[];
}

export default function NegociosManager({ initialNegocios }: NegociosManagerProps) {
    const [negocios, setNegocios] = useState<TenantDto[]>(initialNegocios);
    const [loading, setLoading] = useState(false);

    // State for expanded sucursales: map negocioId -> sucursales[]
    const [expandedNegocios, setExpandedNegocios] = useState<Record<number, SucursalDto[]>>({});
    const [loadingSucursales, setLoadingSucursales] = useState<Record<number, boolean>>({});

    // Modals
    const [isNegocioModalOpen, setIsNegocioModalOpen] = useState(false);
    const [isSucursalModalOpen, setIsSucursalModalOpen] = useState({ open: false, negocioId: 0, negocioNombre: '' });

    // Edit Modals
    const [isEditNegocioModalOpen, setIsEditNegocioModalOpen] = useState(false);
    const [editNegocioForm, setEditNegocioForm] = useState({ id: 0, nombre: '', slug: '' });

    const [isEditSucursalModalOpen, setIsEditSucursalModalOpen] = useState(false);
    const [editSucursalForm, setEditSucursalForm] = useState<SucursalDto & { slug: string }>({
        id: 0, negocioId: 0, nombre: '', slug: '', direccion: '', telefono: '', activo: true
    });

    // Forms
    const [negocioForm, setNegocioForm] = useState<CrearNegocioPayload>({
        nombreNegocio: '',
        direccionCentral: '',
        telefono: '',
        nombreDueno: '',
        email: '',
        password: ''
    });

    const [sucursalForm, setSucursalForm] = useState<Partial<CrearSucursalPayload>>({
        nombre: '',
        direccion: '',
        telefono: ''
    });

    // --- Negocio Actions ---
    const refreshNegocios = async () => {
        try {
            const data = await obtenerTodosLosNegocios();
            setNegocios(data);
        } catch (error) {
            console.error(error);
        }
    };

    const handleToggleNegocio = async (id: number) => {
        try {
            await alternarEstadoNegocio(id);
            toast.success('Estado del negocio actualizado');
            // Update local state optmistically or refresh
            setNegocios(prev => prev.map(n => n.id === id ? { ...n, activo: !n.activo } : n));
        } catch (error) {
            toast.error('Error al actualizar estado');
        }
    };

    const handleCreateNegocio = async (e: React.FormEvent) => {
        e.preventDefault();
        setLoading(true);
        try {
            await crearNegocio(negocioForm);
            toast.success('Negocio creado exitosamente');
            setIsNegocioModalOpen(false);
            setNegocioForm({ nombreNegocio: '', direccionCentral: '', telefono: '', nombreDueno: '', email: '', password: '' });
            await refreshNegocios();
        } catch (error) {
            toast.error('Error al crear negocio');
            console.error(error);
        } finally {
            setLoading(false);
        }
    };

    // --- Sucursal Actions ---
    const toggleSucursalesView = async (negocioId: number) => {
        // If already open, close it (remove from state)
        if (expandedNegocios[negocioId]) {
            const newExpanded = { ...expandedNegocios };
            delete newExpanded[negocioId];
            setExpandedNegocios(newExpanded);
            return;
        }

        // Load and open
        setLoadingSucursales(prev => ({ ...prev, [negocioId]: true }));
        try {
            const data = await obtenerSucursalesPorNegocio(negocioId);
            setExpandedNegocios(prev => ({ ...prev, [negocioId]: data }));
        } catch (error) {
            toast.error('Error al cargar sucursales');
        } finally {
            setLoadingSucursales(prev => ({ ...prev, [negocioId]: false }));
        }
    };

    const handleToggleSucursal = async (sucursalId: number, negocioId: number) => {
        try {
            await alternarEstadoSucursal(sucursalId);
            toast.success('Estado de sucursal actualizado');
            // Update local state
            setExpandedNegocios(prev => {
                const currentSucursales = prev[negocioId];
                if (!currentSucursales) return prev;
                return {
                    ...prev,
                    [negocioId]: currentSucursales.map(s => s.id === sucursalId ? { ...s, activo: !s.activo } : s)
                };
            });
        } catch (error) {
            toast.error('Error al actualizar sucursal');
        }
    };

    const handleCreateSucursal = async (e: React.FormEvent) => {
        e.preventDefault();
        if (!sucursalForm.nombre || !sucursalForm.direccion || !sucursalForm.telefono) return;

        setLoading(true);
        try {
            await crearSucursal({
                negocioId: isSucursalModalOpen.negocioId,
                nombre: sucursalForm.nombre!,
                direccion: sucursalForm.direccion!,
                telefono: sucursalForm.telefono!
            });
            toast.success('Sucursal creada exitosamente');
            setIsSucursalModalOpen({ open: false, negocioId: 0, negocioNombre: '' });
            setSucursalForm({ nombre: '', direccion: '', telefono: '' });
            // Refresh sucursales list for this negocio if it's expanded
            if (expandedNegocios[isSucursalModalOpen.negocioId]) {
                const updatedSucursales = await obtenerSucursalesPorNegocio(isSucursalModalOpen.negocioId);
                setExpandedNegocios(prev => ({ ...prev, [isSucursalModalOpen.negocioId]: updatedSucursales }));
            }
            // Also refresh businesses to update counts if necessary
            refreshNegocios();
        } catch (error) {
            toast.error('Error al crear sucursal');
        } finally {
            setLoading(false);
        }
    };

    const prepareEditNegocio = (negocio: TenantDto) => {
        setEditNegocioForm({ id: negocio.id, nombre: negocio.nombre, slug: negocio.slug || '' });
        setIsEditNegocioModalOpen(true);
    };

    const handleUpdateNegocio = async (e: React.FormEvent) => {
        e.preventDefault();
        setLoading(true);
        try {
            await actualizarNegocio(editNegocioForm.id, {
                nombre: editNegocioForm.nombre,
                slug: editNegocioForm.slug
            });
            toast.success('Negocio actualizado exitosamente');
            setIsEditNegocioModalOpen(false);
            await refreshNegocios();
        } catch (error: any) {
            toast.error(error.response?.data?.message || 'Error al actualizar negocio');
            console.error(error);
        } finally {
            setLoading(false);
        }
    };

    const prepareEditSucursal = (sucursal: SucursalDto) => {
        setEditSucursalForm({ ...sucursal });
        setIsEditSucursalModalOpen(true);
    };

    const handleUpdateSucursal = async (e: React.FormEvent) => {
        e.preventDefault();
        setLoading(true);
        try {
            await sucursalService.updateSucursal(editSucursalForm.id, {
                nombre: editSucursalForm.nombre,
                slug: editSucursalForm.slug,
                direccion: editSucursalForm.direccion,
                telefono: editSucursalForm.telefono,
            });
            toast.success('Sucursal actualizada exitosamente');
            setIsEditSucursalModalOpen(false);

            // Refresh expanded list
            if (editSucursalForm.negocioId) {
                const updated = await obtenerSucursalesPorNegocio(editSucursalForm.negocioId);
                setExpandedNegocios(prev => ({ ...prev, [editSucursalForm.negocioId]: updated }));
            }
        } catch (error: any) {
            toast.error(error.response?.data?.message || 'Error al actualizar sucursal');
        } finally {
            setLoading(false);
        }
    };

    return (
        <div className="animate-fadeIn pb-20">
            {/* Header Actions */}
            <div className="flex justify-end mb-6">
                <button
                    onClick={() => setIsNegocioModalOpen(true)}
                    className="px-5 py-2.5 bg-admin-primary text-white rounded-xl shadow-lg shadow-admin-primary/25 hover:bg-admin-primary-dark transition-all flex items-center gap-2 font-semibold"
                >
                    <svg className="w-5 h-5" fill="none" viewBox="0 0 24 24" stroke="currentColor"><path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M12 4v16m8-8H4" /></svg>
                    Nuevo Restaurante
                </button>
            </div>

            {/* Grid */}
            <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-3 xl:grid-cols-4 gap-6">
                {negocios.map((negocio) => (
                    <div key={negocio.id} className="bg-white rounded-2xl border border-admin-border p-5 shadow-sm hover:shadow-md transition-shadow group flex flex-col">
                        {/* Card Header */}
                        <div className="flex justify-between items-start mb-4">
                            <div className="flex items-center gap-3">
                                <div className="w-12 h-12 rounded-xl bg-admin-primary/10 text-admin-primary flex items-center justify-center">
                                    <svg className="w-6 h-6" fill="none" viewBox="0 0 24 24" stroke="currentColor">
                                        <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M19 21V5a2 2 0 00-2-2H7a2 2 0 00-2 2v16m14 0h2m-2 0h-5m-9 0H3m2 0h5M9 7h1m-1 4h1m4-4h1m-1 4h1m-5 10v-5a1 1 0 011-1h2a1 1 0 011 1v5m-4 0h4" />
                                    </svg>
                                </div>
                                <div>
                                    <h3 className="font-bold text-admin-text line-clamp-1">{negocio.nombre}</h3>
                                    <span className={`inline-flex items-center gap-1.5 px-2 py-0.5 rounded-full text-[10px] font-medium uppercase tracking-wide border ${negocio.activo ? 'bg-green-50 text-green-700 border-green-200' : 'bg-red-50 text-red-700 border-red-200'}`}>
                                        <span className={`w-1.5 h-1.5 rounded-full ${negocio.activo ? 'bg-green-500 animate-pulse' : 'bg-red-500'}`} />
                                        {negocio.activo ? 'Activo' : 'Inactivo'}
                                    </span>
                                </div>
                            </div>
                        </div>

                        {/* Info */}
                        <div className="space-y-3 mb-4 text-sm text-admin-text-secondary">
                            <div className="flex items-center gap-2">
                                <svg className="w-4 h-4 text-gray-400" fill="none" viewBox="0 0 24 24" stroke="currentColor"><path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M3 8l7.89 5.26a2 2 0 002.22 0L21 8M5 19h14a2 2 0 002-2V7a2 2 0 00-2-2H5a2 2 0 00-2 2v10a2 2 0 002 2z" /></svg>
                                <span className="truncate">{negocio.dueñoEmail}</span>
                            </div>
                            <div className="flex items-center gap-2">
                                <svg className="w-4 h-4 text-gray-400" fill="none" viewBox="0 0 24 24" stroke="currentColor"><path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M17.657 16.657L13.414 20.9a1.998 1.998 0 01-2.827 0l-4.244-4.243a8 8 0 1111.314 0z" /><path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M15 11a3 3 0 11-6 0 3 3 0 016 0z" /></svg>
                                <span>{negocio.cantidadSucursales} Sucursales</span>
                            </div>
                        </div>

                        {/* Expanded Sucursales Area */}
                        {expandedNegocios[negocio.id] && expandedNegocios[negocio.id]!.length >= 0 && (
                            <div className="mb-4 bg-gray-50 rounded-xl p-3 border border-admin-border animate-fadeIn">
                                <h4 className="text-xs font-bold text-admin-text-secondary uppercase mb-2">Sucursales</h4>
                                <div className="space-y-2 max-h-[200px] overflow-y-auto pr-1 custom-scrollbar">
                                    {expandedNegocios[negocio.id]!.map(sucursal => (
                                        <div key={sucursal.id} className="flex items-center justify-between p-2 bg-white rounded-lg border border-gray-100 shadow-sm">
                                            <div className="min-w-0">
                                                <p className="text-xs font-bold text-admin-text truncate">{sucursal.nombre}</p>
                                                <p className="text-[10px] text-gray-500 truncate">{sucursal.direccion}</p>
                                            </div>
                                            <div className="flex gap-1">
                                                <button
                                                    onClick={() => prepareEditSucursal(sucursal)}
                                                    className="shrink-0 w-6 h-6 flex items-center justify-center rounded transition-colors text-blue-600 bg-blue-50 hover:bg-blue-100"
                                                    title="Editar"
                                                >
                                                    <svg className="w-4 h-4" fill="none" viewBox="0 0 24 24" stroke="currentColor"><path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M15.232 5.232l3.536 3.536m-2.036-5.036a2.5 2.5 0 113.536 3.536L6.5 21.036H3v-3.572L16.732 3.732z" /></svg>
                                                </button>
                                                <button
                                                    onClick={() => handleToggleSucursal(sucursal.id, negocio.id)}
                                                    className={`shrink-0 w-6 h-6 flex items-center justify-center rounded transition-colors ${sucursal.activo ? 'text-green-600 bg-green-50 hover:bg-green-100' : 'text-red-500 bg-red-50 hover:bg-red-100'}`}
                                                    title={sucursal.activo ? 'Desactivar' : 'Activar'}
                                                >
                                                    <svg className="w-4 h-4" fill="none" viewBox="0 0 24 24" stroke="currentColor"><path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M5 13l4 4L19 7" /></svg>
                                                </button>
                                            </div>
                                        </div>
                                    ))}
                                    {expandedNegocios[negocio.id]!.length === 0 && (
                                        <p className="text-xs text-gray-400 text-center py-2">No hay sucursales</p>
                                    )}
                                </div>
                            </div>
                        )}

                        {/* Actions Footer */}
                        <div className="mt-auto pt-4 border-t border-admin-border flex gap-2">
                            <button
                                onClick={() => toggleSucursalesView(negocio.id)}
                                className={`flex-1 px-3 py-2 rounded-lg text-xs font-semibold transition-all flex items-center justify-center gap-1 ${expandedNegocios[negocio.id]
                                    ? 'bg-admin-text text-white shadow-lg'
                                    : 'bg-white border border-admin-border text-admin-text hover:bg-gray-50'
                                    }`}
                            >
                                {loadingSucursales[negocio.id] ? (
                                    <span className="w-4 h-4 border-2 border-current border-t-transparent rounded-full animate-spin" />
                                ) : (
                                    <svg className={`w-4 h-4 transition-transform ${expandedNegocios[negocio.id] ? 'rotate-180' : ''}`} fill="none" viewBox="0 0 24 24" stroke="currentColor"><path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M19 9l-7 7-7-7" /></svg>
                                )}
                                {expandedNegocios[negocio.id] ? 'Ocultar' : 'Ver Sucursales'}
                            </button>

                            <button
                                onClick={() => prepareEditNegocio(negocio)}
                                className="w-9 h-9 flex items-center justify-center rounded-lg bg-blue-50 text-blue-600 hover:bg-blue-100 transition-colors"
                                title="Editar Negocio"
                            >
                                <svg className="w-5 h-5" fill="none" viewBox="0 0 24 24" stroke="currentColor"><path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M15.232 5.232l3.536 3.536m-2.036-5.036a2.5 2.5 0 113.536 3.536L6.5 21.036H3v-3.572L16.732 3.732z" /></svg>
                            </button>
                            <button
                                onClick={() => handleToggleNegocio(negocio.id)}
                                className={`w-9 h-9 flex items-center justify-center rounded-lg transition-colors ${negocio.activo ? 'bg-red-50 text-red-600 hover:bg-red-100' : 'bg-green-50 text-green-600 hover:bg-green-100'}`}
                                title={negocio.activo ? 'Desactivar Negocio' : 'Activar Negocio'}
                            >
                                <svg className="w-5 h-5" fill="none" viewBox="0 0 24 24" stroke="currentColor"><path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M18.364 18.364A9 9 0 005.636 5.636m12.728 12.728A9 9 0 015.636 5.636m12.728 12.728L5.636 5.636" /></svg>
                            </button>

                            <button
                                onClick={() => setIsSucursalModalOpen({ open: true, negocioId: negocio.id, negocioNombre: negocio.nombre })}
                                className="w-9 h-9 flex items-center justify-center rounded-lg bg-admin-primary/10 text-admin-primary hover:bg-admin-primary/20 transition-colors"
                                title="Agregar Sucursal"
                            >
                                <svg className="w-5 h-5" fill="none" viewBox="0 0 24 24" stroke="currentColor"><path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M12 4v16m8-8H4" /></svg>
                            </button>
                        </div>
                    </div>
                ))}

                {negocios.length === 0 && (
                    <div className="col-span-full py-20 text-center text-admin-text-secondary">
                        <div className="inline-flex w-20 h-20 bg-gray-100 rounded-full items-center justify-center mb-4">
                            <svg className="w-10 h-10 text-gray-300" fill="none" viewBox="0 0 24 24" stroke="currentColor"><path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M19 21V5a2 2 0 00-2-2H7a2 2 0 00-2 2v16m14 0h2m-2 0h-5m-9 0H3m2 0h5M9 7h1m-1 4h1m4-4h1m-1 4h1m-5 10v-5a1 1 0 011-1h2a1 1 0 011 1v5m-4 0h4" /></svg>
                        </div>
                        <p className="text-lg font-medium">No hay restaurantes registrados.</p>
                        <p>Comienza creando uno nuevo con el botón superior.</p>
                    </div>
                )}
            </div>

            {/* Modal Negocio */}
            {isNegocioModalOpen && (
                <div className="fixed inset-0 z-50 flex items-center justify-center p-4 bg-black/60 backdrop-blur-sm animate-fadeIn">
                    <div className="bg-white rounded-2xl w-full max-w-lg shadow-2xl overflow-hidden">
                        <div className="p-5 border-b border-admin-border flex justify-between items-center bg-gray-50/50">
                            <h3 className="font-bold text-xl text-admin-text">Nuevo Restaurante</h3>
                            <button onClick={() => setIsNegocioModalOpen(false)} className="text-gray-400 hover:text-admin-text transition-colors">
                                <svg className="w-6 h-6" fill="none" viewBox="0 0 24 24" stroke="currentColor"><path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M6 18L18 6M6 6l12 12" /></svg>
                            </button>
                        </div>
                        <form onSubmit={handleCreateNegocio} className="p-6 flex flex-col gap-4">
                            <div>
                                <label className="text-xs font-bold text-admin-text-secondary uppercase mb-1 block">Nombre del Negocio</label>
                                <input required type="text" className="w-full px-4 py-2 bg-white border border-admin-border rounded-xl text-sm outline-none focus:ring-2 focus:ring-admin-primary/20 focus:border-admin-primary transition-all"
                                    value={negocioForm.nombreNegocio} onChange={e => setNegocioForm({ ...negocioForm, nombreNegocio: e.target.value })} placeholder="Ej: Pizza House" />
                            </div>
                            <div className="grid grid-cols-2 gap-4">
                                <div>
                                    <label className="text-xs font-bold text-admin-text-secondary uppercase mb-1 block">Teléfono</label>
                                    <input required type="tel" className="w-full px-4 py-2 bg-white border border-admin-border rounded-xl text-sm outline-none focus:ring-2 focus:ring-admin-primary/20 focus:border-admin-primary transition-all"
                                        value={negocioForm.telefono} onChange={e => setNegocioForm({ ...negocioForm, telefono: e.target.value })} placeholder="+56 9..." />
                                </div>
                                <div>
                                    <label className="text-xs font-bold text-admin-text-secondary uppercase mb-1 block">Dirección Central</label>
                                    <input required type="text" className="w-full px-4 py-2 bg-white border border-admin-border rounded-xl text-sm outline-none focus:ring-2 focus:ring-admin-primary/20 focus:border-admin-primary transition-all"
                                        value={negocioForm.direccionCentral} onChange={e => setNegocioForm({ ...negocioForm, direccionCentral: e.target.value })} />
                                </div>
                            </div>
                            <div className="border-t border-admin-border my-2"></div>
                            <div>
                                <label className="text-xs font-bold text-admin-text-secondary uppercase mb-1 block">Nombre Dueño</label>
                                <input required type="text" className="w-full px-4 py-2 bg-white border border-admin-border rounded-xl text-sm outline-none focus:ring-2 focus:ring-admin-primary/20 focus:border-admin-primary transition-all"
                                    value={negocioForm.nombreDueno} onChange={e => setNegocioForm({ ...negocioForm, nombreDueno: e.target.value })} />
                            </div>
                            <div>
                                <label className="text-xs font-bold text-admin-text-secondary uppercase mb-1 block">Email Admin</label>
                                <input required type="email" className="w-full px-4 py-2 bg-white border border-admin-border rounded-xl text-sm outline-none focus:ring-2 focus:ring-admin-primary/20 focus:border-admin-primary transition-all"
                                    value={negocioForm.email} onChange={e => setNegocioForm({ ...negocioForm, email: e.target.value })} placeholder="admin@ejemplo.com" />
                            </div>
                            <div>
                                <label className="text-xs font-bold text-admin-text-secondary uppercase mb-1 block">Contraseña</label>
                                <input required type="password" className="w-full px-4 py-2 bg-white border border-admin-border rounded-xl text-sm outline-none focus:ring-2 focus:ring-admin-primary/20 focus:border-admin-primary transition-all"
                                    value={negocioForm.password} onChange={e => setNegocioForm({ ...negocioForm, password: e.target.value })} placeholder="******" />
                            </div>
                            <div className="flex gap-3 mt-4">
                                <button type="button" onClick={() => setIsNegocioModalOpen(false)} className="flex-1 px-4 py-2.5 rounded-xl border border-admin-border text-admin-text hover:bg-gray-50 font-medium transition-colors">Cancelar</button>
                                <button type="submit" disabled={loading} className="flex-1 px-4 py-2.5 rounded-xl bg-admin-primary text-white font-bold shadow-lg shadow-admin-primary/20 hover:bg-admin-primary-dark transition-colors disabled:opacity-50">
                                    {loading ? 'Creando...' : 'Crear Restaurante'}
                                </button>
                            </div>
                        </form>
                    </div>
                </div>
            )}

            {/* Modal Sucursal */}
            {isSucursalModalOpen.open && (
                <div className="fixed inset-0 z-50 flex items-center justify-center p-4 bg-black/60 backdrop-blur-sm animate-fadeIn">
                    <div className="bg-white rounded-2xl w-full max-w-md shadow-2xl overflow-hidden">
                        <div className="p-5 border-b border-admin-border flex justify-between items-center bg-gray-50/50">
                            <div>
                                <h3 className="font-bold text-xl text-admin-text">Nueva Sucursal</h3>
                                <p className="text-xs text-admin-text-secondary">{isSucursalModalOpen.negocioNombre}</p>
                            </div>
                            <button onClick={() => setIsSucursalModalOpen({ ...isSucursalModalOpen, open: false })} className="text-gray-400 hover:text-admin-text transition-colors">
                                <svg className="w-6 h-6" fill="none" viewBox="0 0 24 24" stroke="currentColor"><path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M6 18L18 6M6 6l12 12" /></svg>
                            </button>
                        </div>
                        <form onSubmit={handleCreateSucursal} className="p-6 flex flex-col gap-4">
                            <div>
                                <label className="text-xs font-bold text-admin-text-secondary uppercase mb-1 block">Nombre Sucursal</label>
                                <input required type="text" className="w-full px-4 py-2 bg-white border border-admin-border rounded-xl text-sm outline-none focus:ring-2 focus:ring-admin-primary/20 focus:border-admin-primary transition-all"
                                    value={sucursalForm.nombre} onChange={e => setSucursalForm({ ...sucursalForm, nombre: e.target.value })} placeholder="Ej: Centro" />
                            </div>
                            <div>
                                <label className="text-xs font-bold text-admin-text-secondary uppercase mb-1 block">Dirección</label>
                                <input required type="text" className="w-full px-4 py-2 bg-white border border-admin-border rounded-xl text-sm outline-none focus:ring-2 focus:ring-admin-primary/20 focus:border-admin-primary transition-all"
                                    value={sucursalForm.direccion} onChange={e => setSucursalForm({ ...sucursalForm, direccion: e.target.value })} placeholder="Calle..." />
                            </div>
                            <div>
                                <label className="text-xs font-bold text-admin-text-secondary uppercase mb-1 block">Teléfono</label>
                                <input required type="tel" className="w-full px-4 py-2 bg-white border border-admin-border rounded-xl text-sm outline-none focus:ring-2 focus:ring-admin-primary/20 focus:border-admin-primary transition-all"
                                    value={sucursalForm.telefono} onChange={e => setSucursalForm({ ...sucursalForm, telefono: e.target.value })} placeholder="123456" />
                            </div>
                            <div className="flex gap-3 mt-4">
                                <button type="button" onClick={() => setIsSucursalModalOpen({ ...isSucursalModalOpen, open: false })} className="flex-1 px-4 py-2.5 rounded-xl border border-admin-border text-admin-text hover:bg-gray-50 font-medium transition-colors">Cancelar</button>
                                <button type="submit" disabled={loading} className="flex-1 px-4 py-2.5 rounded-xl bg-admin-primary text-white font-bold shadow-lg shadow-admin-primary/20 hover:bg-admin-primary-dark transition-colors disabled:opacity-50">
                                    {loading ? 'Creando...' : 'Crear Sucursal'}
                                </button>
                            </div>
                        </form>
                    </div>
                </div>
            )}

            {/* Modal Editar Negocio */}
            {isEditNegocioModalOpen && (
                <div className="fixed inset-0 z-50 flex items-center justify-center p-4 bg-black/60 backdrop-blur-sm animate-fadeIn">
                    <div className="bg-white rounded-2xl w-full max-w-lg shadow-2xl overflow-hidden">
                        <div className="p-5 border-b border-admin-border flex justify-between items-center bg-gray-50/50">
                            <h3 className="font-bold text-xl text-admin-text">Editar Negocio</h3>
                            <button onClick={() => setIsEditNegocioModalOpen(false)} className="text-gray-400 hover:text-admin-text transition-colors">
                                <svg className="w-6 h-6" fill="none" viewBox="0 0 24 24" stroke="currentColor"><path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M6 18L18 6M6 6l12 12" /></svg>
                            </button>
                        </div>
                        <form onSubmit={handleUpdateNegocio} className="p-6 flex flex-col gap-4">
                            <div>
                                <label className="text-xs font-bold text-admin-text-secondary uppercase mb-1 block">Nombre</label>
                                <input required type="text" className="w-full px-4 py-2 bg-white border border-admin-border rounded-xl text-sm outline-none focus:ring-2 focus:ring-admin-primary/20 focus:border-admin-primary transition-all"
                                    value={editNegocioForm.nombre} onChange={e => setEditNegocioForm({ ...editNegocioForm, nombre: e.target.value })} />
                            </div>
                            <div>
                                <label className="text-xs font-bold text-admin-text-secondary uppercase mb-1 block">Slug (URL)</label>
                                <input required type="text" className="w-full px-4 py-2 bg-white border border-admin-border rounded-xl text-sm outline-none focus:ring-2 focus:ring-admin-primary/20 focus:border-admin-primary transition-all font-mono text-xs"
                                    value={editNegocioForm.slug} onChange={e => setEditNegocioForm({ ...editNegocioForm, slug: e.target.value })} />
                                <p className="text-[10px] text-amber-600 mt-1">⚠ Cambiar el slug romperá los códigos QR existentes.</p>
                            </div>
                            <div className="flex gap-3 mt-4">
                                <button type="button" onClick={() => setIsEditNegocioModalOpen(false)} className="flex-1 px-4 py-2.5 rounded-xl border border-admin-border text-admin-text hover:bg-gray-50 font-medium transition-colors">Cancelar</button>
                                <button type="submit" disabled={loading} className="flex-1 px-4 py-2.5 rounded-xl bg-admin-primary text-white font-bold shadow-lg shadow-admin-primary/20 hover:bg-admin-primary-dark transition-colors disabled:opacity-50">
                                    {loading ? 'Guardando...' : 'Guardar Cambios'}
                                </button>
                            </div>
                        </form>
                    </div>
                </div>
            )}

            {/* Modal Editar Sucursal */}
            {isEditSucursalModalOpen && (
                <div className="fixed inset-0 z-50 flex items-center justify-center p-4 bg-black/60 backdrop-blur-sm animate-fadeIn">
                    <div className="bg-white rounded-2xl w-full max-w-lg shadow-2xl overflow-hidden">
                        <div className="p-5 border-b border-admin-border flex justify-between items-center bg-gray-50/50">
                            <h3 className="font-bold text-xl text-admin-text">Editar Sucursal</h3>
                            <button onClick={() => setIsEditSucursalModalOpen(false)} className="text-gray-400 hover:text-admin-text transition-colors">
                                <svg className="w-6 h-6" fill="none" viewBox="0 0 24 24" stroke="currentColor"><path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M6 18L18 6M6 6l12 12" /></svg>
                            </button>
                        </div>
                        <form onSubmit={handleUpdateSucursal} className="p-6 flex flex-col gap-4">
                            <div>
                                <label className="text-xs font-bold text-admin-text-secondary uppercase mb-1 block">Nombre</label>
                                <input required type="text" className="w-full px-4 py-2 bg-white border border-admin-border rounded-xl text-sm outline-none focus:ring-2 focus:ring-admin-primary/20 focus:border-admin-primary transition-all"
                                    value={editSucursalForm.nombre} onChange={e => setEditSucursalForm({ ...editSucursalForm, nombre: e.target.value })} />
                            </div>
                            <div>
                                <label className="text-xs font-bold text-admin-text-secondary uppercase mb-1 block">Slug (URL)</label>
                                <input required type="text" className="w-full px-4 py-2 bg-white border border-admin-border rounded-xl text-sm outline-none focus:ring-2 focus:ring-admin-primary/20 focus:border-admin-primary transition-all font-mono text-xs"
                                    value={editSucursalForm.slug} onChange={e => setEditSucursalForm({ ...editSucursalForm, slug: e.target.value })} />
                                <p className="text-[10px] text-amber-600 mt-1">⚠ Cambiar el slug romperá los códigos QR existentes.</p>
                            </div>
                            <div className="grid grid-cols-2 gap-4">
                                <div>
                                    <label className="text-xs font-bold text-admin-text-secondary uppercase mb-1 block">Dirección</label>
                                    <input required type="text" className="w-full px-4 py-2 bg-white border border-admin-border rounded-xl text-sm outline-none focus:ring-2 focus:ring-admin-primary/20 focus:border-admin-primary transition-all"
                                        value={editSucursalForm.direccion} onChange={e => setEditSucursalForm({ ...editSucursalForm, direccion: e.target.value })} />
                                </div>
                                <div>
                                    <label className="text-xs font-bold text-admin-text-secondary uppercase mb-1 block">Teléfono</label>
                                    <input required type="text" className="w-full px-4 py-2 bg-white border border-admin-border rounded-xl text-sm outline-none focus:ring-2 focus:ring-admin-primary/20 focus:border-admin-primary transition-all"
                                        value={editSucursalForm.telefono} onChange={e => setEditSucursalForm({ ...editSucursalForm, telefono: e.target.value })} />
                                </div>
                            </div>
                            <div className="flex gap-3 mt-4">
                                <button type="button" onClick={() => setIsEditSucursalModalOpen(false)} className="flex-1 px-4 py-2.5 rounded-xl border border-admin-border text-admin-text hover:bg-gray-50 font-medium transition-colors">Cancelar</button>
                                <button type="submit" disabled={loading} className="flex-1 px-4 py-2.5 rounded-xl bg-admin-primary text-white font-bold shadow-lg shadow-admin-primary/20 hover:bg-admin-primary-dark transition-colors disabled:opacity-50">
                                    {loading ? 'Guardando...' : 'Guardar Cambios'}
                                </button>
                            </div>
                        </form>
                    </div>
                </div>
            )}
        </div>
    );
}
