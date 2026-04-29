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
import { toast } from 'react-toastify';

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

    // Construye la query de dirección para Google Maps
    const buildMapsQuery = (pedido: OrderSummaryDTO) => {
        const parts = [pedido.direccionEntrega, pedido.localidadNombre, pedido.codigoPostalEntrega]
            .filter((value): value is string => !!value && value.trim().length > 0)
            .map((value) => value.trim());

        return parts.join(', ');
    };

    // Abre Google Maps en modo navegación hacia la dirección del pedido
    const handleVerEnMaps = (pedido: OrderSummaryDTO) => {
        const query = buildMapsQuery(pedido);

        if (!query) {
            toast.warning('El pedido no tiene dirección suficiente para abrir Google Maps.');
            return;
        }

        // URL de direcciones para que Maps abra en modo ruta (similar a "Como llegar / Iniciar").
        const url = `https://www.google.com/maps/dir/?api=1&destination=${encodeURIComponent(query)}&travelmode=driving&dir_action=navigate`;
        window.location.href = url;
    };

    return (
        <DashboardLayout>
            <div className="space-y-4 sm:space-y-6 font-sans px-2 sm:px-0">
                <div className="flex items-center gap-3">
                    <div className="w-2 h-8 bg-red-500 rounded-full"></div>
                    <h1 className="text-xl sm:text-2xl font-bold text-gray-900">Incidentes y Fallidos</h1>
                </div>

                {loading ? (
                    <div className="flex justify-center py-10">
                        <div className="animate-spin rounded-full h-10 w-10 border-b-2 border-blue-600"></div>
                    </div>
                ) : pedidos.length === 0 ? (
                    <Alert type="info">¡Todo en orden! No hay pedidos fallidos.</Alert>
                ) : (
                    <>
                        {/* TABLA DESKTOP */}
                        <div className="hidden md:block bg-white rounded-2xl shadow-sm border border-gray-100 overflow-hidden">
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
                                                <div className="flex justify-end gap-2">
                                                    {/* Botón Ver Maps — solo visible en pedidos con entrega fallida (estado 8) */}
                                                    {pedido.idEstadoDePedido === 8 && (
                                                        <Button
                                                            size="sm"
                                                            onClick={() => handleVerEnMaps(pedido)}
                                                            className="rounded-lg text-xs px-4 py-2 font-bold border border-amber-200 text-amber-700 hover:bg-amber-50"
                                                        >
                                                            <Navigation className="w-3 h-3 mr-1 inline-block" /> Ver Maps
                                                        </Button>
                                                    )}
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
                                                </div>
                                            </td>
                                        </tr>
                                    ))}
                                </tbody>
                            </table>
                        </div>

                        {/* TARJETAS MOBILE */}
                        <div className="md:hidden space-y-3">
                            {pedidos.map((pedido) => (
                                <div key={pedido.idPedido} className="bg-white rounded-xl border border-gray-100 shadow-sm p-3 sm:p-4">
                                    <div className="flex items-start justify-between gap-3 mb-3">
                                        <div>
                                            <p className="font-bold text-gray-900 text-sm">#{pedido.idPedido}</p>
                                            <p className="text-xs text-gray-500">{pedido.clienteNombre}</p>
                                        </div>
                                        <span className={`inline-flex items-center px-2 py-1 rounded-full text-[10px] font-bold border flex-shrink-0 ${getEstadoStyle(pedido.estadoNombre)}`}>
                                            {pedido.estadoNombre.length > 12 ? pedido.estadoNombre.substring(0, 10) + '...' : pedido.estadoNombre.toUpperCase()}
                                        </span>
                                    </div>
                                    
                                    <div className="text-xs mb-3">
                                        <p className="text-gray-500 mb-1">Fecha</p>
                                        <p className="font-medium text-gray-900">{pedido.fecha ? new Date(pedido.fecha).toLocaleDateString('es-AR') : '-'}</p>
                                    </div>

                                    <div className="flex flex-col gap-2">
                                        {/* Botón Ver Maps — solo visible en pedidos con entrega fallida (estado 8) */}
                                        {pedido.idEstadoDePedido === 8 && (
                                            <Button
                                                size="sm"
                                                onClick={() => handleVerEnMaps(pedido)}
                                                className="w-full rounded-lg text-xs py-2 font-bold border border-amber-200 text-amber-700 hover:bg-amber-50"
                                            >
                                                <Navigation className="w-3 h-3 mr-1 inline-block" /> Ver Maps
                                            </Button>
                                        )}
                                        <Button
                                            size="sm"
                                            onClick={() => handleGestionar(pedido)}
                                            disabled={pedido.idEstadoDePedido === 9}
                                            className={`w-full rounded-lg text-xs py-2 font-bold ${
                                                pedido.idEstadoDePedido === 9 
                                                ? "bg-gray-400 cursor-not-allowed text-white border-none" 
                                                : "bg-green-600 hover:bg-green-700 text-white shadow-sm"
                                            }`}
                                        >
                                            Gestionar Entrega
                                        </Button>
                                    </div>
                                </div>
                            ))}
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
        </DashboardLayout>
    );
};

export default EntregasFallidas;

