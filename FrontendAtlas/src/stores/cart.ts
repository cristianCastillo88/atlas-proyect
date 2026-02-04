import { persistentAtom } from '@nanostores/persistent';
import { computed } from 'nanostores';

// Estructura del Item del Carrito
export interface CartItem {
  uuid: string; // Identificador único para cada línea del carrito
  id: number;
  nombre: string;
  precio: number;
  cantidad: number;
  sucursalId: number;
  stock: number;
  aclaraciones?: string | undefined; // Nota opcional por ítem
}

// Estructura del Estado del Carrito
interface CartState {
  items: CartItem[];
  sucursalActiva: number | null;
}

// Store Persistente del Carrito
export const $cart = persistentAtom<CartState>('atlas_cart', {
  items: [],
  sucursalActiva: null,
}, {
  encode: JSON.stringify,
  decode: JSON.parse,
});

// Helper simple para UUID
const generatePendingId = () => Date.now().toString(36) + Math.random().toString(36).substr(2);

// ===== ACCIONES (ACTIONS) =====

export function agregarItem(
  producto: { id: number; nombre: string; precio: number; stock: number },
  sucursalId: number,
  aclaraciones?: string
): void {
  // Validar stock disponible
  if (producto.stock <= 0) {
    throw new Error('Producto sin stock disponible');
  }

  const state = $cart.get();

  // Verificar si hay items de otra sucursal
  if (state.sucursalActiva !== null && state.sucursalActiva !== sucursalId) {
    console.warn(
      `Carrito limpiado: Cambiaste de sucursal (${state.sucursalActiva} → ${sucursalId})`
    );
    $cart.set({
      items: [],
      sucursalActiva: sucursalId,
    });
  }

  // Reload state after potential clear
  const currentState = $cart.get();

  // Buscar si ya existe un item con el mismo ID y las mismas aclaraciones
  const itemExistente = currentState.items.find(
    item => item.id === producto.id && (item.aclaraciones || '') === (aclaraciones || '')
  );

  if (itemExistente) {
    // Validar stock global del producto en el carrito
    const cantidadTotalEnCarrito = currentState.items
      .filter(i => i.id === producto.id)
      .reduce((acc, curr) => acc + curr.cantidad, 0);

    if (cantidadTotalEnCarrito >= producto.stock) {
      throw new Error(`Solo hay ${producto.stock} unidades disponibles`);
    }

    // Incrementar cantidad del item existente
    $cart.set({
      items: currentState.items.map(item =>
        item.uuid === itemExistente.uuid
          ? { ...item, cantidad: item.cantidad + 1 }
          : item
      ),
      sucursalActiva: currentState.sucursalActiva ?? sucursalId,
    });
  } else {
    // Validar stock antes de agregar nuevo item
    const cantidadTotalEnCarrito = currentState.items
      .filter(i => i.id === producto.id)
      .reduce((acc, curr) => acc + curr.cantidad, 0);

    if (cantidadTotalEnCarrito >= producto.stock) {
      throw new Error(`Solo hay ${producto.stock} unidades disponibles`);
    }

    // Agregar nuevo item
    $cart.set({
      items: [
        ...currentState.items,
        {
          uuid: generatePendingId(),
          id: producto.id,
          nombre: producto.nombre,
          precio: producto.precio,
          cantidad: 1,
          sucursalId: sucursalId,
          stock: producto.stock,
          aclaraciones: aclaraciones
        },
      ],
      sucursalActiva: sucursalId,
    });
  }
}

export function restarItem(uuid: string): void {
  const state = $cart.get();
  const item = state.items.find(i => i.uuid === uuid);

  if (!item) return;

  if (item.cantidad > 1) {
    $cart.set({
      ...state,
      items: state.items.map(i =>
        i.uuid === uuid ? { ...i, cantidad: i.cantidad - 1 } : i
      ),
    });
  } else {
    eliminarItem(uuid);
  }
}

export function eliminarItem(uuid: string): void {
  const state = $cart.get();
  const nuevosItems = state.items.filter(i => i.uuid !== uuid);

  $cart.set({
    items: nuevosItems,
    sucursalActiva: nuevosItems.length > 0 ? state.sucursalActiva : null,
  });
}

export function actualizarCantidad(uuid: string, nuevaCantidad: number): void {
  if (nuevaCantidad < 1) {
    eliminarItem(uuid);
    return;
  }

  const state = $cart.get();
  const item = state.items.find(i => i.uuid === uuid);
  if (!item) return;

  // Validar stock
  const cantidadOtrosItemsMismoProducto = state.items
    .filter(i => i.id === item.id && i.uuid !== uuid)
    .reduce((acc, curr) => acc + curr.cantidad, 0);

  if (cantidadOtrosItemsMismoProducto + nuevaCantidad > item.stock) {
    // No actualizar si excede
    return;
  }

  $cart.set({
    ...state,
    items: state.items.map(i =>
      i.uuid === uuid ? { ...i, cantidad: nuevaCantidad } : i
    ),
  });
}

export function actualizarAclaraciones(uuid: string, aclaraciones: string): void {
  const state = $cart.get();
  $cart.set({
    ...state,
    items: state.items.map(i =>
      i.uuid === uuid ? { ...i, aclaraciones } : i
    ),
  });
}

/**
 * Limpiar completamente el carrito
 */
export function limpiarCarrito(): void {
  $cart.set({
    items: [],
    sucursalActiva: null,
  });
}

// ===== GETTERS (COMPUTED VALUES) =====

/**
 * Cantidad total de items en el carrito (suma de todas las cantidades)
 * Útil para mostrar el numerito rojo en el icono del carrito
 */
export const $cantidadTotal = computed($cart, state =>
  state.items.reduce((total, item) => total + item.cantidad, 0)
);

/**
 * Precio total del carrito (suma de precio * cantidad)
 */
export const $precioTotal = computed($cart, state =>
  state.items.reduce((total, item) => total + item.precio * item.cantidad, 0)
);

/**
 * Verificar si el carrito está vacío
 */
export const $carritoVacio = computed($cart, state => state.items.length === 0);

/**
 * Obtener la sucursal activa del carrito
 */
export const $sucursalActiva = computed($cart, state => state.sucursalActiva);
