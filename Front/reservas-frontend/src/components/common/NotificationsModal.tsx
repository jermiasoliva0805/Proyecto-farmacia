import React, { useState } from 'react';
import { X, AlertTriangle, Clock, User, MapPin } from 'lucide-react';
import type { OrderSummaryDTO } from '@models/pedido.types';

interface NotificationsModalProps {
  isOpen: boolean;
  onClose: () => void;
  notifications: OrderSummaryDTO[];
  loading?: boolean;
}

export const NotificationsModal: React.FC<NotificationsModalProps> = ({
  isOpen,
  onClose,
  notifications,
  loading = false
}) => {
  const [expandedId, setExpandedId] = useState<number | null>(null);

  if (!isOpen) return null;

  // Función para calcular cuánto tiempo está demorado
  const getDelayText = (fechaEstimada: string | Date): string => {
    const ahora = new Date();
    const estimada = new Date(fechaEstimada);
    const diffMs = ahora.getTime() - estimada.getTime();
    const diffDias = Math.floor(diffMs / (1000 * 60 * 60 * 24));
    const diffHoras = Math.floor((diffMs % (1000 * 60 * 60 * 24)) / (1000 * 60 * 60));

    if (diffDias > 0) {
      return `${diffDias}d ${diffHoras}h demorado`;
    }
    return `${diffHoras}h demorado`;
  };

  // Formatear fecha
  const formatDate = (date: string | Date): string => {
    return new Date(date).toLocaleDateString('es-AR', {
      day: '2-digit',
      month: '2-digit',
      year: 'numeric',
      hour: '2-digit',
      minute: '2-digit'
    });
  };

  return (
    <>
      {/* Backdrop */}
      <div
        className="fixed inset-0 bg-black/50 z-40"
        onClick={onClose}
      />

      {/* Modal */}
      <div className="fixed right-4 top-20 w-full max-w-md bg-white rounded-lg shadow-2xl z-50 max-h-[80vh] overflow-y-auto">
        {/* Header */}
        <div className="sticky top-0 bg-gradient-to-r from-orange-500 to-red-500 text-white px-6 py-4 flex items-center justify-between border-b border-orange-600">
          <div className="flex items-center gap-2">
            <AlertTriangle className="w-5 h-5" />
            <h2 className="text-lg font-bold">
              Pedidos Demorados
              {notifications.length > 0 && (
                <span className="ml-2 bg-white text-red-500 rounded-full w-6 h-6 inline-flex items-center justify-center text-sm font-bold">
                  {notifications.length}
                </span>
              )}
            </h2>
          </div>
          <button
            onClick={onClose}
            className="p-1 hover:bg-white/20 rounded transition-colors"
          >
            <X className="w-5 h-5" />
          </button>
        </div>

        {/* Content */}
        <div className="p-4">
          {loading ? (
            <div className="text-center py-8">
              <div className="animate-spin rounded-full h-8 w-8 border-b-2 border-orange-500 mx-auto"></div>
              <p className="text-gray-500 mt-2">Cargando...</p>
            </div>
          ) : notifications.length === 0 ? (
            <div className="text-center py-8">
              <div className="text-4xl mb-2">✓</div>
              <p className="text-gray-500">No hay pedidos demorados</p>
            </div>
          ) : (
            <div className="space-y-3">
              {notifications.map((pedido) => (
                <div
                  key={pedido.IDPedido}
                  className="border border-orange-200 rounded-lg overflow-hidden hover:shadow-md transition-all"
                >
                  {/* Header del pedido */}
                  <button
                    onClick={() =>
                      setExpandedId(expandedId === pedido.IDPedido ? null : pedido.IDPedido)
                    }
                    className="w-full px-4 py-3 bg-orange-50 hover:bg-orange-100 transition-colors text-left"
                  >
                    <div className="flex items-start justify-between">
                      <div className="flex-1">
                        <div className="flex items-center gap-2 mb-1">
                          <span className="font-bold text-lg text-orange-600">
                            #{pedido.IDPedido}
                          </span>
                          <span className="text-xs bg-orange-200 text-orange-800 px-2 py-1 rounded-full">
                            {getDelayText(pedido.FechaEntregaEstimada)}
                          </span>
                        </div>
                        <p className="text-sm font-medium text-gray-800">
                          {pedido.ClienteNombre}
                        </p>
                      </div>
                      <div className="text-right">
                        <p className="text-sm font-semibold text-gray-700">
                          ${pedido.Total.toFixed(2)}
                        </p>
                      </div>
                    </div>
                  </button>

                  {/* Detalles expandibles */}
                  {expandedId === pedido.IDPedido && (
                    <div className="px-4 py-3 bg-gray-50 border-t border-orange-200 space-y-3">
                      {/* Responsable */}
                      {pedido.ResponsableNombre && (
                        <div className="flex items-start gap-2">
                          <User className="w-4 h-4 text-gray-500 mt-0.5 flex-shrink-0" />
                          <div className="text-sm">
                            <p className="text-gray-600">
                              {pedido.ResponsableRol}:
                            </p>
                            <p className="font-medium text-gray-900">
                              {pedido.ResponsableNombre}
                            </p>
                          </div>
                        </div>
                      )}

                      {/* Dirección */}
                      {pedido.DireccionEntrega && (
                        <div className="flex items-start gap-2">
                          <MapPin className="w-4 h-4 text-gray-500 mt-0.5 flex-shrink-0" />
                          <div className="text-sm">
                            <p className="text-gray-600">Dirección:</p>
                            <p className="font-medium text-gray-900">
                              {pedido.DireccionEntrega}
                            </p>
                            {pedido.LocalidadNombre && (
                              <p className="text-gray-700">
                                {pedido.LocalidadNombre}
                              </p>
                            )}
                          </div>
                        </div>
                      )}

                      {/* Fechas */}
                      <div className="flex items-start gap-2">
                        <Clock className="w-4 h-4 text-gray-500 mt-0.5 flex-shrink-0" />
                        <div className="text-sm">
                          <p className="text-gray-600">Entrega estimada:</p>
                          <p className="font-medium text-orange-600">
                            {formatDate(pedido.FechaEntregaEstimada)}
                          </p>
                          {pedido.FechaEntregaReal && (
                            <>
                              <p className="text-gray-600 mt-2">Entregado:</p>
                              <p className="font-medium text-gray-900">
                                {formatDate(pedido.FechaEntregaReal)}
                              </p>
                            </>
                          )}
                        </div>
                      </div>

                      {/* Intentos fallidos */}
                      {pedido.IntentosEntregaFallida > 0 && (
                        <div className="bg-red-50 border border-red-200 rounded p-2">
                          <p className="text-xs text-red-700 font-medium">
                            ⚠️ {pedido.IntentosEntregaFallida} intento(s) de entrega fallido
                          </p>
                        </div>
                      )}

                      {/* Estado */}
                      <div className="text-xs">
                        <span className="bg-gray-200 text-gray-700 px-2 py-1 rounded">
                          {pedido.EstadoNombre}
                        </span>
                      </div>
                    </div>
                  )}
                </div>
              ))}
            </div>
          )}
        </div>

        {/* Footer */}
        <div className="sticky bottom-0 bg-gray-100 border-t border-gray-200 px-4 py-3 text-right">
          <button
            onClick={onClose}
            className="px-4 py-2 bg-orange-500 text-white rounded hover:bg-orange-600 transition-colors font-medium"
          >
            Cerrar
          </button>
        </div>
      </div>
    </>
  );
};
