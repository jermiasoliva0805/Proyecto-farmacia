import React, { useState, useEffect } from 'react';
import { DashboardLayout } from '@components/layout/DashboardLayout';
import { Badge } from '@components/common/Badge';
import { Button } from '@components/common/Button';
import { Alert } from '@components/common/Alert';
import { useAuth } from '@context/AuthContext';
import { pedidosService } from '../service/PedidosService';
import { OrderSummaryDTO } from '../types/pedido.types';
import { ConfirmarEntregaModal } from '../components/pedidos/ConfirmarEntregaModal';

const EntregasFallidas = () => {
    const { user } = useAuth();
    const [pedidos, setPedidos] = useState<OrderSummaryDTO[]>([]);
    const [loading, setLoading] = useState(true);
    
    // Estados para el Modal de Gestión
    const [selectedPedido, setSelectedPedido] = useState<OrderSummaryDTO | null>(null);
    const [modalOpen, setModalOpen] = useState(false);

    const estadosIds = [8, 9]; // Fallidos y Cancelados

    // Lógica de colores corregida: Cancelado y Fallida en Rojo
    const getEstadoStyle = (estado: string) => {
        const est = estado.toLowerCase();
        if (est.includes('fallida') || est.includes('incidente') || est.includes('cancelado')) {
            return 'bg-red-100 text-red-600 border-red-200';
        }
        return 'bg-orange-100 text-orange-600 border-orange-200';
    };

    const loadPedidos = async () => {
        try {
            setLoading(true);
            const data = await pedidosService.getPedidosByRol('Cadete', user!.id);
            const filtrados = data.filter(p => estadosIds.includes(p.idEstadoDePedido));
            setPedidos(filtrados);
        } catch (error) {
            console.error('Error al cargar pedidos fallidos:', error);
        } finally {
            setLoading(false);
        }
    };

    useEffect(() => {
        if (user?.id) loadPedidos();
    }, [user?.id]);

    const handleGestionar = (pedido: OrderSummaryDTO) => {
        setSelectedPedido(pedido);
        setModalOpen(true);
    };

    return (
        <DashboardLayout>
            <div className="space-y-6 font-sans">
                <div className="flex items-center gap-3">
                    <div className="w-2 h-8 bg-red-500 rounded-full"></div>
                    <h1 className="text-2xl font-bold text-gray-900">Incidentes y Fallidos</h1>
                </div>

                {loading ? (
                    <div className="flex justify-center py-10">
                        <div className="animate-spin rounded-full h-10 w-10 border-b-2 border-blue-600"></div>
                    </div>
                ) : pedidos.length === 0 ? (
                    <Alert type="info">¡Todo en orden! No hay pedidos fallidos.</Alert>
                ) : (
                    <div className="bg-white rounded-2xl shadow-sm border border-gray-100 overflow-hidden">
                        <table className="w-full">
                            <thead className="bg-gray-50 text-xs uppercase text-gray-500">
                                <tr>
                                    <th className="p-4 text-left">ID</th>
                                    <th className="p-4 text-left">Fecha</th>
                                    <th className="p-4 text-left">Cliente</th>
                                    <th className="p-4 text-left">Estado</th>
                                    <th className="p-4 text-right">Acciones</th>
                                </tr>
                            </thead>
                            <tbody className="divide-y divide-gray-100">
                                {pedidos.map((pedido) => (
                                    <tr key={pedido.idPedido} className="hover:bg-gray-50 transition-colors">
                                        <td className="p-4 font-bold text-blue-600">#{pedido.idPedido}</td>
                                        <td className="p-4 text-sm text-gray-600">
                                            {pedido.fecha ? new Date(pedido.fecha).toLocaleDateString('es-AR') : '-'}
                                        </td>
                                        <td className="p-4 text-sm text-gray-700">{pedido.clienteNombre}</td>
                                        <td className="p-4">
                                            <span className={`inline-flex items-center px-3 py-1 rounded-full text-[10px] font-bold border ${getEstadoStyle(pedido.estadoNombre)}`}>
                                                {pedido.estadoNombre.toUpperCase()}
                                            </span>
                                        </td>
                                        <td className="p-4 text-right">
                                            <Button
                                                size="sm"
                                                onClick={() => handleGestionar(pedido)}
                                                disabled={pedido.idEstadoDePedido === 9}
                                                className={`rounded-lg text-xs px-4 py-2 font-bold ${
                                                    pedido.idEstadoDePedido === 9 
                                                    ? "bg-gray-400 cursor-not-allowed text-white border-none" 
                                                    : "bg-green-600 hover:bg-green-700 text-white shadow-sm"
                                                }`}
                                            >
                                                Gestionar Entrega
                                            </Button>
                                        </td>
                                    </tr>
                                ))}
                            </tbody>
                        </table>
                    </div>
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
        </DashboardLayout>
    );
};

export default EntregasFallidas;