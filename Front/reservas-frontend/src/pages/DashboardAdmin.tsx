import React, { useState, useEffect } from 'react';
import { DashboardLayout } from '../components/layout/DashboardLayout.tsx';
import { Card } from '../components/common/Card.tsx';
import { Badge } from '../components/common/Badge.tsx';
import { Button } from '../components/common/Button';
import { OrderFilters } from '../components/orders/OrderFilters';
import { pedidosService } from '../service/PedidosService';
import { OrderSummaryDTO } from '../types/pedido.types';
import { useAuth } from '../context/AuthContext';
import { Package, AlertTriangle, CheckCircle, XCircle, Plus } from 'lucide-react';
import { useNavigate } from 'react-router-dom';

export const DashboardAdmin: React.FC = () => {
    const { user } = useAuth();
    const navigate = useNavigate();
    
    const [pedidos, setPedidos] = useState<OrderSummaryDTO[]>([]);
    const [loading, setLoading] = useState(true);
    const [stats, setStats] = useState({ activos: 0, demorados: 0, entregados: 0, cancelados: 0 });

    // Carga inicial al montar el componente
    useEffect(() => { 
        loadDashboardData(); 
    }, []);

    // Esta función se encarga de llamar al servicio. 
    // Se ejecuta al inicio y cada vez que el componente OrderFilters cambia algo.
    const loadDashboardData = async (filtros = {}) => {
        try {
            setLoading(true);
            const data = await pedidosService.getFilteredOrders(filtros);
            setPedidos(data);
            
            // Cálculo de estadísticas basado en los nombres de estado que devuelve C#
            setStats({
                activos: data.filter(p => !['Entregado', 'Cancelado'].includes(p.estadoNombre)).length,
                demorados: data.filter(p => p.estaDemorado).length,
                entregados: data.filter(p => p.estadoNombre === 'Entregado').length,
                cancelados: data.filter(p => p.estadoNombre === 'Cancelado').length,
            });
        } catch (error) { 
            console.error("Error al cargar pedidos:", error); 
            setPedidos([]);
        } finally { 
            setLoading(false); 
        }
    };

    return (
        <DashboardLayout>
            <div className="space-y-6">
                {/* Encabezado del Dashboard */}
                <div className="flex justify-between items-center">
                    <div>
                        <h1 className="text-2xl font-bold text-gray-900">Panel de Control</h1>
                        <p className="text-gray-500">Bienvenido, {user?.nombreCompleto}</p>
                    </div>
                    <Button onClick={() => navigate('/pedidos/nuevo')} className="flex items-center gap-2">
                        <Plus size={18} /> Nuevo Pedido
                    </Button>
                </div>

                {/* Tarjetas de Estadísticas */}
                <div className="grid grid-cols-1 md:grid-cols-4 gap-4">
                    <Card>
                        <div className="flex justify-between">
                            <div>
                                <p className="text-xs text-gray-500 font-semibold uppercase">Activos</p>
                                <p className="text-xl font-bold text-gray-800">{stats.activos}</p>
                            </div>
                            <Package className="text-blue-500" size={24}/>
                        </div>
                    </Card>
                    <Card>
                        <div className="flex justify-between">
                            <div>
                                <p className="text-xs text-gray-500 font-semibold uppercase">Demorados</p>
                                <p className="text-xl font-bold text-gray-800">{stats.demorados}</p>
                            </div>
                            <AlertTriangle className="text-yellow-500" size={24}/>
                        </div>
                    </Card>
                    <Card>
                        <div className="flex justify-between">
                            <div>
                                <p className="text-xs text-gray-500 font-semibold uppercase">Entregados</p>
                                <p className="text-xl font-bold text-gray-800">{stats.entregados}</p>
                            </div>
                            <CheckCircle className="text-green-500" size={24}/>
                        </div>
                    </Card>
                    <Card>
                        <div className="flex justify-between">
                            <div>
                                <p className="text-xs text-gray-500 font-semibold uppercase">Cancelados</p>
                                <p className="text-xl font-bold text-gray-800">{stats.cancelados}</p>
                            </div>
                            <XCircle className="text-red-500" size={24}/>
                        </div>
                    </Card>
                </div>

                {/* Componente de Filtros (Buscador y botones de estado) */}
                <OrderFilters 
                    userRole="Administrador" 
                    onFilterChange={loadDashboardData} 
                />

                {/* Tabla de Resultados */}
                <Card>
                    <div className="overflow-x-auto">
                        <table className="w-full text-left text-sm">
                            <thead className="bg-gray-50 border-b">
                                <tr>
                                    <th className="p-4 font-semibold text-gray-600">ID</th>
                                    <th className="p-4 font-semibold text-gray-600">Cliente</th>
                                    <th className="p-4 font-semibold text-gray-600">Estado</th>
                                    <th className="p-4 text-right font-semibold text-gray-600">Acción</th>
                                </tr>
                            </thead>
                            <tbody className="divide-y divide-gray-100">
                                {loading ? (
                                    <tr>
                                        <td colSpan={4} className="p-10 text-center">
                                            <div className="flex flex-col items-center gap-2">
                                                <div className="animate-spin rounded-full h-8 w-8 border-b-2 border-blue-600"></div>
                                                <span className="text-gray-400">Cargando pedidos...</span>
                                            </div>
                                        </td>
                                    </tr>
                                ) : pedidos.length > 0 ? (
                                    pedidos.map(pedido => (
                                        <tr key={pedido.idPedido} className="hover:bg-gray-50 transition-colors">
                                            <td className="p-4 font-medium text-blue-600">#{pedido.idPedido}</td>
                                            <td className="p-4 font-medium text-gray-700">{pedido.clienteNombre}</td>
                                            <td className="p-4">
                                                <Badge variant={pedido.estaDemorado ? 'warning' : 'info'}>
                                                    {pedido.estadoNombre}
                                                </Badge>
                                            </td>
                                            <td className="p-4 text-right">
                                                <button 
                                                    onClick={() => navigate(`/pedidos/${pedido.idPedido}`)} 
                                                    className="text-blue-600 hover:text-blue-800 font-semibold underline underline-offset-4"
                                                >
                                                    Ver detalle
                                                </button>
                                            </td>
                                        </tr>
                                    ))
                                ) : (
                                    <tr>
                                        <td colSpan={4} className="p-10 text-center text-gray-400 italic">
                                            No se encontraron pedidos con los filtros aplicados.
                                        </td>
                                    </tr>
                                )}
                            </tbody>
                        </table>
                    </div>
                </Card>
            </div>
        </DashboardLayout>
    );
};