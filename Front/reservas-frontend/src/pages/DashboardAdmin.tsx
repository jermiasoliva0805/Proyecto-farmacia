import React, { useState, useEffect } from 'react';
import { DashboardLayout } from '../components/layout/DashboardLayout.tsx';
import { Card } from '../components/common/Card.tsx';
import { Badge } from '@components/common/Badge.tsx';
import { Button } from '@components/common/Button';
import { AsignarOperarioModal } from '@components/pedidos/AsignarOperarioModal';
import { AsignarCadeteModal } from '@components/pedidos/AsignarCadeteModal';
import { DetallePedidoModal } from '@components/pedidos/DetallePedidoModal';
import { OrderFilters } from '@components/orders/OrderFilters';
import { pedidosService } from '../service/PedidosService';
import { OrderSummaryDTO } from '../types/pedido.types';
import { useAuth } from '@context/AuthContext';
import { 
    Package, 
    AlertTriangle, 
    CheckCircle, 
    XCircle,
    Users,
    Truck,
    Eye,
    Plus,
    Clock
} from 'lucide-react';
import { useNavigate } from 'react-router-dom';

export const DashboardAdmin: React.FC = () => {
    const { user } = useAuth();
    const navigate = useNavigate();
    const [pedidos, setPedidos] = useState<OrderSummaryDTO[]>([]);
    const [loading, setLoading] = useState(true);
    
    // Estados para Modales
    const [selectedPedido, setSelectedPedido] = useState<OrderSummaryDTO | null>(null);
    const [modalOpen, setModalOpen] = useState(false);
    
    const [selectedPedidoCadete, setSelectedPedidoCadete] = useState<OrderSummaryDTO | null>(null);
    const [modalCadeteOpen, setModalCadeteOpen] = useState(false);

    const [selectedPedidoDetalle, setSelectedPedidoDetalle] = useState<OrderSummaryDTO | null>(null);
    const [modalDetalleOpen, setModalDetalleOpen] = useState(false);

    const [stats, setStats] = useState({ activos: 0, demorados: 0, entregados: 0, cancelados: 0 });

    useEffect(() => { 
        loadDashboardData(); 
    }, []);

    const loadDashboardData = async (filtros = {}) => {
        try {
            setLoading(true);
            const data = await pedidosService.getFilteredOrders(filtros);
            setPedidos(data);
            
            setStats({
                activos: data.filter(p => !['Entregado', 'Cancelado'].includes(p.estadoNombre)).length,
                demorados: data.filter(p => p.estaDemorado || (p as any).EstaDemorado).length,
                entregados: data.filter(p => p.estadoNombre === 'Entregado').length,
                cancelados: data.filter(p => p.estadoNombre === 'Cancelado').length,
            });
        } catch (error) { 
            console.error("Error cargando pedidos:", error); 
        } finally { 
            setLoading(false); 
        }
    };

    // Función auxiliar para obtener iniciales
    const getInitials = (name: string) => name.split(' ').map(n => n[0]).join('').substring(0, 2).toUpperCase();
    // Función auxiliar para color de avatar aleatorio (fijo por nombre)
    const getAvatarColor = (name: string) => {
        const colors = ['bg-blue-500', 'bg-purple-500', 'bg-pink-500', 'bg-indigo-500', 'bg-teal-500'];
        const index = name.length % colors.length;
        return colors[index];
    };

   const getEstadoStyle = (estado: string, estaDemorado: boolean) => {
        if (estaDemorado) return 'bg-orange-100 text-orange-600 border-orange-200';
        
        const est = estado.toLowerCase();
        switch (est) {
            case 'sin preparar':
                return 'bg-gray-100 text-gray-400 border-gray-200'; // Gris suave: inactivo
            case 'preparar pedido':
            case 'preparando':
            case 'en preparación':
                return 'bg-blue-100 text-blue-600 border-blue-200'; // Azul: acción en curso
                case 'demorado':
                return 'bg-orange-100 text-orange-600 border-orange-200';
            case 'listo para despachar':
                return 'bg-green-100 text-green-600 border-green-200'; // Verde: listo para el siguiente paso
            case 'en camino':
            case 'despachando':
                return 'bg-indigo-100 text-indigo-600 border-indigo-200'; // Indigo: transporte
            case 'entregado':
                return 'bg-emerald-100 text-emerald-700 border-emerald-200'; // Esmeralda: éxito
            case 'cancelado':
            case 'entrega fallida':
                return 'bg-red-100 text-red-600 border-red-200'; // Rojo: error/cancelación
            default:
                return 'bg-gray-50 text-gray-500 border-gray-100';
                
        }
    };

    return (
        <DashboardLayout>
            <div className="space-y-8 font-sans">
                {/* Header */}
                <div className="flex flex-col md:flex-row justify-between items-start md:items-center gap-4">
    <div>
        <h1 className="text-3xl font-bold text-gray-900 tracking-tight">Bienvenido, {user?.nombreCompleto?.split(' ')[0]}</h1>
        <p className="text-gray-500 mt-1 text-sm">Panel de Administración - {user?.nombreSucursal}</p>
    </div>
    <Button onClick={() => navigate('/pedidos/nuevo')} className="bg-blue-700 hover:bg-blue-800 text-white px-6 py-2.5 rounded-lg shadow-lg shadow-blue-500/20 flex items-center gap-2 transition-all">
        <Plus size={20} /> Nuevo Pedido
    </Button>
</div>

                {/* Estadísticas - Estilo Bordeado (Outline) */}
                <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-4 gap-6">
                    {/* Activos - Azul */}
                    <div className="bg-white border-2 border-blue-500 rounded-2xl p-6 shadow-sm relative overflow-hidden group transition-all hover:shadow-md">
                        <div className="relative z-10">
                            <p className="text-blue-600 text-sm font-medium mb-1">Activos</p>
                            <h3 className="text-4xl font-bold mb-2 text-blue-900">{stats.activos}</h3>
                            <p className="text-gray-500 text-xs">En proceso actualmente</p>
                        </div>
                        <div className="absolute right-4 top-4 bg-blue-100/50 p-3 rounded-xl backdrop-blur-sm">
                            <Package className="w-6 h-6 text-blue-600" />
                        </div>
                    </div>

                    {/* Demorados - Naranja */}
                    <div className="bg-white border-2 border-orange-500 rounded-2xl p-6 shadow-sm relative overflow-hidden group transition-all hover:shadow-md">
                        <div className="relative z-10">
                            <p className="text-orange-600 text-sm font-medium mb-1">Demorados</p>
                            <h3 className="text-4xl font-bold mb-2 text-orange-900">{stats.demorados}</h3>
                            <p className="text-gray-500 text-xs">Requieren atención</p>
                        </div>
                        <div className="absolute right-4 top-4 bg-orange-100/50 p-3 rounded-xl backdrop-blur-sm">
                            <AlertTriangle className="w-6 h-6 text-orange-600" />
                        </div>
                    </div>

                    {/* Entregados - Verde */}
                    <div className="bg-white border-2 border-emerald-500 rounded-2xl p-6 shadow-sm relative overflow-hidden group transition-all hover:shadow-md">
                        <div className="relative z-10">
                            <p className="text-emerald-600 text-sm font-medium mb-1">Entregados</p>
                            <h3 className="text-4xl font-bold mb-2 text-emerald-900">{stats.entregados}</h3>
                            <p className="text-gray-500 text-xs">Completados hoy</p>
                        </div>
                        <div className="absolute right-4 top-4 bg-emerald-100/50 p-3 rounded-xl backdrop-blur-sm">
                            <CheckCircle className="w-6 h-6 text-emerald-600" />
                        </div>
                    </div>

                    {/* Cancelados - Rojo */}
                    <div className="bg-white border-2 border-red-500 rounded-2xl p-6 shadow-sm relative overflow-hidden group transition-all hover:shadow-md">
                        <div className="relative z-10">
                            <p className="text-red-600 text-sm font-medium mb-1">Cancelados</p>
                            <h3 className="text-4xl font-bold mb-2 text-red-900">{stats.cancelados}</h3>
                            <p className="text-gray-500 text-xs">Este mes</p>
                        </div>
                        <div className="absolute right-4 top-4 bg-red-100/50 p-3 rounded-xl backdrop-blur-sm">
                            <XCircle className="w-6 h-6 text-red-600" />
                        </div>
                    </div>
                </div>

                {/* Filtros */}
                <div className="bg-white p-2 rounded-xl border border-gray-100 shadow-sm">
                    <OrderFilters 
                        userRole="Administrador" 
                        onFilterChange={(filtros) => loadDashboardData(filtros)} 
                    />
                </div>

                {loading ? (
                    <div className="flex items-center justify-center h-48">
                        <div className="animate-spin rounded-full h-10 w-10 border-4 border-gray-200 border-t-blue-600"></div>
                    </div>
                ) : (
                    <>
                        {/* Tarjetas de Acción Rápida (Estilo Split) */}
                        <div className="grid lg:grid-cols-2 gap-6">
                            {/* Pendientes de Operario */}
                            <div className="bg-white rounded-2xl p-6 shadow-sm border border-gray-100">
                                <div className="flex items-center justify-between mb-6">
                                    <div className="flex items-center gap-4">
                                        <div className="bg-blue-100 p-3 rounded-full text-blue-600">
                                            <Clock size={24} />
                                        </div>
                                        <div>
                                            <h3 className="text-lg font-bold text-gray-800">Pendientes de Operario</h3>
                                            <p className="text-sm text-gray-500">
                                                {pedidos.filter(p => p.estadoNombre === 'Sin preparar').length} pedidos en espera
                                            </p>
                                        </div>
                                    </div>
                                    <Badge variant="info">{pedidos.filter(p => p.estadoNombre === 'Sin preparar').length}</Badge>
                                </div>
                                
                                <div className="space-y-3">
                                    {pedidos.filter(p => p.estadoNombre === 'Sin preparar').slice(0, 3).map((pedido) => (
                                        <div key={pedido.idPedido} className="flex items-center justify-between p-4 bg-gray-50 rounded-xl hover:bg-blue-50/50 transition-colors border border-gray-100">
                                            <div className="flex items-center gap-3">
                                                <div className="w-8 h-8 rounded-full bg-blue-200 flex items-center justify-center text-blue-700 text-xs font-bold">
                                                    #{pedido.idPedido}
                                                </div>
                                                <div>
                                                    <p className="font-semibold text-gray-800 text-sm">{pedido.clienteNombre}</p>
                                                    <p className="text-xs text-gray-500 font-medium">Sin preparar</p>
                                                </div>
                                            </div>
                                            <Button 
                                                size="sm" 
                                                className="bg-blue-600 text-white hover:bg-blue-700 rounded-lg px-4"
                                                onClick={() => { setSelectedPedido(pedido); setModalOpen(true); }}
                                            >
                                                <Eye className="w-4 h-4 mr-2" /> Asignar
                                            </Button>
                                        </div>
                                    ))}
                                    {pedidos.filter(p => p.estadoNombre === 'Sin preparar').length === 0 && (
                                        <div className="text-center py-8 text-gray-400 text-sm">Todo al día. No hay pedidos pendientes.</div>
                                    )}
                                </div>
                            </div>
                            
                            {/* Listos para Cadete */}
                            <div className="bg-white rounded-2xl p-6 shadow-sm border border-gray-100">
                                <div className="flex items-center justify-between mb-6">
                                    <div className="flex items-center gap-4">
                                        <div className="bg-yellow-100 p-3 rounded-full text-yellow-600">
                                            <Package size={24} />
                                        </div>
                                        <div>
                                            <h3 className="text-lg font-bold text-gray-800">Listos para Cadete</h3>
                                            <p className="text-sm text-gray-500">
                                                {pedidos.filter(p => p.estadoNombre === 'Listo para despachar').length} pedidos para despacho
                                            </p>
                                        </div>
                                    </div>
                                    <Badge variant="warning">{pedidos.filter(p => p.estadoNombre === 'Listo para despachar').length}</Badge>
                                </div>

                                <div className="space-y-3">
                                    {pedidos.filter(p => p.estadoNombre === 'Listo para despachar').slice(0, 3).map((pedido) => (
                                        <div key={pedido.idPedido} className="flex items-center justify-between p-4 bg-gray-50 rounded-xl hover:bg-yellow-50/50 transition-colors border border-gray-100">
                                            <div className="flex items-center gap-3">
                                                <div className="w-8 h-8 rounded-full bg-yellow-200 flex items-center justify-center text-yellow-800 text-xs font-bold">
                                                    #{pedido.idPedido}
                                                </div>
                                                <div>
                                                    <p className="font-semibold text-gray-800 text-sm">{pedido.clienteNombre}</p>
                                                    <p className="text-xs text-green-600 font-medium">Listo para despachar</p>
                                                </div>
                                            </div>
                                            <Button 
                                                size="sm" 
                                                className="bg-green-600 text-white hover:bg-green-700 rounded-lg px-4 shadow-sm"
                                                onClick={() => { setSelectedPedidoCadete(pedido); setModalCadeteOpen(true); }}
                                            >
                                                <Truck className="w-4 h-4 mr-2" /> Asignar Cadete
                                            </Button>
                                        </div>
                                    ))}
                                    {pedidos.filter(p => p.estadoNombre === 'Listo para despachar').length === 0 && (
                                        <div className="text-center py-8 text-gray-400 text-sm">No hay pedidos listos para despacho.</div>
                                    )}
                                </div>
                            </div>
                        </div>

                       {/* Tabla General Estilo Imagen */}
<div className="bg-white rounded-2xl shadow-sm border border-gray-100 overflow-hidden">
    <div className="p-6 bg-blue-600 text-white">
        <h2 className="text-lg font-bold">Todos los Pedidos</h2>
        <p className="text-blue-100 text-sm">Gestiona y monitorea todos los pedidos en tiempo real</p>
    </div>
    <div className="overflow-x-auto">
        <table className="w-full text-left border-collapse">
            <thead className="bg-gray-50 text-xs uppercase text-gray-500 font-semibold">
                <tr>
                    <th className="p-5">ID</th>
                    {/* MOVIDO AQUÍ: Fecha después del ID */}
                    <th className="p-5">Fecha</th> 
                    <th className="p-5">Cliente</th>
                    <th className="p-5">Responsable</th>
                    <th className="p-5">Estado</th>
                    <th className="p-5 text-right">Acciones</th>
                </tr>
            </thead>
            <tbody className="divide-y divide-gray-100">
                {pedidos.map((pedido) => (
                    <tr key={pedido.idPedido} className="hover:bg-blue-50/30 transition-colors group">
                        <td className="p-5 font-bold text-gray-700">#{pedido.idPedido}</td>
                        
                        {/* MOVIDO AQUÍ: Celda de Fecha */}
                        <td className="p-5 text-sm text-gray-500 whitespace-nowrap">
                            {new Date(pedido.fecha).toLocaleDateString('es-AR')}
                        </td>

                        <td className="p-5">
                            <div className="flex items-center gap-3">
                                <div className={`w-10 h-10 rounded-full ${getAvatarColor(pedido.clienteNombre)} text-white flex items-center justify-center font-bold text-sm shadow-sm`}>
                                    {getInitials(pedido.clienteNombre)}
                                </div>
                                <span className="font-medium text-gray-800">{pedido.clienteNombre}</span>
                            </div>
                        </td>
                        <td className="p-5 text-sm text-gray-500">
                            {pedido.responsableNombre || <span className="text-gray-400 italic">Sin asignar</span>}
                        </td>
                        <td className="p-5">
                            <span className={`px-3 py-1 rounded-full text-[11px] font-bold border ${getEstadoStyle(pedido.estadoNombre, pedido.estaDemorado || (pedido as any).EstaDemorado)}`}>
                                {pedido.estadoNombre.toUpperCase()}
                            </span>
                        </td>
                        <td className="p-5 text-right">
                            <button 
    onClick={() => { setSelectedPedidoDetalle(pedido); setModalDetalleOpen(true); }} 
    className="text-blue-600 hover:text-blue-800 font-bold text-sm flex items-center justify-end gap-2 ml-auto transition-colors group"
>
    <Eye className="w-4 h-4 text-blue-600 group-hover:text-blue-800" /> 
    <span>Ver detalles</span>
</button>
                        </td>
                    </tr>
                ))}
            </tbody>
        </table>
    </div>
</div>
{/* CIERRES DE SEGURIDAD */}
                    </>
                )}
            </div>

            {/* Modales */}
            {selectedPedido && (
                <AsignarOperarioModal 
                    isOpen={modalOpen} 
                    onClose={() => { setModalOpen(false); setSelectedPedido(null); }} 
                    pedido={selectedPedido} 
                    onSuccess={() => loadDashboardData()} 
                />
            )}

            {selectedPedidoCadete && (
                <AsignarCadeteModal 
                    isOpen={modalCadeteOpen} 
                    onClose={() => { setModalCadeteOpen(false); setSelectedPedidoCadete(null); }} 
                    pedido={selectedPedidoCadete} 
                    onSuccess={() => loadDashboardData()} 
                />
            )}
            
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