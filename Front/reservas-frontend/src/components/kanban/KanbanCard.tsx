import React from 'react';
import { Draggable } from '@hello-pangea/dnd';
import { OrderSummaryDTO } from '../../types/pedido.types';
import { useAuth } from '../../context/AuthContext';
import { AlertTriangle, Loader, Clock } from 'lucide-react';

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
    const { user } = useAuth();
    
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
                        bg-white rounded-lg shadow-md p-2 sm:p-3 md:p-4 mb-2 sm:mb-3
                        transition-all cursor-move relative
                        ${isDragging || snapshot.isDragging ? 'shadow-2xl scale-105 opacity-90 bg-blue-50' : 'hover:shadow-lg'}
                        ${isLoadingPedido ? 'opacity-50' : ''}
                        ${mostrarAlertaDemora ? 'border-l-4 border-orange-500' : ''}
                        ${user?.rol === 'Operario' && !pedido.fechaInicioArmado ? 'border-l-4 border-amber-500' : ''}
                        ${pedido.idEstadoDePedido === 8 && pedido.intentosEntregaFallida >= 3 ? 'border-l-4 border-red-600' : ''}
                        ${pedido.idEstadoDePedido === 5 && pedido.intentosEntregaFallida > 0 ? 'border-l-4 border-yellow-500' : ''}
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
                    <div className="space-y-1 sm:space-y-2">
                        {/* ID Pedido y Alerta de Demora */}
                        <div className="flex items-start justify-between gap-2">
                            <div>
                                <h3 className="font-bold text-xs sm:text-sm text-gray-900">
                                    #{pedido.idPedido}
                                </h3>
                                <p className="text-[10px] sm:text-xs text-gray-500">
                                    {new Date(pedido.fecha).toLocaleDateString('es-AR')}
                                </p>
                            </div>
                            <div className="flex items-center gap-1">
                                {user?.rol === 'Operario' && !pedido.fechaInicioArmado && (
                                    <div title="Armado no iniciado">
                                        <Clock className="w-4 h-4 sm:w-5 sm:h-5 text-amber-500 flex-shrink-0" />
                                    </div>
                                )}
                                {mostrarAlertaDemora && (
                                    <AlertTriangle className="w-4 h-4 sm:w-5 sm:h-5 text-orange-500 flex-shrink-0" />
                                )}
                            </div>
                        </div>

                        {/* Indicador de estado del armado - Solo para Operarios */}
                        {user?.rol === 'Operario' && !pedido.fechaInicioArmado && (
                            <div className="bg-amber-50 rounded p-1 sm:p-2 text-[10px] sm:text-xs text-amber-700 border border-amber-200">
                                ⏱ Armado no iniciado
                            </div>
                        )}
                        {user?.rol === 'Operario' && pedido.fechaInicioArmado && !pedido.fechaFinArmado && (
                            <div className="bg-blue-50 rounded p-1 sm:p-2 text-[10px] sm:text-xs text-blue-700 border border-blue-200">
                                ✓ En progreso
                            </div>
                        )}
                        {user?.rol === 'Operario' && pedido.fechaFinArmado && (
                            <div className="bg-green-50 rounded p-1 sm:p-2 text-[10px] sm:text-xs text-green-700 border border-green-200">
                                ✓ Completado
                            </div>
                        )}

                        {/* Cliente */}
                        <div>
                            <p className="text-xs sm:text-sm font-medium text-gray-700 truncate">
                                {pedido.clienteNombre}
                            </p>
                            {pedido.responsableNombre && (
                                <p className="text-[10px] sm:text-xs text-gray-500 truncate">
                                    {pedido.responsableNombre}
                                </p>
                            )}
                        </div>

                        {/* Total */}
                        <div className="pt-1 sm:pt-2 border-t border-gray-200">
                            <p className="text-xs sm:text-sm font-bold text-green-600">
                                {formatCurrency(pedido.total)}
                            </p>
                        </div>

                        {/* Info de entrega estimada */}
                        {pedido.fechaEntregaEstimada && 
                         (typeof pedido.fechaEntregaEstimada === 'string' ? !pedido.fechaEntregaEstimada.startsWith('0001') : true) &&
                         new Date(pedido.fechaEntregaEstimada).getFullYear() > 1900 && (
                            <p className="text-[10px] sm:text-xs text-blue-600">
                                Entrega: {new Date(pedido.fechaEntregaEstimada).toLocaleDateString('es-AR')}
                            </p>
                        )}


                        {/* Indicador de intentos previos fallidos (cuando está en Despachando pero ya tuvo fallos) */}
                        {pedido.idEstadoDePedido === 5 && pedido.intentosEntregaFallida > 0 && (
                            <div className={`rounded p-1 sm:p-2 text-[10px] sm:text-xs border ${
                                pedido.intentosEntregaFallida >= 2 
                                    ? 'bg-yellow-50 text-yellow-700 border-yellow-200' 
                                    : 'bg-blue-50 text-blue-700 border-blue-200'
                            }`}>
                                {pedido.intentosEntregaFallida >= 2 ? (
                                    <>⚠️ Última oportunidad</>
                                ) : (
                                    <>↩️ Reintentando... ({pedido.intentosEntregaFallida})</>
                                )}
                            </div>
                        )}

                        {/* Indicador de intentos fallidos (solo cuando está realmente en estado 8) */}
                        {pedido.idEstadoDePedido === 8 && pedido.intentosEntregaFallida > 0 && (
                            <div className={`rounded p-2 text-xs border ${
                                pedido.intentosEntregaFallida >= 3 
                                    ? 'bg-red-50 text-red-700 border-red-300' 
                                    : 'bg-yellow-50 text-yellow-700 border-yellow-200'
                            }`}>
                                {pedido.intentosEntregaFallida >= 3 ? (
                                    <>🚫 LÍMITE: {pedido.intentosEntregaFallida}/3 intentos. Se cancelará automáticamente</>
                                ) : (
                                    <>⚠️ {pedido.intentosEntregaFallida}/{3} intento(s) fallido(s) - {3 - pedido.intentosEntregaFallida} reintentos disponibles</>
                                )}
                            </div>
                        )}
                    </div>
                </div>
            )}
        </Draggable>
    );
};
