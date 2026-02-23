import React, { useEffect, useState } from 'react';
import { BarChart, Bar, XAxis, YAxis, CartesianGrid, Tooltip, ResponsiveContainer } from 'recharts';
import { getRankingClientesFacturacion } from '../../service/reporteService';
import { ClienteFacturacionDTO } from '../../types/pedido.types';

export const ReporteFacturacion = () => {
    const [datos, setDatos] = useState<ClienteFacturacionDTO[]>([]);
    const [loading, setLoading] = useState(true);

    useEffect(() => {
        const cargarData = async () => {
            try {
                const res = await getRankingClientesFacturacion();
                setDatos(res);
            } catch (error) {
                console.error("Error al cargar facturación:", error);
            } finally {
                setLoading(false);
            }
        };
        cargarData();
    }, []);

    const totalGeneral = datos.reduce((acc, curr) => acc + curr.totalFacturado, 0);
    const promedioGeneral = datos.length > 0 ? totalGeneral / datos.length : 0;

    if (loading) return <div className="p-10 text-center text-gray-500 font-medium">Cargando reporte de facturación...</div>;

    return (
        <div className="space-y-8">
            {/* Encabezado: Botón sin símbolos como pediste */}
            <div className="flex justify-between items-center">
                <div>
                    <h1 className="text-xl font-bold text-gray-800">Reportes y Análisis</h1>
                    <p className="text-sm text-gray-500">Visualiza métricas y estadísticas de facturación por cliente</p>
                </div>
                <button className="bg-black text-white px-6 py-2.5 rounded-xl font-bold text-sm hover:bg-gray-800 transition-all shadow-md">
                    Exportar Reporte
                </button>
            </div>

            {/* Tarjetas de Resumen Ampliadas */}
            <div className="grid grid-cols-1 md:grid-cols-2 gap-6">
                {/* Ingresos Totales: Fondo blanco, bordes y texto verde */}
                <div className="bg-white p-6 rounded-2xl border border-green-200 shadow-sm">
                    <span className="text-green-600 text-sm font-semibold uppercase tracking-wider">Ingresos Totales (Top 10)</span>
                    <h3 className="text-3xl font-black text-green-700 mt-2">
                        {totalGeneral.toLocaleString('es-AR', { minimumFractionDigits: 2 })}
                    </h3>
                    <div className="text-xs text-green-500/60 mt-1 italic">Suma de pedidos entregados</div>
                </div>
                
                <div className="bg-white p-6 rounded-2xl border border-gray-100 shadow-sm">
                    <span className="text-blue-500 text-sm font-semibold uppercase tracking-wider">Gasto Promedio</span>
                    <h3 className="text-3xl font-black text-blue-600 mt-2">
                        {promedioGeneral.toLocaleString('es-AR', { minimumFractionDigits: 2 })}
                    </h3>
                    <div className="text-xs text-blue-300 mt-1 italic">Promedio por cliente en el top</div>
                </div>
            </div>

            {/* Gráfico de Barras Horizontal */}
            <div className="bg-white p-6 rounded-3xl border border-gray-100 shadow-sm">
                <h4 className="text-sm font-bold text-gray-400 mb-6 uppercase tracking-widest">Distribución de Facturación</h4>
                <div className="h-80 w-full">
                    <ResponsiveContainer width="100%" height="100%">
                        <BarChart
                            data={datos}
                            layout="vertical"
                            margin={{ top: 5, right: 30, left: 40, bottom: 5 }}
                        >
                            <CartesianGrid strokeDasharray="3 3" horizontal={false} stroke="#f3f4f6" />
                            <XAxis type="number" hide />
                            <YAxis 
                                dataKey="nombreCliente" 
                                type="category" 
                                width={120} 
                                tick={{ fontSize: 12, fill: '#9ca3af', fontWeight: 600 }}
                                axisLine={false}
                                tickLine={false}
                            />
                             <Tooltip 
                            cursor={{ fill: '#f9fafb' }}
                            contentStyle={{ borderRadius: '12px', border: 'none', boxShadow: '0 4px 6px -1px rgb(0 0 0 / 0.1)' }}
                            formatter={(value: number | undefined) => [
                              (value ?? 0).toLocaleString('es-AR'), 
                              'Total Facturado'
                            ]}
                        />
                            <Bar dataKey="totalFacturado" fill="#3b82f6" radius={[0, 8, 8, 0]} barSize={24} />
                        </BarChart>
                    </ResponsiveContainer>
                </div>
            </div>

            {/* Tabla de Ranking */}
            <div className="bg-white rounded-3xl overflow-hidden shadow-sm border border-gray-50">
                <table className="w-full text-left">
                    <thead>
                        <tr className="border-b border-gray-50">
                            <th className="py-5 px-4 font-bold text-gray-400 text-xs uppercase tracking-widest">Ranking</th>
                            <th className="py-5 px-4 font-bold text-gray-400 text-xs uppercase tracking-widest">Cliente</th>
                            <th className="py-5 px-4 font-bold text-gray-400 text-xs uppercase tracking-widest text-right">Total Facturado</th>
                            <th className="py-5 px-4 font-bold text-gray-400 text-xs uppercase tracking-widest text-right">Pedidos</th>
                        </tr>
                    </thead>
                    <tbody>
                        {datos.map((cliente, index) => (
                            <tr key={index} className="border-b border-gray-50 hover:bg-gray-50/50 transition-colors group">
                                <td className="py-5 px-4">
                                    <span className="bg-gray-100 text-gray-500 text-xs font-bold px-2.5 py-1 rounded-lg">
                                        #{index + 1}
                                    </span>
                                </td>
                                <td className="py-5 px-4 text-sm font-bold text-gray-800">{cliente.nombreCliente}</td>
                                <td className="py-5 px-4 text-sm font-black text-gray-900 text-right">
                                    {cliente.totalFacturado.toLocaleString('es-AR', { minimumFractionDigits: 2 })}
                                </td>
                                <td className="py-5 px-4 text-sm font-bold text-gray-500 text-right">
                                    {cliente.cantidadPedidos}
                                </td>
                            </tr>
                        ))}
                    </tbody>
                </table>
            </div>
        </div>
    );
};