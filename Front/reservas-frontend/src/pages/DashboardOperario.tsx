import React, { useState, useEffect } from 'react';
import { DashboardLayout } from '@components/layout/DashboardLayout';
import { DetallePedidoModal } from '../components/pedidos/DetallePedidoModal';
import { OrderFilters } from '@components/orders/OrderFilters';
import { pedidosService } from '../service/PedidosService';
import { OrderSummaryDTO } from '../types/pedido.types';
import { useAuth } from '@context/AuthContext';
import { Package, Clock, CheckCircle, Eye, PlayCircle } from 'lucide-react';

export const DashboardOperario: React.FC = () => {
    const { user } = useAuth();
    const [pedidos, setPedidos] = useState<OrderSummaryDTO[]>([]);
    const [loading, setLoading] = useState(true);
    const [selectedPedidoDetalle, setSelectedPedidoDetalle] = useState<OrderSummaryDTO | null>(null);
    const [modalDetalleOpen, setModalDetalleOpen] = useState(false);

    // Lógica de colores unificada y estricta
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

    useEffect(() => { 
        if (user?.id) loadPedidos(); 
    }, [user?.id]);

    const loadPedidos = async (filtros = {}) => {
        try {
            setLoading(true);
            const data = await pedidosService.getPedidosByRol('Operario', user!.id, filtros);
            setPedidos(data);
        } catch (error) {
            console.error('Error al cargar pedidos:', error);
        } finally {
            setLoading(false);
        }
    };

    const handleVerDetalle = (pedido: OrderSummaryDTO) => {
        setSelectedPedidoDetalle(pedido);
        setModalDetalleOpen(true);
    };

    const handleIniciarArmado = async (idPedido: number) => {
        try {
            await pedidosService.cambiarEstado({
                idPedido,
                idNuevoEstado: 2,
                idUsuario: user!.id,
                observaciones: "Iniciando armado de pedido - Cronómetro activado ⏱"
            });
            window.alert("✅ Armado iniciado. El pedido pasó a preparación.");
            loadPedidos();
        } catch (error) {
            console.error("Error al iniciar armado:", error);
            window.alert("❌ No se pudo iniciar el armado.");
        }
    };

    const getInitials = (name: string) => name.split(' ').map(n => n[0]).join('').substring(0, 2).toUpperCase();
    
    const getAvatarColor = (name: string) => {
        const colors = ['bg-blue-500', 'bg-purple-500', 'bg-pink-500', 'bg-indigo-500', 'bg-teal-500'];
        return colors[name.length % colors.length];
    };

    const pedidosPreparando = pedidos.filter(p => p.estadoNombre.toLowerCase().includes('prepara'));
    const pedidosListos = pedidos.filter(p => p.estadoNombre.toLowerCase().includes('listo'));

    return (
        <DashboardLayout>
            <div className="space-y-8 font-sans">
                <div>
                    <h1 className="text-3xl font-bold text-gray-900 tracking-tight">
                        Hola, {user?.nombreCompleto?.split(' ')[0]}
                    </h1>
                    <p className="text-gray-500 mt-1">Panel de Preparación - {user?.nombreSucursal}</p>
                </div>

                {/* Estadísticas */}
                <div className="grid grid-cols-1 md:grid-cols-3 gap-6">
                    <div className="bg-white border-2 border-blue-500 rounded-2xl p-6 shadow-sm relative overflow-hidden transition-all hover:shadow-md">
                        <div className="relative z-10">
                            <p className="text-blue-600 text-sm font-medium mb-1">En Preparación</p>
                            <h3 className="text-4xl font-bold mb-2 text-blue-900">{pedidosPreparando.length}</h3>
                            <p className="text-gray-500 text-xs">Pedidos pendientes de armar</p>
                        </div>
                        <Package className="absolute right-4 top-4 w-12 h-12 text-blue-100" />
                    </div>

                    <div className="bg-white border-2 border-emerald-500 rounded-2xl p-6 shadow-sm relative overflow-hidden transition-all hover:shadow-md">
                        <div className="relative z-10">
                            <p className="text-emerald-600 text-sm font-medium mb-1">Listos</p>
                            <h3 className="text-4xl font-bold mb-2 text-emerald-900">{pedidosListos.length}</h3>
                            <p className="text-gray-500 text-xs">Esperando despacho</p>
                        </div>
                        <CheckCircle className="absolute right-4 top-4 w-12 h-12 text-emerald-100" />
                    </div>

                    <div className="bg-white border-2 border-purple-500 rounded-2xl p-6 shadow-sm relative overflow-hidden transition-all hover:shadow-md">
                        <div className="relative z-10">
                            <p className="text-purple-600 text-sm font-medium mb-1">Total Hoy</p>
                            <h3 className="text-4xl font-bold mb-2 text-purple-900">{pedidos.length}</h3>
                            <p className="text-gray-500 text-xs">Gestión total del turno</p>
                        </div>
                        <Clock className="absolute right-4 top-4 w-12 h-12 text-purple-100" />
                    </div>
                </div>

                {/* Filtros */}
                <div className="bg-white p-2 rounded-xl border border-gray-100 shadow-sm">
                    <OrderFilters userRole="Operario" onFilterChange={loadPedidos} />
                </div>

                {loading ? (
                    <div className="flex items-center justify-center h-48">
                        <div className="animate-spin rounded-full h-10 w-10 border-4 border-gray-200 border-t-blue-600"></div>
                    </div>
                ) : (
                    <div className="bg-white rounded-2xl shadow-sm border border-gray-100 overflow-hidden">
                        <div className="p-6 border-b border-gray-100 flex justify-between items-center">
                            <h2 className="text-lg font-bold text-gray-800">Carga de Trabajo</h2>
                        </div>
                        <div className="overflow-x-auto">
                            <table className="w-full text-left border-collapse">
                                <thead className="bg-gray-50 text-xs uppercase text-gray-500 font-semibold">
                                    <tr>
                                        <th className="p-5">ID</th>
                                        <th className="p-5">Fecha</th>
                                        <th className="p-5">Cliente</th>
                                        <th className="p-5">Estado</th>
                                        <th className="p-5">Total</th>
                                        <th className="p-5 text-right">Acciones</th>
                                    </tr>
                                </thead>
                                <tbody className="divide-y divide-gray-100">
                                    {pedidos.map((pedido) => (
                                        <tr key={pedido.idPedido} className={`hover:bg-gray-50 transition-colors ${pedido.estaDemorado ? 'bg-red-50/20' : ''}`}>
                                            <td className="p-5 font-bold text-gray-900">
                                                #{pedido.idPedido}
                                            </td>
                                            <td className="p-5 text-sm text-gray-600">
                                                {pedido.fecha ? new Date(pedido.fecha).toLocaleDateString('es-AR') : '-'}
                                            </td>
                                            <td className="p-5">
                                                <div className="flex items-center gap-3">
                                                    <div className={`w-8 h-8 rounded-full ${getAvatarColor(pedido.clienteNombre)} text-white flex items-center justify-center font-bold text-xs`}>
                                                        {getInitials(pedido.clienteNombre)}
                                                    </div>
                                                    <span className="font-medium text-gray-800">{pedido.clienteNombre}</span>
                                                </div>
                                            </td>
                                            <td className="p-5">
                                                <span className={`inline-flex items-center px-3 py-1 rounded-full text-[10px] font-bold border ${getEstadoStyle(pedido.estadoNombre, pedido.estaDemorado)}`}>
                                                    {pedido.estadoNombre.toUpperCase()}
                                                </span>
                                            </td>
                                            <td className="p-5 text-gray-600 font-mono">${pedido.total?.toFixed(2)}</td>
                                            <td className="p-5 text-right">
                                                <div className="flex items-center justify-end gap-3">
                                                    {pedido.idEstadoDePedido === 1 && !pedido.fechaInicioArmado && (
                                                        <button
                                                            onClick={() => handleIniciarArmado(pedido.idPedido)}
                                                            className="flex items-center gap-1 text-xs font-bold bg-blue-50 text-blue-600 px-3 py-1.5 rounded-lg hover:bg-blue-100 transition-colors"
                                                        >
                                                            <PlayCircle className="w-4 h-4" /> Iniciar Armado
                                                        </button>
                                                    )}
                                                    <button 
                                                        onClick={() => handleVerDetalle(pedido)} 
                                                        className="text-blue-600 hover:text-blue-800 font-bold text-sm flex items-center justify-end gap-1 ml-auto transition-colors"
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
                    </div>
                )}
            </div>

            {selectedPedidoDetalle && (
                <DetallePedidoModal 
                    isOpen={modalDetalleOpen} 
                    onClose={() => { setModalDetalleOpen(false); setSelectedPedidoDetalle(null); }} 
                    pedido={selectedPedidoDetalle} 
                />
            )}
        </DashboardLayout>
    );
};
