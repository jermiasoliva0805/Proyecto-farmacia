import React, { useState, useEffect } from 'react';
import { Modal } from '@components/common/Modal';
import { Button } from '@components/common/Button';
import { Select } from '@components/common/Select';
import { Alert } from '@components/common/Alert';
import { useAuth } from '../../context/AuthContext';
import { usuariosService } from '../../service/usuariosService';
import { pedidosService } from '../../service/PedidosService';
import { UserDTO } from '../../types/auth.types';
import { OrderSummaryDTO } from '../../types/pedido.types';
import { useCadetesPorZona } from '../../hooks/useCadetesPorZona';

interface AsignarCadeteModalProps {
    isOpen: boolean;
    onClose: () => void;
    pedido: OrderSummaryDTO;
    onSuccess: () => void;
}

export const AsignarCadeteModal: React.FC<AsignarCadeteModalProps> = ({
    isOpen,
    onClose,
    pedido,
    onSuccess,
}) => {
    const { user } = useAuth();
    const [selectedCadete, setSelectedCadete] = useState<string>('');
    const [loading, setLoading] = useState(false);
    const [error, setError] = useState<string>('');

    // Hook personalizado para obtener cadetes disponibles por zona del pedido
    const { cadetes, loading: loadingCadetes, error: errorCadetes } = useCadetesPorZona(
        isOpen ? pedido.idPedido : null
    );

    useEffect(() => {
        setError(errorCadetes || '');
    }, [errorCadetes]);

    const handleSubmit = async (e: React.FormEvent) => {
        e.preventDefault();
        setError('');

        if (!selectedCadete) {
            setError('Por favor seleccione un cadete');
            return;
        }

        setLoading(true);

        try {
            // RF19: Asignar Cadete (Preparado -> Despachando)
            // El backend ya cambia de 4→5, no necesitamos segundo llamado
            await pedidosService.asignarCadete({
                pedidoId: pedido.idPedido,
                cadeteId: parseInt(selectedCadete),
            });

            onSuccess();
            onClose();
        } catch (err: any) {
            setError(err.response?.data?.message || 'Error al asignar cadete');
        } finally {
            setLoading(false);
        }
    };

    return (
        <Modal isOpen={isOpen} onClose={onClose} title="Asignar Cadete" size="md">
            <form onSubmit={handleSubmit} className="space-y-4">
                {error && <Alert type="error">{error}</Alert>}

                {/* Información del Pedido */}
                <div className="bg-green-50 border border-green-200 rounded-lg p-4">
                    <h4 className="font-semibold text-green-900 mb-2">Información del Pedido</h4>
                    <div className="space-y-1 text-sm text-green-800">
                        <p><span className="font-medium">ID:</span> #{pedido.idPedido}</p>
                        <p><span className="font-medium">Cliente:</span> {pedido.clienteNombre}</p>
                        <p><span className="font-medium">Dirección:</span> {pedido.clienteNombre}</p>
                        <p><span className="font-medium">Estado Actual:</span> {pedido.estadoNombre}</p>
                    </div>
                </div>

                {/* Indicador de carga de cadetes */}
                {loadingCadetes && (
                    <div className="flex justify-center py-4">
                        <div className="animate-spin rounded-full h-8 w-8 border-b-2 border-blue-600"></div>
                    </div>
                )}

                {/* Selección de Cadete */}
                {!loadingCadetes && (
                    <Select
                        label="Seleccionar Cadete"
                        value={selectedCadete}
                        onChange={(e) => setSelectedCadete(e.target.value)}
                        options={cadetes.map(cadete => ({
                            value: cadete.idUsuario.toString(),
                            label: `${cadete.nombre} ${cadete.apellido}`,
                        }))}
                        required
                        disabled={cadetes.length === 0 || loadingCadetes}
                    />
                )}

                {/* Mensaje si no hay cadetes disponibles */}
                {!loadingCadetes && cadetes.length === 0 && (
                    <Alert type="warning">
                        No hay cadetes disponibles en la zona de este pedido.
                    </Alert>
                )}

                {/* Botones */}
                <div className="flex gap-3 justify-end pt-4">
                    <Button
                        type="button"
                        variant="secondary"
                        onClick={onClose}
                        disabled={loading || loadingCadetes}
                    >
                        Cancelar
                    </Button>
                    <Button
                        type="submit"
                        variant="success"
                        isLoading={loading}
                        disabled={cadetes.length === 0 || !selectedCadete || loadingCadetes}
                    >
                        Asignar Cadete
                    </Button>
                </div>
            </form>
        </Modal>
    );
};