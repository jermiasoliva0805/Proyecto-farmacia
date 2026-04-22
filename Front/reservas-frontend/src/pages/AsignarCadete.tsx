import React, { useState, useEffect } from 'react';
import { DashboardLayout } from '@components/layout/DashboardLayout';
import { Card } from '@components/common/Card';
import { Badge } from '@components/common/Badge';
import { Button } from '@components/common/Button';
import { AsignarCadeteModal } from '../components/pedidos/AsignarCadeteModal';
import { pedidosService } from '../service/PedidosService';
import { OrderSummaryDTO } from '../types/pedido.types';
import { Truck } from 'lucide-react';

export const AsignarCadetePage: React.FC = () => {
    const [pedidos, setPedidos] = useState<OrderSummaryDTO[]>([]);
    const [loading, setLoading] = useState(true);
    const [selectedPedido, setSelectedPedido] = useState<OrderSummaryDTO | null>(null);
    const [modalOpen, setModalOpen] = useState(false);

    useEffect(() => {
        loadPedidos();
    }, []);

    // Color verde para indicar que el proceso de preparación terminó con éxito
    const getEstadoStyle = (estado: string) => {
        return 'bg-green-100 text-green-600 border-green-200';
    };

    const loadPedidos = async () => {
        setLoading(true);
        try {
            const data = await pedidosService.getPendientesCadete();
            setPedidos(data);
        } catch (error) {
            console.error('Error cargando pedidos para cadete:', error);
        } finally {
            setLoading(false);
        }
    };

    const handleAsignar = (pedido: OrderSummaryDTO) => {
        setSelectedPedido(pedido);
        setModalOpen(true);
    };

    return (
        <DashboardLayout>
            <div className="p-6 bg-gray-50 min-h-screen">
                <div className="flex justify-between items-center mb-6">
                    <div>
                        <h1 className="text-2xl font-bold text-gray-800">Asignar Cadetes</h1>
                    </div>
                    <Badge variant="success" size="md">
                        {pedidos.length} por despachar
                    </Badge>
                </div>

                <div className="bg-white rounded-xl shadow-sm border border-gray-100 overflow-hidden">
                    {loading ? (
                        <div className="flex justify-center py-12">
                            <div className="animate-spin rounded-full h-12 w-12 border-b-2 border-blue-600"></div>
                        </div>
                    ) : pedidos.length === 0 ? (
                        <div className="text-center py-12">
                            <p className="text-gray-500">No hay pedidos pendientes de despacho.</p>
                        </div>
                    ) : (
                        <div className="overflow-x-auto">
                            <table className="w-full">
                                <thead className="bg-gray-50">
                                    <tr>
                                        <th className="px-4 py-3 text-left text-sm font-medium text-gray-600">ID</th>
                                        <th className="px-4 py-3 text-left text-sm font-medium text-gray-600">Cliente</th>
                                        <th className="px-4 py-3 text-left text-sm font-medium text-gray-600">Preparado por</th>
                                        <th className="px-4 py-3 text-left text-sm font-medium text-gray-600">Estado</th>
                                        <th className="px-4 py-3 text-left text-sm font-medium text-gray-600">Total</th>
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
                                                <Badge variant="info">
                                                    {pedido.responsableNombre || 'Sistema'}
                                                </Badge>
                                            </td>
                                            {/* COLUMNA DE ESTADO EN VERDE */}
                                            <td className="px-4 py-3">
                                                <span className={`px-3 py-1 rounded-full text-[11px] font-bold border ${getEstadoStyle(pedido.estadoNombre)}`}>
                                                    {pedido.estadoNombre.toUpperCase()}
                                                </span>
                                            </td>
                                            <td className="px-4 py-3 text-sm font-medium text-gray-900">
                                                ${pedido.total.toFixed(2)}
                                            </td>
                                            <td className="px-4 py-3 text-right">
                                                <Button
                                                    size="sm"
                                                    variant="success"
                                                    className="bg-green-600 hover:bg-green-700 text-white"
                                                    onClick={() => handleAsignar(pedido)}
                                                >
                                                    Asignar Cadete
                                                </Button>
                                            </td>
                                        </tr>
                                    ))}
                                </tbody>
                            </table>
                        </div>
                    )}
                </div>
            </div>

            {selectedPedido && (
                <AsignarCadeteModal
                    isOpen={modalOpen}
                    onClose={() => setModalOpen(false)}
                    pedido={selectedPedido}
                    onSuccess={loadPedidos}
                />
            )}
        </DashboardLayout>
    );
};