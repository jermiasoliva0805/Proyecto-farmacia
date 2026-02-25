import React, { useEffect, useState } from 'react';
import { useAuth } from '@/context/AuthContext';
import { pedidosService } from '../service/PedidosService';
import { OrderSummaryDTO } from '../types/pedido.types'; 

interface Props {
    titulo: string;
    estadosIds: number[]; 
    mensajeVacio: string;
    colorIcono: string;
}

export const PedidosCadeteTemplate: React.FC<Props> = ({ titulo, estadosIds, mensajeVacio, colorIcono }) => {
    const { user } = useAuth();
    const [pedidos, setPedidos] = useState<OrderSummaryDTO[]>([]);
    const [loading, setLoading] = useState(true);

    useEffect(() => {
        const cargar = async () => {
            if (!user?.id) return;
            try {
                // Traemos todos los pedidos DEL CADETE
                const data = await pedidosService.getFilteredOrders({ idUsuario: user.id });
                // Filtramos por los estados que esta página específica necesita
                const filtrados = data.filter(p => estadosIds.includes(p.idEstadoDePedido));
                setPedidos(filtrados);
            } catch (error) {
                console.error("Error:", error);
            } finally {
                setLoading(false);
            }
        };
        cargar();
    }, [user?.id, estadosIds]);

    if (loading) return <div className="p-8 text-center text-gray-500">Cargando...</div>;

    return (
        <div className="p-6 max-w-7xl mx-auto">
            <div className="flex items-center gap-3 mb-8">
                <div className={`w-2 h-8 rounded-full ${colorIcono}`}></div>
                <h1 className="text-3xl font-extrabold text-gray-800">{titulo}</h1>
            </div>

            <div className="bg-white rounded-2xl shadow-sm border border-gray-100 overflow-hidden">
                {pedidos.length === 0 ? (
                    <div className="p-16 text-center text-gray-400">
                        <p className="text-lg">{mensajeVacio}</p>
                    </div>
                ) : (
                    <div className="overflow-x-auto">
                        <table className="w-full">
                            <thead className="bg-gray-50/50 text-gray-500 text-xs uppercase tracking-wider">
                                <tr>
                                    <th className="px-6 py-4 text-left">Pedido</th>
                                    <th className="px-6 py-4 text-left">Cliente</th>
                                    <th className="px-6 py-4 text-left">Estado Actual</th>
                                    <th className="px-6 py-4 text-center">Acción</th>
                                </tr>
                            </thead>
                            <tbody className="divide-y divide-gray-100">
                                {pedidos.map((p) => (
                                    <tr key={p.idPedido} className="hover:bg-gray-50/80 transition-colors">
                                        <td className="px-6 py-4 font-bold text-blue-600">#{p.idPedido}</td>
                                        <td className="px-6 py-4 text-gray-600">{p.clienteNombre}</td>
                                        <td className="px-6 py-4">
                                            <span className={`px-3 py-1 rounded-full text-xs font-semibold ${
                                                p.idEstadoDePedido === 8 ? 'bg-orange-100 text-orange-700' : 'bg-blue-100 text-blue-700'
                                            }`}>
                                                {p.estadoNombre}
                                            </span>
                                        </td>
                                        <td className="px-6 py-4 text-center">
                                            <button className="bg-white border border-gray-200 text-gray-700 px-4 py-1.5 rounded-lg text-sm font-medium hover:bg-gray-50 shadow-sm transition-all">
                                                Gestionar
                                            </button>
                                        </td>
                                    </tr>
                                ))}
                            </tbody>
                        </table>
                    </div>
                )}
            </div>
        </div>
    );
};