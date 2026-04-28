import React, { useState, useEffect } from 'react';
import { DashboardLayout } from '../components/layout/DashboardLayout.tsx';
import { Card } from '../components/common/Card.tsx';
import { Badge } from '@components/common/Badge.tsx';
import { Button } from '@components/common/Button';
import { AsignarOperarioModal } from '@components/pedidos/AsignarOperarioModal';
import { AsignarCadeteModal } from '@components/pedidos/AsignarCadeteModal';
import { DetallePedidoModal } from '@components/pedidos/DetallePedidoModal';
import { OrderFilters } from '@components/orders/OrderFilters';
import { TableroKanban } from '@components/kanban/TableroKanban';
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
  Clock,
} from 'lucide-react';
import { useNavigate } from 'react-router-dom';

export const DashboardAdmin: React.FC = () => {
  const { user } = useAuth();
  const navigate = useNavigate();

  const [pedidos, setPedidos] = useState<OrderSummaryDTO[]>([]);
  const [pedidosFiltrados, setPedidosFiltrados] = useState<OrderSummaryDTO[]>([]);
  const [loading, setLoading] = useState(true);
  const [selectedPedido, setSelectedPedido] = useState<OrderSummaryDTO | null>(null);
  const [modalOpen, setModalOpen] = useState(false);
  const [selectedPedidoCadete, setSelectedPedidoCadete] = useState<OrderSummaryDTO | null>(null);
  const [modalCadeteOpen, setModalCadeteOpen] = useState(false);
  const [selectedPedidoDetalle, setSelectedPedidoDetalle] = useState<OrderSummaryDTO | null>(null);
  const [modalDetalleOpen, setModalDetalleOpen] = useState(false);
  const [stats, setStats] = useState({ activos: 0, demorados: 0, entregados: 0, cancelados: 0 });
  const [viewMode, setViewMode] = useState<'tabla' | 'kanban'>('tabla');

  useEffect(() => {
    loadDashboardData();
  }, []);

  const loadDashboardData = async (filtros: {
    estado?: string;
    search?: string;
    soloDemorados?: boolean;
  } = {}) => {
    try {
      setLoading(true);

      // Traer todos los pedidos del backend (con filtros de estado/search si aplica)
      const backendFiltros: any = {};
      if (filtros.estado) backendFiltros.estado = filtros.estado;
      if (filtros.search) backendFiltros.search = filtros.search;

      const data = await pedidosService.getFilteredOrders(backendFiltros);
      setPedidos(data);

      // Contar demorados desde el endpoint dedicado (fuente de verdad del backend)
      const API_BASE = import.meta.env.VITE_API_BASE_URL || 'http://localhost:5000/api';
      const demoradosResponse = await fetch(`${API_BASE}/Reporte/pedidos-demorados`);
      const pedidosDemoradosBackend: OrderSummaryDTO[] = demoradosResponse.ok
        ? await demoradosResponse.json()
        : [];

      const demoradosCount =
        pedidosDemoradosBackend.length > 0
          ? pedidosDemoradosBackend.length
          : data.filter((p) => p.estaDemorado || (p as any).EstaDemorado).length;

      setStats({
        activos: data.filter((p) => !['Entregado', 'Cancelado'].includes(p.estadoNombre)).length,
        // La tarjeta de demorados cuenta TODOS los pedidos con subestado demorado,
        // independientemente del estado principal en el que estén.
        demorados: demoradosCount,
        entregados: data.filter((p) => p.estadoNombre === 'Entregado').length,
        cancelados: data.filter((p) => p.estadoNombre === 'Cancelado').length,
      });

      // Aplicar filtro de demorados en el frontend si se activa el toggle
      aplicarFiltroLocal(data, filtros.soloDemorados ?? false);
    } catch (error) {
      console.error('Error cargando pedidos:', error);
    } finally {
      setLoading(false);
    }
  };

  /**
   * Filtra los pedidos localmente por subestado demorado.
   * Se aplica como capa adicional sobre el filtrado por estado principal.
   */
  const aplicarFiltroLocal = (data: OrderSummaryDTO[], soloDemorados: boolean) => {
    if (soloDemorados) {
      setPedidosFiltrados(
        data.filter((p) => p.estaDemorado || (p as any).EstaDemorado)
      );
    } else {
      setPedidosFiltrados(data);
    }
  };

  const handleFilterChange = (filtros: {
    estado?: string;
    search?: string;
    soloDemorados?: boolean;
  }) => {
    loadDashboardData(filtros);
  };

  const getInitials = (name: string) =>
    name.split(' ').map((n) => n[0]).join('').substring(0, 2).toUpperCase();

  const getAvatarColor = (name: string) => {
    const colors = ['bg-blue-500', 'bg-purple-500', 'bg-pink-500', 'bg-indigo-500', 'bg-teal-500'];
    return colors[name.length % colors.length];
  };

  /**
   * Estilo del badge de estado principal.
   * Si el pedido está demorado, se añade el badge naranja de subestado
   * de forma separada — no se sobreescribe el estado principal.
   */
  const getEstadoStyle = (estado: string) => {
    const est = estado.toLowerCase();
    switch (est) {
      case 'sin preparar': return 'bg-gray-100 text-gray-400 border-gray-200';
      case 'preparar pedido':
      case 'preparando':
      case 'en preparación': return 'bg-blue-100 text-blue-600 border-blue-200';
      case 'listo para despachar': return 'bg-green-100 text-green-600 border-green-200';
      case 'en camino':
      case 'despachando': return 'bg-indigo-100 text-indigo-600 border-indigo-200';
      case 'entregado': return 'bg-emerald-100 text-emerald-700 border-emerald-200';
      case 'cancelado':
      case 'entrega fallida': return 'bg-red-100 text-red-600 border-red-200';
      default: return 'bg-gray-50 text-gray-500 border-gray-100';
    }
  };

  return (
    <DashboardLayout>
      <div className="p-6 bg-gray-50 min-h-screen">
        <div className="max-w-6xl mx-auto space-y-8 font-sans">

          {/* Header */}
          <div className="flex flex-col md:flex-row justify-between items-start gap-4">
            <div>
              <h1 className="text-3xl font-bold text-gray-900 tracking-tight">
                Bienvenido, {user?.nombreCompleto?.split(' ')[0]}
              </h1>
              <p className="text-gray-500 mt-1 text-sm">
                Panel de Administración — {user?.nombreSucursal}
              </p>
            </div>
            <Button
              onClick={() => navigate('/pedidos/nuevo')}
              className="bg-blue-700 hover:bg-blue-800 text-white px-6 py-2.5 rounded-lg shadow-lg shadow-blue-500/20 flex items-center gap-2 transition-all"
            >
              <Plus size={20} /> Nuevo Pedido
            </Button>
          </div>

          {/* Estadísticas */}
          <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-4 gap-6">
            {/* Activos */}
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

            {/* Demorados — cuenta TODOS los que tienen subestado demorado, sin importar el estado principal */}
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

            {/* Entregados */}
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

            {/* Cancelados */}
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
              userRole="Encargado"
              onFilterChange={handleFilterChange}
            />
          </div>

          {loading ? (
            <div className="flex items-center justify-center h-48">
              <div className="animate-spin rounded-full h-10 w-10 border-4 border-gray-200 border-t-blue-600" />
            </div>
          ) : (
            <>
              {/* Tarjetas de Acción Rápida */}
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
                          {pedidos.filter((p) => p.estadoNombre === 'Sin preparar').length} pedidos en espera
                        </p>
                      </div>
                    </div>
                    <Badge variant="info">
                      {pedidos.filter((p) => p.estadoNombre === 'Sin preparar').length}
                    </Badge>
                  </div>
                  <div className="space-y-3">
                    {pedidos
                      .filter((p) => p.estadoNombre === 'Sin preparar')
                      .slice(0, 3)
                      .map((pedido) => (
                        <div
                          key={pedido.idPedido}
                          className="flex items-center justify-between p-4 bg-gray-50 rounded-xl hover:bg-blue-50/50 transition-colors border border-gray-100"
                        >
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
                    {pedidos.filter((p) => p.estadoNombre === 'Sin preparar').length === 0 && (
                      <div className="text-center py-8 text-gray-400 text-sm">
                        Todo al día. No hay pedidos pendientes.
                      </div>
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
                          {pedidos.filter((p) => p.estadoNombre === 'Listo para despachar').length} pedidos para despacho
                        </p>
                      </div>
                    </div>
                    <Badge variant="warning">
                      {pedidos.filter((p) => p.estadoNombre === 'Listo para despachar').length}
                    </Badge>
                  </div>
                  <div className="space-y-3">
                    {pedidos
                      .filter((p) => p.estadoNombre === 'Listo para despachar')
                      .slice(0, 3)
                      .map((pedido) => (
                        <div
                          key={pedido.idPedido}
                          className="flex items-center justify-between p-4 bg-gray-50 rounded-xl hover:bg-yellow-50/50 transition-colors border border-gray-100"
                        >
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
                    {pedidos.filter((p) => p.estadoNombre === 'Listo para despachar').length === 0 && (
                      <div className="text-center py-8 text-gray-400 text-sm">
                        No hay pedidos listos para despacho.
                      </div>
                    )}
                  </div>
                </div>
              </div>

              {/* Tablero / Tabla */}
              <div className="bg-white rounded-2xl shadow-sm border border-gray-100 overflow-hidden">
                <div className="p-6 bg-gradient-to-r from-blue-600 to-indigo-600 text-white flex justify-between items-center">
                  <div>
                    <h2 className="text-lg font-bold">Gestión de Pedidos</h2>
                    <p className="text-blue-100 text-sm">Visualiza y gestiona todos tus pedidos</p>
                  </div>
                  <div className="flex gap-2 bg-blue-700 p-1 rounded-lg">
                    <button
                      onClick={() => setViewMode('tabla')}
                      className={`px-4 py-2 rounded-md font-medium text-sm transition-all ${
                        viewMode === 'tabla' ? 'bg-white text-blue-600 shadow-md' : 'text-blue-100 hover:text-white'
                      }`}
                    >
                      📊 Tabla
                    </button>
                    <button
                      onClick={() => setViewMode('kanban')}
                      className={`px-4 py-2 rounded-md font-medium text-sm transition-all ${
                        viewMode === 'kanban' ? 'bg-white text-blue-600 shadow-md' : 'text-blue-100 hover:text-white'
                      }`}
                    >
                      📋 Kanban
                    </button>
                  </div>
                </div>

                {viewMode === 'tabla' && (
                  <div className="overflow-x-auto">
                    <table className="w-full text-left border-collapse">
                      <thead className="bg-gray-50 text-xs uppercase text-gray-500 font-semibold">
                        <tr>
                          <th className="p-5">ID</th>
                          <th className="p-5">Fecha</th>
                          <th className="p-5">Cliente</th>
                          <th className="p-5">Responsable</th>
                          <th className="p-5">Estado</th>
                          <th className="p-5 text-right">Acciones</th>
                        </tr>
                      </thead>
                      <tbody className="divide-y divide-gray-100">
                        {pedidosFiltrados.map((pedido) => {
                          const demorado = pedido.estaDemorado || (pedido as any).EstaDemorado;
                          return (
                            <tr key={pedido.idPedido} className="hover:bg-blue-50/30 transition-colors group">
                              <td className="p-5 font-bold text-gray-700">#{pedido.idPedido}</td>
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
                                {pedido.responsableNombre || (
                                  <span className="text-gray-400 italic">Sin asignar</span>
                                )}
                              </td>
                              <td className="p-5">
                                {/* Estado principal */}
                                <span className={`px-3 py-1 rounded-full text-[11px] font-bold border ${getEstadoStyle(pedido.estadoNombre)}`}>
                                  {pedido.estadoNombre.toUpperCase()}
                                </span>
                                {/* Subestado demorado — se muestra JUNTO al estado principal */}
                                {demorado && (
                                  <span className="ml-2 inline-flex items-center gap-1 bg-orange-100 text-orange-700 text-[10px] font-semibold px-2 py-0.5 rounded-full border border-orange-200">
                                    <Clock className="w-3 h-3" />
                                    Demorado
                                  </span>
                                )}
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
                          );
                        })}
                      </tbody>
                    </table>
                  </div>
                )}

                {viewMode === 'kanban' && (
                  <div className="p-4">
                    <TableroKanban pedidos={pedidosFiltrados} onUpdate={loadDashboardData} />
                  </div>
                )}
              </div>
            </>
          )}
        </div>
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

