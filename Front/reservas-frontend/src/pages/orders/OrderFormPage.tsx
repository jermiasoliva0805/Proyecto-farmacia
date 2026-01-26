import React, { useState } from 'react';
import { Plus, Trash2, User, Package, Save, ArrowLeft, Filter, Clock, CheckCircle2, Truck, AlertCircle } from 'lucide-react';
import { SearchableSelect } from '@components/common/SearchableSelect';
import { useNavigate } from 'react-router-dom';

const OrderFormPage: React.FC = () => {
    const navigate = useNavigate();
    const [items, setItems] = useState([{ tempId: Date.now(), productId: '', quantity: 1 }]);
    
    // Estado para el filtro seleccionado
    const [filterStatus, setFilterStatus] = useState('Todos');

    // Definición de los botones de estado
    const estados = [
        { id: 'Todos', label: 'Todos', icon: Filter, color: 'text-gray-500', bg: 'bg-gray-100' },
        { id: 'Pendiente', label: 'Pendientes', icon: Clock, color: 'text-amber-500', bg: 'bg-amber-100' },
        { id: 'Procesado', label: 'Procesados', icon: CheckCircle2, color: 'text-blue-500', bg: 'bg-blue-100' },
        { id: 'Enviado', label: 'Enviados', icon: Truck, color: 'text-purple-500', bg: 'bg-purple-100' },
    ];

    // Datos de ejemplo para demostrar el filtrado
    const allProducts = [
        { id: '1', name: 'Producto A', status: 'Pendiente' },
        { id: '2', name: 'Producto B', status: 'Procesado' },
        { id: '3', name: 'Producto C', status: 'Enviado' },
        { id: '4', name: 'Producto D', status: 'Pendiente' },
    ];

    // Lógica de filtrado
    const filteredProducts = filterStatus === 'Todos' 
        ? allProducts 
        : allProducts.filter(p => p.status === filterStatus);

    return (
        <div className="min-h-screen bg-gray-50 p-4 lg:p-8">
            <div className="max-w-4xl mx-auto">
                <div className="flex items-center gap-4 mb-6">
                    <button onClick={() => navigate(-1)} className="p-2 hover:bg-gray-200 rounded-full text-gray-600">
                        <ArrowLeft size={20} />
                    </button>
                    <h1 className="text-2xl font-bold text-gray-800">Crear Nuevo Pedido</h1>
                </div>

                {/* Filtros por Estado */}
                <div className="bg-white p-2 rounded-2xl shadow-sm border border-gray-200 mb-6 flex gap-2 overflow-x-auto">
                    {estados.map((estado) => {
                        const Icon = estado.icon;
                        const isActive = filterStatus === estado.id;
                        return (
                            <button
                                key={estado.id}
                                onClick={() => setFilterStatus(estado.id)}
                                className={`flex items-center gap-2 px-4 py-2 rounded-xl transition-all font-medium whitespace-nowrap ${
                                    isActive ? `${estado.bg} ${estado.color} ring-1 ring-inset ring-black/5` : 'text-gray-500 hover:bg-gray-50'
                                }`}
                            >
                                <Icon size={18} />
                                {estado.label}
                            </button>
                        );
                    })}
                </div>

                <div className="bg-white rounded-2xl shadow-sm border border-gray-200 overflow-hidden">
                    <div className="bg-blue-600 px-6 py-4">
                        <h2 className="text-white font-semibold flex items-center gap-2">
                            <Plus size={20} /> Datos del Pedido
                        </h2>
                    </div>

                    <div className="p-6 space-y-8">
                        <div className="max-w-md">
                            <SearchableSelect 
                                label="Cliente / Paciente" 
                                options={[]} 
                                onSelect={() => {}} 
                                icon={User} 
                                placeholder="Buscar cliente..." 
                            />
                        </div>

                        {/* Listado de Productos Filtrados */}
                        <div className="space-y-4">
                            <h3 className="text-lg font-bold text-gray-800 flex items-center gap-2">
                                <Package size={20} className="text-blue-600" />
                                Productos ({filterStatus})
                            </h3>
                            
                            <div className="grid gap-3">
                                {filteredProducts.map((product) => (
                                    <div key={product.id} className="flex items-center justify-between p-4 bg-gray-50 rounded-xl border border-gray-100">
                                        <div>
                                            <span className="font-semibold">{product.name}</span>
                                            <span className="ml-3 text-xs font-bold uppercase px-2 py-0.5 rounded bg-white text-gray-500 border">
                                                {product.status}
                                            </span>
                                        </div>
                                        <button className="text-red-400 hover:text-red-600">
                                            <Trash2 size={18} />
                                        </button>
                                    </div>
                                ))}
                                
                                {filteredProducts.length === 0 && (
                                    <div className="text-center py-8 text-gray-400 italic">
                                        No hay productos con estado {filterStatus}
                                    </div>
                                )}
                            </div>
                        </div>

                        <div className="pt-6 border-t flex justify-end gap-3">
                            <button onClick={() => navigate(-1)} className="px-6 py-2 text-gray-600 hover:bg-gray-50">
                                Cancelar
                            </button>
                            <button className="bg-blue-600 text-white px-10 py-2 rounded-xl font-bold flex items-center gap-2">
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