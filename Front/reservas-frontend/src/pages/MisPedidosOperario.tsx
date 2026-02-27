import React, { useState, useEffect } from 'react';
import { DashboardLayout } from '@components/layout/DashboardLayout';
import { Alert } from '@components/common/Alert';
import { useAuth } from '@context/AuthContext';
import { pedidosService } from '../service/PedidosService';
import { OrderSummaryDTO } from '../types/pedido.types';
import { DetallePedidoModal } from '../components/pedidos/DetallePedidoModal';
import { Eye, LayoutGrid, List } from 'lucide-react';

const MisPedidosOperario = () => {
    const { user } = useAuth();
    const [pedidos, setPedidos] = useState<OrderSummaryDTO[]>([]);
    const [loading, setLoading] = useState(true);
    const [view, setView] = useState<'tabla' | 'kanban'>('tabla');
    
    const [selectedPedido, setSelectedPedido] = useState<OrderSummaryDTO | null>(null);
    const [modalDetalleOpen, setModalDetalleOpen] = useState(false);

    // Lógica de colores unificada (Estricta)
    const getEstadoStyle = (estado: string, estaDemorado: boolean) => {
        if (estaDemorado) return 'bg-orange-100 text-orange-600 border-orange-200';
        
        const est = estado.toLowerCase();
        switch (est) {
            case 'sin preparar':
                return 'bg-gray-100 text-gray-400 border-gray-200';
            case 'preparar pedido':
            case 'preparando':
            case 'en preparación':
                return 'bg-blue-100 text-blue-600 border-blue-200';
            case 'demorado':
                return 'bg-orange-100 text-orange-600 border-orange-200';
            case 'listo para despachar':
                return 'bg-green-100 text-green-600 border-green-200';
            case 'en camino':
            case 'despachando':
                return 'bg-indigo-100 text-indigo-600 border-indigo-200';
            case 'entregado':
                return 'bg-emerald-100 text-emerald-700 border-emerald-200';
            case 'cancelado':
            case 'entrega fallida':
                return 'bg-red-100 text-red-600 border-red-200';
            default:
                return 'bg-gray-50 text-gray-500 border-gray-100';
        }
    };

    const loadPedidos = async () => {
        try {
            setLoading(true);
            const data = await pedidosService.getPedidosByRol('Operario', user!.id);
            // Filtramos para que solo vea lo que tiene que preparar (excluimos listos)
            const pendientes = data.filter(p => p.estadoNombre !== 'Listo para despachar');
            setPedidos(pendientes);
        } catch (error) {
            console.error('Error al cargar pedidos del operario:', error);
        } finally {
            setLoading(false);
        }
    };

    useEffect(() => {
        if (user?.id) loadPedidos();
    }, [user?.id]);

    const getInitials = (name: string) => name.split(' ').map(n => n[0]).join('').substring(0, 2).toUpperCase();

    return (
        <DashboardLayout>
            <div className="space-y-6 font-sans">
                <div className="flex flex-col md:flex-row md:items-center justify-between gap-4">
                    <div className="flex items-center gap-3">
                        <div className="w-2 h-8 bg-indigo-500 rounded-full"></div>
                        <h1 className="text-2xl font-bold text-gray-900">Mis Pedidos Asignados</h1>
                    </div>

                    <div className="flex items-center gap-2 bg-gray-100 p-1 rounded-xl w-fit">
                        <button 
                            onClick={() => setView('tabla')}
                            className={`flex items-center gap-2 px-4 py-2 rounded-lg text-sm font-medium transition-all ${view === 'tabla' ? 'bg-white shadow-sm text-indigo-600' : 'text-gray-500 hover:text-gray-700'}`}
                        >
                            <List className="w-4 h-4" /> Tabla
                        </button>
                        <button disabled className="flex items-center gap-2 px-4 py-2 rounded-lg text-sm font-medium text-gray-400 cursor-not-allowed">
                            <LayoutGrid className="w-4 h-4" /> Kanban
                        </button>
                    </div>
                </div>

                {loading ? (
                    <div className="flex justify-center py-10">
                        <div className="animate-spin rounded-full h-10 w-10 border-b-2 border-indigo-600"></div>
                    </div>
                ) : pedidos.length === 0 ? (
                    <Alert type="info">No tienes pedidos pendientes de preparación.</Alert>
                ) : (
                    <div className="bg-white rounded-2xl shadow-sm border border-gray-100 overflow-hidden">
                        <table className="w-full text-sm">
                            <thead className="bg-gray-50 text-gray-500 font-semibold uppercase text-[11px]">
                                <tr>
                                    <th className="p-4 text-left">ID</th>
                                    <th className="p-4 text-left">Fecha</th>
                                    <th className="p-4 text-left">Cliente</th>
                                    <th className="p-4 text-left">Estado</th>
                                    <th className="p-4 text-left">Total</th>
                                    <th className="p-4 text-right">Acciones</th>
                                </tr>
                            </thead>
                            <tbody className="divide-y divide-gray-100">
                                {pedidos.map((pedido) => (
                                    <tr key={pedido.idPedido} className="hover:bg-gray-50 transition-colors">
                                        <td className="p-4 font-bold text-gray-900">#{pedido.idPedido}</td>
                                        <td className="p-4 text-gray-600">
                                            {pedido.fecha ? new Date(pedido.fecha).toLocaleDateString('es-AR') : '-'}
                                        </td>
                                        <td className="p-4">
                                            <div className="flex items-center gap-2">
                                                <div className="w-7 h-7 bg-indigo-500 rounded-full flex items-center justify-center text-white text-[10px] font-bold">
                                                    {getInitials(pedido.clienteNombre)}
                                                </div>
                                                <span className="font-medium text-gray-700">{pedido.clienteNombre}</span>
                                            </div>
                                        </td>
                                        <td className="p-4">
                                            <span className={`inline-flex items-center px-3 py-1 rounded-full text-[10px] font-bold border ${getEstadoStyle(pedido.estadoNombre, pedido.estaDemorado)}`}>
                                                {pedido.estadoNombre.toUpperCase()}
                                            </span>
                                        </td>
                                        <td className="p-4 text-gray-600 font-mono">${pedido.total?.toFixed(2)}</td>
                                        <td className="p-4 text-right">
                                            <button 
                                                onClick={() => { setSelectedPedido(pedido); setModalDetalleOpen(true); }}
                                                className="text-blue-600 hover:text-blue-800 font-bold flex items-center justify-end gap-1 ml-auto"
                                            >
                                                <Eye className="w-4 h-4" /> Ver detalle
                                            </button>
                                        </td>
                                    </tr>
                                ))}
                            </tbody>
                        </table>
                    </div>
                )}
            </div>

            {selectedPedido && (
                <DetallePedidoModal
                    isOpen={modalDetalleOpen}
                    onClose={() => setModalDetalleOpen(false)}
                    pedido={selectedPedido}
                />
            )}
        </DashboardLayout>
    );
};

export default MisPedidosOperario;