import React, { useState, useEffect } from 'react';
import { useParams } from 'react-router-dom';
import { trackingService } from '../service/trackingService';
import { OrderTrackingDTO, TrackingHistoryItemDTO } from '../types/tracking.types';
import { AlertCircle, Check, Clock, Package, Truck, CheckCircle, XCircle, Loader } from 'lucide-react';

interface OrderDetailsState extends OrderTrackingDTO {
    productosCount?: number;
    clienteNombre?: string;
    clienteEmail?: string;
}

export const TrackingPage: React.FC = () => {
    const { id } = useParams<{ id: string }>();
    const [tracking, setTracking] = useState<OrderDetailsState | null>(null);
    const [loading, setLoading] = useState(true);
    const [error, setError] = useState<string | null>(null);

    // Mapeo de estados con colores
    const getEstadoColor = (nombreEstado: string): { bg: string; text: string; border: string; icon: React.ReactNode } => {
        const estado = nombreEstado.toLowerCase().trim();

        if (estado.includes('sin preparar')) {
            return {
                bg: 'bg-gray-100',
                text: 'text-gray-400',
                border: 'border-gray-200',
                icon: <Clock className="w-5 h-5" />
            };
        }
        if (estado.includes('preparando') || estado.includes('en preparación') || estado.includes('preparar pedido')) {
            return {
                bg: 'bg-blue-100',
                text: 'text-blue-600',
                border: 'border-blue-200',
                icon: <Package className="w-5 h-5" />
            };
        }
        if (estado.includes('demorado')) {
            return {
                bg: 'bg-orange-100',
                text: 'text-orange-600',
                border: 'border-orange-200',
                icon: <AlertCircle className="w-5 h-5" />
            };
        }
        if (estado.includes('listo para despachar')) {
            return {
                bg: 'bg-green-100',
                text: 'text-green-600',
                border: 'border-green-200',
                icon: <Check className="w-5 h-5" />
            };
        }
        if (estado.includes('despachando') || estado.includes('despacho')) {
            return {
                bg: 'bg-indigo-100',
                text: 'text-indigo-600',
                border: 'border-indigo-200',
                icon: <Truck className="w-5 h-5" />
            };
        }
        if (estado.includes('en camino')) {
            return {
                bg: 'bg-indigo-100',
                text: 'text-indigo-600',
                border: 'border-indigo-200',
                icon: <Truck className="w-5 h-5" />
            };
        }
        if (estado.includes('entregado')) {
            return {
                bg: 'bg-emerald-100',
                text: 'text-emerald-700',
                border: 'border-emerald-200',
                icon: <CheckCircle className="w-5 h-5" />
            };
        }
        if (estado.includes('cancelado') || estado.includes('entrega fallida')) {
            return {
                bg: 'bg-red-100',
                text: 'text-red-600',
                border: 'border-red-200',
                icon: <XCircle className="w-5 h-5" />
            };
        }

        return {
            bg: 'bg-gray-50',
            text: 'text-gray-500',
            border: 'border-gray-100',
            icon: <Clock className="w-5 h-5" />
        };
    };

    // Definir el orden visual de los estados
    const estadosOrdenados = [
        'Sin preparar',
        'Preparando',
        'Demorado',
        'Listo para despachar',
        'Despachado',
        'En camino',
        'Entregado',
        'Entrega fallida',
        'Cancelado',
    ];

    // Obtener datos del pedido
    useEffect(() => {
        const fetchTracking = async () => {
            try {
                setLoading(true);
                setError(null);

                if (!id || isNaN(Number(id))) {
                    setError('ID de pedido inválido');
                    return;
                }

                const data = await trackingService.getSeguimiento(Number(id));
                setTracking(data as OrderDetailsState);
            } catch (err) {
                console.error('Error al obtener tracking:', err);
                setError('No se pudo obtener la información del pedido. Por favor, verifica que el ID sea correcto.');
            } finally {
                setLoading(false);
            }
        };

        fetchTracking();
    }, [id]);

    if (loading) {
        return (
            <div className="min-h-screen bg-gradient-to-br from-blue-50 to-indigo-100 flex items-center justify-center">
                <div className="text-center">
                    <Loader className="w-12 h-12 text-indigo-600 animate-spin mx-auto mb-4" />
                    <p className="text-lg text-gray-700">Cargando información del pedido...</p>
                </div>
            </div>
        );
    }

    if (error || !tracking) {
        return (
            <div className="min-h-screen bg-gradient-to-br from-blue-50 to-indigo-100 flex items-center justify-center p-4">
                <div className="bg-white rounded-lg shadow-lg p-8 max-w-md w-full">
                    <XCircle className="w-12 h-12 text-red-500 mx-auto mb-4" />
                    <h1 className="text-2xl font-bold text-center text-gray-800 mb-2">Error</h1>
                    <p className="text-center text-gray-600">
                        {error || 'No se encontró el pedido solicitado.'}
                    </p>
                    <div className="mt-6 text-center">
                        <p className="text-sm text-gray-500">
                            Si crees que esto es un error, por favor contacta a nuestro equipo de soporte.
                        </p>
                    </div>
                </div>
            </div>
        );
    }

    const estadoActualColor = getEstadoColor(tracking.estadoActual);

    // Filtrar estados duplicados consecutivos, EXCEPTO para "entrega fallida"
    // Esto permite mostrar cada intento fallido como un evento separado
    const historialFiltrado = tracking.historial.reduce((acc: TrackingHistoryItemDTO[], item: TrackingHistoryItemDTO, index: number) => {
        const esEntregaFallida = item.nombreEstado.toLowerCase().includes('entrega fallida');
        const prevEsEntregaFallida = index > 0 && tracking.historial[index - 1].nombreEstado.toLowerCase().includes('entrega fallida');
        
        // Permitir duplicados de "entrega fallida" (cada intento es un evento)
        // Filtrar duplicados para otros estados
        if (index === 0 || esEntregaFallida || item.nombreEstado.toLowerCase() !== tracking.historial[index - 1].nombreEstado.toLowerCase()) {
            acc.push(item);
        }
        return acc;
    }, []);

    // NO expandir - cada item en historial ya es un evento separado
    // Solo añadir identificador de intento para las tarjetas de "entrega fallida"
    const historialExpandido = historialFiltrado.map((item: TrackingHistoryItemDTO, index: number) => {
        const esEntregaFallida = item.nombreEstado.toLowerCase().includes('entrega fallida');
        
        if (esEntregaFallida) {
            // Contar cuántos "entrega fallida" hay antes de este (para mostrar Intento 1, 2, 3)
            const numeroIntento = historialFiltrado.filter((h, i) => 
                i <= index && h.nombreEstado.toLowerCase().includes('entrega fallida')
            ).length;
            
            return {
                ...item,
                intentoNumero: numeroIntento,
                isTarjetaDuplicada: true
            };
        }
        
        return { ...item, isTarjetaDuplicada: false };
    });

    return (
        <div className="min-h-screen bg-gradient-to-br from-blue-50 to-indigo-100 py-8 px-4">
            <div className="max-w-3xl mx-auto">
                {/* Header */}
                <div className="bg-white rounded-lg shadow-lg p-6 mb-6">
                    <div className="flex items-center justify-between mb-4">
                        <div>
                            <h1 className="text-3xl font-bold text-gray-800">
                                Pedido #{tracking.idPedido.toString().padStart(6, '0')}
                            </h1>
                            <p className="text-gray-600 mt-1">
                                Fecha de actualización:{' '}
                                <span className="font-semibold">
                                    {new Date(tracking.ultimaActualizacion).toLocaleDateString('es-ES', {
                                        year: 'numeric',
                                        month: 'long',
                                        day: 'numeric',
                                        hour: '2-digit',
                                        minute: '2-digit',
                                    })}
                                </span>
                            </p>
                        </div>
                        <div className={`text-center p-4 rounded-lg ${estadoActualColor.bg} border-2 ${estadoActualColor.border}`}>
                            <div className={`flex items-center justify-center gap-2 mb-2 ${estadoActualColor.text}`}>
                                {estadoActualColor.icon}
                            </div>
                            <p className={`font-bold ${estadoActualColor.text}`}>
                                {tracking.estadoActual}
                            </p>
                        </div>
                    </div>
                </div>

                {/* Timeline */}
                <div className="bg-white rounded-lg shadow-lg overflow-hidden">
                    <div className="px-6 py-4 bg-gradient-to-r from-indigo-600 to-blue-600 text-white">
                        <h2 className="text-xl font-bold">Historial del Pedido</h2>
                    </div>

                    <div className="p-6">
                        {historialExpandido && historialExpandido.length > 0 ? (
                            <div className="space-y-6">
                                {historialExpandido.map((item: any, index: number) => {
                                    const color = getEstadoColor(item.nombreEstado);
                                    const isLast = index === historialExpandido.length - 1;

                                    return (
                                        <div key={`${index}-${item.isTarjetaDuplicada ? item.intentoNumero : 'original'}`} className="flex gap-4">
                                            {/* Timeline connector */}
                                            <div className="flex flex-col items-center">
                                                <div className={`w-10 h-10 rounded-full flex items-center justify-center ${color.bg} border-2 ${color.border}`}>
                                                    {color.icon}
                                                </div>
                                                {!isLast && (
                                                    <div className={`w-1 h-16 mt-2 ${color.bg}`}></div>
                                                )}
                                            </div>

                                            {/* Content */}
                                            <div className="flex-1 pb-4">
                                                <div className={`p-4 rounded-lg ${color.bg} border ${color.border}`}>
                                                    <div className="flex items-center justify-between">
                                                        <h3 className={`font-bold text-lg ${color.text}`}>
                                                            {item.nombreEstado}
                                                        </h3>
                                                    </div>
                                                    <p className="text-gray-600 text-sm mt-1">
                                                        {new Date(item.fechaHora).toLocaleDateString('es-ES', {
                                                            year: 'numeric',
                                                            month: 'long',
                                                            day: 'numeric',
                                                            hour: '2-digit',
                                                            minute: '2-digit',
                                                        })}
                                                    </p>

                                                    {item.responsable && (
                                                        <div className="mt-2 pt-2 border-t border-gray-300">
                                                            <p className="text-xs text-gray-600">
                                                                <span className="font-semibold">Responsable:</span> {item.responsable}
                                                            </p>
                                                        </div>
                                                    )}

                                                    {item.motivoCancelacion && (
                                                        <div className="mt-2 pt-2 border-t border-red-300">
                                                            <p className="text-xs text-red-700">
                                                                <span className="font-semibold">Motivo:</span> {item.motivoCancelacion}
                                                            </p>
                                                        </div>
                                                    )}

                                                    {item.observaciones && (
                                                        <div className="mt-2 pt-2 border-t border-gray-300">
                                                            <p className="text-xs text-gray-700">
                                                                <span className="font-semibold">Observaciones:</span> {item.observaciones}
                                                            </p>
                                                        </div>
                                                    )}
                                                </div>
                                            </div>
                                        </div>
                                    );
                                })}
                            </div>
                        ) : (
                            <div className="text-center py-8 text-gray-500">
                                <Package className="w-12 h-12 mx-auto mb-4 text-gray-300" />
                                <p>No hay actualizaciones disponibles aún.</p>
                            </div>
                        )}
                    </div>
                </div>

                {/* Footer */}
                <div className="text-center mt-8 text-gray-600 text-sm">
                    <p>Farmacia General Paz © 2026 - Síguenos en línea</p>
                </div>
            </div>
        </div>
    );
};

export default TrackingPage;
