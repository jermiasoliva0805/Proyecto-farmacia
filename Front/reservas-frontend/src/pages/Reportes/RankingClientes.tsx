import React, { useState, useEffect } from 'react';
import { Card } from '../../components/common/Card';
import { Calendar, MapPin } from 'lucide-react';
import { getRankingClientes } from '../../service/reporteService';
import { RankingClienteDTO } from '../../types/pedido.types';

export const RankingClientes: React.FC = () => {
    const [ranking, setRanking] = useState<RankingClienteDTO[]>([]);
    const [loading, setLoading] = useState(true);
    const [periodo, setPeriodo] = useState("7");
    const [sucursal, setSucursal] = useState("todas");

    useEffect(() => {
        const fetchDatos = async () => {
            try {
                setLoading(true);
                const data = await getRankingClientes();
                setRanking(data);
            } catch (error) {
                console.error("Error al cargar ranking:", error);
            } finally {
                setLoading(false);
            }
        };
        fetchDatos();
    }, [periodo, sucursal]);

    if (loading) return <p className="p-6 text-gray-500 font-medium">Cargando reporte...</p>;

    return (
        <div className="p-6 bg-[#f8f9fa] min-h-screen">
            {/* Header */}
            <div className="flex justify-between items-center mb-6">
                <div>
                    <h1 className="text-xl font-bold text-gray-800">Ranking de Clientes Frecuentes</h1>
                    <p className="text-sm text-gray-500">Top 10 clientes por volumen de pedidos</p>
                </div>
                <button className="bg-black text-white px-5 py-2.5 rounded-xl font-bold text-sm hover:bg-gray-800 transition-all shadow-md">
                    Exportar Reporte
                </button>
            </div>

            {/* Selectores - AHORA IGUALES AL DE ENTREGAS */}
            <div className="grid grid-cols-1 md:grid-cols-2 gap-4 mb-6">
                <Selector
                    icon={<Calendar size={18} />}
                    label="Periodo:"
                    value={periodo}
                    options={[
                        { value: "7", label: "Últimos 7 días" },
                        { value: "30", label: "Últimos 30 días" },
                        { value: "90", label: "Últimos 90 días" },
                    ]}
                    onChange={(e: React.ChangeEvent<HTMLSelectElement>) => setPeriodo(e.target.value)}
                />
                <Selector
                    icon={<MapPin size={18} />}
                    label="Sucursal:"
                    value={sucursal}
                    options={[
                        { value: "todas", label: "Todas las sucursales" },
                        { value: "centro", label: "Sucursal Centro" },
                        { value: "norte", label: "Sucursal Norte" },
                    ]}
                    onChange={(e: React.ChangeEvent<HTMLSelectElement>) => setSucursal(e.target.value)}
                />
            </div>

            {/* Tabla */}
            <Card className="p-6">
                <h3 className="text-sm font-bold text-gray-700 mb-6">Top Clientes</h3>
                <table className="w-full text-left">
                    <thead>
                        <tr className="text-gray-400 text-xs uppercase border-b border-gray-100">
                            <th className="pb-4 font-semibold text-center w-12">Ranking</th>
                            <th className="pb-4 font-semibold">Cliente</th>
                            <th className="pb-4 font-semibold text-center">Pedidos</th>
                            <th className="pb-4 font-semibold text-right">Gasto Total</th>
                            <th className="pb-4 font-semibold text-right text-green-600">Ticket Prom.</th>
                        </tr>
                    </thead>
                    <tbody className="divide-y divide-gray-50 text-sm">
                        {ranking.map((item, index) => (
                            <tr key={index} className="hover:bg-gray-50 transition-all">
                                <td className="py-4 text-center font-bold text-gray-300">{index + 1}</td>
                                <td className="py-4 font-medium text-gray-700">{item.nombreCliente}</td>
                                <td className="py-4 text-center">{item.cantidadPedidos}</td>
                                <td className="py-4 text-right font-bold text-gray-800">
                                    ${item.gastoTotal.toLocaleString()}
                                </td>
                                <td className="py-4 text-right font-bold text-green-600">
                                    ${item.ticketPromedio.toLocaleString()}
                                </td>
                            </tr>
                        ))}
                    </tbody>
                </table>
            </Card>
        </div>
    );
};

// Componente Selector (MODIFICADO PARA SER IGUAL AL DE ENTREGAS)
const Selector = ({ icon, label, value, options, onChange }: any) => (
    <div className="bg-white p-3 rounded-xl border border-gray-200 flex items-center justify-between">
        <div className="flex items-center gap-2 text-gray-500 text-sm">
            {icon} <span>{label}</span>
        </div>
        <select
            value={value}
            onChange={onChange}
            className="ml-2 px-2 py-1 border border-gray-300 rounded-md text-sm focus:ring-2 focus:ring-blue-500 outline-none transition-all"
        >
            {options.map((opt: any) => (
                <option key={opt.value} value={opt.value}>
                    {opt.label}
                </option>
            ))}
        </select>
    </div>
);