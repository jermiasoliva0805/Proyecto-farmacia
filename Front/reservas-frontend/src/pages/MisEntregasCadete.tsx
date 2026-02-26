import React, { useState, useEffect } from 'react';
import { DashboardLayout } from '@components/layout/DashboardLayout';
import { Badge } from '@components/common/Badge';
import { Button } from '@components/common/Button';
import { Alert } from '@components/common/Alert';
import { useAuth } from '@context/AuthContext';
import { pedidosService } from '../service/PedidosService';
import { OrderSummaryDTO } from '../types/pedido.types';
import { ConfirmarEntregaModal } from '../components/pedidos/ConfirmarEntregaModal';
import { Navigation } from 'lucide-react';

const MisEntregas = () => {
    const { user } = useAuth();
    const [pedidos, setPedidos] = useState<OrderSummaryDTO[]>([]);
    const [loading, setLoading] = useState(true);
    
    // Estados para el Modal de Gestión
    const [selectedPedido, setSelectedPedido] = useState<OrderSummaryDTO | null>(null);
    const [modalOpen, setModalOpen] = useState(false);

    // Estados correspondientes a "En Camino" o similares
    const estadosIds = [5, 6]; 

    const loadPedidos = async () => {
        try {
            setLoading(true);
            const data = await pedidosService.getPedidosByRol('Cadete', user!.id);
            const filtrados = data.filter(p => estadosIds.includes(p.idEstadoDePedido));
            setPedidos(filtrados);
        } catch (error) {
            console.error('Error al cargar mis entregas:', error);
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
                    <div className="w-2 h-8 bg-blue-500 rounded-full"></div>
                    <h1 className="text-2xl font-bold text-gray-900">Entregas Activas</h1>
                </div>

                {loading ? (
                    <div className="flex justify-center py-10">
                        <div className="animate-spin rounded-full h-10 w-10 border-b-2 border-blue-600"></div>
                    </div>
                ) : pedidos.length === 0 ? (
                    <Alert type="info">No tienes repartos pendientes ahora mismo.</Alert>
                ) : (
                    <div className="bg-white rounded-2xl shadow-sm border border-gray-100 overflow-hidden">
                        <table className="w-full">
                            <thead className="bg-gray-50 text-xs uppercase text-gray-500">
                                <tr>
                                    <th className="p-4 text-left">Pedido</th>
                                    <th className="p-4 text-left">Cliente</th>
                                    <th className="p-4 text-left">Estado</th>
                                    <th className="p-4 text-right">Acciones</th>
                                </tr>
                            </thead>
                            <tbody className="divide-y divide-gray-100">
                                {pedidos.map((pedido) => (
                                    <tr key={pedido.idPedido} className="hover:bg-gray-50 transition-colors">
                                        <td className="p-4 font-bold text-blue-600">#{pedido.idPedido}</td>
                                        <td className="p-4 text-sm text-gray-700">{pedido.clienteNombre}</td>
                                        <td className="p-4">
                                            <Badge 
                                                variant="info"
                                                className="px-3 py-1 bg-blue-100 text-blue-700 border-blue-200"
                                            >
                                                {pedido.estadoNombre}
                                            </Badge>
                                        </td>
                                        <td className="p-4 text-right">
                                            <Button
                                                size="sm"
                                                onClick={() => handleGestionar(pedido)}
                                                className="bg-green-600 hover:bg-green-700 text-white rounded-lg px-4 py-2 font-bold shadow-sm flex items-center gap-2 ml-auto"
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

export default MisEntregas;