import React from 'react';
import { Droppable } from '@hello-pangea/dnd';
import { OrderSummaryDTO } from '../../types/pedido.types';
import { KanbanCard } from './KanbanCard';
import { ESTADO_MAP } from '../../types/kanban.types';

interface KanbanColumnProps {
    estadoId: number;
    pedidos: OrderSummaryDTO[];
    dragBlockedPedidoId?: number;
    isLoadingPedidoId?: number;
}

export const KanbanColumn: React.FC<KanbanColumnProps> = ({
    estadoId,
    pedidos,
    dragBlockedPedidoId,
    isLoadingPedidoId,
}) => {
    const estadoNombre = ESTADO_MAP[estadoId] || 'Desconocido';

    // Colores por estado
    const getEstadoStyle = (estado: number) => {
        const styleMap: Record<number, { bg: string; text: string; border: string }> = {
            1: { bg: 'bg-gray-50', text: 'text-gray-600', border: 'border-gray-200' },
            2: { bg: 'bg-blue-50', text: 'text-blue-600', border: 'border-blue-200' },
            3: { bg: 'bg-orange-50', text: 'text-orange-600', border: 'border-orange-200' },
            4: { bg: 'bg-green-50', text: 'text-green-600', border: 'border-green-200' },
            5: { bg: 'bg-indigo-50', text: 'text-indigo-600', border: 'border-indigo-200' },
            6: { bg: 'bg-purple-50', text: 'text-purple-600', border: 'border-purple-200' },
            7: { bg: 'bg-emerald-50', text: 'text-emerald-600', border: 'border-emerald-200' },
            8: { bg: 'bg-red-50', text: 'text-red-600', border: 'border-red-200' },
            9: { bg: 'bg-slate-50', text: 'text-slate-600', border: 'border-slate-200' },
        };
        return styleMap[estado] || styleMap[1];
    };

    const estilo = getEstadoStyle(estadoId);

    return (
        <div className={`flex-none w-72 sm:w-80 md:w-96 rounded-lg ${estilo.bg} border-2 ${estilo.border} p-3 sm:p-4 flex flex-col`}>
            {/* Encabezado de columna */}
            <div className="mb-3 sm:mb-4 pb-2 sm:pb-3 border-b-2 border-gray-300">
                <h2 className={`text-xs sm:text-sm font-bold ${estilo.text} uppercase tracking-wider`}>
                    {estadoNombre}
                </h2>
                <p className="text-[11px] sm:text-xs text-gray-500 mt-1">
                    {pedidos.length} pedido{pedidos.length !== 1 ? 's' : ''}
                </p>
            </div>

            {/* Área droppable para tarjetas */}
            <Droppable droppableId={`columna-${estadoId}`}>
                {(provided, snapshot) => (
                    <div
                        ref={provided.innerRef}
                        {...provided.droppableProps}
                        className={`
                            flex-1 min-h-80 sm:min-h-96 rounded-lg p-2 transition-all
                            ${snapshot.isDraggingOver ? 'bg-blue-200 ring-2 ring-blue-400' : 'bg-white'}
                        `}
                    >
                        {/* Tarjetas de pedidos */}
                        {pedidos.length > 0 ? (
                            pedidos.map((pedido, index) => (
                                <KanbanCard
                                    key={pedido.idPedido}
                                    pedido={pedido}
                                    index={index}
                                    isDragging={dragBlockedPedidoId === pedido.idPedido}
                                    isLoadingPedido={isLoadingPedidoId === pedido.idPedido}
                                    mostrarAlertaDemora={pedido.estaDemorado}
                                />
                            ))
                        ) : (
                            <div className="h-full flex items-center justify-center text-gray-400 text-xs sm:text-sm">
                                Sin pedidos
                            </div>
                        )}

                        {/* Placeholder para drag and drop */}
                        {provided.placeholder}
                    </div>
                )}
            </Droppable>
        </div>
    );
};
