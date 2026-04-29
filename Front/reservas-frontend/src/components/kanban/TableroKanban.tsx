import React, { useState, useEffect } from 'react';
import { DragDropContext, DropResult } from '@hello-pangea/dnd';
import { OrderSummaryDTO } from '../../types/pedido.types';
import { useAuth } from '../../context/AuthContext';
import { pedidosService } from '../../service/PedidosService';
import { KanbanColumn } from './KanbanColumn';
import { KanbanMobileView } from './KanbanMobileView';
import { CancelarConMotivoModal } from './CancelarConMotivoModal';
import { AsignarCadeteModal } from '../pedidos/AsignarCadeteModal';
import { ConfirmarEntregaModal } from '../pedidos/ConfirmarEntregaModal'; // ← NUEVO
import {
    COLUMNAS_POR_ROL,
    VALIDACIONES_OPERARIO,
    VALIDACIONES_CADETE,
    VALIDACIONES_ENCARGADO,
    ValidationRuleSet,
    ESTADO_MAP
} from '../../types/kanban.types';
import { AlertCircle, CheckCircle2 } from 'lucide-react';

interface TableroKanbanProps {
    pedidos: OrderSummaryDTO[];
    onUpdate: () => Promise<void>;
    usuarioId?: number;
}

interface ToastState {
    visible: boolean;
    type: 'success' | 'error' | 'info';
    message: string;
}

