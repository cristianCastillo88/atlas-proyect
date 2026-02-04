import React, { useState } from 'react';
import { toast } from 'sonner';
import type { Empleado, CrearEmpleadoDto } from '../../../types/db';
import { sucursalService } from '../../../services/sucursal';

interface StaffManagerProps {
    sucursalId: number;
    initialEmpleados: Empleado[];
}

export default function StaffManager({ sucursalId, initialEmpleados }: StaffManagerProps) {
    const [empleados, setEmpleados] = useState<Empleado[]>(initialEmpleados);
    const [isModalOpen, setIsModalOpen] = useState(false);
    const [loading, setLoading] = useState(false);
    const [formData, setFormData] = useState<CrearEmpleadoDto>({
        nombre: '',
        email: '',
        password: '',
        sucursalId
    });

    const handleSubmit = async (e: React.FormEvent) => {
        e.preventDefault();
        setLoading(true);
        try {
            await sucursalService.createEmpleado(formData);
            toast.success('Empleado registrado exitosamente');
            const updated = await sucursalService.getEmpleadosPorSucursal(sucursalId);
            setEmpleados(updated);
            setIsModalOpen(false);
            setFormData({ nombre: '', email: '', password: '', sucursalId }); // Reset
        } catch (error) {
            console.error(error);
            toast.error('Error al registrar empleado');
        } finally {
            setLoading(false);
        }
    };

    const handleDelete = async (id: number) => {
        if (!confirm('¿Estás seguro de dar de baja a este empleado?')) return;
        try {
            await sucursalService.deleteEmpleado(id);
            toast.success('Empleado dado de baja');
            setEmpleados(empleados.filter(e => e.id !== id));
        } catch (error) {
            console.error(error);
            toast.error('Error al dar de baja');
        }
    };

    return (
        <div className="flex flex-col h-full space-y-4 animate-fadeIn">
            <div className="flex justify-between items-center bg-white p-4 rounded-xl border border-admin-border shadow-sm">
                <h2 className="text-xl font-bold text-admin-text">Personal</h2>
                <button
                    onClick={() => setIsModalOpen(true)}
                    className="px-4 py-2 bg-admin-primary text-white rounded-lg text-sm font-medium shadow-lg shadow-admin-primary/25 hover:bg-admin-primary-dark transition-all flex items-center gap-2"
                >
                    <svg className="w-4 h-4" fill="none" viewBox="0 0 24 24" stroke="currentColor"><path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M18 9v3m0 0v3m0-3h3m-3 0h-3m-2-5a4 4 0 11-8 0 4 4 0 018 0zM3 20a6 6 0 0112 0v1H3v-1z" /></svg>
                    Registrar Empleado
                </button>
            </div>

            <div className="flex-1 overflow-hidden bg-white rounded-xl border border-admin-border shadow-sm">
                <div className="overflow-x-auto">
                    <table className="w-full text-left text-sm">
                        <thead className="bg-admin-surface border-b border-admin-border text-admin-text-secondary font-medium">
                            <tr>
                                <th className="px-6 py-4">Nombre</th>
                                <th className="px-6 py-4">Email</th>
                                <th className="px-6 py-4">Rol</th>
                                <th className="px-6 py-4 text-right">Acciones</th>
                            </tr>
                        </thead>
                        <tbody className="divide-y divide-admin-border text-admin-text">
                            {empleados.map((empleado) => (
                                <tr key={empleado.id} className="hover:bg-admin-surface/50 transition-colors">
                                    <td className="px-6 py-4 font-medium">
                                        <div className="flex items-center gap-3">
                                            <div className="w-8 h-8 rounded-full bg-admin-primary/10 text-admin-primary flex items-center justify-center font-bold text-xs uppercase">
                                                {empleado.nombre.substring(0, 2)}
                                            </div>
                                            {empleado.nombre}
                                        </div>
                                    </td>
                                    <td className="px-6 py-4">{empleado.email}</td>
                                    <td className="px-6 py-4">
                                        <span className="px-2 py-1 rounded-full bg-blue-50 text-blue-600 text-xs font-medium">
                                            Empleado
                                        </span>
                                    </td>
                                    <td className="px-6 py-4 text-right">
                                        <button
                                            onClick={() => handleDelete(empleado.id)}
                                            className="text-red-500 hover:text-red-700 hover:bg-red-50 px-3 py-1 rounded-lg transition-colors text-xs font-medium"
                                        >
                                            Dar de Baja
                                        </button>
                                    </td>
                                </tr>
                            ))}
                            {empleados.length === 0 && (
                                <tr>
                                    <td colSpan={4} className="px-6 py-12 text-center text-admin-text-secondary">
                                        No hay empleados registrados.
                                    </td>
                                </tr>
                            )}
                        </tbody>
                    </table>
                </div>
            </div>

            {isModalOpen && (
                <div className="fixed inset-0 z-50 flex items-center justify-center p-4 bg-black/50 backdrop-blur-sm animate-fadeIn">
                    <div className="bg-white rounded-2xl shadow-2xl w-full max-w-md overflow-hidden">
                        <div className="p-4 border-b border-admin-border flex justify-between items-center bg-admin-surface">
                            <h3 className="font-bold text-lg text-admin-text">Nuevo Empleado</h3>
                            <button onClick={() => setIsModalOpen(false)} className="text-admin-text-secondary hover:text-admin-text">
                                <svg className="w-5 h-5" fill="none" viewBox="0 0 24 24" stroke="currentColor"><path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M6 18L18 6M6 6l12 12" /></svg>
                            </button>
                        </div>
                        <form onSubmit={handleSubmit} className="p-6 flex flex-col gap-4">
                            <div className="space-y-1">
                                <label className="text-xs font-semibold text-admin-text-secondary uppercase">Nombre Completo</label>
                                <input required type="text" className="w-full px-3 py-2 bg-admin-surface border border-admin-border rounded-lg text-sm outline-none focus:border-admin-primary focus:ring-2 focus:ring-admin-primary/20 transition-all"
                                    value={formData.nombre} onChange={e => setFormData({ ...formData, nombre: e.target.value })}
                                />
                            </div>
                            <div className="space-y-1">
                                <label className="text-xs font-semibold text-admin-text-secondary uppercase">Email</label>
                                <input required type="email" className="w-full px-3 py-2 bg-admin-surface border border-admin-border rounded-lg text-sm outline-none focus:border-admin-primary focus:ring-2 focus:ring-admin-primary/20 transition-all"
                                    value={formData.email} onChange={e => setFormData({ ...formData, email: e.target.value })}
                                />
                            </div>
                            <div className="space-y-1">
                                <label className="text-xs font-semibold text-admin-text-secondary uppercase">Contraseña</label>
                                <input required type="password" className="w-full px-3 py-2 bg-admin-surface border border-admin-border rounded-lg text-sm outline-none focus:border-admin-primary focus:ring-2 focus:ring-admin-primary/20 transition-all"
                                    value={formData.password} onChange={e => setFormData({ ...formData, password: e.target.value })}
                                />
                            </div>
                            <div className="flex justify-end gap-3 pt-4">
                                <button type="button" onClick={() => setIsModalOpen(false)} className="px-4 py-2 rounded-lg text-sm font-medium text-admin-text-secondary hover:bg-admin-surface transition-all">Cancelar</button>
                                <button type="submit" disabled={loading} className="px-4 py-2 bg-admin-primary text-white rounded-lg text-sm font-medium shadow-lg shadow-admin-primary/25 hover:bg-admin-primary-dark transition-all">
                                    {loading ? 'Registrando...' : 'Registrar'}
                                </button>
                            </div>
                        </form>
                    </div>
                </div>
            )}
        </div>
    );
}
