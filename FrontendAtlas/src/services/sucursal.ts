import { api } from '../lib/api';
import type {
  Sucursal,
  Categoria,
  Producto,
  Empleado,
  Pedido,
  MetodoPago,
  TipoEntrega,
  CrearEmpleadoDto,
  ProductoCreateReq,
  ProductoUpdateReq,
  CrearPedidoDto
} from '../types/db';

export const sucursalService = {
  // --- Categorías ---
  async getCategorias(): Promise<Categoria[]> {
    const { data } = await api.get<Categoria[]>('/Categorias');
    return data;
  },

  async getCategoriasBySucursal(sucursalId: number): Promise<Categoria[]> {
    const { data } = await api.get<Categoria[]>(`/Categorias/sucursal/${sucursalId}`);
    return data;
  },

  async createCategoria(categoria: { nombre: string; sucursalId: number }): Promise<Categoria> {
    const { data } = await api.post<Categoria>('/Categorias', categoria);
    return data;
  },

  // --- Productos ---
  async getProductos(sucursalId: number): Promise<Producto[]> {
    const { data } = await api.get<Producto[]>(`/Productos/sucursal/${sucursalId}`);
    return data;
  },

  async createProducto(producto: ProductoCreateReq): Promise<Producto> {
    const { data } = await api.post<Producto>('/Productos', producto);
    return data;
  },

  async updateProducto(producto: ProductoUpdateReq): Promise<Producto> {
    const { data } = await api.put<Producto>(`/Productos/${producto.id}`, producto);
    return data;
  },

  async deleteProducto(id: number): Promise<void> {
    await api.delete(`/Productos/${id}`);
  },

  // --- Sucursal ---
  async getSucursal(id: number): Promise<Sucursal> {
    const { data } = await api.get<Sucursal>(`/Sucursales/${id}`);
    return data;
  },

  async updateSucursal(id: number, sucursal: Partial<Sucursal>): Promise<Sucursal> {
    const { data } = await api.put<Sucursal>(`/Sucursales/${id}`, sucursal);
    return data;
  },

  // --- Empleados ---
  async getEmpleadosPorSucursal(sucursalId: number): Promise<Empleado[]> {
    const { data } = await api.get<Empleado[]>(`/Empleados/sucursal/${sucursalId}`);
    return data;
  },

  async createEmpleado(payload: CrearEmpleadoDto): Promise<Empleado> {
    const { data } = await api.post<Empleado>('/Empleados', payload);
    return data;
  },

  async deleteEmpleado(empleadoId: number): Promise<void> {
    await api.delete(`/Empleados/${empleadoId}`);
  },

  // --- Pedidos ---
  async getPedidosPorSucursal(sucursalId: number, estado?: string): Promise<Pedido[]> {
    const qs = estado ? `?estado=${encodeURIComponent(estado)}` : '';
    const { data } = await api.get<Pedido[]>(`/Pedidos/sucursal/${sucursalId}${qs}`);
    return data;
  },

  async cambiarEstadoPedido(pedidoId: number, nuevoEstadoId: number): Promise<void> {
    await api.patch(`/Pedidos/${pedidoId}/estado`, { nuevoEstadoId });
  },

  async createPedido(payload: CrearPedidoDto): Promise<void> {
    await api.post('/Pedidos', payload);
  },

  // --- Maestros ---
  async getMetodosPago(): Promise<MetodoPago[]> {
    const { data } = await api.get<MetodoPago[]>('/Maestros/pagos');
    return data;
  },

  async getTiposEntrega(): Promise<TipoEntrega[]> {
    const { data } = await api.get<TipoEntrega[]>('/Maestros/entregas');
    return data;
  }
};
