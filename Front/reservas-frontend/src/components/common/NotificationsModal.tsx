import React from 'react';
import { X, Clock, AlertTriangle } from 'lucide-react';
import { OrderSummaryDTO } from '../../types/pedido.types';
import { ESTADO_MAP } from '../../types/kanban.types';

interface NotificationsModalProps {
  isOpen: boolean;
  onClose: () => void;
  notifications: OrderSummaryDTO[];
  loading: boolean;
}

/**
 * Modal de notificaciones para pedidos demorados.
 * Muestra: ID pedido, estado principal actual, responsable (nombre).
 */
export const NotificationsModal: React.FC<NotificationsModalProps> = ({
  isOpen,
  onClose,
  notifications,
  loading,
}) => {
  if (!isOpen) return null;

  return (
    <div className="fixed inset-0 z-[60] overflow-y-auto">
      {/* Overlay */}
      <div
        className="fixed inset-0 bg-black/40 backdrop-blur-sm"
        onClick={onClose}
      />

      {/* Panel */}
      <div className="absolute top-16 right-4 w-full max-w-sm bg-white rounded-2xl shadow-2xl border border-gray-100 overflow-hidden">
        {/* Header */}
        <div className="flex items-center justify-between px-5 py-4 bg-orange-50 border-b border-orange-100">
          <div className="flex items-center gap-2">
            <AlertTriangle className="w-5 h-5 text-orange-500" />
            <h3 className="font-semibold text-orange-900 text-sm">
              Pedidos demorados
            </h3>
            {notifications.length > 0 && (
              <span className="bg-orange-500 text-white text-xs font-bold rounded-full px-2 py-0.5">
                {notifications.length}
              </span>
            )}
          </div>
          <button
            onClick={onClose}
            className="text-gray-400 hover:text-gray-600 transition-colors"
          >
            <X className="w-5 h-5" />
          </button>
        </div>

        {/* Content */}
        <div className="max-h-96 overflow-y-auto">
          {loading ? (
            <div className="flex items-center justify-center py-10">
              <div className="animate-spin rounded-full h-8 w-8 border-b-2 border-orange-500" />
            </div>
          ) : notifications.length === 0 ? (
            <div className="flex flex-col items-center justify-center py-10 text-gray-400">
              <Clock className="w-10 h-10 mb-2 opacity-40" />
              <p className="text-sm">No hay pedidos demorados</p>
            </div>
          ) : (
            <ul className="divide-y divide-gray-100">
              {notifications.map((pedido) => {
                // Normalizar campos (la API puede devolver PascalCase o camelCase)
                const id = pedido.IDPedido ?? pedido.idPedido;
                const estadoId = pedido.IDEstadoDePedido ?? pedido.idEstadoDePedido;
                const estadoNombre =
                  (estadoId ? ESTADO_MAP[estadoId] : null) ??
                  pedido.EstadoNombre ??
                  pedido.estadoNombre ??
                  'Desconocido';
                const responsable =
                  pedido.ResponsableNombre ??
                  pedido.responsableNombre ??
                  pedido.operarioNombre ??
                  'Sin asignar';

                return (
                  <li key={id} className="px-5 py-3 hover:bg-orange-50 transition-colors">
                    <div className="flex items-start justify-between gap-2">
                      <div className="flex-1 min-w-0">
                        {/* ID */}
                        <p className="text-xs font-bold text-gray-900 mb-0.5">
                          Pedido #{id}
                        </p>
                        {/* Estado principal */}
                        <span className="inline-block bg-gray-100 text-gray-700 text-xs px-2 py-0.5 rounded-full font-medium mb-1">
                          {estadoNombre}
                        </span>
                        {/* Responsable */}
                        <p className="text-xs text-gray-500 truncate">
                          Responsable:{' '}
                          <span className="font-medium text-gray-700">
                            {responsable}
                          </span>
                        </p>
                      </div>
                      {/* Badge demorado */}
                      <span className="flex-shrink-0 flex items-center gap-1 bg-orange-100 text-orange-700 text-xs font-semibold px-2 py-1 rounded-full">
                        <Clock className="w-3 h-3" />
                        Demorado
                      </span>
                    </div>
                  </li>
                );
              })}
            </ul>
          )}
        </div>

        {/* Footer */}
        {notifications.length > 0 && (
          <div className="px-5 py-3 bg-gray-50 border-t border-gray-100 text-center">
            <p className="text-xs text-gray-400">
              Los pedidos demorados superaron las 48hs hábiles sin completarse.
            </p>
          </div>
        )}
      </div>
    </div>
  );
};

