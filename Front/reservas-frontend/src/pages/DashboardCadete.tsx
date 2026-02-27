import React, { useState, useEffect } from 'react';
import { DashboardLayout } from '@components/layout/DashboardLayout';
import { Card } from '@components/common/Card';
import { Badge } from '@components/common/Badge';
import { Button } from '@components/common/Button';
import { Alert } from '@components/common/Alert';
import { ConfirmarEntregaModal } from '../components/pedidos/ConfirmarEntregaModal';
import { DetallePedidoModal } from '../components/pedidos/DetallePedidoModal';
import { OrderFilters } from '@components/orders/OrderFilters';
import { pedidosService } from '../service/PedidosService';
import { OrderSummaryDTO } from '../types/pedido.types';
import { useAuth } from '@context/AuthContext';
import { Truck, MapPin, CheckCircle, Navigation, Eye, AlertCircle } from 'lucide-react';

export const DashboardCadete: React.FC = () => {
    const { user } = useAuth();
    const [pedidos, setPedidos] = useState<OrderSummaryDTO[]>([]);
    const [loading, setLoading] = useState(true);
    const [selectedPedido, setSelectedPedido] = useState<OrderSummaryDTO | null>(null);
    const [modalOpen, setModalOpen] = useState(false);
    const [selectedPedidoDetalle, setSelectedPedidoDetalle] = useState<OrderSummaryDTO | null>(null);
    const [modalDetalleOpen, setModalDetalleOpen] = useState(false);

    // Función de colores unificada para mantener consistencia
    const getEstadoStyle = (estado: string, estaDemorado: boolean) => {
        if (estaDemorado) return 'bg-orange-100 text-orange-600 border-orange-200';
        const est = estado.toLowerCase();
        switch (est) {
            case 'sin preparar': return 'bg-gray-100 text-gray-400 border-gray-200';
            case 'preparar pedido':
            case 'preparando':
            case 'en preparación': return 'bg-blue-100 text-blue-600 border-blue-200';
            case 'demorado': return 'bg-orange-100 text-orange-600 border-orange-200';
            case 'listo para despachar': return 'bg-green-100 text-green-600 border-green-200';
            case 'en camino':
            case 'despachando': return 'bg-indigo-100 text-indigo-600 border-indigo-200';
            case 'entregado': return 'bg-emerald-100 text-emerald-700 border-emerald-200';
            case 'cancelado':
            case 'entrega fallida': return 'bg-red-100 text-red-600 border-red-200';
            default: return 'bg-gray-50 text-gray-500 border-gray-100';
        }
    };

    useEffect(() => { 
        if (user?.id) loadPedidos(); 
    }, [user?.id]);

    const loadPedidos = async (filtros = {}) => {
        try {
            setLoading(true);
            const data = await pedidosService.getPedidosByRol('Cadete', user!.id, filtros);
            setPedidos(data);
        } catch (error) {
            console.error('Error al cargar pedidos del cadete:', error);
        } finally {
            setLoading(false);
        }
    };

    const handleConfirmarEntrega = (pedido: OrderSummaryDTO) => {
        setSelectedPedido(pedido);
        setModalOpen(true);
    };

    const handleVerDetalle = (pedido: OrderSummaryDTO) => {
        setSelectedPedidoDetalle(pedido);
        setModalDetalleOpen(true);
    };

    const getInitials = (name: string) => name.split(' ').map(n => n[0]).join('').substring(0, 2).toUpperCase();
    const getAvatarColor = (name: string) => {
        const colors = ['bg-blue-500', 'bg-purple-500', 'bg-pink-500', 'bg-indigo-500', 'bg-teal-500'];
        return colors[name.length % colors.length];
    };

    const pedidosEnCamino = pedidos.filter(p => p.estadoNombre === 'En camino');

    const entregadosHoyCount = pedidos.filter(p => {
        if (p.estadoNombre !== 'Entregado') return false;
        const hoy = new Date().toLocaleDateString('es-AR');
        const fechaABuscar = p.fechaEntregaReal || p.fecha; 
        return new Date(fechaABuscar).toLocaleDateString('es-AR') === hoy;
    }).length;

    return (
        <DashboardLayout>
            <div className="space-y-8 font-sans">
                <div>
                    <h1 className="text-3xl font-bold text-gray-900 tracking-tight">Ruta de {user?.nombreCompleto?.split(' ')[0]}</h1>
                    <p className="text-gray-500 mt-1">Panel de Entregas - {user?.nombreSucursal}</p>
                </div>

                <div className="grid grid-cols-1 md:grid-cols-2 gap-6">
                    <div className="bg-white border-2 border-amber-500 rounded-2xl p-6 shadow-sm relative overflow-hidden group transition-all hover:shadow-md">
                        <div className="relative z-10">
                            <p className="text-amber-600 text-sm font-medium mb-1">En Camino</p>
                            <h3 className="text-4xl font-bold mb-2 text-amber-900">{pedidosEnCamino.length}</h3>
                            <p className="text-gray-500 text-xs">Entregas pendientes ahora</p>
                        </div>
                        <div className="absolute right-4 top-4 bg-amber-100/50 p-3 rounded-xl backdrop-blur-sm">
                            <Truck className="w-6 h-6 text-amber-600" />
                        </div>
                    </div>
                    <div className="bg-white border-2 border-emerald-500 rounded-2xl p-6 shadow-sm relative overflow-hidden group transition-all hover:shadow-md">
                        <div className="relative z-10">
                            <p className="text-emerald-600 text-sm font-medium mb-1">Entregados Hoy</p>
                            <h3 className="text-4xl font-bold mb-2 text-emerald-900">{entregadosHoyCount}</h3>
                            <p className="text-gray-500 text-xs">Objetivo diario</p>
                        </div>
                        <div className="absolute right-4 top-4 bg-emerald-100/50 p-3 rounded-xl backdrop-blur-sm">
                            <CheckCircle className="w-6 h-6 text-emerald-600" />
                        </div>
                    </div>
                </div>

                <div className="bg-white p-2 rounded-xl border border-gray-100 shadow-sm">
                    <OrderFilters userRole="Cadete" onFilterChange={loadPedidos} />
                </div>

                {loading && pedidos.length === 0 ? (
                    <div className="flex items-center justify-center h-64">
                        <div className="animate-spin rounded-full h-12 w-12 border-b-2 border-blue-600"></div>
                    </div>
                ) : (
                    <>
                        <div>
                            <div className="flex items-center gap-2 mb-4">
                                <MapPin className="w-5 h-5 text-amber-500" /> 
                                <h2 className="text-xl font-bold text-gray-900">Pedidos en Camino</h2>
                            </div>

                            {pedidosEnCamino.length === 0 ? (
                                <Alert type="info">No tienes pedidos en ruta actualmente.</Alert>
                            ) : (
                                <div className="grid md:grid-cols-2 lg:grid-cols-3 gap-6">
                                    {pedidosEnCamino.map((pedido) => (
                                        <div
                                            key={pedido.idPedido}
                                            className={`bg-white rounded-2xl p-5 border shadow-sm transition-all hover:-translate-y-1 ${pedido.estaDemorado ? 'border-red-200 ring-1 ring-red-100' : 'border-gray-100'}`}
                                        >
                                            <div className="flex justify-between items-start mb-4">
                                                <div className="flex items-center gap-3">
                                                    <div className={`w-10 h-10 rounded-full ${getAvatarColor(pedido.clienteNombre)} text-white flex items-center justify-center font-bold text-sm`}>
                                                        {getInitials(pedido.clienteNombre)}
                                                    </div>
                                                    <div>
                                                        <p className="font-bold text-gray-900">#{pedido.idPedido}</p>
                                                        <p className="text-xs text-gray-500">{pedido.clienteNombre}</p>
                                                    </div>
                                                </div>
                                                {pedido.estaDemorado && <AlertCircle className="text-red-500 w-5 h-5" />}
                                            </div>
                                            
                                            <div className="mb-4">
                                                <span className={`flex w-full justify-center py-1 rounded-full text-[11px] font-bold border ${getEstadoStyle(pedido.estadoNombre, pedido.estaDemorado)}`}>
                                                    {pedido.estadoNombre.toUpperCase()}
                                                </span>
                                            </div>

                                            <div className="grid grid-cols-2 gap-3">
                                                <Button
                                                    variant="primary"
                                                    size="sm"
                                                    onClick={() => handleConfirmarEntrega(pedido)}
                                                    className="w-full bg-blue-600 hover:bg-blue-700 text-white rounded-lg"
                                                >
                                                    <Navigation className="w-3 h-3 mr-2" /> Entregar
                                                </Button>
                                                <Button
                                                    variant="secondary"
                                                    size="sm"
                                                    onClick={() => handleVerDetalle(pedido)}
                                                    className="w-full border border-gray-200 hover:bg-gray-50 rounded-lg"
                                                >
                                                    <Eye className="w-3 h-3 mr-2" /> Ver
                                                </Button>
                                            </div>
                                        </div>
                                    ))}
                                </div>
                            )}
                        </div>

                        <div className="bg-white rounded-2xl shadow-sm border border-gray-100 overflow-hidden mt-8">
                            <div className="p-6 border-b border-gray-100">
                                <h2 className="text-lg font-bold text-gray-800">Historial de Entregas</h2>
                            </div>
                            <div className="overflow-x-auto">
                                <table className="w-full">
                                    <thead className="bg-gray-50 text-xs uppercase text-gray-500">
                                        <tr>
                                            <th className="p-4 text-left">ID</th>
                                            <th className="p-4 text-left">Cliente</th>
                                            <th className="p-4 text-left">Estado</th>
                                            <th className="p-4 text-right">Acciones</th>
                                        </tr>
                                    </thead>
                                    <tbody className="divide-y divide-gray-100">
                                        {pedidos
                                            .filter(p => p.estadoNombre !== 'En camino')
                                            .map((pedido) => (
                                                <tr key={pedido.idPedido} className="hover:bg-gray-50">
                                                    <td className="p-4 font-medium text-gray-700">#{pedido.idPedido}</td>
                                                    <td className="p-4 text-sm">{pedido.clienteNombre}</td>
                                                    <td className="p-4">
                                                        <span className={`flex w-full justify-center py-1 rounded-full text-[10px] font-bold border ${getEstadoStyle(pedido.estadoNombre, pedido.estaDemorado)}`}>
                                                            {pedido.estadoNombre.toUpperCase()}
                                                        </span>
                                                    </td>
                                                    <td className="p-4 text-right flex gap-2 justify-end items-center">
                                                        <button 
                                                            onClick={() => { setSelectedPedidoDetalle(pedido); setModalDetalleOpen(true); }} 
                                                            className="text-blue-600 hover:text-blue-800 font-bold text-sm flex items-center gap-1 transition-colors"
                                                        >
                                                            <Eye className="w-4 h-4" /> 
                                                            <span>Ver Detalle</span>
                                                        </button>
                                                        <Button
                                                            size="sm"
                                                            onClick={() => handleConfirmarEntrega(pedido)}
                                                            disabled={pedido.idEstadoDePedido === 7 || pedido.idEstadoDePedido === 9}
                                                            className={`rounded-lg text-xs px-3 py-1 ${
                                                                (pedido.idEstadoDePedido === 7 || pedido.idEstadoDePedido === 9) 
                                                                ? "bg-gray-400 cursor-not-allowed text-white" 
                                                                : "bg-green-600 hover:bg-green-700 text-white"
                                                            }`}
                                                        >
                                                            {pedido.idEstadoDePedido === 7 ? 'Entregado' : 
                                                             pedido.idEstadoDePedido === 9 ? 'Cancelado' : 
                                                             'Gestionar'}
                                                        </Button>
                                                    </td>
                                                </tr>
                                            ))}
                                    </tbody>
                                </table>
                            </div>
                        </div>
                    </>
                )}
            </div>

            {selectedPedido && (
                <ConfirmarEntregaModal
                    isOpen={modalOpen}
                    onClose={() => setModalOpen(false)}
                    pedido={selectedPedido}
                    onSuccess={loadPedidos}
                />
            )}
            
            {selectedPedidoDetalle && (
                <DetallePedidoModal
                    isOpen={modalDetalleOpen}
                    onClose={() => setModalDetalleOpen(false)}
                    pedido={selectedPedidoDetalle}
                />
            )}
        </DashboardLayout>
    );
};