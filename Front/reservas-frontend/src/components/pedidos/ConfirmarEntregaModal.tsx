import React, { useState } from 'react';
import { Modal } from '@components/common/Modal';
import { Button } from '@components/common/Button';
import { Alert } from '@components/common/Alert';
import { pedidosService } from '../../service/PedidosService';
import { OrderSummaryDTO } from '../../types/pedido.types';
import { useAuth } from '@context/AuthContext';
import { CheckCircle, XCircle, AlertTriangle } from 'lucide-react';

interface ConfirmarEntregaModalProps {
    isOpen: boolean;
    onClose: () => void;
    pedido: OrderSummaryDTO;
    onSuccess: () => void;
}

export const ConfirmarEntregaModal: React.FC<ConfirmarEntregaModalProps> = ({
    isOpen,
    onClose,
    pedido,
    onSuccess,
}) => {
    const { user } = useAuth();
    const [tipoResultado, setTipoResultado] = useState<'entregado' | 'no-entregado' | null>(null);
    const [observaciones, setObservaciones] = useState('');
    const [loading, setLoading] = useState(false);
    const [error, setError] = useState('');

    const handleSubmit = async (e: React.FormEvent) => {
        e.preventDefault();
        
        if (!tipoResultado) {
            setError('Por favor seleccione un resultado');
            return;
        }

        if (tipoResultado === 'no-entregado' && !observaciones.trim()) {
            setError('Las observaciones son obligatorias para entregas fallidas');
            return;
        }

        setLoading(true);
        setError('');

        // Determinamos el ID del estado: 7 para entregado, 8 para fallido 
        const nuevoEstadoId = tipoResultado === 'entregado' ? 7 : 8;

        try {
            await pedidosService.cambiarEstado({
                idPedido: pedido.idPedido,
                idNuevoEstado: nuevoEstadoId,
                idUsuario: user!.id,
                observaciones: observaciones,
            });
            
            onSuccess(); 
            resetAndClose();
        } catch (err: any) {
            setError(err.response?.data?.message || 'Error al actualizar el estado');
        } finally {
            setLoading(false);
        }
    };

    const resetAndClose = () => {
        setTipoResultado(null);
        setObservaciones('');
        setError('');
        onClose();
    };

    return (
        <Modal isOpen={isOpen} onClose={resetAndClose} title="Confirmar Entrega" size="md">
            <form onSubmit={handleSubmit} className="space-y-4">
                {error && <Alert type="error">{error}</Alert>}

                {/* Información del Pedido */}
                <div className="bg-blue-50 border border-blue-200 rounded-lg p-4">
                    <h4 className="font-semibold text-blue-900 mb-2">Información del Pedido</h4>
                    <div className="space-y-1 text-sm text-blue-800">
                        <p><span className="font-medium">ID:</span> #{pedido.idPedido}</p>
                        <p><span className="font-medium">Cliente:</span> {pedido.clienteNombre}</p>
                        <p><span className="font-medium">Total:</span> ${pedido.total.toFixed(2)}</p>
                        
                        {/* Indicador de Intentos Fallidos */}
                        <div className="mt-3 pt-2 border-t border-blue-200">
                            <p className="text-xs font-bold uppercase mb-1">Intentos realizados: {pedido.intentosEntregaFallida} / 3</p>
                            <div className="flex gap-1">
                                {[1, 2, 3].map((i) => (
                                    <div 
                                        key={i} 
                                        className={`h-2 w-full rounded-full ${
                                            i <= pedido.intentosEntregaFallida ? 'bg-red-500' : 'bg-blue-200'
                                        }`}
                                    />
                                ))}
                            </div>
                            {pedido.intentosEntregaFallida === 2 && (
                                <div className="flex items-center gap-1 mt-2 text-red-600 animate-pulse">
                                    <AlertTriangle size={14} />
                                    <p className="text-xs font-bold">Último intento: Si falla, se cancelará el pedido.</p>
                                </div>
                            )}
                        </div>
                    </div>
                </div>

                {/* Selección del Resultado */}
                <div>
                    <label className="block text-sm font-medium text-gray-700 mb-3">
                        ¿Cuál fue el resultado de la entrega?
                    </label>
                    <div className="grid grid-cols-2 gap-3">
                        <button
                            type="button"
                            onClick={() => setTipoResultado('entregado')}
                            className={`p-4 rounded-lg border-2 transition-all ${
                                tipoResultado === 'entregado'
                                    ? 'bg-green-600 text-white border-transparent shadow-lg'
                                    : 'bg-white text-gray-700 border-gray-200 hover:border-green-300'
                            }`}
                        >
                            <CheckCircle className={`w-8 h-8 mx-auto mb-2 ${
                                tipoResultado === 'entregado' ? 'text-white' : 'text-green-600'
                            }`} />
                            <span className="font-medium">Entregado</span>
                        </button>

                        <button
                            type="button"
                            onClick={() => setTipoResultado('no-entregado')}
                            className={`p-4 rounded-lg border-2 transition-all ${
                                tipoResultado === 'no-entregado'
                                    ? 'bg-red-600 text-white border-transparent shadow-lg'
                                    : 'bg-white text-gray-700 border-gray-200 hover:border-red-300'
                            }`}
                        >
                            <XCircle className={`w-8 h-8 mx-auto mb-2 ${
                                tipoResultado === 'no-entregado' ? 'text-white' : 'text-red-600'
                            }`} />
                            <span className="font-medium">Entrega fallida</span>
                        </button>
                    </div>
                </div>

                {/* Observaciones */}
                <div>
                    <label className="block text-sm font-medium text-gray-700 mb-1">
                        Observaciones {tipoResultado === 'no-entregado' && <span className="text-red-600">*</span>}
                    </label>
                    <textarea
                        value={observaciones}
                        onChange={(e) => setObservaciones(e.target.value)}
                        required={tipoResultado === 'no-entregado'}
                        placeholder={
                            tipoResultado === 'no-entregado' 
                                ? 'Indique el motivo por el cual no se pudo entregar...' 
                                : 'Observaciones adicionales (opcional)...'
                        }
                        className="w-full px-4 py-2 border border-gray-300 rounded-lg focus:ring-2 focus:ring-blue-500 focus:border-transparent"
                        rows={4}
                    />
                </div>

                {/* Botones */}
                <div className="flex gap-3 justify-end pt-4">
                    <Button
                        type="button"
                        variant="secondary"
                        onClick={resetAndClose}
                        disabled={loading}
                    >
                        Cancelar
                    </Button>
                <Button
    type="submit"
    variant={tipoResultado === 'entregado' ? 'success' : 'danger'}
    isLoading={loading}
    // Agregamos la lógica de bloqueo aquí:
    disabled={
        !tipoResultado || 
        pedido.idEstadoDePedido === 7 || // 7 = Entregado
        pedido.idEstadoDePedido === 9 || // 9 = Cancelado
        (tipoResultado === 'no-entregado' && pedido.intentosEntregaFallida >= 3)
    }
>
    Confirmar {tipoResultado === 'entregado' ? 'Entrega' : 'Fallo en Entrega'}
</Button>
                </div>
            </form>
        </Modal>
    );
};