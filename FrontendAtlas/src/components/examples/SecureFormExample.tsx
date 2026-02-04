/**
 * EJEMPLO DE REFERENCIA: Componente con Validación Segura
 * 
 * Este archivo muestra las MEJORES PRÁCTICAS para crear formularios
 * con validación frontend que cumple con los estándares de seguridad.
 * 
 * Úsalo como plantilla para nuevos componentes.
 */

import React, { useState, useCallback } from 'react';
import { validatePedidoField, getCharacterCount } from '../../utils/sanitization';

interface SecureFormExampleProps {
    onSubmit: (data: FormData) => Promise<void>;
}

interface FormData {
    nombre: string;
    telefono: string;
    observaciones: string;
}

interface FormErrors {
    nombre?: string;
    telefono?: string;
    observaciones?: string;
}

export function SecureFormExample({ onSubmit }: SecureFormExampleProps) {
    const [formData, setFormData] = useState<FormData>({
        nombre: '',
        telefono: '',
        observaciones: '',
    });

    const [errors, setErrors] = useState<FormErrors>({});
    const [touched, setTouched] = useState<Set<keyof FormData>>(new Set());

    /**
     * Maneja cambios en los inputs con validación en tiempo real
     */
    const handleChange = useCallback((field: keyof FormData) => (
        e: React.ChangeEvent<HTMLInputElement | HTMLTextAreaElement>
    ) => {
        const value = e.target.value;

        // Actualizar valor
        setFormData(prev => ({ ...prev, [field]: value }));

        // Validar solo si el campo ha sido tocado
        if (touched.has(field)) {
            const validation = validatePedidoField(
                field === 'nombre' ? 'nombreCliente' :
                    field === 'telefono' ? 'telefonoCliente' :
                        'observaciones',
                value
            );

            setErrors(prev => ({
                ...prev,
                [field]: validation.isValid ? undefined : validation.error,
            }));
        }
    }, [touched]);

    /**
     * Marca el campo como tocado al salir del input
     */
    const handleBlur = (field: keyof FormData) => () => {
        setTouched(prev => new Set(prev).add(field));

        // Validar al perder foco
        const validation = validatePedidoField(
            field === 'nombre' ? 'nombreCliente' :
                field === 'telefono' ? 'telefonoCliente' :
                    'observaciones',
            formData[field]
        );

        setErrors(prev => ({
            ...prev,
            [field]: validation.isValid ? undefined : validation.error,
        }));
    };

    /**
     * Valida todo el formulario antes de enviar
     */
    const validateForm = (): boolean => {
        const newErrors: FormErrors = {};

        // Validar nombre
        const nombreValidation = validatePedidoField('nombreCliente', formData.nombre);
        if (!nombreValidation.isValid) {
            newErrors.nombre = nombreValidation.error ?? 'Error de validación';
        }

        // Validar teléfono
        const telefonoValidation = validatePedidoField('telefonoCliente', formData.telefono);
        if (!telefonoValidation.isValid) {
            newErrors.telefono = telefonoValidation.error ?? 'Error de validación';
        }

        // Validar observaciones
        if (formData.observaciones) {
            const obsValidation = validatePedidoField('observaciones', formData.observaciones);
            if (!obsValidation.isValid) {
                newErrors.observaciones = obsValidation.error ?? 'Error de validación';
            }
        }

        setErrors(newErrors);
        return Object.keys(newErrors).length === 0;
    };

    /**
     * Maneja el envío del formulario
     */
    const handleSubmit = async (e: React.FormEvent) => {
        e.preventDefault();

        // Marcar todos los campos como tocados
        setTouched(new Set(['nombre', 'telefono', 'observaciones']));

        // Validar
        if (!validateForm()) {
            return;
        }

        // Enviar al backend (que hará la sanitización real)
        try {
            await onSubmit(formData);
        } catch (error) {
            console.error('Error al enviar formulario:', error);
        }
    };

    return (
        <form onSubmit={handleSubmit} className="space-y-4">
            {/* Campo: Nombre */}
            <div>
                <label htmlFor="nombre" className="block text-sm font-medium text-gray-700 mb-1">
                    Nombre *
                </label>
                <input
                    id="nombre"
                    type="text"
                    value={formData.nombre}
                    onChange={handleChange('nombre')}
                    onBlur={handleBlur('nombre')}
                    maxLength={100}
                    className={`w-full px-3 py-2 border rounded-lg focus:ring-2 focus:ring-blue-500 ${errors.nombre ? 'border-red-500' : 'border-gray-300'
                        }`}
                    placeholder="Ingresa tu nombre"
                    aria-invalid={!!errors.nombre}
                    aria-describedby={errors.nombre ? 'nombre-error' : undefined}
                />
                {errors.nombre && (
                    <p id="nombre-error" className="mt-1 text-sm text-red-600" role="alert">
                        {errors.nombre}
                    </p>
                )}
                <p className="mt-1 text-xs text-gray-500">
                    {getCharacterCount(formData.nombre.length, 100)}
                </p>
            </div>

            {/* Campo: Teléfono */}
            <div>
                <label htmlFor="telefono" className="block text-sm font-medium text-gray-700 mb-1">
                    Teléfono *
                </label>
                <input
                    id="telefono"
                    type="tel"
                    value={formData.telefono}
                    onChange={handleChange('telefono')}
                    onBlur={handleBlur('telefono')}
                    maxLength={20}
                    className={`w-full px-3 py-2 border rounded-lg focus:ring-2 focus:ring-blue-500 ${errors.telefono ? 'border-red-500' : 'border-gray-300'
                        }`}
                    placeholder="+54 9 123 456-7890"
                    aria-invalid={!!errors.telefono}
                    aria-describedby={errors.telefono ? 'telefono-error' : undefined}
                />
                {errors.telefono && (
                    <p id="telefono-error" className="mt-1 text-sm text-red-600" role="alert">
                        {errors.telefono}
                    </p>
                )}
                <p className="mt-1 text-xs text-gray-500">
                    Solo números y caracteres: + - ( ) espacios
                </p>
            </div>

            {/* Campo: Observaciones */}
            <div>
                <label htmlFor="observaciones" className="block text-sm font-medium text-gray-700 mb-1">
                    Observaciones
                </label>
                <textarea
                    id="observaciones"
                    value={formData.observaciones}
                    onChange={handleChange('observaciones')}
                    onBlur={handleBlur('observaciones')}
                    maxLength={500}
                    rows={3}
                    className={`w-full px-3 py-2 border rounded-lg focus:ring-2 focus:ring-blue-500 resize-none ${errors.observaciones ? 'border-red-500' : 'border-gray-300'
                        }`}
                    placeholder="Ej: Sin cebolla, bien cocido..."
                    aria-invalid={!!errors.observaciones}
                    aria-describedby={errors.observaciones ? 'observaciones-error' : undefined}
                />
                {errors.observaciones && (
                    <p id="observaciones-error" className="mt-1 text-sm text-red-600" role="alert">
                        {errors.observaciones}
                    </p>
                )}
                <div className="flex justify-between mt-1">
                    <p className="text-xs text-gray-500">Opcional</p>
                    <p className={`text-xs ${formData.observaciones.length > 450 ? 'text-amber-600 font-medium' : 'text-gray-500'
                        }`}>
                        {getCharacterCount(formData.observaciones.length, 500)}
                    </p>
                </div>
            </div>

            {/* Botón de Envío */}
            <div className="flex gap-3 pt-4">
                <button
                    type="submit"
                    className="flex-1 bg-blue-600 text-white px-6 py-3 rounded-lg font-medium hover:bg-blue-700 transition-colors disabled:bg-gray-300 disabled:cursor-not-allowed"
                    disabled={Object.keys(errors).length > 0}
                >
                    Enviar Pedido
                </button>
            </div>

            {/* Indicador de seguridad */}
            <div className="flex items-center gap-2 text-xs text-gray-500 bg-gray-50 p-2 rounded">
                <svg className="w-4 h-4 text-green-600" fill="none" viewBox="0 0 24 24" stroke="currentColor">
                    <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M9 12l2 2 4-4m5.618-4.016A11.955 11.955 0 0112 2.944a11.955 11.955 0 01-8.618 3.04A12.02 12.02 0 003 9c0 5.591 3.824 10.29 9 11.622 5.176-1.332 9-6.03 9-11.622 0-1.042-.133-2.052-.382-3.016z" />
                </svg>
                <span>Tus datos están protegidos y validados automáticamente</span>
            </div>
        </form>
    );
}

/**
 * NOTAS IMPORTANTES PARA DESARROLLADORES:
 * 
 * 1. React escapa automáticamente {variables} en JSX - NO necesitas sanitizar manualmente
 * 
 * 2. Las validaciones frontend son para UX, el backend SIEMPRE valida y sanitiza
 * 
 * 3. NUNCA uses dangerouslySetInnerHTML con datos del usuario
 * 
 * 4. Los límites de maxLength en inputs son una capa extra de defensa,
 *    pero el backend es quien tiene la última palabra
 * 
 * 5. containsXSSPatterns() es útil para feedback temprano, pero no confíes
 *    solo en ella - el backend usa HtmlSanitizer que es más robusto
 */
