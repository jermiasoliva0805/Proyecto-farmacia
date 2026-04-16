import { useState, useEffect } from 'react';
import { UserDTO } from '../types/auth.types';

interface CadeteDisponibleDTO {
    idUsuario: number;
    nombre: string;
    apellido: string;
    usuarioNombre: string;
    rol: string;
    mail: string;
    zonaId: number | null;
    requiereAsignacionZona: boolean;
}

/**
 * Hook personalizado para obtener cadetes disponibles por zona
 * Filtra automáticamente los cadetes que pertenecen a la misma zona del pedido
 * 
 * @param pedidoId - ID del pedido para el cual se requieren cadetes
 * @returns Objeto con cadetes disponibles, estado de carga y errores
 */
export const useCadetesPorZona = (pedidoId: number | null) => {
    const [cadetes, setCadetes] = useState<CadeteDisponibleDTO[]>([]);
    const [loading, setLoading] = useState(false);
    const [error, setError] = useState<string | null>(null);

    useEffect(() => {
        if (!pedidoId) {
            setCadetes([]);
            return;
        }

        const fetchCadetesDisponibles = async () => {
            setLoading(true);
            setError(null);

            try {
                const response = await fetch(
                    `${import.meta.env.VITE_API_BASE_URL}/orders/cadetes-disponibles/${pedidoId}`,
                    {
                        method: 'GET',
                        headers: {
                            'Content-Type': 'application/json',
                            'Authorization': `Bearer ${localStorage.getItem('token')}`,
                        },
                    }
                );

                if (!response.ok) {
                    if (response.status === 404) {
                        setError('Pedido no encontrado');
                    } else if (response.status === 400) {
                        setError('El pedido no tiene una zona de reparto asignada');
                    } else {
                        setError(`Error: ${response.statusText}`);
                    }
                    setCadetes([]);
                    return;
                }

                const data = await response.json();
                setCadetes(data || []);

                if (data.length === 0) {
                    setError('No hay cadetes disponibles en esta zona');
                }
            } catch (err: any) {
                setError(
                    err.message || 'Error al cargar los cadetes disponibles'
                );
                setCadetes([]);
            } finally {
                setLoading(false);
            }
        };

        fetchCadetesDisponibles();
    }, [pedidoId]);

    return {
        cadetes,
        loading,
        error,
    };
};
