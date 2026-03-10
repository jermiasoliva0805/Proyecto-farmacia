import React, { useState, useEffect } from 'react';
import { X, AlertTriangle } from 'lucide-react';
import { useAuth } from '../../context/AuthContext';

interface MotivoCancelacion {
    id: number;
    nombre: string;
    activo: boolean;
}

interface CancelarConMotivoModalProps {
    isOpen: boolean;
    pedidoId: number;
    clienteNombre: string;
    onConfirm: (motivo: string) => Promise<void>;
    onCancel: () => void;
}

const API_BASE = (import.meta.env.VITE_API_BASE_URL ?? 'http://localhost:5000/api').replace(/\/$/, '');

export const CancelarConMotivoModal: React.FC<CancelarConMotivoModalProps> = ({
    isOpen,
    pedidoId,
    clienteNombre,
    onConfirm,
    onCancel
}) => {
    const { user } = useAuth();
    const [motivos, setMotivos] = useState<MotivoCancelacion[]>([]);
    const [selectedMotivoId, setSelectedMotivoId] = useState<number | null>(null);
    const [justificacion, setJustificacion] = useState('');
    const [isLoading, setIsLoading] = useState(false);
    const [loadingMotivos, setLoadingMotivos] = useState(false);
    const [error, setError] = useState<string | null>(null);

    // Cargar motivos cuando se abre el modal
    useEffect(() => {
        if (isOpen) {
            cargarMotivos();
            setSelectedMotivoId(null);
            setJustificacion('');
            setError(null);
        }
    }, [isOpen]);

    const cargarMotivos = async () => {
        try {
            setLoadingMotivos(true);
            const token = localStorage.getItem('farmacia_token');
            const response = await fetch(`${API_BASE}/orders/motivos-cancelacion`, {
                headers: {
                    'Authorization': `Bearer ${token}`
                }
            });

            if (!response.ok) throw new Error('Error al obtener motivos');

            const data = await response.json();
            setMotivos(data);
        } catch (err) {
            console.error('Error:', err);
            // Fallback: usar motivos predeterminados
            setMotivos([
                { id: 1, nombre: 'Arrepentimiento', activo: true },
                { id: 2, nombre: 'Falta de stock', activo: true },
                { id: 3, nombre: 'Error en el pago', activo: true },
                { id: 4, nombre: 'Dirección incorrecta', activo: true }
            ]);
        } finally {
            setLoadingMotivos(false);
        }
    };

    if (!isOpen) return null;

    const esJustificacionRequerida = (motivoId: number | null) => {
        return motivoId === 2 || motivoId === 3;
    };

    const puedeEnviar = () => {
        if (!selectedMotivoId) return false;
        if (esJustificacionRequerida(selectedMotivoId) && !justificacion.trim()) return false;
        return true;
    };

    const handleConfirm = async () => {
        if (!puedeEnviar() || isLoading) return;

        setIsLoading(true);
        setError(null);

        try {
            const token = localStorage.getItem('farmacia_token');
            if (!token) {
                setError('No se encontró el token de autenticación.');
                setIsLoading(false);
                return;
            }

            const payload = {
                pedidoId: pedidoId,
                motivoCancelacionId: selectedMotivoId,
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

            // Ejecutar callback
            await onConfirm(data.message || 'Pedido cancelado correctamente');
            handleClose();
        } catch (err) {
            console.error('Error en handleConfirm:', err);
            setError(err instanceof Error ? err.message : 'Error desconocido');
        } finally {
            setIsLoading(false);
        }
    };

    const handleClose = () => {
        setSelectedMotivoId(null);
        setJustificacion('');
        setError(null);
        onCancel();
    };

    const selectedMotivo = motivos.find(m => m.id === selectedMotivoId);

    return (
        <div className="fixed inset-0 bg-black/50 flex items-center justify-center z-50 p-4">
            <div className="bg-white rounded-xl shadow-2xl max-w-md w-full p-6">
                {/* Header */}
                <div className="flex items-center justify-between mb-4">
                    <div className="flex items-center gap-3">
                        <div className="bg-red-100 p-2 rounded-lg">
                            <AlertTriangle className="w-6 h-6 text-red-600" />
                        </div>
                        <div>
                            <h2 className="text-lg font-bold text-gray-900">Cancelar Pedido</h2>
                            <p className="text-xs text-gray-500">Pedido #{pedidoId}</p>
                        </div>
                    </div>
                    <button
                        onClick={handleClose}
                        disabled={isLoading}
                        className="text-gray-400 hover:text-gray-600 transition-colors"
                    >
                        <X className="w-5 h-5" />
                    </button>
                </div>

                {/* Contenido */}
                <div className="space-y-4 mb-6">
                    <div className="bg-orange-50 border border-orange-200 rounded-lg p-3">
                        <p className="text-sm text-orange-700 font-medium">
                            ⚠️ Estás por cancelar el pedido #{pedidoId} de <strong>{clienteNombre}</strong>
                        </p>
                    </div>

                    {error && (
                        <div className="bg-red-50 border border-red-200 rounded-lg p-3 flex gap-2">
                            <AlertTriangle className="w-5 h-5 text-red-600 flex-shrink-0 mt-0.5" />
                            <p className="text-sm text-red-700">{error}</p>
                        </div>
                    )}

                    {/* Motivos predefinidos */}
                    <div>
                        <label className="block text-sm font-medium text-gray-700 mb-3">
                            Motivo de Cancelación <span className="text-red-500">*</span>
                        </label>
                        {loadingMotivos ? (
                            <p className="text-sm text-gray-500 text-center py-4">Cargando motivos...</p>
                        ) : (
                            <div className="space-y-2">
                                {motivos.filter(m => m.activo).map(motivo => (
                                    <label key={motivo.id} className="flex items-start gap-3 p-3 border border-gray-200 rounded-lg cursor-pointer hover:bg-gray-50 transition">
                                        <input
                                            type="radio"
                                            name="motivo"
                                            value={motivo.id}
                                            checked={selectedMotivoId === motivo.id}
                                            onChange={(e) => setSelectedMotivoId(parseInt(e.target.value))}
                                            disabled={isLoading}
                                            className="mt-1"
                                        />
                                        <div>
                                            <p className="font-medium text-gray-800">{motivo.nombre}</p>
                                            <p className="text-xs text-gray-500">
                                                {motivo.id === 1 && "Cliente solicita cancelación"}
                                                {motivo.id === 2 && "Requiere especificar qué productos no tienen stock"}
                                                {motivo.id === 3 && "Requiere detallar el tipo de error"}
                                                {motivo.id === 4 && "Para direcciones incorrectas o incompletas"}
                                            </p>
                                        </div>
                                    </label>
                                ))}
                            </div>
                        )}
                    </div>

                    {/* Justificación adicional */}
                    {selectedMotivo && (
                        <div>
                            <label className="block text-sm font-medium text-gray-700 mb-2">
                                {esJustificacionRequerida(selectedMotivoId) ? '* ' : ''}
                                Detalles / Justificación
                            </label>
                            <textarea
                                value={justificacion}
                                onChange={(e) => setJustificacion(e.target.value)}
                                disabled={isLoading}
                                className="w-full p-3 border border-gray-300 rounded-lg focus:outline-none focus:ring-2 focus:ring-red-500 focus:border-transparent resize-none text-sm"
                                rows={3}
                                placeholder={
                                    selectedMotivoId === 2
                                        ? "Ej: EDP Balance agotado, Alcohol en gel sin stock..."
                                        : selectedMotivoId === 3
                                        ? "Ej: Tarjeta rechazada, fondos insuficientes..."
                                        : "Información adicional (opcional)"
                                }
                                maxLength={500}
                            />
                            <p className="text-xs text-gray-500 mt-1">
                                {justificacion.length}/500 caracteres
                                {esJustificacionRequerida(selectedMotivoId) && !justificacion.trim() && (
                                    <span className="text-red-600 block mt-1">⚠️ Información requerida para este motivo</span>
                                )}
                            </p>
                        </div>
                    )}

                    <div className="bg-blue-50 border border-blue-200 rounded-lg p-3">
                        <p className="text-xs text-blue-700">
                            ℹ️ <strong>Importante:</strong> Esta acción registrará la cancelación en el historial del pedido y no se puede deshacer.
                        </p>
                    </div>
                </div>

                {/* Botones */}
                <div className="flex gap-3">
                    <button
                        onClick={handleClose}
                        disabled={isLoading}
                        className="flex-1 px-4 py-2 border border-gray-300 text-gray-700 font-medium rounded-lg hover:bg-gray-50 transition-colors disabled:opacity-50"
                    >
                        Volver
                    </button>
                    <button
                        onClick={handleConfirm}
                        disabled={isLoading || !puedeEnviar()}
                        className="flex-1 px-4 py-2 bg-red-600 hover:bg-red-700 text-white font-medium rounded-lg transition-colors disabled:opacity-50 disabled:cursor-not-allowed"
                    >
                        {isLoading ? 'Cancelando...' : 'Confirmar Cancelación'}
                    </button>
                </div>
            </div>
        </div>
    );
};
