import React, { useState, useEffect, useCallback } from 'react';
import { pedidosService } from '../service/PedidosService';
import { OrderSummaryDTO } from '@/types/pedido.types';
// Importa tus componentes de UI (ajusta las rutas según tu proyecto)
import OrderFilters from '@/components/orders/OrderFilters'; 

export const DashboardAdmin: React.FC = () => {
    const [orders, setOrders] = useState<OrderSummaryDTO[]>([]);
    const [loading, setLoading] = useState(true);
    const [error, setError] = useState<string | null>(null);

    // Estados para los filtros (se sincronizan con OrderFilters)
    const [activeTab, setActiveTab] = useState('Todos');
    const [searchTerm, setSearchTerm] = useState('');
    const [dateRange, setDateRange] = useState({
        from: new Date().toISOString().split('T')[0],
        to: new Date().toISOString().split('T')[0]
    });

    // Función para cargar pedidos usando el servicio filtrado
    const loadOrders = useCallback(async () => {
        try {
            setLoading(true);
            const filters = {
                estado: activeTab === 'Todos' ? '' : activeTab,
                search: searchTerm,
                fechaDesde: dateRange.from,
                fechaHasta: dateRange.to
            };
            
            const data = await pedidosService.getFilteredOrders(filters);
            setOrders(data);
            setError(null);
        } catch (err) {
            setError('No se pudieron cargar los pedidos');
            console.error(err);
        } finally {
            setLoading(false);
        }
    }, [activeTab, searchTerm, dateRange]);

    // Ejecutar carga cuando cambien los filtros
    useEffect(() => {
        loadOrders();
    }, [loadOrders]);

    return (
        <div className="p-6 bg-gray-50 min-h-screen">
            <header className="mb-8">
                <h1 className="text-3xl font-bold text-gray-800">Panel de Control - Admin</h1>
                <p className="text-gray-600">Gestión global de pedidos y logística</p>
            </header>

            {/* Componente de Filtros que dispara los cambios de estado */}
            <div className="mb-6">
                <OrderFilters 
                    activeTab={activeTab}
                    onTabChange={setActiveTab}
                    searchTerm={searchTerm}
                    onSearchChange={setSearchTerm}
                    dateRange={dateRange}
                    onDateChange={setDateRange}
                />
            </div>

            {/* Tabla de Resultados */}
            <div className="bg-white rounded-lg shadow overflow-hidden">
                {loading ? (
                    <div className="p-10 text-center">Cargando pedidos...</div>
                ) : error ? (
                    <div className="p-10 text-center text-red-500">{error}</div>
                ) : (
                    <table className="min-w-full divide-y divide-gray-200">
                        <thead className="bg-gray-50">
                            <tr>
                                <th className="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase">ID</th>
                                <th className="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase">Cliente</th>
                                <th className="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase">Estado</th>
                                <th className="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase">Total</th>
                            </tr>
                        </thead>
                        <tbody className="bg-white divide-y divide-gray-200">
                            {orders.map((order) => (
                                <tr key={order.idPedido} className="hover:bg-gray-50">
                                    <td className="px-6 py-4 whitespace-nowrap font-medium text-blue-600">#{order.idPedido}</td>
                                    <td className="px-6 py-4 whitespace-nowrap">{order.clienteNombre}</td>
                                    <td className="px-6 py-4 whitespace-nowrap">
                                        <span className="px-2 py-1 text-xs rounded-full bg-blue-100 text-blue-800">
                                            {order.estadoNombre}
                                        </span>
                                    </td>
                                    <td className="px-6 py-4 whitespace-nowrap font-bold">
                                        ${order.total.toLocaleString()}
                                    </td>
                                </tr>
                            ))}
                            {orders.length === 0 && (
                                <tr>
                                    <td colSpan={4} className="px-6 py-10 text-center text-gray-400">
                                        No se encontraron pedidos con estos filtros.
                                    </td>
                                </tr>
                            )}
                        </tbody>
                    </table>
                )}
            </div>
        </div>
    );
};