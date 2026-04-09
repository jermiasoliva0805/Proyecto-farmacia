import React, { useState, useEffect } from 'react';
import { DragDropContext, DropResult, Droppable, Draggable } from '@hello-pangea/dnd';
import { OrderSummaryDTO } from '../../types/pedido.types';
import { useAuth } from '../../context/AuthContext';
import { pedidosService } from '../../service/PedidosService';
import { COLUMNAS_POR_ROL, ESTADO_MAP, VALIDACIONES_OPERARIO, VALIDACIONES_CADETE, VALIDACIONES_ENCARGADO } from '../../types/kanban.types';
import { ChevronDown, ChevronUp, AlertTriangle, Loader, CheckCircle2, AlertCircle } from 'lucide-react';
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
    [key: number]: boolean;
}

export const KanbanMobileView: React.FC<KanbanMobileViewProps> = ({ pedidos, onUpdate, usuarioId }) => {
    const { user } = useAuth();
    const [toast, setToast] = useState<ToastState>({ visible: false, type: 'info', message: '' });
    const [isLoadingPedidoId, setIsLoadingPedidoId] = useState<number | undefined>();
    const [dragBlockedPedidoId, setDragBlockedPedidoId] = useState<number | undefined>();
    const [pedidosLocal, setPedidosLocal] = useState<OrderSummaryDTO[]>(pedidos);
    const [expandedColumns, setExpandedColumns] = useState<ExpandedStateMap>({});
    const [showCancelModal, setShowCancelModal] = useState(false);
    const [pedidoACancelar, setPedidoACancelar] = useState<OrderSummaryDTO | null>(null);
    const [showAsignarCadeteModal, setShowAsignarCadeteModal] = useState(false);
    const [pedidoAAsignarCadete, setPedidoAAsignarCadete] = useState<OrderSummaryDTO | null>(null);

    useEffect(() => {
        setPedidosLocal(pedidos);
    }, [pedidos]);

    const showToast = (type: 'success' | 'error' | 'info', message: string) => {
        setToast({ visible: true, type, message });
        setTimeout(() => setToast({ ...toast, visible: false }), 4000);
    };

    const getColumnasVisibles = (): number[] => {
        if (!user) return COLUMNAS_POR_ROL['Encargado'];
        return COLUMNAS_POR_ROL[user.rol] || COLUMNAS_POR_ROL['Encargado'];
    };

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

    const columnasVisibles = getColumnasVisibles();
    const pedidosPorEstado: Record<number, OrderSummaryDTO[]> = {};
    columnasVisibles.forEach(estadoId => {
        pedidosPorEstado[estadoId] = pedidosLocal.filter(p => p.idEstadoDePedido === estadoId);
    });

    const toggleColumn = (estadoId: number) => {
        setExpandedColumns(prev => ({ ...prev, [estadoId]: !prev[estadoId] }));
    };

    const formatCurrency = (value: number) => {
        return new Intl.NumberFormat('es-AR', { style: 'currency', currency: 'ARS' }).format(value);
    };

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

    const handleDragEnd = async (result: DropResult) => {
        const { source, destination, draggableId } = result;
        if (!destination) return;
        if (source.droppableId === destination.droppableId && source.index === destination.index) return;

        const estadoOrigenId = parseInt(source.droppableId.split('-')[1]);
        const estadoDestinoId = parseInt(destination.droppableId.split('-')[1]);
        const pedidoId = parseInt(draggableId.split('-')[1]);

        const pedido = pedidosLocal.find(p => p.idPedido === pedidoId);
        if (!pedido) return;

        const validador = getValidador();
        const colsVisibles = getColumnasVisibles();

        if (!colsVisibles.includes(estadoDestinoId)) {
            showToast('error', 'No tienes permisos para cambiar a este estado');
            return;
        }

        if (!validador.puedeMoverHacia(estadoOrigenId, estadoDestinoId)) {
            showToast('error', 'No tienes permisos para este cambio de estado');
            return;
        }

        if ([7, 9].includes(estadoOrigenId)) {
            showToast('error', 'Este pedido ya alcanzó un estado final');
            return;
        }

        if (user?.rol === 'Operario' && !pedido.fechaInicioArmado) {
            showToast('error', 'Debes presionar "Comenzar armado" antes de cambiar estados');
            return;
        }

        if (user?.rol === 'Encargado' && estadoOrigenId === 4 && estadoDestinoId === 5) {
            setPedidoAAsignarCadete(pedido);
            setShowAsignarCadeteModal(true);
            return;
        }

        if (estadoDestinoId === 9) {
            setPedidoACancelar(pedido);
            setShowCancelModal(true);
            return;
        }

        setDragBlockedPedidoId(pedidoId);
        setIsLoadingPedidoId(pedidoId);

        try {
            await pedidosService.cambiarEstado({
                idPedido: pedidoId,
                idNuevoEstado: estadoDestinoId,
                idUsuario: usuarioId || user?.id || 0,
                observaciones: ''
            });

            const nuevaListaPedidos = pedidosLocal.map(p => {
                if (p.idPedido === pedidoId) {
                    return { ...p, idEstadoDePedido: estadoDestinoId, estadoNombre: ESTADO_MAP[estadoDestinoId] };
                }
                return p;
            });

            setPedidosLocal(nuevaListaPedidos);
            showToast('success', `Pedido #${pedidoId} movido a ${ESTADO_MAP[estadoDestinoId]}`);

            if (user?.rol === 'Operario' && estadoDestinoId === 4) {
                setTimeout(() => {
                    setPedidosLocal(prev => prev.filter(p => p.idPedido !== pedidoId));
                }, 1000);
            }

            await onUpdate();
        } catch (error: any) {
            setPedidosLocal(pedidos);
            const mensajeError = error.response?.data?.message || 'Error al cambiar estado';
            showToast('error', mensajeError);
        } finally {
            setDragBlockedPedidoId(undefined);
            setIsLoadingPedidoId(undefined);
        }
    };

    return (
        <DragDropContext onDragEnd={handleDragEnd}>
            <div className="w-full">
                {toast.visible && (
                    <div className={`fixed top-4 right-4 max-w-sm p-3 rounded-lg shadow-lg flex items-center gap-2 z-50 text-sm ${
                        toast.type === 'success' ? 'bg-emerald-50 text-emerald-800 border border-emerald-200' : 
                        toast.type === 'error' ? 'bg-red-50 text-red-800 border border-red-200' : 
                        'bg-blue-50 text-blue-800 border border-blue-200'
                    }`}>
                        {toast.type === 'success' && <CheckCircle2 className="w-4 h-4 flex-shrink-0" />}
                        {toast.type === 'error' && <AlertCircle className="w-4 h-4 flex-shrink-0" />}
                        <p className="font-medium">{toast.message}</p>
                    </div>
                )}

                <div className="space-y-4 pb-4">
                    {columnasVisibles.map(estadoId => {
                        const colores = getColoresEstado(estadoId);
                        const pedidosColumna = pedidosPorEstado[estadoId];
                        const isExpanded = expandedColumns[estadoId] !== false;

                        return (
                            <div key={estadoId} className={`rounded-lg border-2 ${colores.border} overflow-hidden`}>
                                <button 
                                    onClick={() => toggleColumn(estadoId)} 
                                    className={`w-full ${colores.header} px-4 py-3 flex items-center justify-between hover:opacity-80 transition-opacity`}>
                                    <div className="text-left">
                                        <h3 className={`font-bold text-sm ${colores.text} uppercase tracking-wider`}>{ESTADO_MAP[estadoId]}</h3>
                                        <p className="text-xs text-gray-500 mt-0.5">{pedidosColumna.length} {pedidosColumna.length !== 1 ? 'pedidos' : 'pedido'}</p>
                                    </div>
                                    <div className={`transition-transform ${isExpanded ? '' : 'rotate-180'}`}>
                                        {isExpanded ? <ChevronUp className="w-5 h-5" /> : <ChevronDown className="w-5 h-5" />}
                                    </div>
                                </button>

                                {isExpanded && (
                                    <Droppable droppableId={`columna-${estadoId}`}>
                                        {(provided, snapshot) => (
                                            <div ref={provided.innerRef} {...provided.droppableProps} className={`${colores.bg} p-3 space-y-3 min-h-40 ${snapshot.isDraggingOver ? 'bg-blue-200 ring-2 ring-blue-400' : ''}`}>
                                                {pedidosColumna.length > 0 ? (
                                                    pedidosColumna.map((pedido, index) => (
                                                        <Draggable key={pedido.idPedido} draggableId={`pedido-${pedido.idPedido}`} index={index} isDragDisabled={dragBlockedPedidoId === pedido.idPedido || [7, 9].includes(pedido.idEstadoDePedido)}>
                                                            {(provided, snapshot) => (
                                                                <div ref={provided.innerRef} {...provided.draggableProps} {...provided.dragHandleProps} className={`bg-white rounded-lg p-3 border border-gray-200 transition-all ${snapshot.isDragging ? 'shadow-2xl scale-105 opacity-90 bg-blue-50' : 'hover:shadow-lg'} ${isLoadingPedidoId === pedido.idPedido ? 'opacity-50' : ''} ${pedido.estaDemorado ? 'border-l-4 border-orange-500' : ''}`}>
                                                                    {isLoadingPedidoId === pedido.idPedido && (
                                                                        <div className="absolute inset-0 flex items-center justify-center bg-white/70 rounded-lg">
                                                                            <Loader className="w-5 h-5 animate-spin text-blue-500" />
                                                                        </div>
                                                                    )}
                                                                    <div className="flex items-start justify-between gap-2 mb-2">
                                                                        <div>
                                                                            <h4 className="font-bold text-base text-gray-900">#{pedido.idPedido}</h4>
                                                                            <p className="text-xs text-gray-500">{new Date(pedido.fecha).toLocaleDateString('es-AR')}</p>
                                                                        </div>
                                                                        <div>
                                                                            {pedido.estaDemorado && (
                                                                                <div className="flex items-center gap-1 text-orange-600">
                                                                                    <AlertTriangle className="w-4 h-4" />
                                                                                    <span className="text-xs">Demorado</span>
                                                                                </div>
                                                                            )}
                                                                        </div>
                                                                    </div>
                                                                    <div className="mb-2">
                                                                        <p className="text-sm font-medium text-gray-800">{pedido.clienteNombre}</p>
                                                                        {pedido.responsableNombre && <p className="text-xs text-gray-600">{pedido.responsableNombre}</p>}
                                                                    </div>
                                                                    <div className="mb-2 pb-2 border-t border-gray-100">
                                                                        <p className="text-sm font-bold text-green-600 mt-2">{formatCurrency(pedido.total)}</p>
                                                                    </div>
                                                                    {pedido.fechaEntregaEstimada && new Date(pedido.fechaEntregaEstimada).getFullYear() > 1900 && (
                                                                        <p className="text-xs text-blue-600 mb-2">📅 {new Date(pedido.fechaEntregaEstimada).toLocaleDateString('es-AR')}</p>
                                                                    )}
                                                                </div>
                                                            )}
                                                        </Draggable>
                                                    ))
                                                ) : (
                                                    <div className="text-center py-6 text-gray-400"><p className="text-sm">Sin pedidos</p></div>
                                                )}
                                                {provided.placeholder}
                                            </div>
                                        )}
                                    </Droppable>
                                )}
                            </div>
                        );
                    })}
                </div>

                {pedidosLocal.length === 0 && (
                    <div className="flex items-center justify-center h-64 text-gray-500">
                        <AlertCircle className="w-12 h-12 mx-auto mb-2 text-gray-300" />
                        <p>Sin pedidos</p>
                    </div>
                )}

                {pedidoACancelar && (
                    <CancelarConMotivoModal
                        isOpen={showCancelModal}
                        pedidoId={pedidoACancelar.idPedido}
                        clienteNombre={pedidoACancelar.clienteNombre}
                        onConfirm={async () => {
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
        </DragDropContext>
    );
};

