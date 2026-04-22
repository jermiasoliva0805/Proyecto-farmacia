import React, { useState, useEffect } from 'react';
import { DashboardLayout } from '@components/layout/DashboardLayout';
import { Alert } from '@components/common/Alert';
import { TableroKanban } from '@components/kanban/TableroKanban';
import { useAuth } from '@context/AuthContext';
import { pedidosService } from '../service/PedidosService';
import { OrderSummaryDTO } from '../types/pedido.types';
import { DetallePedidoModal } from '../components/pedidos/DetallePedidoModal';
import { Eye, LayoutGrid, List, PlayCircle, CheckCircle2 } from 'lucide-react';

interface ToastState {
    visible: boolean;
    type: 'success' | 'error';
    message: string;
}

const MisPedidosOperario = () => {
    const { user } = useAuth();
    const [pedidos, setPedidos] = useState<OrderSummaryDTO[]>([]);
    const [loading, setLoading] = useState(true);
    const [view, setView] = useState<'tabla' | 'kanban'>('tabla');
    const [toast, setToast] = useState<ToastState>({ visible: false, type: 'success', message: '' });
    
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
            
            // Obtener fecha de hoy
            const hoy = new Date();
            hoy.setHours(0, 0, 0, 0);
            
            // Si vista es Tabla: mostrar Estado 1 (Sin preparar) y Estado 2 (Preparar pedido) - sin restricción de fecha
            // Si vista es Kanban: mostrar Estados 2, 3 siempre, pero 4 solo de HOY
            if (view === 'tabla') {
                const pendientes = data.filter(p => {
                    return [1, 2].includes(p.idEstadoDePedido);
                });
                setPedidos(pendientes);
            } else {
                const enProceso = data.filter(p => {
                    const fechaPedido = new Date(p.fecha);
                    fechaPedido.setHours(0, 0, 0, 0);
                    if ([2, 3].includes(p.idEstadoDePedido)) return true;
                    if (p.idEstadoDePedido === 4 && fechaPedido.getTime() === hoy.getTime()) return true;
                    return false;
                });
                setPedidos(enProceso);
            }
        } catch (error) {
            console.error('Error al cargar pedidos del operario:', error);
        } finally {
            setLoading(false);
        }
    };

    // --- FUNCIÓN PARA CU25: Iniciar Armado (Estado 1→2) ---
    // En este momento comienza a contar el tiempo para los reportes RF6.4
    const handleCambiarEstado = async (idPedido: number, nuevoEstadoId: number) => {
        try {
            await pedidosService.cambiarEstado({
                idPedido: idPedido,
                idNuevoEstado: nuevoEstadoId,
                idUsuario: user!.id,
                observaciones: "Iniciando armado de pedido - Cronómetro activado ⏱"
            });
            
            showToast('success', '✓ Armado iniciado. El pedido ha pasado al Kanban para gestionarlo.');
            console.log(`[CU25] Pedido ${idPedido} - Tiempo de armado: INICIADO`);
            loadPedidos(); 
        } catch (error) {
            console.error("Error al cambiar estado:", error);
            showToast('error', '❌ No se pudo iniciar el armado.');
        }
    };

    useEffect(() => {
        if (user?.id) loadPedidos();
    }, [user?.id, view]);

    const getInitials = (name: string) => name.split(' ').map(n => n[0]).join('').substring(0, 2).toUpperCase();

    const showToast = (type: 'success' | 'error', message: string) => {
        setToast({ visible: true, type, message });
        setTimeout(() => setToast({ ...toast, visible: false }), 3000);
    };

    return (
        <DashboardLayout>
            {/* Toast Notificación */}
            {toast.visible && (
                <div className={`fixed top-4 right-4 max-w-sm p-4 rounded-lg shadow-lg flex items-center gap-3 animate-in fade-in slide-in-from-top-[10px] z-50 ${
                    toast.type === 'success' ? 'bg-green-50 text-green-700 border border-green-200' : 'bg-red-50 text-red-700 border border-red-200'
                }`}>
                    {toast.type === 'success' && <CheckCircle2 className="w-5 h-5 flex-shrink-0" />}
                    <p className="text-sm font-medium">{toast.message}</p>
                </div>
            )}

            <div className="space-y-6 font-sans flex flex-col items-center w-full">
                <div className="flex flex-col md:flex-row md:items-center justify-between gap-4 w-full max-w-5xl mx-auto text-center">
                    <div className="flex flex-col items-center w-full">
                        <div className="flex items-center gap-3 justify-center w-full">
                            <div className="w-2 h-8 bg-indigo-500 rounded-full"></div>
                            <div>
                                <h1 className="text-2xl font-bold text-gray-900">Pedidos Pendientes de Armado</h1>
                            </div>
                        </div>
                    </div>

                    <div className="flex items-center gap-2 bg-gray-100 p-1 rounded-xl w-fit mx-auto">
                        <button 
                            onClick={() => setView('tabla')}
                            className={`flex items-center gap-2 px-4 py-2 rounded-lg text-sm font-medium transition-all ${view === 'tabla' ? 'bg-white shadow-sm text-indigo-600' : 'text-gray-500 hover:text-gray-700'}`}
                        >
                            <List className="w-4 h-4" /> Tabla
                        </button>
                        <button 
                            onClick={() => setView('kanban')}
                            className={`flex items-center gap-2 px-4 py-2 rounded-lg text-sm font-medium transition-all ${view === 'kanban' ? 'bg-white shadow-sm text-indigo-600' : 'text-gray-500 hover:text-gray-700'}`}
                        >
                            <LayoutGrid className="w-4 h-4" /> Kanban
                        </button>
                    </div>
                </div>

                {loading ? (
                    <div className="flex justify-center py-10">
                        <div className="animate-spin rounded-full h-10 w-10 border-b-2 border-indigo-600"></div>
                    </div>
                ) : pedidos.length === 0 ? (
                    <Alert type="info">No tienes pedidos en esta vista.</Alert>
                ) : view === 'tabla' ? (
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
                                            <div className="flex items-center justify-end gap-3">
                                                {[1, 2].includes(pedido.idEstadoDePedido) && !pedido.fechaInicioArmado && (
                                                    <button 
                                                        onClick={() => handleCambiarEstado(pedido.idPedido, 2)}
                                                        className="flex items-center gap-1 text-xs font-bold bg-blue-50 text-blue-600 px-3 py-1.5 rounded-lg hover:bg-blue-100 transition-colors"
                                                    >
                                                        <PlayCircle className="w-4 h-4" /> Iniciar Armado
                                                    </button>
                                                )}

                                                <button 
                                                    onClick={() => { setSelectedPedido(pedido); setModalDetalleOpen(true); }}
                                                    className="text-blue-600 hover:text-blue-800 font-bold flex items-center justify-end gap-1"
                                                >
                                                    <Eye className="w-4 h-4" /> Ver detalle
                                                </button>
                                            </div>
                                        </td>
                                    </tr>
                                ))}
                            </tbody>
                        </table>
                    </div>
                ) : (
                    <div className="bg-white rounded-2xl shadow-sm border border-gray-100 overflow-hidden">
                        <div className="p-6 bg-gradient-to-r from-blue-600 to-indigo-600 text-white">
                            <h2 className="text-lg font-bold">Tablero Kanban - Pedidos en Progreso</h2>
                            <p className="text-blue-100 text-sm">Estados: Preparar (2) • Demorado (3) • Listo (4)</p>
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

export default MisPedidosOperario;
