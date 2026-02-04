import React, { useState, useEffect } from 'react';
import { toast } from 'sonner';
import type { Producto, Categoria, ProductoCreateReq, ProductoUpdateReq } from '../../../types/db';
import { sucursalService } from '../../../services/sucursal';

interface ProductManagerProps {
    sucursalId: number;
    initialProductos: Producto[];
    categorias: Categoria[];
}

export default function ProductManager({
    sucursalId,
    initialProductos,
    categorias
}: ProductManagerProps) {
    const [localCategories, setLocalCategories] = useState<Categoria[]>(categorias);
    const [productos, setProductos] = useState<Producto[]>(initialProductos);
    const [search, setSearch] = useState('');
    const [isModalOpen, setIsModalOpen] = useState(false);
    const [editingProduct, setEditingProduct] = useState<Producto | null>(null);
    const [loading, setLoading] = useState(false);
    const [categoryModalOpen, setCategoryModalOpen] = useState(false);
    const [newCategoryName, setNewCategoryName] = useState('');

    // Form State
    const [formData, setFormData] = useState<ProductoCreateReq>({
        nombre: '',
        descripcion: '',
        precio: 0,
        stock: 0,
        urlImagen: '',
        categoriaId: localCategories[0]?.id || 0,
        sucursalId
    });

    useEffect(() => {
        // Reset form when modal opens/closes or editing changes
        if (editingProduct) {
            setFormData({
                nombre: editingProduct.nombre,
                descripcion: editingProduct.descripcion,
                precio: editingProduct.precio,
                stock: editingProduct.stock,
                urlImagen: editingProduct.urlImagen || '',
                categoriaId: editingProduct.categoriaId || localCategories[0]?.id || 0,
                sucursalId
            });
        } else {
            setFormData({
                nombre: '',
                descripcion: '',
                precio: 0,
                stock: 0,
                urlImagen: '',
                categoriaId: localCategories[0]?.id || 0,
                sucursalId
            });
        }
    }, [editingProduct, isModalOpen, localCategories, sucursalId]);

    const filteredProducts = productos.filter(p =>
        p.nombre.toLowerCase().includes(search.toLowerCase()) ||
        p.categoriaNombre?.toLowerCase().includes(search.toLowerCase())
    );

    const handleSubmit = async (e: React.FormEvent) => {
        e.preventDefault();
        setLoading(true);
        try {
            if (editingProduct) {
                const payload: ProductoUpdateReq = { ...formData, id: editingProduct.id };
                await sucursalService.updateProducto(payload);
                toast.success('Producto actualizado');
            } else {
                await sucursalService.createProducto(formData);
                toast.success('Producto creado');
            }
            const updated = await sucursalService.getProductos(sucursalId);
            setProductos(updated);
            setIsModalOpen(false);
            setEditingProduct(null);
        } catch (error) {
            console.error(error);
            toast.error('Error al guardar producto');
        } finally {
            setLoading(false);
        }
    };

    const handleDelete = async (id: number) => {
        if (!confirm('¿Estás seguro de eliminar este producto?')) return;
        try {
            await sucursalService.deleteProducto(id);
            toast.success('Producto eliminado');
            setProductos(productos.filter(p => p.id !== id));
        } catch (error) {
            console.error(error);
            toast.error('Error al eliminar producto');
        }
    };

    const handleCreateCategory = async () => {
        if (!newCategoryName.trim()) return;
        try {
            await sucursalService.createCategoria({ nombre: newCategoryName, sucursalId });
            const updatedCats = await sucursalService.getCategoriasBySucursal(sucursalId);
            setLocalCategories(updatedCats);
            toast.success('Categoría creada exitosamente');
            setCategoryModalOpen(false);
            setNewCategoryName('');

            // Also update current form category if it's the first one or simply selecting the new one
            // Optionally set the new category as selected in form
            const newCat = updatedCats.find(c => c.nombre === newCategoryName);
            if (newCat) {
                setFormData(prev => ({ ...prev, categoriaId: newCat.id }));
            }
        } catch (error) {
            toast.error('Error al crear categoría');
        }
    };

    return (
        <div className="flex flex-col h-full space-y-4 animate-fadeIn">
            {/* Header Actions */}
            <div className="flex flex-col sm:flex-row justify-between items-start sm:items-center gap-4 bg-white p-4 rounded-xl border border-admin-border shadow-sm">
                <div className="relative w-full sm:w-64">
                    <svg className="absolute left-3 top-1/2 -translate-y-1/2 w-4 h-4 text-admin-text-secondary" fill="none" viewBox="0 0 24 24" stroke="currentColor">
                        <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M21 21l-6-6m2-5a7 7 0 11-14 0 7 7 0 0114 0z" />
                    </svg>
                    <input
                        type="text"
                        placeholder="Buscar productos..."
                        className="w-full pl-9 pr-4 py-2 bg-admin-surface border border-admin-border rounded-lg text-sm focus:ring-2 focus:ring-admin-primary/20 focus:border-admin-primary outline-none transition-all"
                        value={search}
                        onChange={(e) => setSearch(e.target.value)}
                    />
                </div>
                <button
                    onClick={() => { setEditingProduct(null); setIsModalOpen(true); }}
                    className="px-4 py-2 bg-admin-primary text-white rounded-lg text-sm font-medium shadow-lg shadow-admin-primary/25 hover:bg-admin-primary-dark transition-all flex items-center gap-2"
                >
                    <svg className="w-4 h-4" fill="none" viewBox="0 0 24 24" stroke="currentColor">
                        <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M12 4v16m8-8H4" />
                    </svg>
                    Nuevo Producto
                </button>
            </div>

            {/* Table */}
            <div className="flex-1 overflow-hidden bg-white rounded-xl border border-admin-border shadow-sm flex flex-col">
                <div className="overflow-x-auto">
                    <table className="w-full text-left text-sm">
                        <thead className="bg-admin-surface border-b border-admin-border text-admin-text-secondary font-medium">
                            <tr>
                                <th className="px-6 py-4">Producto</th>
                                <th className="px-6 py-4">Categoría</th>
                                <th className="px-6 py-4">Precio</th>
                                <th className="px-6 py-4">Stock</th>
                                <th className="px-6 py-4 text-right">Acciones</th>
                            </tr>
                        </thead>
                        <tbody className="divide-y divide-admin-border text-admin-text">
                            {filteredProducts.map((product) => (
                                <tr key={product.id} className="hover:bg-admin-surface/50 transition-colors group">
                                    <td className="px-6 py-4">
                                        <div className="flex items-center gap-3">
                                            <div className="w-10 h-10 rounded-lg bg-gray-100 overflow-hidden shrink-0 border border-admin-border">
                                                {product.urlImagen ? (
                                                    <img src={product.urlImagen} alt="" className="w-full h-full object-cover" />
                                                ) : (
                                                    <div className="w-full h-full flex items-center justify-center text-gray-300">ISO</div>
                                                )}
                                            </div>
                                            <div>
                                                <div className="font-medium">{product.nombre}</div>
                                                <div className="text-xs text-admin-text-secondary truncate max-w-[200px]">{product.descripcion}</div>
                                            </div>
                                        </div>
                                    </td>
                                    <td className="px-6 py-4">
                                        <span className="px-2 py-1 rounded-full bg-blue-50 text-blue-600 text-xs font-medium">
                                            {product.categoriaNombre || 'Sin Categoría'}
                                        </span>
                                    </td>
                                    <td className="px-6 py-4 font-medium">${product.precio}</td>
                                    <td className="px-6 py-4">
                                        <span className={`px-2 py-1 rounded-full text-xs font-medium ${product.stock < 5 ? 'bg-red-50 text-red-600' : 'bg-green-50 text-green-600'}`}>
                                            {product.stock} u.
                                        </span>
                                    </td>
                                    <td className="px-6 py-4 text-right">
                                        <div className="flex items-center justify-end gap-2 opacity-0 group-hover:opacity-100 transition-opacity">
                                            <button
                                                onClick={() => { setEditingProduct(product); setIsModalOpen(true); }}
                                                className="p-1.5 text-admin-text-secondary hover:text-admin-primary hover:bg-admin-primary/10 rounded-lg transition-colors"
                                            >
                                                <svg className="w-4 h-4" fill="none" viewBox="0 0 24 24" stroke="currentColor"><path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M15.232 5.232l3.536 3.536m-2.036-5.036a2.5 2.5 0 113.536 3.536L6.5 21.036H3v-3.572L16.732 3.732z" /></svg>
                                            </button>
                                            <button
                                                onClick={() => handleDelete(product.id)}
                                                className="p-1.5 text-admin-text-secondary hover:text-red-600 hover:bg-red-50 rounded-lg transition-colors"
                                            >
                                                <svg className="w-4 h-4" fill="none" viewBox="0 0 24 24" stroke="currentColor"><path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M19 7l-.867 12.142A2 2 0 0116.138 21H7.862a2 2 0 01-1.995-1.858L5 7m5 4v6m4-6v6m1-10V4a1 1 0 00-1-1h-4a1 1 0 00-1 1v3M4 7h16" /></svg>
                                            </button>
                                        </div>
                                    </td>
                                </tr>
                            ))}
                            {filteredProducts.length === 0 && (
                                <tr>
                                    <td colSpan={5} className="px-6 py-12 text-center text-admin-text-secondary">
                                        No se encontraron productos.
                                    </td>
                                </tr>
                            )}
                        </tbody>
                    </table>
                </div>
            </div>

            {/* Edit/Create Modal */}
            {isModalOpen && (
                <div className="fixed inset-0 z-50 flex items-center justify-center p-4 bg-black/50 backdrop-blur-sm animate-fadeIn">
                    <div className="bg-white rounded-2xl shadow-2xl w-full max-w-lg overflow-hidden flex flex-col max-h-[90vh]">
                        <div className="p-4 border-b border-admin-border flex justify-between items-center bg-admin-surface">
                            <h3 className="font-bold text-lg text-admin-text">
                                {editingProduct ? 'Editar Producto' : 'Nuevo Producto'}
                            </h3>
                            <button onClick={() => setIsModalOpen(false)} className="text-admin-text-secondary hover:text-admin-text">
                                <svg className="w-5 h-5" fill="none" viewBox="0 0 24 24" stroke="currentColor"><path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M6 18L18 6M6 6l12 12" /></svg>
                            </button>
                        </div>

                        <form onSubmit={handleSubmit} className="p-6 overflow-y-auto flex flex-col gap-4">
                            <div className="space-y-1">
                                <label className="text-xs font-semibold text-admin-text-secondary uppercase">Nombre</label>
                                <input required type="text" className="w-full px-3 py-2 bg-admin-surface border border-admin-border rounded-lg text-sm outline-none focus:border-admin-primary focus:ring-2 focus:ring-admin-primary/20 transition-all"
                                    value={formData.nombre} onChange={e => setFormData({ ...formData, nombre: e.target.value })}
                                />
                            </div>

                            <div className="grid grid-cols-2 gap-4">
                                <div className="space-y-1">
                                    <label className="text-xs font-semibold text-admin-text-secondary uppercase">Precio</label>
                                    <input required type="number" min="0" step="0.01" className="w-full px-3 py-2 bg-admin-surface border border-admin-border rounded-lg text-sm outline-none focus:border-admin-primary focus:ring-2 focus:ring-admin-primary/20 transition-all"
                                        value={formData.precio} onChange={e => setFormData({ ...formData, precio: parseFloat(e.target.value) })}
                                    />
                                </div>
                                <div className="space-y-1">
                                    <label className="text-xs font-semibold text-admin-text-secondary uppercase">Stock</label>
                                    <input required type="number" min="0" className="w-full px-3 py-2 bg-admin-surface border border-admin-border rounded-lg text-sm outline-none focus:border-admin-primary focus:ring-2 focus:ring-admin-primary/20 transition-all"
                                        value={formData.stock} onChange={e => setFormData({ ...formData, stock: parseInt(e.target.value) })}
                                    />
                                </div>
                            </div>

                            <div className="space-y-1">
                                <label className="text-xs font-semibold text-admin-text-secondary uppercase">Categoría</label>
                                <div className="flex gap-2">
                                    <select className="flex-1 px-3 py-2 bg-admin-surface border border-admin-border rounded-lg text-sm outline-none focus:border-admin-primary focus:ring-2 focus:ring-admin-primary/20 transition-all"
                                        value={formData.categoriaId} onChange={e => setFormData({ ...formData, categoriaId: Number(e.target.value) })}
                                    >
                                        {localCategories.map(c => <option key={c.id} value={c.id}>{c.nombre}</option>)}
                                    </select>
                                    <button type="button" onClick={() => setCategoryModalOpen(true)} className="px-3 py-2 bg-admin-surface border border-admin-border rounded-lg hover:bg-admin-border text-admin-text-secondary">
                                        +
                                    </button>
                                </div>
                            </div>

                            <div className="space-y-1">
                                <label className="text-xs font-semibold text-admin-text-secondary uppercase">URL Imagen</label>
                                <input type="url" className="w-full px-3 py-2 bg-admin-surface border border-admin-border rounded-lg text-sm outline-none focus:border-admin-primary focus:ring-2 focus:ring-admin-primary/20 transition-all"
                                    value={formData.urlImagen || ''} onChange={e => setFormData({ ...formData, urlImagen: e.target.value })}
                                />
                            </div>

                            <div className="space-y-1">
                                <label className="text-xs font-semibold text-admin-text-secondary uppercase">Descripción</label>
                                <textarea className="w-full px-3 py-2 bg-admin-surface border border-admin-border rounded-lg text-sm outline-none focus:border-admin-primary focus:ring-2 focus:ring-admin-primary/20 transition-all resize-none h-24"
                                    value={formData.descripcion} onChange={e => setFormData({ ...formData, descripcion: e.target.value })}
                                />
                            </div>

                            <div className="flex justify-end gap-3 pt-4 border-t border-admin-border">
                                <button type="button" onClick={() => setIsModalOpen(false)} className="px-4 py-2 rounded-lg text-sm font-medium text-admin-text-secondary hover:bg-admin-surface transition-all">Cancelar</button>
                                <button type="submit" disabled={loading} className="px-4 py-2 bg-admin-primary text-white rounded-lg text-sm font-medium shadow-lg shadow-admin-primary/25 hover:bg-admin-primary-dark transition-all">
                                    {loading ? 'Guardando...' : 'Guardar Producto'}
                                </button>
                            </div>
                        </form>
                    </div>
                </div>
            )}

            {/* Category Modal - simplified */}
            {categoryModalOpen && (
                <div className="fixed inset-0 z-[60] flex items-center justify-center p-4 bg-black/50 backdrop-blur-sm">
                    <div className="bg-white rounded-xl shadow-2xl p-6 w-full max-w-sm">
                        <h3 className="font-bold text-lg mb-4">Nueva Categoría</h3>
                        <input
                            autoFocus
                            type="text"
                            placeholder="Nombre de la categoría"
                            className="w-full mb-4 px-3 py-2 border rounded-lg"
                            value={newCategoryName}
                            onChange={e => setNewCategoryName(e.target.value)}
                        />
                        <div className="flex justify-end gap-2">
                            <button onClick={() => setCategoryModalOpen(false)} className="px-3 py-1.5 text-sm text-gray-500">Cancelar</button>
                            <button onClick={handleCreateCategory} className="px-3 py-1.5 text-sm bg-admin-primary text-white rounded-lg">Crear</button>
                        </div>
                    </div>
                </div>
            )}
        </div>
    );
}
