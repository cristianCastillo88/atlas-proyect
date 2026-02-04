# Guía de Seguridad Frontend - Atlas

## 🛡️ Principios de Seguridad

### 1. React Auto-Escapa por Defecto

React automáticamente escapa todo contenido renderizado con JSX:

```tsx
// ✅ SEGURO - React escapa automáticamente
<div>{userInput}</div>
<p>{pedido.nombreCliente}</p>
<span>{item.aclaraciones}</span>
```

### 2. NUNCA Usar `dangerouslySetInnerHTML`

```tsx
// ❌ EXTREMADAMENTE PELIGROSO
<div dangerouslySetInnerHTML={{__html: userInput}} />

// ✅ Si realmente necesitas HTML, usa DOMPurify
import DOMPurify from 'dompurify';
<div dangerouslySetInnerHTML={{__html: DOMPurify.sanitize(trustedHtml)}} />
```

### 3. Validación en Múltiples Capas

```
Usuario → Frontend (UX) → Backend (Seguridad) → Base de Datos (Límites)
```

- **Frontend**: Validación para mejorar UX (feedback instantáneo)
- **Backend**: Validación REAL y autoritativa
- **Base de Datos**: Última línea de defensa (constraints)

## 📋 Checklist de Seguridad

### Antes de Renderizar Datos del Usuario

- [ ] ¿Los datos vienen de un input del usuario?
- [ ] ¿Estás usando `{variable}` en JSX? (React escapa automáticamente)
- [ ] ¿NO estás usando `dangerouslySetInnerHTML`?
- [ ] ¿NO estás usando `.innerHTML` en JavaScript?

### Al Crear Formularios

- [ ] ¿Validación frontend con utilidades de `utils/sanitization.ts`?
- [ ] ¿Límites de longitud coinciden con el backend?
- [ ] ¿Mensajes de error claros para el usuario?
- [ ] ¿Feedback visual (contador de caracteres, etc.)?

### Al Manejar URLs

```tsx
// ❌ PELIGROSO - JavaScript puede ejecutarse
<a href={userInput}>Link</a>

// ✅ SEGURO - Validar esquema
const isSafeUrl = (url: string) => url.startsWith('http://') || url.startsWith('https://');
<a href={isSafeUrl(url) ? url : '#'}>Link</a>
```

## 🔍 Campos Críticos en Atlas

### Datos de Pedidos (Riesgo ALTO)

- `nombreCliente` - Máx 100 chars
- `direccionCliente` - Máx 200 chars
- `telefonoCliente` - Máx 20 chars, solo `[\d\s\+\-\(\)]`
- `observaciones` - Máx 500 chars
- `aclaraciones` (items) - Máx 200 chars

**Dónde se renderizan:**

- `KitchenDisplay.tsx` - Líneas 164, 173, 176
- `TicketTemplate.tsx` - Plantilla de impresión
- Notificaciones de SignalR

## ✅ Ejemplo de Implementación Segura

```tsx
import { validatePedidoField, getCharacterCount } from '../utils/sanitization';

function OrderForm() {
    const [observaciones, setObservaciones] = useState('');
    const [error, setError] = useState('');

    const handleChange = (e: React.ChangeEvent<HTMLTextAreaElement>) => {
        const value = e.target.value;
        const validation = validatePedidoField('observaciones', value);
        
        if (!validation.isValid) {
            setError(validation.error!);
        } else {
            setError('');
            setObservaciones(value);
        }
    };

    return (
        <div>
            <textarea
                value={observaciones}
                onChange={handleChange}
                maxLength={500} {/* Límite del navegador como fallback */}
            />
            <div className="text-sm text-gray-500">
                {getCharacterCount(observaciones.length, 500)}
            </div>
            {error && <span className="text-red-500">{error}</span>}
        </div>
    );
}
```

## 🚨 Casos de Uso Peligrosos

### 1. Rich Text Editors

Si necesitas permitir texto enriquecido (negrita, cursiva, etc.):

```typescript
// ❌ NUNCA hagas esto
<div dangerouslySetInnerHTML={{__html: userRichText}} />

// ✅ Usa una librería dedicada
import DOMPurify from 'dompurify';
const clean = DOMPurify.sanitize(userRichText, {
    ALLOWED_TAGS: ['b', 'i', 'em', 'strong', 'u'],
    ALLOWED_ATTR: []
});
<div dangerouslySetInnerHTML={{__html: clean}} />
```

### 2. Markdown

```typescript
// ✅ Usa una librería de Markdown que sanitice por defecto
import ReactMarkdown from 'react-markdown';
<ReactMarkdown>{userMarkdown}</ReactMarkdown>
```

### 3. URLs Dinámicas

```typescript
// ❌ PELIGROSO
<a href={userInput}>Click</a>

// ✅ SEGURO
const SAFE_URL_REGEX = /^https?:\/\//;
<a href={SAFE_URL_REGEX.test(userInput) ? userInput : '#'}>Click</a>
```

## 📚 Recursos Adicionales

- [OWASP XSS Prevention Cheat Sheet](https://cheatsheetseries.owasp.org/cheatsheets/Cross_Site_Scripting_Prevention_Cheat_Sheet.html)
- [React Security Best Practices](https://react.dev/learn/keeping-components-pure#side-effects-unintended-consequences)
- [DOMPurify Documentation](https://github.com/cure53/DOMPurify)

## 🔄 Revisión Regular

**Cada Sprint:**

1. Buscar `dangerouslySetInnerHTML` en el código
2. Revisar nuevos formularios añadidos
3. Verificar que las validaciones coincidan con el backend

**Comando de auditoría automática:**

```bash
# Buscar usos peligrosos
grep -r "dangerouslySetInnerHTML" src/
grep -r "innerHTML" src/
```

---

**Última actualización:** 2026-02-03  
**Responsable:** Equipo de Desarrollo Atlas
