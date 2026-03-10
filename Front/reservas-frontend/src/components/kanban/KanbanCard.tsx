import React from 'react';
import { Draggable } from '@hello-pangea/dnd';
import { OrderSummaryDTO } from '../../types/pedido.types';
import { AlertTriangle, Loader } from 'lucide-react';

interface KanbanCardProps {
    pedido: OrderSummaryDTO;
    index: number;
    isDragging: boolean;
    isLoadingPedido: boolean;
    mostrarAlertaDemora: boolean;
}

export const KanbanCard: React.FC<KanbanCardProps> = ({
    pedido,
    index,
    isDragging,
    isLoadingPedido,
    mostrarAlertaDemora
}) => {
    // Formatear total como moneda
    const formatCurrency = (value: number) => {
        return new Intl.NumberFormat('es-AR', {
            style: 'currency',
            currency: 'ARS'
        }).format(value);
    };

    return (
        <Draggable draggableId={`pedido-${pedido.idPedido}`} index={index}>
            {(provided, snapshot) => (
                <div
                    ref={provided.innerRef}
                    {...provided.draggableProps}
                    {...provided.dragHandleProps}
                    className={`
                        bg-white rounded-lg shadow-md p-4 mb-3 
                        transition-all cursor-move relative
                        ${isDragging || snapshot.isDragging ? 'shadow-2xl scale-105 opacity-90 bg-blue-50' : 'hover:shadow-lg'}
                        ${isLoadingPedido ? 'opacity-50' : ''}
                        ${mostrarAlertaDemora ? 'border-l-4 border-orange-500' : ''}
                    `}
                    style={provided.draggableProps.style}
                >
                    {/* Indicador de carga */}
                    {isLoadingPedido && (
                        <div className="absolute inset-0 flex items-center justify-center bg-white/70 rounded-lg">
                            <Loader className="w-5 h-5 animate-spin text-blue-500" />
                        </div>
                    )}

                    {/* Contenido de la tarjeta */}
                    <div className="space-y-2">
                        {/* ID Pedido y Alerta de Demora */}
                        <div className="flex items-start justify-between gap-2">
                            <div>
                                <h3 className="font-bold text-sm text-gray-900">
                                    #{pedido.idPedido}
                                </h3>
                                <p className="text-xs text-gray-500">
                                    {new Date(pedido.fecha).toLocaleDateString('es-AR')}
                                </p>
                            </div>
                            {mostrarAlertaDemora && (
                                <AlertTriangle className="w-5 h-5 text-orange-500 flex-shrink-0" />
                            )}
                        </div>

                        {/* Cliente */}
                        <div>
                            <p className="text-sm font-medium text-gray-700 truncate">
                                {pedido.clienteNombre}
                            </p>
                            {pedido.responsableNombre && (
                                <p className="text-xs text-gray-500">
                                    Responsable: {pedido.responsableNombre}
                                </p>
                            )}
                        </div>

                        {/* Total */}
                        <div className="pt-2 border-t border-gray-200">
                            <p className="text-sm font-bold text-green-600">
                                {formatCurrency(pedido.total)}
                            </p>
                        </div>

                        {/* Info de entrega estimada */}
                        {pedido.fechaEntregaEstimada && (
                            <p className="text-xs text-blue-600">
                                Entrega: {new Date(pedido.fechaEntregaEstimada).toLocaleDateString('es-AR')}
                            </p>
                        )}

                        {/* Intentos fallidos */}
                        {pedido.intentosEntregaFallida > 0 && (
                            <div className="bg-red-50 rounded p-1 text-xs text-red-600">
                                ⚠️ {pedido.intentosEntregaFallida} intento(s) fallido(s)
                            </div>
                        )}
                    </div>
                </div>
            )}
        </Draggable>
    );
};
