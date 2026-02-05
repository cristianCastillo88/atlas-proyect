import React from 'react';
import { logout } from '../../../stores/auth';

interface Props {
    message?: string;
}

export default function AccessDenied({ message = "No tienes permisos para acceder a esta sección." }: Props) {
    return (
        <div className="min-h-screen bg-gray-50 flex flex-col items-center justify-center p-4">
            <div className="bg-white rounded-2xl shadow-xl p-8 max-w-md w-full text-center border border-gray-100">
                <div className="w-20 h-20 bg-red-100 rounded-full flex items-center justify-center mx-auto mb-6">
                    <svg className="w-10 h-10 text-red-600" fill="none" viewBox="0 0 24 24" stroke="currentColor">
                        <path strokeLinecap="round" strokeLinejoin="round" strokeWidth="2" d="M12 15v2m-6 4h12a2 2 0 002-2v-6a2 2 0 00-2-2H6a2 2 0 00-2 2v6a2 2 0 002 2zm10-10V7a4 4 0 00-8 0v4h8z" />
                    </svg>
                </div>

                <h1 className="text-2xl font-bold text-gray-900 mb-2">Acceso Denegado</h1>
                <p className="text-gray-500 mb-8">{message}</p>

                <div className="space-y-3">
                    <button
                        onClick={() => window.history.back()}
                        className="w-full px-4 py-3 bg-gray-100 hover:bg-gray-200 text-gray-700 font-medium rounded-xl transition-colors"
                    >
                        Volver Atrás
                    </button>

                    <button
                        onClick={() => {
                            logout();
                            window.location.href = '/login';
                        }}
                        className="w-full px-4 py-3 bg-red-600 hover:bg-red-700 text-white font-medium rounded-xl transition-colors"
                    >
                        Cerrar Sesión
                    </button>
                </div>
            </div>

            <p className="mt-8 text-sm text-gray-400">
                Si crees que esto es un error, contacta al administrador.
            </p>
        </div>
    );
}
