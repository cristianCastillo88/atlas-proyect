/**
 * Frontend Input Sanitization Utilities
 * 
 * IMPORTANTE: Este módulo proporciona sanitización del lado del cliente como
 * DEFENSA ADICIONAL. La sanitización principal y confiable SIEMPRE debe
 * hacerse en el backend. Nunca confíes únicamente en la sanitización frontend.
 * 
 * React ya escapa automáticamente el contenido en JSX cuando usas {variable},
 * pero estas utilidades son útiles para:
 * 1. Validación en tiempo real (feedback al usuario)
 * 2. Limpieza de datos antes de enviar al servidor (reducir carga)
 * 3. Defensa en profundidad (múltiples capas de seguridad)
 */

/**
 * Detecta patrones comunes de inyección XSS
 * @param input - Texto a validar
 * @returns true si el input contiene patrones peligrosos
 */
export function containsXSSPatterns(input: string): boolean {
    if (!input) return false;

    const dangerousPatterns = [
        /<script/i,
        /<\/script>/i,
        /javascript:/i,
        /onerror\s*=/i,
        /onload\s*=/i,
        /<iframe/i,
        /<object/i,
        /<embed/i,
        /eval\s*\(/i,
        /expression\s*\(/i,
        /<img[^>]+src[^>]*>/i,  // img tags pueden ejecutar código con onerror
    ];

    return dangerousPatterns.some(pattern => pattern.test(input));
}

/**
 * Escapa caracteres HTML especiales
 * @param input - Texto a escapar
 * @returns Texto con caracteres HTML escapados
 */
export function escapeHtml(input: string): string {
    if (!input) return '';

    const htmlEscapes: Record<string, string> = {
        '&': '&amp;',
        '<': '&lt;',
        '>': '&gt;',
        '"': '&quot;',
        "'": '&#x27;',
        '/': '&#x2F;',
    };

    return input.replace(/[&<>"'\/]/g, char => htmlEscapes[char] || char);
}

/**
 * Limpia texto removiendo caracteres potencialmente peligrosos
 * Mantiene solo texto alfanumérico, espacios y puntuación básica
 * @param input - Texto a limpiar
 * @returns Texto sanitizado
 */
export function sanitizeText(input: string): string {
    if (!input) return '';

    // Remover cualquier tag HTML
    let sanitized = input.replace(/<[^>]*>/g, '');

    // Remover nullbytes y otros caracteres de control
    sanitized = sanitized.replace(/[\0\x08\x09\x1b]/g, '');

    // Normalizar espacios múltiples
    sanitized = sanitized.replace(/\s+/g, ' ').trim();

    return sanitized;
}

/**
 * Valida longitud de texto según especificaciones del backend
 * @param input - Texto a validar
 * @param maxLength - Longitud máxima permitida
 * @returns true si la longitud es válida
 */
export function validateLength(input: string, maxLength: number): boolean {
    return input.length <= maxLength;
}

/**
 * Valida formato de teléfono
 * @param phone - Número de teléfono
 * @returns true si el formato es válido
 */
export function validatePhone(phone: string): boolean {
    if (!phone) return false;
    // Solo permite números, espacios, +, -, (, )
    return /^[\d\s\+\-\(\)]+$/.test(phone) && phone.length <= 20;
}

/**
 * Reglas de validación del backend para campos de pedidos
 */
export const VALIDATION_RULES = {
    nombreCliente: { maxLength: 100, required: true },
    direccionCliente: { maxLength: 200, required: false },
    telefonoCliente: { maxLength: 20, required: true },
    observaciones: { maxLength: 500, required: false },
    aclaraciones: { maxLength: 200, required: false },
} as const;

/**
 * Valida input de pedido según reglas del backend
 * @param fieldName - Nombre del campo
 * @param value - Valor a validar
 * @returns Objeto con validación y mensaje de error si aplica
 */
export function validatePedidoField(
    fieldName: keyof typeof VALIDATION_RULES,
    value: string
): { isValid: boolean; error?: string } {
    const rules = VALIDATION_RULES[fieldName];

    if (rules.required && !value.trim()) {
        return { isValid: false, error: 'Este campo es obligatorio' };
    }

    if (value && !validateLength(value, rules.maxLength)) {
        return {
            isValid: false,
            error: `Máximo ${rules.maxLength} caracteres`,
        };
    }

    if (value && containsXSSPatterns(value)) {
        return {
            isValid: false,
            error: 'El texto contiene caracteres no permitidos',
        };
    }

    if (fieldName === 'telefonoCliente' && value && !validatePhone(value)) {
        return {
            isValid: false,
            error: 'Formato de teléfono inválido',
        };
    }

    return { isValid: true };
}

/**
 * Hook helper para mostrar contador de caracteres
 * @param current - Longitud actual
 * @param max - Longitud máxima
 * @returns Texto del contador
 */
export function getCharacterCount(current: number, max: number): string {
    const remaining = max - current;
    const percentage = (current / max) * 100;

    if (percentage >= 90) {
        return `⚠️ ${remaining} restantes`;
    }

    return `${current}/${max}`;
}

/**
 * NOTA DE SEGURIDAD:
 * 
 * React automáticamente escapa valores en JSX cuando usas {variable}.
 * NUNCA uses dangerouslySetInnerHTML a menos que sea absolutamente necesario
 * y estés 100% seguro de que el contenido está sanitizado.
 * 
 * ✅ SEGURO:
 *   <div>{userInput}</div>
 * 
 * ❌ PELIGROSO:
 *   <div dangerouslySetInnerHTML={{__html: userInput}} />
 * 
 * Si necesitas renderizar HTML, SIEMPRE sanitízalo primero con una librería
 * como DOMPurify y valida que sea realmente necesario.
 */
