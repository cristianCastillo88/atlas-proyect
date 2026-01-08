export interface Producto {
  id?: number;
  nombre: string;
  descripcion: string;
  precio: number;
  urlImagen: string;
  categoriaId?: number;
  // Agrega otros campos si es necesario según el backend
}