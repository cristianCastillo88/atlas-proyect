import React, { useState } from 'react';
import { changePassword } from '../../services/auth';
import { PasswordStrengthMeter } from '../auth/PasswordStrengthMeter';

export const ChangePasswordPanel = () => {
    const [formData, setFormData] = useState({
        passwordActual: '',
        passwordNueva: '',
        confirmarPasswordNueva: ''
    });

    const [status, setStatus] = useState<'idle' | 'loading' | 'success' | 'error'>('idle');
    const [message, setMessage] = useState('');

    const handleChange = (e: React.ChangeEvent<HTMLInputElement>) => {
        setFormData({
            ...formData,
            [e.target.name]: e.target.value
        });
        // Clear errors when user types
        if (status === 'error') {
            setStatus('idle');
            setMessage('');
        }
    };

    const handleSubmit = async (e: React.FormEvent) => {
        e.preventDefault();

        if (formData.passwordNueva !== formData.confirmarPasswordNueva) {
            setStatus('error');
            setMessage('Las nuevas contraseñas no coinciden.');
            return;
        }

        if (formData.passwordNueva.length < 8) {
            setStatus('error');
            setMessage('La nueva contraseña es muy corta.');
            return;
        }

        setStatus('loading');
        setMessage('');

        try {
            await changePassword(formData);
            setStatus('success');
            setMessage('Contraseña actualizada correctamente.');
            setFormData({ passwordActual: '', passwordNueva: '', confirmarPasswordNueva: '' });

            // Auto hide success message after 5 seconds
            setTimeout(() => {
                setStatus('idle');
                setMessage('');
            }, 5000);

        } catch (error: any) {
            setStatus('error');
            setMessage(error.message || 'Error al actualizar la contraseña.');
        }
    };

    return (
        <div className="bg-white shadow sm:rounded-lg">
            <div className="px-4 py-5 sm:p-6">
                <h3 className="text-lg leading-6 font-medium text-gray-900">Cambiar Contraseña</h3>
                <div className="mt-2 max-w-xl text-sm text-gray-500">
                    <p>Asegúrate de usar una contraseña larga y aleatoria para mantener tu cuenta segura.</p>
                </div>

                <form className="mt-5 space-y-4" onSubmit={handleSubmit}>
                    <div className="grid grid-cols-1 gap-y-4">
                        <div>
                            <label htmlFor="passwordActual" className="block text-sm font-medium text-gray-700">
                                Contraseña Actual
                            </label>
                            <input
                                type="password"
                                name="passwordActual"
                                id="passwordActual"
                                required
                                value={formData.passwordActual}
                                onChange={handleChange}
                                className="mt-1 block w-full sm:text-sm border-gray-300 rounded-md shadow-sm focus:ring-blue-500 focus:border-blue-500 p-2 border"
                            />
                        </div>

                        <div>
                            <label htmlFor="passwordNueva" className="block text-sm font-medium text-gray-700">
                                Nueva Contraseña
                            </label>
                            <input
                                type="password"
                                name="passwordNueva"
                                id="passwordNueva"
                                required
                                value={formData.passwordNueva}
                                onChange={handleChange}
                                className="mt-1 block w-full sm:text-sm border-gray-300 rounded-md shadow-sm focus:ring-blue-500 focus:border-blue-500 p-2 border"
                            />
                            <PasswordStrengthMeter password={formData.passwordNueva} />
                        </div>

                        <div>
                            <label htmlFor="confirmarPasswordNueva" className="block text-sm font-medium text-gray-700">
                                Confirmar Nueva Contraseña
                            </label>
                            <input
                                type="password"
                                name="confirmarPasswordNueva"
                                id="confirmarPasswordNueva"
                                required
                                value={formData.confirmarPasswordNueva}
                                onChange={handleChange}
                                className="mt-1 block w-full sm:text-sm border-gray-300 rounded-md shadow-sm focus:ring-blue-500 focus:border-blue-500 p-2 border"
                            />
                        </div>
                    </div>

                    {message && (
                        <div className={`mt-2 p-3 rounded-md text-sm ${status === 'success' ? 'bg-green-50 text-green-700' : 'bg-red-50 text-red-700 border-l-4 border-red-400'}`}>
                            {message}
                        </div>
                    )}

                    <div className="pt-3">
                        <button
                            type="submit"
                            disabled={status === 'loading'}
                            className={`inline-flex justify-center py-2 px-4 border border-transparent shadow-sm text-sm font-medium rounded-md text-white 
                                ${status === 'loading' ? 'bg-blue-400' : 'bg-blue-600 hover:bg-blue-700 focus:outline-none focus:ring-2 focus:ring-offset-2 focus:ring-blue-500'}`}
                        >
                            {status === 'loading' ? 'Guardando...' : 'Actualizar Contraseña'}
                        </button>
                    </div>
                </form>
            </div>
        </div>
    );
};
