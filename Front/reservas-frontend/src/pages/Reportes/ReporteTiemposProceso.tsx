import React, { useState, useEffect } from 'react';
import { Card } from '../../components/common/Card';
import { Calendar, MapPin, AlertCircle, Zap } from 'lucide-react';
import { BarChart, Bar, XAxis, YAxis, CartesianGrid, Tooltip, Legend, ResponsiveContainer } from 'recharts';
import { getReporteTiempos } from '../../service/reporteService';
import { TiemposProcesoDTO, DetalleTiempoProcesoDTO } from '../../types/pedido.types';

export const ReporteTiemposProceso: React.FC = () => {
    const [tiempos, setTiempos] = useState<TiemposProcesoDTO | null>(null);
    const [loading, setLoading] = useState(true);
    const [periodo, setPeriodo] = useState("7");
    const [idSucursal, setIdSucursal] = useState<number | null>(null);

    useEffect(() => {
        const fetchDatos = async () => {
            try {
                setLoading(true);
                const data = await getReporteTiempos(periodo, idSucursal);
                setTiempos(data);
            } catch (error) {
                console.error("Error al cargar tiempos:", error);
            } finally {
                setLoading(false);
            }
        };
        fetchDatos();
    }, [periodo, idSucursal]);

    if (loading) return <p className="p-6 text-gray-500 font-medium">Cargando reporte...</p>;
    if (!tiempos) return <p className="p-6 text-gray-500 font-medium">Sin datos disponibles</p>;

    // Preparar datos para el gráfico
    const dataGrafico = tiempos.fases.map(fase => ({
        nombre: fase.nombre,
        minutos: Math.round(fase.tiempoPromedio),
        fill: fase.color
    }));

    return (
        <div className="p-6 bg-[#f8f9fa] min-h-screen">
            {/* Header */}
            <div className="flex justify-between items-center mb-6">
                <div>
                    <h1 className="text-xl font-bold text-gray-800">Analítica de Tiempos de Proceso</h1>
                    <p className="text-sm text-gray-500">Lead Time y cuellos de botella en operaciones</p>
                </div>
                <button className="bg-black text-white px-5 py-2.5 rounded-xl font-bold text-sm hover:bg-gray-800 transition-all shadow-md">
                    Exportar Reporte
                </button>
            </div>

            {/* Selectores */}
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
                    value={idSucursal !== null ? idSucursal.toString() : ""}
                    options={[
                        { value: "", label: "Todas las sucursales" },
                        { value: "1", label: "Sucursal Centro" },
                        { value: "2", label: "Sucursal Norte" },
                    ]}
                    onChange={(e: React.ChangeEvent<HTMLSelectElement>) => setIdSucursal(e.target.value === "" ? null : parseInt(e.target.value))}
                />
            </div>

            {/* Gráfico de Barras Horizontal */}
            <Card className="p-6 mb-6">
                <h3 className="text-sm font-bold text-gray-700 mb-6">Duración Promedio por Fase</h3>
                <ResponsiveContainer width="100%" height={300}>
                    <BarChart data={dataGrafico} layout="vertical">
                        <CartesianGrid strokeDasharray="3 3" />
                        <XAxis type="number" />
                        <YAxis dataKey="nombre" type="category" width={150} />
                        <Tooltip formatter={(value) => `${value} min`} />
                        <Bar dataKey="minutos" fill="#8b5cf6" radius={8} />
                    </BarChart>
                </ResponsiveContainer>
            </Card>

            {/* Widgets Indicadores */}
            <div className="grid grid-cols-1 md:grid-cols-2 gap-6 mb-6">
                {/* Punto Crítico */}
                <Card className="p-6 bg-gradient-to-br from-red-50 to-red-100 border border-red-200">
                    <div className="flex items-start justify-between">
                        <div>
                            <p className="text-xs uppercase text-red-600 font-semibold">Punto Crítico</p>
                            <h4 className="text-lg font-bold text-red-900 mt-2">{tiempos.puntoCritico}</h4>
                            <p className="text-sm text-red-700 mt-1">{Math.round(tiempos.tiempoPuntoCritico)} min promedio</p>
                        </div>
                        <AlertCircle size={32} className="text-red-500" />
                    </div>
                </Card>

                {/* Eficiencia de Despacho */}
                <Card className="p-6 bg-gradient-to-br from-green-50 to-green-100 border border-green-200">
                    <div className="flex items-start justify-between">
                        <div>
                            <p className="text-xs uppercase text-green-600 font-semibold">Eficiencia de Despacho</p>
                            <h4 className="text-lg font-bold text-green-900 mt-2">{tiempos.eficienciaDespacho}%</h4>
                            <p className="text-sm text-green-700 mt-1">Pedidos &lt;30 min en logística</p>
                        </div>
                        <Zap size={32} className="text-green-500" />
                    </div>
                </Card>
            </div>

            {/* Tabla de Auditoría Detallada */}
            <Card className="p-6">
                <h3 className="text-sm font-bold text-gray-700 mb-6">Detalles de Tiempos por Pedido</h3>
                <div className="overflow-x-auto">
                    <table className="w-full text-left text-sm">
                        <thead>
                            <tr className="text-gray-400 text-xs uppercase border-b border-gray-100">
                                <th className="pb-4 font-semibold">ID Pedido</th>
                                <th className="pb-4 font-semibold text-center">Espera (min)</th>
                                <th className="pb-4 font-semibold text-center">Preparación (min)</th>
                                <th className="pb-4 font-semibold text-center">Despacho (min)</th>
                                <th className="pb-4 font-semibold text-center">Viaje (min)</th>
                                <th className="pb-4 font-semibold">Estado Final</th>
                            </tr>
                        </thead>
                        <tbody className="divide-y divide-gray-50">
                            {tiempos.detalles.map((detalle, index) => (
                                <tr 
                                    key={index} 
                                    className={`hover:bg-gray-50 transition-all ${detalle.esAlertaDespacho ? 'bg-red-50' : ''}`}
                                >
                                    <td className="py-4 font-medium text-gray-700">#{detalle.idPedido}</td>
                                    <td className="py-4 text-center">{Math.round(detalle.espera)}</td>
                                    <td className="py-4 text-center">{Math.round(detalle.preparacion)}</td>
                                    <td className={`py-4 text-center font-bold ${detalle.esAlertaDespacho ? 'text-red-600' : ''}`}>
                                        {Math.round(detalle.despacho)}
                                    </td>
                                    <td className="py-4 text-center">{Math.round(detalle.viaje)}</td>
                                    <td className="py-4">
                                        <span className="px-3 py-1 rounded-full text-xs font-semibold bg-blue-100 text-blue-700">
                                            {detalle.estadoFinal}
                                        </span>
                                    </td>
                                </tr>
                            ))}
                        </tbody>
                    </table>
                </div>
            </Card>

            {/* Estadísticas Generales */}
            <div className="mt-6 p-4 bg-white rounded-xl border border-gray-200">
                <p className="text-sm text-gray-600">
                    <span className="font-semibold">Total de pedidos analizados:</span> {tiempos.totalPedidos}
                </p>
            </div>
        </div>
    );
};

// Componente Selector
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
