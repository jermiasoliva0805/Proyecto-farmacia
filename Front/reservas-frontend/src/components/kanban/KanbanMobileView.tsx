import React, { useState } from 'react';
import { OrderSummaryDTO } from '../../types/pedido.types';
import { useAuth } from '../../context/AuthContext';
import { pedidosService } from '../../service/PedidosService';
import { COLUMNAS_POR_ROL, ESTADO_MAP, VALIDACIONES_OPERARIO, VALIDACIONES_CADETE, VALIDACIONES_ENCARGADO } from '../../types/kanban.types';
import { ChevronDown, ChevronUp, AlertTriangle, Loader, Clock, CheckCircle2, AlertCircle } from 'lucide-react';
import { CancelarConMotivoModal } from './CancelarConMotivoModal';
import { AsignarCadeteModal } from '../pedidos/AsignarCadeteModal';

interface KanbanMobileViewProps {
    pedidos: OrderSummaryDTO[];
    onUpdate: () => Promise<void>;
    usuarioId?: number;
}

interface ToastState {
    visible: boolean;
    type: 'success' | 'error' | 'info';
    message: string;
}

interface ExpandedStateMap {
    [estadoId: number]: boolean;
}

export const KanbanMobileView: React.FC<KanbanMobileViewProps> = ({
    pedidos,
    onUpdate,
    usuarioId
}) => {
    const { user } = useAuth();
    const [toast, setToast] = useState<ToastState>({ visible: false, type: 'info', message: '' });
    const [isLoadingPedidoId, setIsLoadingPedidoId] = useState<number | undefined>();
    const [pedidosLocal, setPedidosLocal] = useState<OrderSummaryDTO[]>(pedidos);
    const [expandedColumns, setExpandedColumns] = useState<ExpandedStateMap>({});
    
    // Estados para modales
    const [showCancelModal, setShowCancelModal] = useState(false);
    const [pedidoACancelar, setPedidoACancelar] = useState<OrderSummaryDTO | null>(null);
    const [showAsignarCadeteModal, setShowAsignarCadeteModal] = useState(false);
    const [pedidoAAsignarCadete, setPedidoAAsignarCadete] = useState<OrderSummaryDTO | null>(null);

    // Sincronizar pedidos locales
    React.useEffect(() => {
        setPedidosLocal(pedidos);
    }, [pedidos]);

    // Mostrar toast
    const showToast = (type: 'success' | 'error' | 'info', message: string) => {
        setToast({ visible: true, type, message });
        setTimeout(() => setToast({ ...toast, visible: false }), 4000);
    };

    // Obtener columnas visibles según rol
    const getColumnasVisibles = (): number[] => {
        if (!user) return COLUMNAS_POR_ROL['Encargado'];
        return COLUMNAS_POR_ROL[user.rol] || COLUMNAS_POR_ROL['Encargado'];
    };

    // Obtener validador según rol
    const getValidador = () => {
        if (!user) return VALIDACIONES_ENCARGADO;
        switch (user.rol) {
            case 'Operario':
                return VALIDACIONES_OPERARIO;
            case 'Cadete':
                return VALIDACIONES_CADETE;
            default:
                return VALIDACIONES_ENCARGADO;
        }
    };

    // Agrupar pedidos por estado
    const pedidosPorEstado: Record<number, OrderSummaryDTO[]> = {};
    getColumnasVisibles().forEach(estadoId => {
        pedidosPorEstado[estadoId] = pedidosLocal.filter(p => p.idEstadoDePedido === estadoId);
    });

    // Toggle expansión de columna
    const toggleColumn = (estadoId: number) => {
        setExpandedColumns(prev => ({
            ...prev,
            [estadoId]: !prev[estadoId]
        }));
    };

    // Formatear moneda
    const formatCurrency = (value: number) => {
        return new Intl.NumberFormat('es-AR', {
            style: 'currency',
            currency: 'ARS'
        }).format(value);
    };

    // Cambiar estado de un pedido
    const handleCambiarEstado = async (pedido: OrderSummaryDTO, nuevoEstadoId: number) => {
        // Validaciones
        const validador = getValidador();
        const columnasVisibles = getColumnasVisibles();

        if (!columnasVisibles.includes(nuevoEstadoId)) {
            showToast('error', 'No tienes permisos para cambiar a este estado');
            return;
        }

        if (!validador.puedeMoverHacia(pedido.idEstadoDePedido, nuevoEstadoId)) {
            showToast('error', 'No tienes permisos para este cambio de estado');
            return;
        }

        if ([7, 9].includes(pedido.idEstadoDePedido)) {
            showToast('error', 'Este pedido ya alcanzó un estado final');
            return;
        }

        if (user?.rol === 'Operario' && !pedido.fechaInicioArmado) {
            showToast('error', '⚠️ Debes presionar "Comenzar armado" antes');
            return;
        }

        // Caso especial: Asignar cadete
        if (user?.rol === 'Encargado' && pedido.idEstadoDePedido === 4 && nuevoEstadoId === 5) {
            setPedidoAAsignarCadete(pedido);
            setShowAsignarCadeteModal(true);
            return;
        }

        // Caso especial: Cancelación
        if (nuevoEstadoId === 9) {
            setPedidoACancelar(pedido);
            setShowCancelModal(true);
            return;
        }

        // Cambiar estado
        setIsLoadingPedidoId(pedido.idPedido);
        try {
            await pedidosService.cambiarEstado({
                idPedido: pedido.idPedido,
                idNuevoEstado: nuevoEstadoId,
                idUsuario: usuarioId || user?.id || 0,
                observaciones: ''
            });

            const nuevaListaPedidos = pedidosLocal.map(p => {
                if (p.idPedido === pedido.idPedido) {
                    return {
                        ...p,
                        idEstadoDePedido: nuevoEstadoId,
                        estadoNombre: ESTADO_MAP[nuevoEstadoId]
                    };
                }
                return p;
            });

            setPedidosLocal(nuevaListaPedidos);
            showToast('success', `Pedido #${pedido.idPedido} movido a ${ESTADO_MAP[nuevoEstadoId]}`);

            if (user?.rol === 'Operario' && nuevoEstadoId === 4) {
                setTimeout(() => {
                    setPedidosLocal(prev => prev.filter(p => p.idPedido !== pedido.idPedido));
                }, 1000);
            }

            await onUpdate();
        } catch (error: any) {
            const mensajeError = error.response?.data?.message || 'Error al cambiar estado';
            showToast('error', mensajeError);
        } finally {
            setIsLoadingPedidoId(undefined);
        }
    };

    // Obtener siguientes estados válidos
    const getSiguientesEstados = (estadoActual: number): number[] => {
        const validador = getValidador();
        const columnasVisibles = getColumnasVisibles();
        
        return columnasVisibles.filter(estado => 
            estado !== estadoActual && 
            validador.puedeMoverHacia(estadoActual, estado)
        );
    };

    // Obtener colores por estado
    const getColoresEstado = (estado: number) => {
        const colorMap: Record<number, { bg: string; text: string; border: string; header: string }> = {
            1: { bg: 'bg-gray-50', text: 'text-gray-700', border: 'border-gray-200', header: 'bg-gray-100' },
            2: { bg: 'bg-blue-50', text: 'text-blue-700', border: 'border-blue-200', header: 'bg-blue-100' },
            3: { bg: 'bg-orange-50', text: 'text-orange-700', border: 'border-orange-200', header: 'bg-orange-100' },
            4: { bg: 'bg-green-50', text: 'text-green-700', border: 'border-green-200', header: 'bg-green-100' },
            5: { bg: 'bg-indigo-50', text: 'text-indigo-700', border: 'border-indigo-200', header: 'bg-indigo-100' },
            6: { bg: 'bg-purple-50', text: 'text-purple-700', border: 'border-purple-200', header: 'bg-purple-100' },
            7: { bg: 'bg-emerald-50', text: 'text-emerald-700', border: 'border-emerald-200', header: 'bg-emerald-100' },
            8: { bg: 'bg-red-50', text: 'text-red-700', border: 'border-red-200', header: 'bg-red-100' },
            9: { bg: 'bg-slate-50', text: 'text-slate-700', border: 'border-slate-200', header: 'bg-slate-100' },
        };
        return colorMap[estado] || colorMap[1];
    };

    // Mostrar alerta si pedido está demorado
    const mostrarAlertaDemora = (pedido: OrderSummaryDTO) => {
        return pedido.estaDemorado ? (
            <div className="flex items-center gap-1 text-orange-600">
                <AlertTriangle className="w-4 h-4" />
                <span className="text-xs font-medium">Demorado</span>
            </div>
        ) : null;
    };

    // Mostrar indicador de armado para operarios
    const mostrarIndicadorArmado = (pedido: OrderSummaryDTO) => {
        if (user?.rol !== 'Operario') return null;

        if (!pedido.fechaInicioArmado) {
            return (
                <div className="flex items-center gap-1 text-amber-600">
                    <Clock className="w-4 h-4" />
                    <span className="text-xs font-medium">No iniciado</span>
                </div>
            );
        }
        if (!pedido.fechaFinArmado) {
            return (
                <div className="flex items-center gap-1 text-blue-600">
                    <Loader className="w-4 h-4 animate-spin" />
                    <span className="text-xs font-medium">En progreso</span>
                </div>
            );
        }
        return (
            <div className="flex items-center gap-1 text-green-600">
                <CheckCircle2 className="w-4 h-4" />
                <span className="text-xs font-medium">Completado</span>
            </div>
        );
    };

    const columnasVisibles = getColumnasVisibles();

    return (
        <div className="w-full">
            {/* Toast */}
            {toast.visible && (
                <div
                    className={`
                        fixed top-4 right-4 max-w-sm p-3 rounded-lg shadow-lg 
                        flex items-center gap-2 animate-in fade-in slide-in-from-top-[10px]
                        z-50 text-sm
                        ${toast.type === 'success' ? 'bg-emerald-50 text-emerald-800 border border-emerald-200' : ''}
                        ${toast.type === 'error' ? 'bg-red-50 text-red-800 border border-red-200' : ''}
                        ${toast.type === 'info' ? 'bg-blue-50 text-blue-800 border border-blue-200' : ''}
                    `}
                >
                    {toast.type === 'success' && <CheckCircle2 className="w-4 h-4 flex-shrink-0" />}
                    {toast.type === 'error' && <AlertCircle className="w-4 h-4 flex-shrink-0" />}
                    <p className="font-medium">{toast.message}</p>
                </div>
            )}

            {/* Columnas agrupadas verticalmente */}
            <div className="space-y-4 pb-4">
                {columnasVisibles.map(estadoId => {
                    const colores = getColoresEstado(estadoId);
                    const pedidosColumna = pedidosPorEstado[estadoId];
                    const isExpanded = expandedColumns[estadoId] !== false; // Expandidas por defecto

                    return (
                        <div
                            key={estadoId}
                            className={`rounded-lg border-2 ${colores.border} overflow-hidden`}
                        >
                            {/* Header - siempre visible */}
                            <button
                                onClick={() => toggleColumn(estadoId)}
                                className={`w-full ${colores.header} px-4 py-3 flex items-center justify-between hover:opacity-80 transition-opacity`}
                            >
                                <div className="text-left">
                                    <h3 className={`font-bold text-sm ${colores.text} uppercase tracking-wider`}>
                                        {ESTADO_MAP[estadoId]}
                                    </h3>
                                    <p className="text-xs text-gray-500 mt-0.5">
                                        {pedidosColumna.length} {pedidosColumna.length !== 1 ? 'pedidos' : 'pedido'}
                                    </p>
                                </div>
                                <div className={`transition-transform ${isExpanded ? '' : 'rotate-180'}`}>
                                    {isExpanded ? (
                                        <ChevronUp className="w-5 h-5 text-gray-600" />
                                    ) : (
                                        <ChevronDown className="w-5 h-5 text-gray-600" />
                                    )}
                                </div>
                            </button>

                            {/* Contenido - colapsable */}
                            {isExpanded && (
                                <div className={`${colores.bg} p-3 space-y-3`}>
                                    {pedidosColumna.length > 0 ? (
                                        pedidosColumna.map(pedido => (
                                            <div
                                                key={pedido.idPedido}
                                                className={`bg-white rounded-lg p-3 border border-gray-200 ${
                                                    isLoadingPedidoId === pedido.idPedido ? 'opacity-50' : ''
                                                }`}
                                            >
                                                {/* Loading indicator */}
                                                {isLoadingPedidoId === pedido.idPedido && (
                                                    <div className="absolute inset-0 flex items-center justify-center bg-white/70 rounded-lg">
                                                        <Loader className="w-5 h-5 animate-spin text-blue-500" />
                                                    </div>
                                                )}

                                                {/* ID y Fecha */}
                                                <div className="flex items-start justify-between gap-2 mb-2">
                                                    <div>
                                                        <h4 className="font-bold text-base text-gray-900">
                                                            #{pedido.idPedido}
                                                        </h4>
                                                        <p className="text-xs text-gray-500">
                                                            {new Date(pedido.fecha).toLocaleDateString('es-AR')}
                                                        </p>
                                                    </div>
                                                    <div className="flex flex-col gap-1 items-end">
                                                        {mostrarAlertaDemora(pedido)}
                                                        {mostrarIndicadorArmado(pedido)}
                                                    </div>
                                                </div>

                                                {/* Cliente */}
                                                <div className="mb-2">
                                                    <p className="text-sm font-medium text-gray-800">
                                                        {pedido.clienteNombre}
                                                    </p>
                                                    {pedido.responsableNombre && (
                                                        <p className="text-xs text-gray-600">
                                                            {pedido.responsableNombre}
                                                        </p>
                                                    )}
                                                </div>

                                                {/* Total */}
                                                <div className="mb-2 pb-2 border-t border-gray-100">
                                                    <p className="text-sm font-bold text-green-600 mt-2">
                                                        {formatCurrency(pedido.total)}
                                                    </p>
                                                </div>

                                                {/* Fecha de entrega */}
                                                {pedido.fechaEntregaEstimada && 
                                                 !pedido.fechaEntregaEstimada.startsWith('0001') &&
                                                 new Date(pedido.fechaEntregaEstimada).getFullYear() > 1900 && (
                                                    <p className="text-xs text-blue-600 mb-2">
                                                        📅 {new Date(pedido.fechaEntregaEstimada).toLocaleDateString('es-AR')}
                                                    </p>
                                                )}

                                                {/* Indicadores especiales */}
                                                {user?.rol === 'Operario' && !pedido.fechaInicioArmado && (
                                                    <div className="bg-amber-50 rounded p-2 text-xs text-amber-700 border border-amber-200 mb-2">
                                                        ⏱ Armado no iniciado
                                                    </div>
                                                )}

                                                {pedido.idEstadoDePedido === 8 && (
                                                    <div className="bg-blue-50 rounded p-2 text-xs text-blue-700 border border-blue-200 mb-2">
                                                        💡 Reintenta el envío
                                                    </div>
                                                )}

                                                {pedido.idEstadoDePedido === 5 && pedido.intentosEntregaFallida > 0 && (
                                                    <div className={`rounded p-2 text-xs border mb-2 ${
                                                        pedido.intentosEntregaFallida >= 2 
                                                            ? 'bg-yellow-50 text-yellow-700 border-yellow-200' 
                                                            : 'bg-blue-50 text-blue-700 border-blue-200'
                                                    }`}>
                                                        {pedido.intentosEntregaFallida >= 2 
                                                            ? '⚠️ Última oportunidad' 
                                                            : `↩️ Reintento ${pedido.intentosEntregaFallida}`}
                                                    </div>
                                                )}

                                                {/* Botones de acción */}
                                                <div className="flex flex-wrap gap-2 pt-2 border-t border-gray-100">
                                                    {getSiguientesEstados(pedido.idEstadoDePedido).map(nuevoEstado => (
                                                        <button
                                                            key={nuevoEstado}
                                                            onClick={() => handleCambiarEstado(pedido, nuevoEstado)}
                                                            disabled={isLoadingPedidoId === pedido.idPedido}
                                                            className={`flex-1 text-xs font-medium py-1.5 px-2 rounded border transition-all ${
                                                                nuevoEstado === 9
                                                                    ? 'bg-red-50 border-red-200 text-red-700 hover:bg-red-100'
                                                                    : 'bg-blue-50 border-blue-200 text-blue-700 hover:bg-blue-100'
                                                            } disabled:opacity-50`}
                                                        >
                                                            → {ESTADO_MAP[nuevoEstado]}
                                                        </button>
                                                    ))}
                                                </div>
                                            </div>
                                        ))
                                    ) : (
                                        <div className="text-center py-6 text-gray-400">
                                            <p className="text-sm">Sin pedidos</p>
                                        </div>
                                    )}
                                </div>
                            )}
                        </div>
                    );
                })}
            </div>

            {/* Mensaje si no hay pedidos */}
            {pedidosLocal.length === 0 && (
                <div className="flex items-center justify-center h-64 text-gray-500">
                    <div className="text-center">
                        <AlertCircle className="w-12 h-12 mx-auto mb-2 text-gray-300" />
                        <p>No hay pedidos para mostrar</p>
                    </div>
                </div>
            )}

            {/* Modales */}
            {pedidoACancelar && (
                <CancelarConMotivoModal
                    isOpen={showCancelModal}
                    pedidoId={pedidoACancelar.idPedido}
                    clienteNombre={pedidoACancelar.clienteNombre}
                    onConfirm={async (motivo) => {
                        const nuevaListaPedidos = pedidosLocal.map(p => {
                            if (p.idPedido === pedidoACancelar.idPedido) {
                                return { ...p, idEstadoDePedido: 9, estadoNombre: ESTADO_MAP[9] };
                            }
                            return p;
                        });
                        setPedidosLocal(nuevaListaPedidos);
                        showToast('success', `Pedido #${pedidoACancelar.idPedido} cancelado`);
                        setShowCancelModal(false);
                        setPedidoACancelar(null);
                        await onUpdate();
                    }}
                    onCancel={() => {
                        setShowCancelModal(false);
                        setPedidoACancelar(null);
                    }}
                />
            )}

            {pedidoAAsignarCadete && (
                <AsignarCadeteModal
                    isOpen={showAsignarCadeteModal}
                    pedido={pedidoAAsignarCadete}
                    onClose={() => {
                        setShowAsignarCadeteModal(false);
                        setPedidoAAsignarCadete(null);
                    }}
                    onSuccess={() => {
                        setShowAsignarCadeteModal(false);
                        setPedidoAAsignarCadete(null);
                        onUpdate();
                        showToast('success', 'Cadete asignado correctamente');
                    }}
                />
            )}
        </div>
    );
};