export const TableroKanban: React.FC<TableroKanbanProps> = ({
    pedidos,
    onUpdate,
    usuarioId
}) => {
    const { user } = useAuth();
    const [toast, setToast] = useState<ToastState>({ visible: false, type: 'info', message: '' });
    const [isLoadingPedidoId, setIsLoadingPedidoId] = useState<number | undefined>();
    const [dragBlockedPedidoId, setDragBlockedPedidoId] = useState<number | undefined>();
    const [pedidosLocal, setPedidosLocal] = useState<OrderSummaryDTO[]>(pedidos);
    const [isMobile, setIsMobile] = useState(window.innerWidth < 1024);
    
    // Estado para modal de cancelación
    const [showCancelModal, setShowCancelModal] = useState(false);
    const [pedidoACancelar, setPedidoACancelar] = useState<OrderSummaryDTO | null>(null);
    const [operacionCancelacionPendiente, setOperacionCancelacionPendiente] = useState(false);

    // Estado para modal de asignar cadete (cuando encargado mueve de 4→5)
    const [showAsignarCadeteModal, setShowAsignarCadeteModal] = useState(false);
    const [pedidoAAsignarCadete, setPedidoAAsignarCadete] = useState<OrderSummaryDTO | null>(null);

    // ← NUEVO: Estado para modal de confirmar entrega (cuando cadete arrastra a Entrega Fallida)
    const [showConfirmarEntregaModal, setShowConfirmarEntregaModal] = useState(false);
    const [pedidoAConfirmarEntrega, setPedidoAConfirmarEntrega] = useState<OrderSummaryDTO | null>(null);

    // Detectar cambios de tamaño de pantalla
    useEffect(() => {
        const handleResize = () => {
            setIsMobile(window.innerWidth < 1024);
        };

        window.addEventListener('resize', handleResize);
        return () => window.removeEventListener('resize', handleResize);
    }, []);

    // Sincronizar pedidos locales cuando cambian los props
    useEffect(() => {
        setPedidosLocal(pedidos);
    }, [pedidos]);

    // Mostrar toast
    const showToast = (type: 'success' | 'error' | 'info', message: string) => {
        setToast({ visible: true, type, message });
        setTimeout(() => setToast({ ...toast, visible: false }), 4000);
    };

    // Manejar confirmación de cancelación con motivo
    const handleConfirmCancelacion = async (motivo: string) => {
        if (!pedidoACancelar) return;

        const pedidoId = pedidoACancelar.idPedido;
        setDragBlockedPedidoId(pedidoId);
        setIsLoadingPedidoId(pedidoId);

        try {
            const nuevaListaPedidos = pedidosLocal.map(p => {
                if (p.idPedido === pedidoId) {
                    return {
                        ...p,
                        idEstadoDePedido: 9,
                        estadoNombre: ESTADO_MAP[9]
                    };
                }
                return p;
            });

            setPedidosLocal(nuevaListaPedidos);
            showToast('success', `Pedido #${pedidoId} cancelado correctamente`);
            await onUpdate();
            setShowCancelModal(false);
            setPedidoACancelar(null);
            setOperacionCancelacionPendiente(false);

        } catch (error: any) {
            console.error('Error al procesar cancelación:', error);
            const mensajeError = error.response?.data?.message || 
                                 error.response?.data?.title ||
                                 'Error al procesar la cancelación';
            showToast('error', mensajeError);
        } finally {
            setDragBlockedPedidoId(undefined);
            setIsLoadingPedidoId(undefined);
        }
    };

    // Obtener validador según rol
    const getValidador = (): ValidationRuleSet => {
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

    // Obtener columnas visibles según rol
    const getColumnasVisibles = (): number[] => {
        if (!user) return COLUMNAS_POR_ROL['Encargado'];
        return COLUMNAS_POR_ROL[user.rol] || COLUMNAS_POR_ROL['Encargado'];
    };

    // Agrupar pedidos por estado
    const pedidosPorEstado: Record<number, OrderSummaryDTO[]> = {};
    getColumnasVisibles().forEach(estadoId => {
        pedidosPorEstado[estadoId] = pedidosLocal.filter(p => p.idEstadoDePedido === estadoId);
    });

    // Lógica principal de drag and drop
    const handleDragEnd = async (result: DropResult) => {
        const { source, destination, draggableId } = result;

        if (!destination) {
            showToast('info', 'Operación cancelada');
            return;
        }

        if (
            source.droppableId === destination.droppableId &&
            source.index === destination.index
        ) {
            return;
        }

        const estadoOrigenId = parseInt(source.droppableId.split('-')[1]);
        const estadoDestinoId = parseInt(destination.droppableId.split('-')[1]);
        const pedidoId = parseInt(draggableId.split('-')[1]);

        const pedido = pedidosLocal.find(p => p.idPedido === pedidoId);
        if (!pedido) return;

        // ========== VALIDACIONES ==========
        const validador = getValidador();
        const columnasVisibles = getColumnasVisibles();

        if (!columnasVisibles.includes(estadoDestinoId)) {
            showToast('error', 'No tienes permisos para cambiar a este estado');
            return;
        }

        if (!validador.puedeMoverHacia(estadoOrigenId, estadoDestinoId)) {
            showToast('error', 'No tienes permisos para este cambio de estado');
            return;
        }

        if ([7, 9].includes(estadoOrigenId)) {
            showToast('error', 'Este pedido ya alcanzó un estado final y no puede modificarse');
            return;
        }

        if (user?.rol === 'Operario' && !pedido.fechaInicioArmado) {
            showToast('error', `⚠️ Debes presionar "Comenzar armado" antes de cambiar estados. Vuelve a la vista Tabla para iniciarlo.`);
            return;
        }

        // ========== CASO ESPECIAL: ASIGNAR CADETE AL PASAR A DESPACHANDO ==========
        if (user?.rol === 'Encargado' && estadoOrigenId === 4 && estadoDestinoId === 5) {
            setPedidoAAsignarCadete(pedido);
            setShowAsignarCadeteModal(true);
            return;
        }

        // ← NUEVO: ========== CASO ESPECIAL: ENTREGA FALLIDA ==========
        // Si el cadete arrastra al estado 8 (Entrega Fallida), mostrar modal de confirmación
        if (estadoDestinoId === 8) {
            setPedidoAConfirmarEntrega(pedido);
            setShowConfirmarEntregaModal(true);
            return;
        }

        // ========== CASO ESPECIAL: CANCELACIÓN CON MOTIVO ==========
        if (estadoDestinoId === 9) {
            setPedidoACancelar(pedido);
            setShowCancelModal(true);
            setOperacionCancelacionPendiente(true);
            return;
        }

        // ========== Bloquear drag durante la petición ==========
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
                    return {
                        ...p,
                        idEstadoDePedido: estadoDestinoId,
                        estadoNombre: ESTADO_MAP[estadoDestinoId]
                    };
                }
                return p;
            });

            setPedidosLocal(nuevaListaPedidos);
            showToast('success', `Pedido #${pedidoId} movido a ${ESTADO_MAP[estadoDestinoId]}`);

            if (user?.rol === 'Operario' && estadoDestinoId === 4) {
                setTimeout(() => {
                    setPedidosLocal(prev => prev.filter(p => p.idPedido !== pedidoId));
                    showToast('success', `Tu tarea de armado está completa. Pedido #${pedidoId} listo para despachar.`);
                }, 1000);
            }

            await onUpdate();

        } catch (error: any) {
            console.error('Error al cambiar estado:', error);
            setPedidosLocal(pedidos);
            const mensajeError = error.response?.data?.message || 
                                 error.response?.data?.title ||
                                 'Error al cambiar estado del pedido';
            showToast('error', mensajeError);
        } finally {
            setDragBlockedPedidoId(undefined);
            setIsLoadingPedidoId(undefined);
        }
    };

    // ========== RENDER ==========
    if (isMobile) {
        return (
            <KanbanMobileView
                pedidos={pedidos}
                onUpdate={onUpdate}
                usuarioId={usuarioId}
            />
        );
    }

    return (
        <div className="w-full">
            {/* Toast de notificaciones */}
            {toast.visible && (
                <div
                    className={`
                        fixed top-4 right-4 max-w-md p-4 rounded-lg shadow-lg 
                        flex items-center gap-3 animate-in fade-in slide-in-from-top-[10px]
                        z-50
                        ${toast.type === 'success' ? 'bg-emerald-50 text-emerald-800 border border-emerald-200' : ''}
                        ${toast.type === 'error' ? 'bg-red-50 text-red-800 border border-red-200' : ''}
                        ${toast.type === 'info' ? 'bg-blue-50 text-blue-800 border border-blue-200' : ''}
                    `}
                >
                    {toast.type === 'success' && <CheckCircle2 className="w-5 h-5 flex-shrink-0" />}
                    {toast.type === 'error' && <AlertCircle className="w-5 h-5 flex-shrink-0" />}
                    <p className="text-sm font-medium">{toast.message}</p>
                </div>
            )}

            {/* Contenedor del Kanban */}
            <DragDropContext onDragEnd={handleDragEnd}>
                <div className="w-full overflow-x-auto" style={{ scrollBehavior: 'smooth' }}>
                    <div className="inline-flex gap-1 sm:gap-2 md:gap-4 p-1 sm:p-2 md:p-4 min-w-min">
                        {getColumnasVisibles().map(estadoId => (
                            <KanbanColumn
                                key={estadoId}
                                estadoId={estadoId}
                                pedidos={pedidosPorEstado[estadoId]}
                                dragBlockedPedidoId={dragBlockedPedidoId}
                                isLoadingPedidoId={isLoadingPedidoId}
                            />
                        ))}
                    </div>
                </div>
            </DragDropContext>

            {/* Mensaje si no hay pedidos */}
            {pedidosLocal.length === 0 && (
                <div className="flex items-center justify-center h-64 text-gray-500">
                    <div className="text-center">
                        <AlertCircle className="w-12 h-12 mx-auto mb-2 text-gray-300" />
                        <p>No hay pedidos para mostrar</p>
                    </div>
                </div>
            )}

            {/* Modal de Cancelación con Motivo */}
            {pedidoACancelar && (
                <CancelarConMotivoModal
                    isOpen={showCancelModal}
                    pedidoId={pedidoACancelar.idPedido}
                    clienteNombre={pedidoACancelar.clienteNombre}
                    onConfirm={handleConfirmCancelacion}
                    onCancel={() => {
                        setShowCancelModal(false);
                        setPedidoACancelar(null);
                        setOperacionCancelacionPendiente(false);
                    }}
                />
            )}

            {/* Modal de Asignar Cadete (cuando encargado mueve de 4→5) */}
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

            {/* ← NUEVO: Modal de Confirmar Entrega (cuando cadete arrastra a Entrega Fallida) */}
            {pedidoAConfirmarEntrega && (
                <ConfirmarEntregaModal
                    isOpen={showConfirmarEntregaModal}
                    pedido={pedidoAConfirmarEntrega}
                    onClose={() => {
                        setShowConfirmarEntregaModal(false);
                        setPedidoAConfirmarEntrega(null);
                    }}
                    onSuccess={() => {
                        setShowConfirmarEntregaModal(false);
                        setPedidoAConfirmarEntrega(null);
                        onUpdate();
                        showToast('success', `Pedido #${pedidoAConfirmarEntrega.idPedido} actualizado correctamente`);
                    }}
                />
            )}
        </div>
    );
};

