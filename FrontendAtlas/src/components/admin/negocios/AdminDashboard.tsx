import React, { useState, useEffect } from 'react';
import NegociosManager from './NegociosManager';
import SucursalesList from './SucursalesList';
import { obtenerTodosLosNegocios } from '../../../services/admin';
import type { TenantDto } from '../../../services/admin';

export default function AdminDashboard() {
    const [loading, setLoading] = useState(true);
    const [role, setRole] = useState<string | null>(null);
    const [userData, setUserData] = useState<any>(null);
    const [negocios, setNegocios] = useState<TenantDto[]>([]);

    useEffect(() => {
        const rawUser = sessionStorage.getItem('userStore') || localStorage.getItem('userStore');
        if (!rawUser) {
            window.location.href = '/login';
            return;
        }

        try {
            const parsed = JSON.parse(rawUser);
            // Support direct object or nanostores structure
            const userObj = parsed.state?.user || parsed;

            if (!userObj || !userObj.token) {
                window.location.href = '/login';
                return;
            }

            setUserData(userObj);
            // Handle various casing from backend
            const userRole = userObj.role || userObj.rol || userObj.Role;
            const parsedRole = String(userRole); // Ensure string

            setRole(parsedRole);

            if (parsedRole === 'SuperAdmin') {
                loadSuperAdminData();
            } else if (parsedRole === 'AdminNegocio') {
                setLoading(false);
            } else if (parsedRole === 'Empleado') {
                const sId = userObj.sucursalId || userObj.SucursalId;
                if (sId) {
                    window.location.href = `/admin/sucursal/${sId}`;
                } else {
                    alert('Cuenta sin sucursal asignada.');
                    window.location.href = '/login';
                }
            } else {
                console.warn('Unknown role:', parsedRole);
                setLoading(false);
            }

        } catch (e) {
            console.error('Auth error', e);
            window.location.href = '/login';
        }
    }, []);

    const loadSuperAdminData = async () => {
        try {
            const data = await obtenerTodosLosNegocios();
            setNegocios(data);
        } catch (e) {
            console.error('Error loading negocios', e);
        } finally {
            setLoading(false);
        }
    };

    if (loading) {
        return (
            <div className="space-y-6 p-1">
                <div className="h-8 w-48 bg-gray-200 rounded animate-pulse"></div>
                <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-4 gap-6">
                    {[1, 2, 3, 4].map(i => (
                        <div key={i} className="h-64 bg-gray-100 rounded-2xl animate-pulse"></div>
                    ))}
                </div>
            </div>
        );
    }

    if (role === 'SuperAdmin') {
        return <NegociosManager initialNegocios={negocios} />;
    }

    if (role === 'AdminNegocio') {
        return <SucursalesList negocioId={userData.negocioId || userData.NegocioId} />;
    }

    return <div className="p-10 text-center">Acceso no autorizado o rol desconocido: {role}</div>;
}
