import React, { useState, useEffect } from 'react';
import { resetPassword } from '../../services/auth';
import { PasswordStrengthMeter } from './PasswordStrengthMeter';

export const ResetPasswordForm = () => {
    const [token, setToken] = useState('');
    const [formData, setFormData] = useState({
        password: '',
        confirmPassword: ''
    });
    const [status, setStatus] = useState<'idle' | 'loading' | 'success' | 'error'>('idle');
    const [errorMessage, setErrorMessage] = useState('');

    useEffect(() => {
        // Extract token securely from URL search params
        const params = new URLSearchParams(window.location.search);
        const tokenFromUrl = params.get('token');
        if (tokenFromUrl) {
            setToken(tokenFromUrl);
        } else {
            setStatus('error');
            setErrorMessage('Enlace inválido o incompleto. Por favor solicita uno nuevo.');
        }
    }, []);

    const handleChange = (e: React.ChangeEvent<HTMLInputElement>) => {
        setFormData({
            ...formData,
            [e.target.name]: e.target.value
        });
    };

    const handleSubmit = async (e: React.FormEvent) => {
        e.preventDefault();

        if (!token) {
            setErrorMessage('Token no proporcionado.');
            return;
        }

        if (formData.password !== formData.confirmPassword) {
            setErrorMessage('Las contraseñas no coinciden.');
            return;
        }

        if (formData.password.length < 8) {
            setErrorMessage('La contraseña es demasiado corta.');
            return;
        }

        setStatus('loading');
        setErrorMessage('');

        try {
            await resetPassword({
                token: token,
                nuevaPassword: formData.password,
                confirmarPassword: formData.confirmPassword
            });
            setStatus('success');
        } catch (error: any) {
            setStatus('error');
            setErrorMessage(error.message || 'El token ha expirado o no es válido.');
        }
    };

    if (status === 'success') {
        return (
            <div className="bg-green-50 border border-green-200 rounded-lg p-6 text-center animate-fade-in">
                <div className="mx-auto flex items-center justify-center h-12 w-12 rounded-full bg-green-100 mb-4">
                    <svg className="h-6 w-6 text-green-600" fill="none" viewBox="0 0 24 24" stroke="currentColor">
                        <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M9 12l2 2 4-4m6 2a9 9 0 11-18 0 9 9 0 0118 0z" />
                    </svg>
                </div>
                <h3 className="text-lg leading-6 font-medium text-gray-900">¡Contraseña Restablecida!</h3>
                <p className="mt-2 text-sm text-gray-600">
                    Tu contraseña ha sido actualizada exitosamente. Ya puedes acceder a tu cuenta.
                </p>
                <div className="mt-6">
                    <a href="/login" className="w-full inline-flex justify-center py-2 px-4 border border-transparent rounded-md shadow-sm text-sm font-medium text-white bg-green-600 hover:bg-green-700 focus:outline-none focus:ring-2 focus:ring-offset-2 focus:ring-green-500">
                        Ir al Login
                    </a>
                </div>
            </div>
        );
    }

    return (
        <form className="space-y-6" onSubmit={handleSubmit}>
            {/* Hidden user feedback for better UX when token is missing */}
            {!token && status === 'error' && (
                <div className="bg-red-50 border-l-4 border-red-400 p-4 mb-4">
                    <p className="text-sm text-red-700">{errorMessage}</p>
                    <div className="mt-2">
                        <a href="/forgot-password" className="text-sm font-medium text-red-700 underline hover:text-red-600">
                            Solicitar nuevo enlace
                        </a>
                    </div>
                </div>
            )}

            {token && (
                <>
                    <div>
                        <label htmlFor="password" className="block text-sm font-medium text-gray-700">
                            Nueva contraseña
                        </label>
                        <div className="mt-1">
                            <input
                                id="password"
                                name="password"
                                type="password"
                                required
                                className="block w-full sm:text-sm border-gray-300 rounded-md focus:ring-blue-500 focus:border-blue-500 p-2.5 border"
                                value={formData.password}
                                onChange={handleChange}
                                disabled={status === 'loading'}
                            />
                        </div>
                        {/* Password Strength Meter Component */}
                        <PasswordStrengthMeter password={formData.password} />
                    </div>

                    <div>
                        <label htmlFor="confirmPassword" className="block text-sm font-medium text-gray-700">
                            Confirmar nueva contraseña
                        </label>
                        <div className="mt-1">
                            <input
                                id="confirmPassword"
                                name="confirmPassword"
                                type="password"
                                required
                                className="block w-full sm:text-sm border-gray-300 rounded-md focus:ring-blue-500 focus:border-blue-500 p-2.5 border"
                                value={formData.confirmPassword}
                                onChange={handleChange}
                                disabled={status === 'loading'}
                            />
                        </div>
                    </div>

                    {status === 'error' && errorMessage && token && (
                        <div className="bg-red-50 border-l-4 border-red-400 p-4">
                            <div className="flex">
                                <div className="ml-3">
                                    <p className="text-sm text-red-700">{errorMessage}</p>
                                </div>
                            </div>
                        </div>
                    )}

                    <div>
                        <button
                            type="submit"
                            disabled={status === 'loading' || !token}
                            className={`w-full flex justify-center py-2.5 px-4 border border-transparent rounded-md shadow-sm text-sm font-medium text-white 
                                ${status === 'loading' ? 'bg-blue-400 cursor-not-allowed' : 'bg-blue-600 hover:bg-blue-700 focus:outline-none focus:ring-2 focus:ring-offset-2 focus:ring-blue-500'}
                                transition duration-150 ease-in-out`}
                        >
                            {status === 'loading' ? 'Actualizando...' : 'Restablecer Contraseña'}
                        </button>
                    </div>
                </>
            )}
        </form>
    );
};
