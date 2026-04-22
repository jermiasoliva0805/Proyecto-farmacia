import React, { useState, useEffect } from 'react';
import { DashboardLayout } from '@components/layout/DashboardLayout';
import { Card } from '@components/common/Card';
import { Badge } from '@components/common/Badge';
import { Button } from '@components/common/Button';
import { AsignarOperarioModal } from '@components/pedidos/AsignarOperarioModal';
import { pedidosService } from '../service/PedidosService'; 
import { OrderSummaryDTO } from '@/types/pedido.types';
import { Users } from 'lucide-react';

export const AsignarOperarioPage: React.FC = () => {
    const [pedidos, setPedidos] = useState<OrderSummaryDTO[]>([]);
    const [loading, setLoading] = useState(true);
    const [selectedPedido, setSelectedPedido] = useState<OrderSummaryDTO | null>(null);
    const [modalOpen, setModalOpen] = useState(false);

    useEffect(() => {
        loadPedidos();
    }, []);

    // Función de colores para mantener la consistencia visual
    const getEstadoStyle = (estado: string) => {
        const est = estado.toLowerCase();
        if (est === 'sin preparar') {
            return 'bg-gray-100 text-gray-400 border-gray-200'; // Gris para "Sin preparar"
        }
        return 'bg-blue-100 text-blue-600 border-blue-200';
    };

    const loadPedidos = async () => {
        setLoading(true);
        try {
            const data = await pedidosService.getPendientesOperario();
            if (data && data.length > 0) {
                setPedidos(data);
            } else {
                setPedidos([]);
            }
        } catch (error) {
            console.error('Error cargando pedidos:', error);
            setPedidos([]);
        } finally {
            setLoading(false);
        }
    };

    const handleAsignar = (pedido: OrderSummaryDTO) => {
        setSelectedPedido(pedido);
        setModalOpen(true);
    };

    const handleSuccess = () => {
        setModalOpen(false);
        loadPedidos();
    };

    return (
        <DashboardLayout>
            <div className="space-y-6 w-full">
                <div className="flex flex-col md:flex-row justify-between items-start w-full max-w-4xl mx-auto gap-4">
                    <div>
                        <h1 className="text-3xl font-bold text-gray-900 flex items-center gap-2">
                            <Users className="w-8 h-8" />
                            Asignar Operarios
                        </h1>
                        <p className="text-gray-600 mt-1">
                            Solo se muestran pedidos en estado <strong>'Sin preparar'</strong>.
                        </p>
                    </div>
                    <Badge variant="info" size="md">
                        {pedidos.length} pedidos por asignar
                    </Badge>
                </div>

                <Card className="w-full max-w-4xl mx-auto">
                    {loading ? (
                        <div className="flex justify-center py-12">
                            <div className="animate-spin rounded-full h-12 w-12 border-b-2 border-blue-600"></div>
                        </div>
                    ) : pedidos.length === 0 ? (
                        <div className="text-center py-12">
                            <p className="text-gray-500">No hay pedidos pendientes de asignación de operario.</p>
                        </div>
                    ) : (
                        <div className="overflow-x-auto">
                            <table className="w-full">
                                <thead className="bg-gray-50">
                                    <tr>
                                        <th className="px-4 py-3 text-left text-sm font-medium text-gray-600">ID</th>
                                        <th className="px-4 py-3 text-left text-sm font-medium text-gray-600">Cliente</th>
                                        <th className="px-4 py-3 text-left text-sm font-medium text-gray-600">Fecha</th>
                                        <th className="px-4 py-3 text-left text-sm font-medium text-gray-600">Total</th>
                                        <th className="px-4 py-3 text-left text-sm font-medium text-gray-600">Estado</th>
                                        <th className="px-4 py-3 text-right text-sm font-medium text-gray-600">Acciones</th>
                                    </tr>
                                </thead>
                                <tbody className="divide-y divide-gray-200">
                                    {pedidos.map((pedido) => (
                                        <tr key={pedido.idPedido} className="hover:bg-gray-50">
                                            <td className="px-4 py-3 text-sm font-medium text-gray-900">
                                                #{pedido.idPedido}
                                            </td>
                                            <td className="px-4 py-3 text-sm text-gray-700">
                                                {pedido.clienteNombre}
                                            </td>
                                            <td className="px-4 py-3 text-sm text-gray-600">
                                                {new Date(pedido.fecha).toLocaleDateString('es-AR')}
                                            </td>
                                            <td className="px-4 py-3 text-sm font-medium text-gray-900">
                                                ${pedido.total.toFixed(2)}
                                            </td>
                                            <td className="px-4 py-3">
                                                {/* AQUÍ VA EL COLOR Y EL ESTADO */}
                                                <span className={`px-3 py-1 rounded-full text-[11px] font-bold border ${getEstadoStyle(pedido.estadoNombre)}`}>
                                                    {pedido.estadoNombre.toUpperCase()}
                                                </span>
                                            </td>
                                            <td className="px-4 py-3 text-right">
                                                <Button
                                                    size="sm"
                                                    variant="primary"
                                                    className="bg-blue-600 hover:bg-blue-700 text-white"
                                                    onClick={() => handleAsignar(pedido)}
                                                >
                                                    Asignar Operario
                                                </Button>
                                            </td>
                                        </tr>
                                    ))}
                                </tbody>
                            </table>
                        </div>
                    )}
                </Card>
            </div>

            {selectedPedido && (
                <AsignarOperarioModal
                    isOpen={modalOpen}
                    onClose={() => setModalOpen(false)}
                    pedido={selectedPedido}
                    onSuccess={handleSuccess}
                />
            )}
        </DashboardLayout>
    );
};