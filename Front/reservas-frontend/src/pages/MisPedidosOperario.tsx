import React, { useState, useEffect } from 'react';
import { DashboardLayout } from '@components/layout/DashboardLayout';
import { Badge } from '@components/common/Badge';
import { Button } from '@components/common/Button';
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

    // ESTADOS QUE DEBE VER EL OPERARIO: 
    // Excluimos el estado "Listo para despachar" para que no aparezcan una vez finalizados.
    const estadosVivosOperario = [ 2, 3]; // Ajustar según tus IDs de "Sin preparar", "En preparación", etc.

    useEffect(() => {
        const loadPedidos = async () => {
            try {
                setLoading(true);
                const data = await pedidosService.getPedidosByRol('Operario', user!.id);
                // Filtramos para que NO aparezcan los pedidos que ya están en "Listo para despachar"
                const pendientes = data.filter(p => p.estadoNombre !== 'Listo para despachar');
                setPedidos(pendientes);
            } catch (error) {
                console.error('Error al cargar pedidos del operario:', error);
            } finally {
                setLoading(false);
            }
        };
        if (user?.id) loadPedidos();
    }, [user?.id]);

    return (
        <DashboardLayout>
            <div className="space-y-6 font-sans">
                <div className="flex flex-col md:flex-row md:items-center justify-between gap-4">
                    <div className="flex items-center gap-3">
                        <div className="w-2 h-8 bg-indigo-500 rounded-full"></div>
                        <h1 className="text-2xl font-bold text-gray-900">Mis Pedidos Asignados</h1>
                    </div>

                    {/* Selector de Vista (Imagen image_54005a.png) */}
                    <div className="flex items-center gap-2 bg-gray-100 p-1 rounded-xl w-fit">
                        <button 
                            onClick={() => setView('tabla')}
                            className={`flex items-center gap-2 px-4 py-2 rounded-lg text-sm font-medium transition-all ${view === 'tabla' ? 'bg-white shadow-sm text-indigo-600' : 'text-gray-500 hover:text-gray-700'}`}
                        >
                            <List className="w-4 h-4" /> Tabla
                        </button>
                        <button 
                            disabled // Deshabilitado según requerimiento actual
                            className="flex items-center gap-2 px-4 py-2 rounded-lg text-sm font-medium text-gray-400 cursor-not-allowed"
                            title="Próximamente"
                        >
                            <LayoutGrid className="w-4 h-4" /> Kanban
                        </button>
                    </div>
                </div>

                {/* Renderizado de Tabla (image_54783e.png) */}
                {loading ? (
                    <div className="flex justify-center py-10"><div className="animate-spin rounded-full h-10 w-10 border-b-2 border-indigo-600"></div></div>
                ) : pedidos.length === 0 ? (
                    <Alert type="info">No tienes pedidos pendientes de preparación.</Alert>
                ) : (
                    <div className="bg-white rounded-2xl shadow-sm border border-gray-100 overflow-hidden">
                        <table className="w-full text-sm">
                            <thead className="bg-gray-50 text-gray-500 font-semibold">
                                <tr>
                                    <th className="p-4 text-left">ID</th>
                                    <th className="p-4 text-left">CLIENTE</th>
                                    <th className="p-4 text-left">ESTADO</th>
                                    <th className="p-4 text-left">TOTAL</th>
                                    <th className="p-4 text-right">ACCIONES</th>
                                </tr>
                            </thead>
                            <tbody className="divide-y divide-gray-100">
                                {pedidos.map((pedido) => (
                                    <tr key={pedido.idPedido} className="hover:bg-gray-50 transition-colors">
                                        <td className="p-4 font-bold text-gray-900">#{pedido.idPedido}</td>
                                        <td className="p-4">
                                            <div className="flex items-center gap-2">
                                                <div className="w-7 h-7 bg-blue-500 rounded-full flex items-center justify-center text-white text-[10px] font-bold">JP</div>
                                                {pedido.clienteNombre}
                                            </div>
                                        </td>
                                        <td className="p-4">
                                            <span className="px-3 py-1 bg-green-100 text-green-700 rounded-full text-xs font-medium">
                                                {pedido.estadoNombre}
                                            </span>
                                        </td>
                                        <td className="p-4 text-gray-600">${pedido.total?.toFixed(2)}</td>
                                        <td className="p-4 text-right">
                                            <button 
                                                onClick={() => { setSelectedPedido(pedido); setModalDetalleOpen(true); }}
                                                className="text-blue-600 hover:text-blue-800 font-medium flex items-center justify-end gap-1 ml-auto"
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