import React, { useState, useEffect } from 'react';
import { Users, Calendar, MapPin, Package, TrendingUp, Download } from 'lucide-react';
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

    return (
        <div className="space-y-6">
            <div className="flex justify-between items-center mb-6">
                <div>
                    <h1 className="text-xl font-bold text-gray-800 flex items-center gap-2">
                        <Users size={22} className="text-blue-600" /> Ranking de Clientes Frecuentes
                    </h1>
                    <p className="text-sm text-gray-500">Top 10 clientes por volumen de pedidos</p>
                </div>
                <button className="bg-black text-white px-5 py-2.5 rounded-xl font-bold text-sm flex items-center gap-2 hover:bg-gray-800 transition-all">
                    <Download size={18} /> Exportar
                </button>
            </div>

            <div className="grid grid-cols-1 md:grid-cols-2 gap-4 mb-6">
                <div className="bg-white p-3 rounded-xl border border-gray-200 flex items-center justify-between">
                    <div className="flex items-center gap-2 text-gray-500 text-sm">
                        <Calendar size={18} /> <span>Periodo:</span>
                    </div>
                    <select value={periodo} onChange={(e) => setPeriodo(e.target.value)} className="text-sm font-bold outline-none">
                        <option value="7">Últimos 7 días</option>
                        <option value="30">Últimos 30 días</option>
                    </select>
                </div>
                <div className="bg-white p-3 rounded-xl border border-gray-200 flex items-center justify-between">
                    <div className="flex items-center gap-2 text-gray-500 text-sm">
                        <MapPin size={18} /> <span>Sucursal:</span>
                    </div>
                    <select value={sucursal} onChange={(e) => setSucursal(e.target.value)} className="text-sm font-bold outline-none">
                        <option value="todas">Todas las sucursales</option>
                    </select>
                </div>
            </div>

            <div className="bg-white rounded-2xl overflow-hidden border border-gray-100">
                <table className="w-full text-left">
                    <thead>
                        <tr className="bg-gray-50 text-gray-400 text-[11px] uppercase tracking-wider">
                            <th className="px-6 py-4 font-bold text-center">#</th>
                            <th className="px-6 py-4 font-bold">Cliente</th>
                            <th className="px-6 py-4 text-center font-bold">Pedidos</th>
                            <th className="px-6 py-4 text-right font-bold">Gasto Total</th>
                            <th className="px-6 py-4 text-right font-bold">Ticket Promedio</th>
                        </tr>
                    </thead>
                    <tbody className="divide-y divide-gray-50 text-sm">
                        {loading ? (
                            <tr><td colSpan={5} className="py-10 text-center text-gray-400">Cargando datos...</td></tr>
                        ) : ranking.map((item, index) => (
                            <tr key={index} className="hover:bg-blue-50/30 transition-all">
                                <td className="px-6 py-4 text-center font-bold text-gray-300">{index + 1}</td>
                                <td className="px-6 py-4 font-semibold text-gray-700">{item.nombreCliente}</td>
                                <td className="px-6 py-4 text-center">
                                    <span className="bg-gray-100 px-3 py-1 rounded-lg font-bold text-gray-600">{item.cantidadPedidos}</span>
                                </td>
                                <td className="px-6 py-4 text-right font-bold text-gray-800">${item.gastoTotal.toLocaleString()}</td>
                                <td className="px-6 py-4 text-right text-green-600 font-medium">${item.ticketPromedio.toLocaleString()}</td>
                            </tr>
                        ))}
                    </tbody>
                </table>
            </div>
        </div>
    );
};