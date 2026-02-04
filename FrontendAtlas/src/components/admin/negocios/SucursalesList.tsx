import React, { useState, useEffect } from 'react';
import type { SucursalDto } from '../../../services/admin';
import { obtenerSucursalesPorNegocio } from '../../../services/admin';

interface SucursalesListProps {
    negocioId: number;
}

export default function SucursalesList({ negocioId }: SucursalesListProps) {
    const [sucursales, setSucursales] = useState<SucursalDto[]>([]);
    const [loading, setLoading] = useState(true);

    useEffect(() => {
        const fetchSucursales = async () => {
            try {
                const data = await obtenerSucursalesPorNegocio(negocioId);
                setSucursales(data);
            } catch (error) {
                console.error('Error fetching sucursales', error);
            } finally {
                setLoading(false);
            }
        };

        if (negocioId) {
            fetchSucursales();
        }
    }, [negocioId]);

    if (loading) {
        return (
            <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-3 xl:grid-cols-4 gap-6 animate-pulse">
                {[1, 2, 3].map(i => (
                    <div key={i} className="h-48 bg-gray-100 rounded-xl border border-gray-200"></div>
                ))}
            </div>
        );
    }

    if (sucursales.length === 0) {
        return (
            <div className="text-center py-20 text-gray-500">
                <p>No tienes sucursales registradas.</p>
            </div>
        );
    }

    return (
        <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-3 xl:grid-cols-4 gap-6 animate-fadeIn">
            {sucursales.map((s, index) => (
                <div
                    key={s.id}
                    className="bg-white rounded-xl border border-admin-border p-5 transition-all hover:shadow-lg hover:border-admin-primary/30 group flex flex-col"
                    style={{ animationDelay: `${index * 50}ms` }}
                >
                    {/* Header */}
                    <div className="flex items-start justify-between mb-4">
                        <div className="flex items-center gap-3">
                            <div className="w-12 h-12 bg-gradient-to-br from-admin-primary/20 to-admin-primary/5 rounded-lg flex items-center justify-center">
                                <svg className="w-6 h-6 text-admin-primary" fill="none" viewBox="0 0 24 24" stroke="currentColor">
                                    <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M19 21V5a2 2 0 00-2-2H7a2 2 0 00-2 2v16m14 0h2m-2 0h-5m-9 0H3m2 0h5M9 7h1m-1 4h1m4-4h1m-1 4h1m-5 10v-5a1 1 0 011-1h2a1 1 0 011 1v5m-4 0h4" />
                                </svg>
                            </div>
                            <div>
                                <h3 className="text-lg font-semibold text-admin-text line-clamp-1">{s.nombre}</h3>
                            </div>
                        </div>

                        <span className={`inline-flex items-center gap-1.5 px-3 py-1 rounded-full text-xs font-medium ${s.activo ? 'bg-green-50 text-green-700' : 'bg-red-50 text-red-700'}`}>
                            <span className={`w-2 h-2 rounded-full ${s.activo ? 'bg-green-500 animate-pulse' : 'bg-red-500'}`} />
                            {s.activo ? 'Activa' : 'Inactiva'}
                        </span>
                    </div>

                    {/* Info */}
                    <div className="space-y-2 mb-6 flex-1">
                        <div className="flex items-center gap-2 text-sm text-admin-text-secondary">
                            <svg className="w-4 h-4 shrink-0" fill="none" viewBox="0 0 24 24" stroke="currentColor">
                                <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M17.657 16.657L13.414 20.9a1.998 1.998 0 01-2.827 0l-4.244-4.243a8 8 0 1111.314 0z" />
                                <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M15 11a3 3 0 11-6 0 3 3 0 016 0z" />
                            </svg>
                            <span className="truncate">{s.direccion}</span>
                        </div>
                        {/* If phone exists roughly */}
                        {(s as any).telefono && (
                            <div className="flex items-center gap-2 text-sm text-admin-text-secondary">
                                <svg className="w-4 h-4 shrink-0" fill="none" viewBox="0 0 24 24" stroke="currentColor"><path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M3 5a2 2 0 012-2h3.28a1 1 0 01.948.684l1.498 4.493a1 1 0 01-.502 1.21l-2.257 1.13a11.042 11.042 0 005.516 5.516l1.13-2.257a1 1 0 011.21-.502l4.493 1.498a1 1 0 01.684.949V19a2 2 0 01-2 2h-1C9.716 21 3 14.284 3 6V5z" /></svg>
                                <span>{(s as any).telefono}</span>
                            </div>
                        )}
                    </div>

                    <div className="pt-4 border-t border-admin-border mt-auto">
                        <a
                            href={`/admin/sucursal/${s.id}`}
                            className="block w-full px-4 py-2.5 bg-admin-primary text-white rounded-lg hover:bg-admin-primary-dark transition-colors text-sm font-medium text-center shadow-lg shadow-admin-primary/25"
                        >
                            Administrar Sucursal
                        </a>
                    </div>
                </div>
            ))}
        </div>
    );
}
