import React, { useState, useEffect } from 'react';
import { Plus, Trash2, User, Package, Save, ArrowLeft, ShoppingCart } from 'lucide-react';
import { SearchableSelect } from '@/components/common/SearchableSelect';
import { useNavigate } from 'react-router-dom';
import { pedidosService } from '@/service/PedidosService';
import { catalogoService } from '@/service/catalogoService';
import { ClientDTO, ProductDTO } from '@/types/common.types';

const OrderFormPage: React.FC = () => {
    const navigate = useNavigate();
    const [clienteId, setClienteId] = useState<string>('');
    const [items, setItems] = useState([{ tempId: Date.now(), productId: '', quantity: 1 }]);
    const [clientes, setClientes] = useState<ClientDTO[]>([]);
    const [productos, setProductos] = useState<ProductDTO[]>([]);
    const [loading, setLoading] = useState(true);
    const [error, setError] = useState<string>('');

    useEffect(() => {
        loadData();
    }, []);

    const loadData = async () => {
        try {
            setLoading(true);
            setError('');
            const [clientesData, productosData] = await Promise.all([
                catalogoService.getClientes().catch(err => {
                    console.error('Error al cargar clientes:', err);
                    return [];
                }),
                catalogoService.getProductos().catch(err => {
                    console.error('Error al cargar productos:', err);
                    return [];
                })
            ]);
            
            setClientes(clientesData || []);
            setProductos(productosData || []);
            
            if (!clientesData?.length) {
                setError('No hay clientes disponibles');
            }
            if (!productosData?.length) {
                setError(prev => prev ? prev + ' | No hay productos disponibles' : 'No hay productos disponibles');
            }
        } catch (error) {
            console.error('Error al cargar datos:', error);
            setError('Error al cargar datos del servidor');
        } finally {
            setLoading(false);
        }
    };

    const addProduct = () => {
        setItems([...items, { tempId: Date.now(), productId: '', quantity: 1 }]);
    };

    const removeProduct = (tempId: number) => {
        if (items.length > 1) {
            setItems(items.filter(item => item.tempId !== tempId));
        }
    };

    // Función para actualizar un producto específico en la lista
    const updateProductInList = (index: number, prodId: string) => {
        const newItems = [...items];
        newItems[index].productId = prodId;
        setItems(newItems);
    };

    const updateQuantity = (index: number, qty: number) => {
        const newItems = [...items];
        newItems[index].quantity = qty;
        setItems(newItems);
    };

    const handleSave = async () => {
        if (!clienteId) return alert("Por favor selecciona un cliente");
        
        const pedido = {
            idCliente: parseInt(clienteId),
            fecha: new Date().toISOString(),
            detalles: items
                .filter(i => i.productId !== '') // Solo enviamos los que tienen producto seleccionado
                .map(i => ({
                    idProducto: parseInt(i.productId),
                    cantidad: i.quantity
                }))
        };

        if (pedido.detalles.length === 0) return alert("Agrega al menos un producto");

        try {
            await pedidosService.createOrder(pedido);
            alert("Pedido creado con éxito");
            navigate('/pedidos'); 
        } catch (error) {
            console.error("Error al guardar:", error);
            alert("Error al guardar el pedido");
        }
    };

    return (
        <div className="min-h-screen bg-gray-50 p-4 lg:p-8">
            <div className="max-w-4xl mx-auto">
                <div className="flex items-center gap-4 mb-6">
                    <button onClick={() => navigate(-1)} className="p-2 hover:bg-gray-200 rounded-full text-gray-600">
                        <ArrowLeft size={20} />
                    </button>
                    <h1 className="text-2xl font-bold text-gray-800">Crear Nuevo Pedido</h1>
                </div>

                <div className="bg-white rounded-2xl shadow-sm border border-gray-200 overflow-hidden">
                    <div className="bg-blue-600 px-6 py-4 text-white font-semibold flex items-center gap-2">
                        <Plus size={20} /> Datos del Pedido
                    </div>

                    {error && (
                        <div className="px-6 py-3 bg-red-50 border-l-4 border-red-500 text-red-700">
                            {error}
                        </div>
                    )}

                    <div className="p-6 space-y-8">
                        {/* Selección de Cliente */}
                        <div className="max-w-md">
                            <SearchableSelect 
                                label="Cliente / Paciente" 
                                options={clientes.map(c => ({
                                    id: c.id,
                                    label: `${c.nombre || 'Sin nombre'} ${c.apellido || ''}`.trim()
                                }))}
                                onSelect={(opt: any) => setClienteId(String(opt.id))} 
                                icon={User} 
                                placeholder={loading ? "Cargando clientes..." : clientes.length === 0 ? "No hay clientes" : "Seleccione un cliente..."} 
                            />
                        </div>

                        {/* Listado de Productos */}
                        <div className="space-y-4">
                            <h3 className="text-lg font-medium text-gray-700 flex items-center gap-2">
                                <Package size={18} /> Productos
                            </h3>
                            
                            {items.map((item, index) => (
                                <div key={item.tempId} className="flex items-end gap-3 p-4 bg-gray-50 rounded-xl border border-gray-100">
                                    <div className="flex-1">
                                        <SearchableSelect 
                                            label={`Producto #${index + 1}`}
                                            options={productos.map(p => ({
                                                id: p.id,
                                                label: `${p.nombre || 'Sin nombre'} - $${(p.precio || 0).toFixed(2)} (Stock: ${p.stock || 0})`
                                            }))}
                                            onSelect={(opt: any) => updateProductInList(index, String(opt.id))}
                                            icon={ShoppingCart}
                                            placeholder={loading ? "Cargando productos..." : productos.length === 0 ? "No hay productos" : "Buscar producto..."}
                                        />
                                    </div>
                                    <div className="w-24">
                                        <label className="block text-xs font-bold text-gray-500 mb-1 uppercase">Cant.</label>
                                        <input 
                                            type="number" 
                                            min="1"
                                            value={item.quantity}
                                            onChange={(e) => updateQuantity(index, parseInt(e.target.value) || 1)}
                                            className="w-full p-2 border border-gray-300 rounded-lg outline-none focus:ring-2 focus:ring-blue-500"
                                        />
                                    </div>
                                    <button 
                                        onClick={() => removeProduct(item.tempId)}
                                        className="p-2.5 text-red-500 hover:bg-red-50 rounded-lg transition-colors"
                                    >
                                        <Trash2 size={20} />
                                    </button>
                                </div>
                            ))}

                            <button 
                                onClick={addProduct}
                                className="w-full py-3 border-2 border-dashed border-gray-300 rounded-xl text-gray-500 hover:border-blue-500 hover:text-blue-500 flex items-center justify-center gap-2 font-medium"
                            >
                                <Plus size={18} /> Agregar otro producto
                            </button>
                        </div>
                        
                        <div className="pt-6 border-t flex justify-end gap-3">
                            <button onClick={() => navigate(-1)} className="px-6 py-2 text-gray-600 hover:bg-gray-100 rounded-xl">
                                Cancelar
                            </button>
                            <button 
                                onClick={handleSave}
                                className="bg-blue-600 hover:bg-blue-700 text-white px-10 py-2 rounded-xl font-bold flex items-center gap-2 shadow-lg shadow-blue-200"
                            >
                                <Save size={18} /> Guardar Pedido
                            </button>
                        </div>
                    </div>
                </div>
            </div>
        </div>
    );
};

export default OrderFormPage;