import type { Producto } from '../types/api';

const API_URL = import.meta.env.PUBLIC_API_URL;

export async function obtenerProductos(): Promise<Producto[]> {
  try {
    const response = await fetch(`${API_URL}/api/productos`);
    if (!response.ok) {
      throw new Error(`Error en la respuesta: ${response.status}`);
    }
    const productos: Producto[] = await response.json();
    return productos;
  } catch (error) {
    console.error('Error al obtener productos:', error);
    return [];
  }
}