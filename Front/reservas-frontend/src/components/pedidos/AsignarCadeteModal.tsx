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
    const [cadetes, setCadetes] = useState<UserDTO[]>([]);
    const [selectedCadete, setSelectedCadete] = useState<string>('');
    const [loading, setLoading] = useState(false);
    const [error, setError] = useState<string>('');

    useEffect(() => {
        if (isOpen) {
            loadCadetes();
        }
    }, [isOpen]);

    const loadCadetes = async () => {
        try {
            const data = await usuariosService.getUsuariosByRol('Cadete');
            setCadetes(data);
        } catch (err) {
            setError('Error al cargar la lista de cadetes');
        }
    };

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

                {/* Selección de Cadete */}
                <Select
                    label="Seleccionar Cadete"
                    value={selectedCadete}
                    onChange={(e) => setSelectedCadete(e.target.value)}
                    options={cadetes.map(cadete => ({
                        value: cadete.id.toString(),
                        label: `${cadete.nombreCompleto} - ${cadete.nombreSucursal}`,
                    }))}
                    required
                />

                {/* Botones */}
                <div className="flex gap-3 justify-end pt-4">
                    <Button
                        type="button"
                        variant="secondary"
                        onClick={onClose}
                        disabled={loading}
                    >
                        Cancelar
                    </Button>
                    <Button
                        type="submit"
                        variant="success"
                        isLoading={loading}
                    >
                        Asignar Cadete
                    </Button>
                </div>
            </form>
        </Modal>
    );
};