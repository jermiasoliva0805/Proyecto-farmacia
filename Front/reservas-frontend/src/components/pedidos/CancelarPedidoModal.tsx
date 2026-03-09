import React, { useState, useEffect } from 'react';
import { X, AlertTriangle, Ban } from 'lucide-react';
import { useAuth } from '../../context/AuthContext';

interface MotivoCancelacion {
  id: number;
  nombre: string;
  activo: boolean;
}

interface Props {
  isOpen: boolean;
  onClose: () => void;
  pedidoId: number;
  onCancelarSuccess?: () => void;
}

const API_BASE = (import.meta.env.VITE_API_BASE_URL ?? 'http://localhost:5000/api').replace(/\/$/, '');

export const CancelarPedidoModal: React.FC<Props> = ({ 
  isOpen, 
  onClose, 
  pedidoId,
  onCancelarSuccess 
}) => {
  const { user } = useAuth();
  const [motivos, setMotivos] = useState<MotivoCancelacion[]>([]);
  const [selectedMotivo, setSelectedMotivo] = useState<number | null>(null);
  const [justificacion, setJustificacion] = useState('');
  const [loading, setLoading] = useState(false);
  const [error, setError] = useState<string | null>(null);
  const [success, setSuccess] = useState(false);
  const [puedeCancel, setPuedeCancel] = useState(true);

  // Cargar motivos cuando se abre el modal
  useEffect(() => {
    if (isOpen) {
      cargarMotivos();
      validarCancelacion();
      setSelectedMotivo(null);
      setJustificacion('');
      setError(null);
      setSuccess(false);
    }
  }, [isOpen]);

  const cargarMotivos = async () => {
    try {
      const response = await fetch(`${API_BASE}/orders/motivos-cancelacion`, {
        headers: {
          'Authorization': `Bearer ${localStorage.getItem('farmacia_token')}`
        }
      });

      if (!response.ok) throw new Error('Error al obtener motivos');

      const data = await response.json();
      setMotivos(data);
    } catch (err) {
      console.error('Error:', err);
      setError('No se pudieron cargar los motivos de cancelación');
    }
  };

  const validarCancelacion = async () => {
    try {
      const token = localStorage.getItem('farmacia_token');
      if (!token) {
        setError('Sesión expirada. Por favor, inicia sesión nuevamente.');
        return;
      }

      const response = await fetch(`${API_BASE}/orders/${pedidoId}/puede-cancelarse`, {
        headers: {
          'Authorization': `Bearer ${token}`
        }
      });

      const responseText = await response.text();
      let data: any = {};

      if (responseText) {
        try {
          data = JSON.parse(responseText);
        } catch (e) {
          console.error('Error parseando validación:', e);
        }
      }

      if (!response.ok) {
        setPuedeCancel(false);
        setError(data.reason || 'Este pedido no puede ser cancelado');
        return;
      }

      if (!data.canCancel) {
        setPuedeCancel(false);
        setError(data.reason || 'Este pedido no puede ser cancelado');
      } else {
        setPuedeCancel(true);
      }
    } catch (err) {
      console.error('Error en validarCancelacion:', err);
    }
  };

  // Determinar si la justificación es requerida según el motivo
  const esJustificacionRequerida = (motivoId: number | null) => {
    // 2 = Falta de stock, 3 = Error en pago
    return motivoId === 2 || motivoId === 3;
  };

  // Obtener el nombre del motivo seleccionado
  const motivoSeleccionado = motivos.find(m => m.id === selectedMotivo);

  // Validar formulario
  const puedeEnviar = () => {
    if (!selectedMotivo) return false;
    if (esJustificacionRequerida(selectedMotivo) && !justificacion.trim()) return false;
    if (justificacion.length > 500) return false;
    return true;
  };

  const handleCancelar = async () => {
    if (!puedeEnviar()) return;

    setLoading(true);
    setError(null);

    try {
      const token = localStorage.getItem('farmacia_token');
      if (!token) {
        setError('No se encontró el token de autenticación.');
        setLoading(false);
        return;
      }

      const payload = {
        pedidoId: pedidoId,
        motivoCancelacionId: selectedMotivo,
        justificacion: justificacion.trim() || '',
        usuarioId: user?.id ? String(user.id) : '1'
      };

      console.log('Enviando cancelación:', payload);

      const response = await fetch(`${API_BASE}/orders/cancelar`, {
        method: 'POST',
        headers: {
          'Content-Type': 'application/json',
          'Authorization': `Bearer ${token}`
        },
        body: JSON.stringify(payload)
      });

      console.log('Respuesta status:', response.status);

      const responseText = await response.text();
      console.log('Respuesta texto:', responseText);

      let data: any = {};
      if (responseText) {
        try {
          data = JSON.parse(responseText);
        } catch (e) {
          console.error('Error parseando JSON:', e);
          data = { message: responseText };
        }
      }

      if (!response.ok) {
        setError(data.message || `Error ${response.status}: ${responseText || 'Error desconocido'}`);
        return;
      }

      setSuccess(true);
      
      setTimeout(() => {
        onCancelarSuccess?.();
        onClose();
      }, 2000);
    } catch (err) {
      console.error('Error en handleCancelar:', err);
      setError(err instanceof Error ? err.message : 'Error desconocido');
    } finally {
      setLoading(false);
    }
  };

  if (!isOpen) return null;

  if (!puedeCancel) {
    return (
      <div className="fixed inset-0 z-50 flex items-center justify-center bg-black bg-opacity-50 p-4">
        <div className="bg-white rounded-xl shadow-xl w-full max-w-md overflow-hidden">
          <div className="p-4 border-b flex justify-between items-start">
            <div className="flex gap-3">
              <Ban className="w-5 h-5 text-red-600 shrink-0 mt-1" />
              <div>
                <h2 className="text-lg font-bold text-gray-900">Cancelar Pedido</h2>
                <p className="text-sm text-gray-500">Pedido #{pedidoId}</p>
              </div>
            </div>
            <button 
              onClick={onClose} 
              className="text-gray-400 hover:text-gray-600"
            >
              <X className="w-5 h-5" />
            </button>
          </div>
          <div className="p-4">
            <div className="bg-red-50 border border-red-200 p-3 rounded-lg flex gap-2">
              <AlertTriangle className="w-5 h-5 text-red-600 shrink-0 mt-0.5" />
              <p className="text-sm text-red-800">{error}</p>
            </div>
          </div>
          <div className="p-4 border-t">
            <button
              onClick={onClose}
              className="w-full px-4 py-2 bg-blue-600 text-white font-bold rounded-lg hover:bg-blue-700 transition"
            >
              Cerrar
            </button>
          </div>
        </div>
      </div>
    );
  }

  return (
    <div className="fixed inset-0 z-50 flex items-center justify-center bg-black bg-opacity-50 p-4">
      <div className="bg-white rounded-xl shadow-xl w-full max-w-md overflow-hidden">
        {/* Header */}
        <div className="p-4 border-b flex justify-between items-start">
          <div className="flex gap-3">
            <Ban className="w-5 h-5 text-red-600 shrink-0 mt-1" />
            <div>
              <h2 className="text-lg font-bold text-gray-900">Cancelar Pedido</h2>
              <p className="text-sm text-gray-500">Pedido #{pedidoId}</p>
            </div>
          </div>
          <button 
            onClick={onClose} 
            className="text-gray-400 hover:text-gray-600"
          >
            <X className="w-5 h-5" />
          </button>
        </div>

        {/* Content */}
        <div className="p-4 space-y-4">
          {success && (
            <div className="bg-green-50 border border-green-200 p-3 rounded-lg">
              <p className="text-sm font-bold text-green-800">✓ Pedido cancelado exitosamente</p>
            </div>
          )}

          {error && (
            <div className="bg-red-50 border border-red-200 p-3 rounded-lg flex gap-2">
              <AlertTriangle className="w-5 h-5 text-red-600 shrink-0 mt-0.5" />
              <p className="text-sm text-red-800">{error}</p>
            </div>
          )}

          {!success && (
            <>
              {/* Seleccionar Motivo */}
              <div>
                <label className="block text-sm font-bold text-gray-700 mb-2">
                  Motivo de Cancelación *
                </label>
                <div className="space-y-2">
                  {motivos.length === 0 ? (
                    <p className="text-sm text-gray-500">Cargando motivos...</p>
                  ) : (
                    motivos.map(motivo => (
                      <label key={motivo.id} className="flex items-center gap-3 p-3 border rounded-lg cursor-pointer hover:bg-gray-50 transition">
                        <input
                          type="radio"
                          name="motivo"
                          value={motivo.id}
                          checked={selectedMotivo === motivo.id}
                          onChange={(e) => setSelectedMotivo(parseInt(e.target.value))}
                          className="w-4 h-4 text-blue-600"
                        />
                        <div>
                          <p className="font-medium text-gray-900">{motivo.nombre}</p>
                          <p className="text-xs text-gray-500">
                            {motivo.id === 1 && "Solo disponible si el pedido aún no está siendo preparado"}
                            {motivo.id === 2 && "Requiere especificar qué productos no tienen stock"}
                            {motivo.id === 3 && "Requiere detallar el tipo de error"}
                            {motivo.id === 4 && "Para direcciones incorrectas o incompletas"}
                          </p>
                        </div>
                      </label>
                    ))
                  )}
                </div>
              </div>

              {/* Campo de Justificación */}
              {selectedMotivo && (
                <div>
                  <label className="block text-sm font-bold text-gray-700 mb-2">
                    {esJustificacionRequerida(selectedMotivo) ? '* ' : ''}
                    Detalles / Justificación
                  </label>
                  <textarea
                    value={justificacion}
                    onChange={(e) => setJustificacion(e.target.value)}
                    placeholder={
                      selectedMotivo === 2
                        ? "Ej: EDP Balance agotado, Alcohol en gel sin stock..."
                        : selectedMotivo === 3
                        ? "Ej: Tarjeta rechazada, fondos insuficientes, error de procesamiento..."
                        : selectedMotivo === 4
                        ? "Ej: Edificio sin número, dirección incompleta..."
                        : "Información adicional (opcional)"
                    }
                    maxLength={500}
                    rows={4}
                    className="w-full px-3 py-2 border border-gray-300 rounded-lg text-sm focus:outline-none focus:ring-2 focus:ring-blue-500"
                  />
                  <p className="text-xs text-gray-500 mt-1">
                    {justificacion.length}/500 caracteres
                    {esJustificacionRequerida(selectedMotivo) && !justificacion.trim() && (
                      <span className="text-red-600 block mt-1">⚠️ Información requerida para este motivo</span>
                    )}
                  </p>
                </div>
              )}

              {/* Advertencia importante */}
              <div className="bg-yellow-50 border border-yellow-200 p-3 rounded-lg">
                <p className="text-xs text-yellow-800">
                  <strong>⚠️ Importante:</strong> Esta acción registrará la cancelación en el historial del pedido y no se puede deshacer.
                </p>
              </div>
            </>
          )}
        </div>

        {/* Footer */}
        {!success && (
          <div className="p-4 border-t flex gap-2">
            <button
              onClick={onClose}
              className="flex-1 px-4 py-2 text-gray-700 font-medium border border-gray-300 rounded-lg hover:bg-gray-50 transition"
            >
              Cancelar
            </button>
            <button
              onClick={handleCancelar}
              disabled={!puedeEnviar() || loading}
              className={`flex-1 px-4 py-2 font-bold rounded-lg text-white transition ${
                puedeEnviar() && !loading
                  ? 'bg-red-600 hover:bg-red-700 cursor-pointer'
                  : 'bg-gray-300 cursor-not-allowed'
              }`}
            >
              {loading ? 'Procesando...' : 'Cancelar Pedido'}
            </button>
          </div>
        )}

        {success && (
          <div className="p-4 border-t flex gap-2">
            <button
              onClick={onClose}
              className="flex-1 px-4 py-2 bg-blue-600 text-white font-bold rounded-lg hover:bg-blue-700 transition"
            >
              Cerrar
            </button>
          </div>
        )}
      </div>
    </div>
  );
};
