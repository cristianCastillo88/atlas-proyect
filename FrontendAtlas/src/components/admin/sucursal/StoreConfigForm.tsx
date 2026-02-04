import React, { useState } from 'react';
import { toast } from 'sonner';
import type { Sucursal } from '../../../types/db';
import { sucursalService } from '../../../services/sucursal';

interface StoreConfigFormProps {
    sucursalId: number;
    initialData: Sucursal;
}

export default function StoreConfigForm({ sucursalId, initialData }: StoreConfigFormProps) {
    const [formData, setFormData] = useState<Partial<Sucursal>>({
        nombre: initialData.nombre,
        direccion: initialData.direccion,
        telefono: initialData.telefono,
        horario: initialData.horario || '',
        urlInstagram: initialData.urlInstagram || '',
        urlFacebook: initialData.urlFacebook || '',
        precioDelivery: initialData.precioDelivery || 0
    });
    const [loading, setLoading] = useState(false);

    const handleSubmit = async (e: React.FormEvent) => {
        e.preventDefault();
        setLoading(true);
        try {
            await sucursalService.updateSucursal(sucursalId, formData);
            toast.success('Configuración actualizada');
        } catch (error) {
            console.error(error);
            toast.error('Error al actualizar configuración');
        } finally {
            setLoading(false);
        }
    };

    return (
        <div className="max-w-3xl mx-auto space-y-6 animate-fadeIn">
            <div className="bg-white p-6 rounded-2xl border border-admin-border shadow-sm">
                <h2 className="text-xl font-bold text-admin-text mb-1">Configuración de Sucursal</h2>
                <p className="text-sm text-admin-text-secondary mb-6">Administra la información básica de tu negocio.</p>

                <form onSubmit={handleSubmit} className="flex flex-col gap-6">
                    <div className="grid grid-cols-1 md:grid-cols-2 gap-6">
                        <div className="space-y-2">
                            <label className="text-xs font-bold text-admin-text-secondary uppercase tracking-wider">Nombre del Local</label>
                            <input
                                required
                                type="text"
                                className="w-full px-4 py-3 bg-admin-surface border border-admin-border rounded-xl text-sm outline-none focus:border-admin-primary focus:ring-4 focus:ring-admin-primary/10 transition-all font-medium text-admin-text"
                                value={formData.nombre}
                                onChange={e => setFormData({ ...formData, nombre: e.target.value })}
                            />
                        </div>

                        <div className="space-y-2">
                            <label className="text-xs font-bold text-admin-text-secondary uppercase tracking-wider">Teléfono de Contacto</label>
                            <input
                                type="tel"
                                className="w-full px-4 py-3 bg-admin-surface border border-admin-border rounded-xl text-sm outline-none focus:border-admin-primary focus:ring-4 focus:ring-admin-primary/10 transition-all font-medium text-admin-text"
                                value={formData.telefono || ''}
                                onChange={e => setFormData({ ...formData, telefono: e.target.value })}
                            />
                        </div>
                    </div>

                    <div className="space-y-2">
                        <label className="text-xs font-bold text-admin-text-secondary uppercase tracking-wider">Dirección Física</label>
                        <input
                            required
                            type="text"
                            className="w-full px-4 py-3 bg-admin-surface border border-admin-border rounded-xl text-sm outline-none focus:border-admin-primary focus:ring-4 focus:ring-admin-primary/10 transition-all font-medium text-admin-text"
                            value={formData.direccion}
                            onChange={e => setFormData({ ...formData, direccion: e.target.value })}
                        />
                    </div>

                    <div className="space-y-2">
                        <label className="text-xs font-bold text-admin-text-secondary uppercase tracking-wider">Horarios de Atención</label>
                        <textarea
                            className="w-full px-4 py-3 bg-admin-surface border border-admin-border rounded-xl text-sm outline-none focus:border-admin-primary focus:ring-4 focus:ring-admin-primary/10 transition-all font-medium text-admin-text min-h-[100px] resize-y"
                            value={formData.horario || ''}
                            onChange={e => setFormData({ ...formData, horario: e.target.value })}
                            placeholder="Ej: Lunes a Viernes 9:00 - 18:00"
                        />
                    </div>

                    <div className="bg-admin-surface/50 p-6 rounded-xl border border-admin-border space-y-4">
                        <h3 className="font-bold text-admin-text text-sm uppercase tracking-wider flex items-center gap-2">
                            <span className="w-1 h-4 bg-admin-primary rounded-full"></span>
                            Redes Sociales
                        </h3>
                        <div className="grid grid-cols-1 md:grid-cols-2 gap-6">
                            <div className="space-y-2">
                                <label className="text-xs font-bold text-admin-text-secondary uppercase tracking-wider">Instagram URL</label>
                                <input
                                    type="url"
                                    className="w-full px-4 py-3 bg-white border border-admin-border rounded-xl text-sm outline-none focus:border-pink-500 focus:ring-4 focus:ring-pink-500/10 transition-all font-medium text-admin-text"
                                    value={formData.urlInstagram || ''}
                                    placeholder="https://instagram.com/..."
                                    onChange={e => setFormData({ ...formData, urlInstagram: e.target.value })}
                                />
                            </div>
                            <div className="space-y-2">
                                <label className="text-xs font-bold text-admin-text-secondary uppercase tracking-wider">Facebook URL</label>
                                <input
                                    type="url"
                                    className="w-full px-4 py-3 bg-white border border-admin-border rounded-xl text-sm outline-none focus:border-blue-600 focus:ring-4 focus:ring-blue-600/10 transition-all font-medium text-admin-text"
                                    value={formData.urlFacebook || ''}
                                    placeholder="https://facebook.com/..."
                                    onChange={e => setFormData({ ...formData, urlFacebook: e.target.value })}
                                />
                            </div>
                        </div>
                    </div>

                    <div className="bg-admin-surface/50 p-6 rounded-xl border border-admin-border space-y-4">
                        <h3 className="font-bold text-admin-text text-sm uppercase tracking-wider flex items-center gap-2">
                            <span className="w-1 h-4 bg-admin-primary rounded-full"></span>
                            Configuración de Entregas
                        </h3>
                        <div className="space-y-2">
                            <label className="text-xs font-bold text-admin-text-secondary uppercase tracking-wider">Costo de Delivery</label>
                            <div className="relative">
                                <span className="absolute left-4 top-1/2 -translate-y-1/2 text-gray-500 font-bold">$</span>
                                <input
                                    type="number"
                                    step="0.01"
                                    min="0"
                                    className="w-full pl-8 pr-4 py-3 bg-white border border-admin-border rounded-xl text-sm outline-none focus:border-admin-primary focus:ring-4 focus:ring-admin-primary/10 transition-all font-medium text-admin-text"
                                    value={formData.precioDelivery || 0}
                                    onChange={e => setFormData({ ...formData, precioDelivery: parseFloat(e.target.value) || 0 })}
                                />
                            </div>
                            <p className="text-xs text-gray-500">Este precio se aplicará a todos los pedidos con entrega a domicilio de esta sucursal.</p>
                        </div>
                    </div>

                    <div className="pt-4 border-t border-admin-border flex justify-end">
                        <button
                            type="submit"
                            disabled={loading}
                            className="px-8 py-3 bg-admin-primary text-white rounded-xl font-semibold shadow-lg shadow-admin-primary/25 hover:bg-admin-primary-dark hover:scale-[1.02] active:scale-[0.98] transition-all flex items-center gap-2"
                        >
                            {loading ? (
                                <span className="animate-spin h-5 w-5 border-2 border-white border-t-transparent rounded-full" />
                            ) : (
                                <>
                                    <svg className="w-5 h-5" fill="none" viewBox="0 0 24 24" stroke="currentColor">
                                        <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M8 7H5a2 2 0 00-2 2v9a2 2 0 002 2h14a2 2 0 002-2V9a2 2 0 00-2-2h-3m-1 4l-3 3m0 0l-3-3m3 3V4" />
                                    </svg>
                                    Guardar Cambios
                                </>
                            )}
                        </button>
                    </div>
                </form>
            </div>
        </div>
    );
}
