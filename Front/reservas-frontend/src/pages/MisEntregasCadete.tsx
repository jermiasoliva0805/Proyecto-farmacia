import React, { useState, useEffect } from 'react';
import { DashboardLayout } from '@components/layout/DashboardLayout';
import { Alert } from '@components/common/Alert';
import { TableroKanban } from '@components/kanban/TableroKanban';
import { useAuth } from '@context/AuthContext';
import { pedidosService } from '../service/PedidosService';
import { OrderSummaryDTO } from '../types/pedido.types';
import { DetallePedidoModal } from '../components/pedidos/DetallePedidoModal';
import { Eye, LayoutGrid, List, Truck } from 'lucide-react';

const MisEntregas = () => {
    const { user } = useAuth();
    const [pedidos, setPedidos] = useState<OrderSummaryDTO[]>([]);
    const [loading, setLoading] = useState(true);
    const [view, setView] = useState<'tabla' | 'kanban'>('tabla');
    
    const [selectedPedido, setSelectedPedido] = useState<OrderSummaryDTO | null>(null);
    const [modalDetalleOpen, setModalDetalleOpen] = useState(false);

    // Lógica de colores
    const getEstadoStyle = (estado: string, estaDemorado: boolean) => {
        if (estaDemorado) return 'bg-orange-100 text-orange-600 border-orange-200';
        
        const est = estado.toLowerCase();
        switch (est) {
            case 'despachando':
                return 'bg-indigo-100 text-indigo-600 border-indigo-200';
            case 'en camino':
                return 'bg-purple-100 text-purple-600 border-purple-200';
            case 'entregado':
                return 'bg-emerald-100 text-emerald-700 border-emerald-200';
            case 'entrega fallida':
                return 'bg-red-100 text-red-600 border-red-200';
            default:
                return 'bg-gray-50 text-gray-500 border-gray-100';
        }
    };

    const loadPedidos = async () => {
        try {
            setLoading(true);
            const data = await pedidosService.getPedidosByRol('Cadete', user!.id);
            
            // Obtener fecha de hoy
            const hoy = new Date();
            hoy.setHours(0, 0, 0, 0);
            
            // Mostrar:
            // - Estados [5, 6] (Despachando + En camino): TODOS
            // - Estados [7, 8] (Entregado + Fallo): Solo de HOY
            const entregas = data.filter(p => {
                if ([5, 6].includes(p.idEstadoDePedido)) {
                    return true; // Mostrar todos los activos
                } else if ([7, 8].includes(p.idEstadoDePedido)) {
                    // Solo mostrar entregados/fallidos de hoy
                    const fechaEntrega = p.fechaEntregaReal ? new Date(p.fechaEntregaReal) : null;
                    if (!fechaEntrega) return false;
                    fechaEntrega.setHours(0, 0, 0, 0);
                    return fechaEntrega.getTime() === hoy.getTime();
                }
                return false;
            });
            setPedidos(entregas);
        } catch (error) {
            console.error('Error al cargar entregas del cadete:', error);
        } finally {
            setLoading(false);
        }
    };

    useEffect(() => {
        if (user?.id) loadPedidos();
    }, [user?.id, view]);

    const getInitials = (name: string) => name.split(' ').map(n => n[0]).join('').substring(0, 2).toUpperCase();

    return (
        <DashboardLayout>
            <div className="space-y-4 sm:space-y-6 font-sans px-2 sm:px-0">
                <div className="flex flex-col md:flex-row md:items-center justify-between gap-4">
                    <div className="flex items-center gap-3">
                        <div className="w-2 h-8 bg-purple-500 rounded-full"></div>
                        <div>
                            <h1 className="text-xl sm:text-2xl font-bold text-gray-900">Mis Entregas</h1>
                            <p className="text-xs text-gray-500 mt-1">Gestiona tus entregas y sigue el estado de cada pedido</p>
                        </div>
                    </div>

                    <div className="flex items-center gap-2 bg-gray-100 p-1 rounded-xl w-fit text-xs sm:text-sm">
                        <button 
                            onClick={() => setView('tabla')}
                            className={`flex items-center gap-2 px-3 sm:px-4 py-2 rounded-lg text-xs sm:text-sm font-medium transition-all ${view === 'tabla' ? 'bg-white shadow-sm text-purple-600' : 'text-gray-500 hover:text-gray-700'}`}
                        >
                            <List className="w-4 h-4" /> Tabla
                        </button>
                        <button 
                            onClick={() => setView('kanban')}
                            className={`flex items-center gap-2 px-3 sm:px-4 py-2 rounded-lg text-xs sm:text-sm font-medium transition-all ${view === 'kanban' ? 'bg-white shadow-sm text-purple-600' : 'text-gray-500 hover:text-gray-700'}`}
                        >
                            <LayoutGrid className="w-4 h-4" /> Kanban
                        </button>
                    </div>
                </div>

                {loading ? (
                    <div className="flex justify-center py-10">
                        <div className="animate-spin rounded-full h-10 w-10 border-b-2 border-purple-600"></div>
                    </div>
                ) : pedidos.length === 0 ? (
                    <Alert type="info">No tienes entregas asignadas en esta vista.</Alert>
                ) : view === 'tabla' ? (
                    <>
                        {/* VISTA TABLA DESKTOP */}
                        <div className="hidden md:block bg-white rounded-2xl shadow-sm border border-gray-100 overflow-hidden">
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
                                                    <div className="w-7 h-7 bg-purple-500 rounded-full flex items-center justify-center text-white text-[10px] font-bold">
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
                                            <td className="p-4 text-gray-600 font-mono text-sm">${pedido.total?.toFixed(2)}</td>
                                            <td className="p-4 text-right">
                                                <button 
                                                    onClick={() => { setSelectedPedido(pedido); setModalDetalleOpen(true); }}
                                                    className="text-purple-600 hover:text-purple-800 font-bold flex items-center justify-end gap-1 text-sm"
                                                >
                                                    <Eye className="w-4 h-4" /> Ver detalle
                                                </button>
                                            </td>
                                        </tr>
                                    ))}
                                </tbody>
                            </table>
                        </div>

                        {/* VISTA TARJETAS MOBILE */}
                        <div className="md:hidden space-y-3">
                            {pedidos.map((pedido) => (
                                <div key={pedido.idPedido} className="bg-white rounded-xl border border-gray-100 shadow-sm p-3 sm:p-4">
                                    <div className="flex items-start justify-between gap-3 mb-3">
                                        <div className="flex items-start gap-2 flex-1">
                                            <div className="w-7 h-7 bg-purple-500 rounded-full flex items-center justify-center text-white text-[10px] font-bold flex-shrink-0">
                                                {getInitials(pedido.clienteNombre)}
                                            </div>
                                            <div className="min-w-0">
                                                <p className="font-bold text-gray-900 text-sm">#{pedido.idPedido}</p>
                                                <p className="text-xs text-gray-500 truncate">{pedido.clienteNombre}</p>
                                            </div>
                                        </div>
                                        <span className={`inline-flex items-center px-2 py-1 rounded-full text-[10px] font-bold border flex-shrink-0 ${getEstadoStyle(pedido.estadoNombre, pedido.estaDemorado)}`}>
                                            {pedido.estadoNombre.length > 12 ? pedido.estadoNombre.substring(0, 10) + '...' : pedido.estadoNombre.toUpperCase()}
                                        </span>
                                    </div>
                                    
                                    <div className="grid grid-cols-2 gap-3 mb-3">
                                        <div className="text-xs">
                                            <p className="text-gray-500">Fecha</p>
                                            <p className="font-medium text-gray-900">{pedido.fecha ? new Date(pedido.fecha).toLocaleDateString('es-AR') : '-'}</p>
                                        </div>
                                        <div className="text-xs">
                                            <p className="text-gray-500">Total</p>
                                            <p className="font-medium text-gray-900">${pedido.total?.toFixed(2)}</p>
                                        </div>
                                    </div>

                                    <button 
                                        onClick={() => { setSelectedPedido(pedido); setModalDetalleOpen(true); }}
                                        className="w-full text-purple-600 hover:text-purple-800 font-bold text-xs py-2 border border-purple-200 rounded-lg hover:bg-purple-50 transition-colors flex items-center justify-center gap-2"
                                    >
                                        <Eye className="w-3 h-3" /> Ver detalle
                                    </button>
                                </div>
                            ))}
                        </div>
                    </>
                ) : (
                    <div className="bg-white rounded-2xl shadow-sm border border-gray-100 overflow-hidden">
                        <div className="p-6 bg-gradient-to-r from-purple-600 to-pink-600 text-white">
                            <div className="flex items-center gap-3">
                                <Truck className="w-6 h-6" />
                                <div>
                                    <h2 className="text-lg font-bold">Tablero Kanban - Mis Entregas</h2>
                                    <p className="text-purple-100 text-sm">Estados: Despachando (5) • En Camino (6) • Entregado (7) • Fallo (8)</p>
                                </div>
                            </div>
                        </div>
                        <div className="p-4">
                            <TableroKanban 
                                pedidos={pedidos}
                                onUpdate={loadPedidos}
                                usuarioId={user?.id}
                            />
                        </div>
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

export default MisEntregas;