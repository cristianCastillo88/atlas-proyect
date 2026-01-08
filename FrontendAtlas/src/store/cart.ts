import { persistentAtom } from '@nanostores/persistent';
import { computed } from 'nanostores';

// Estructura del Item del Carrito
export interface CartItem {
  id: number;
  nombre: string;
  precio: number;
  cantidad: number;
  sucursalId: number;
  stock: number; // Stock disponible del producto
}

// Estructura del Estado del Carrito
interface CartState {
  items: CartItem[];
  sucursalActiva: number | null;
}

// Store Persistente del Carrito
// Se guarda en localStorage con la clave 'atlas_cart'
export const $cart = persistentAtom<CartState>('atlas_cart', {
  items: [],
  sucursalActiva: null,
}, {
  encode: JSON.stringify,
  decode: JSON.parse,
});

// ===== ACCIONES (ACTIONS) =====

/**
 * Agregar un producto al carrito
 * Si el carrito tiene items de otra sucursal, limpia el carrito primero
 * Si el producto ya existe, incrementa la cantidad
 * Valida que haya stock disponible antes de agregar
 */
export function agregarItem(
  producto: { id: number; nombre: string; precio: number; stock: number },
  sucursalId: number
): void {
  // Validar stock disponible
  if (producto.stock <= 0) {
    throw new Error('Producto sin stock disponible');
  }

  const state = $cart.get();
  
  // Verificar si hay items de otra sucursal
  if (state.sucursalActiva !== null && state.sucursalActiva !== sucursalId) {
    // Limpiar carrito anterior (diferente sucursal)
    console.warn(
      `Carrito limpiado: Cambiaste de sucursal (${state.sucursalActiva} → ${sucursalId})`
    );
    $cart.set({
      items: [],
      sucursalActiva: sucursalId,
    });
  }

  // Verificar si el producto ya existe en el carrito
  const itemExistente = state.items.find(item => item.id === producto.id);

  if (itemExistente) {
    // Validar que no exceda el stock disponible
    if (itemExistente.cantidad >= producto.stock) {
      throw new Error(`Solo hay ${producto.stock} unidades disponibles`);
    }
    // Incrementar cantidad del producto existente
    $cart.set({
      items: state.items.map(item =>
        item.id === producto.id
          ? { ...item, cantidad: item.cantidad + 1 }
          : item
      ),
      sucursalActiva: state.sucursalActiva ?? sucursalId,
    });
  } else {
    // Agregar nuevo producto al carrito
    $cart.set({
      items: [
        ...state.items,
        {
          id: producto.id,
          nombre: producto.nombre,
          precio: producto.precio,
          cantidad: 1,
          sucursalId: sucursalId,
          stock: producto.stock,
        },
      ],
      sucursalActiva: sucursalId,
    });
  }
}

/**
 * Restar cantidad de un producto
 * Si la cantidad llega a 0, elimina el item del carrito
 */
export function restarItem(productoId: number): void {
  const state = $cart.get();
  const item = state.items.find(i => i.id === productoId);

  if (!item) {
    console.warn(`Producto ${productoId} no encontrado en el carrito`);
    return;
  }

  if (item.cantidad > 1) {
    // Decrementar cantidad
    $cart.set({
      ...state,
      items: state.items.map(i =>
        i.id === productoId ? { ...i, cantidad: i.cantidad - 1 } : i
      ),
    });
  } else {
    // Eliminar item si cantidad es 1
    eliminarItem(productoId);
  }
}

/**
 * Eliminar completamente un producto del carrito
 */
export function eliminarItem(productoId: number): void {
  const state = $cart.get();
  const nuevosItems = state.items.filter(i => i.id !== productoId);

  $cart.set({
    items: nuevosItems,
    sucursalActiva: nuevosItems.length > 0 ? state.sucursalActiva : null,
  });
}

/**
 * Actualizar la cantidad de un producto directamente
 */
export function actualizarCantidad(productoId: number, nuevaCantidad: number): void {
  if (nuevaCantidad < 1) {
    eliminarItem(productoId);
    return;
  }

  const state = $cart.get();
  $cart.set({
    ...state,
    items: state.items.map(i =>
      i.id === productoId ? { ...i, cantidad: nuevaCantidad } : i
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
