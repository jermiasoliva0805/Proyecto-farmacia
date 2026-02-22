import React, { useState, useEffect, useCallback } from 'react';
import { Calendar, MapPin, User, TrendingUp, Package, Zap, ClipboardList, Clock, DollarSign } from 'lucide-react';
import { BarChart, Bar, XAxis, YAxis, CartesianGrid, Tooltip, ResponsiveContainer, Cell } from 'recharts';

interface ReporteOperario {
    nombreOperario: string;
    pedidosTotales: number;
    dentroUmbral: number;
    fueraUmbral: number;
    tiempoPromedioMinutos: number;
    porcentajeEficiencia: number;
}

const ReporteOperarios = () => {
    const [datos, setDatos] = useState<ReporteOperario[]>([]);
    const [loading, setLoading] = useState(true);
    const [periodo, setPeriodo] = useState('7');
    const [sucursal, setSucursal] = useState('');

    const fetchDatos = useCallback(async () => {
        setLoading(true);
        try {
            const params = new URLSearchParams({ dias: periodo, idSucursal: sucursal });
            const response = await fetch(`http://localhost:5000/api/Orders/reporte-tiempos-operarios?${params}`, {
                headers: { 'Authorization': `Bearer ${localStorage.getItem('token')}` }
            });
            if (response.ok) {
                const data = await response.json();
                setDatos(data);
            }
        } catch (error) {
            console.error("Error:", error);
        } finally {
            setLoading(false);
        }
    }, [periodo, sucursal]);

    useEffect(() => { fetchDatos(); }, [fetchDatos]);

    const totalPedidos = datos.reduce((acc, curr) => acc + curr.pedidosTotales, 0);
    const totalDentro = datos.reduce((acc, curr) => acc + curr.dentroUmbral, 0);
    const totalFuera = datos.reduce((acc, curr) => acc + curr.fueraUmbral, 0);
    const eficienciaGral = totalPedidos > 0 ? Math.round((totalDentro / totalPedidos) * 100) : 0;
    const tiempoGralPromedio = datos.length > 0 ? Math.round(datos.reduce((acc, curr) => acc + curr.tiempoPromedioMinutos, 0) / datos.length) : 0;

    const getEficienciaColor = (porcentaje: number) => {
        if (porcentaje >= 90) return 'text-green-600 bg-green-50';
        if (porcentaje >= 75) return 'text-orange-500 bg-orange-50';
        return 'text-red-600 bg-red-50';
    };

    return (
        <div className="p-6 bg-gray-50 min-h-screen">
            {/* ENCABEZADO CON BOTÓN EXPORTAR */}
            <div className="flex justify-between items-start mb-6">
                <div>
                    <h1 className="text-2xl font-black text-gray-800 tracking-tight">Desempeño de Operarios</h1>
                    <p className="text-gray-500 font-medium text-sm">Análisis de eficiencia y tiempos de preparación (Umbral: 30 min)</p>
                </div>
                <button className="bg-black text-white px-5 py-2.5 rounded-xl font-bold text-sm flex items-center gap-2 hover:bg-gray-800 transition-all shadow-md">
                
                    Exportar Reporte
                </button>
            </div>

            {/* FILTROS (SUCURSAL Y FECHA) */}
            <div className="flex flex-col md:flex-row gap-4 mb-8">
                <div className="flex-1 flex items-center gap-3 bg-white px-4 py-3 rounded-2xl border border-gray-200 shadow-sm group">
                    <MapPin size={20} className="text-gray-400 group-hover:text-purple-500 transition-colors" />
                    <span className="text-gray-500 font-medium text-sm">Sucursal:</span>
                    <select 
                        value={sucursal} 
                        onChange={(e) => setSucursal(e.target.value)} 
                        className="flex-1 bg-transparent font-bold text-sm outline-none appearance-none cursor-pointer"
                    >
                        <option value="">Todas las sucursales</option>
                        <option value="1">Sucursal Centro</option>
                        <option value="2">Sucursal Norte</option>
                    </select>
                </div>

                <div className="flex items-center gap-3 bg-white px-6 py-3 rounded-2xl border border-gray-200 shadow-sm group">
                    <Calendar size={20} className="text-gray-400 group-hover:text-blue-500 transition-colors" />
                    <select 
                        value={periodo} 
                        onChange={(e) => setPeriodo(e.target.value)} 
                        className="bg-transparent font-bold text-sm outline-none cursor-pointer"
                    >
                        <option value="7">Últimos 7 días</option>
                        <option value="30">Últimos 30 días</option>
                    </select>
                </div>
            </div>

            {/* KPIs */}
            <div className="grid grid-cols-1 md:grid-cols-4 gap-6 mb-8">
                <KPICard title="Operarios Activos" value={datos.length} icon={User} color="blue" />
                <KPICard title="Total Pedidos" value={totalPedidos} icon={Package} color="purple" />
                <KPICard title="En Umbral" value={totalDentro} icon={TrendingUp} color="green" />
                <KPICard title="Eficiencia Gral." value={`${eficienciaGral}%`} icon={Zap} color="orange" />
            </div>

            {/* GRÁFICO */}
            <div className="bg-white p-6 rounded-3xl shadow-sm border border-gray-100 mb-8">
                <h3 className="font-bold text-gray-700 mb-6 flex items-center gap-2 tracking-tight">
                    <TrendingUp size={20} className="text-green-500" />
                    % de Pedidos dentro del Umbral por Operario
                </h3>
                <div className="h-[350px] w-full">
                    {!loading && datos.length > 0 ? (
                        <ResponsiveContainer width="99%" height="100%">
                            <BarChart data={datos} layout="vertical" margin={{ left: 40, right: 40 }}>
                                <CartesianGrid strokeDasharray="3 3" horizontal={false} stroke="#f0f0f0" />
                                <XAxis type="number" domain={[0, 100]} hide />
                                <YAxis dataKey="nombreOperario" type="category" axisLine={false} tickLine={false} width={120} style={{ fontWeight: '600', fontSize: '12px' }} />
                                <Tooltip cursor={{ fill: '#f8fafc' }} contentStyle={{ borderRadius: '15px', border: 'none' }} />
                                <Bar dataKey="porcentajeEficiencia" radius={[0, 10, 10, 0]} barSize={35}>
                                    {datos.map((entry, index) => (
                                        <Cell key={index} fill={entry.porcentajeEficiencia >= 90 ? '#22c55e' : entry.porcentajeEficiencia >= 75 ? '#f97316' : '#ef4444'} />
                                    ))}
                                </Bar>
                            </BarChart>
                        </ResponsiveContainer>
                    ) : <div className="flex h-full items-center justify-center text-gray-400 font-bold uppercase text-xs tracking-widest">Cargando gráfico...</div>}
                </div>
            </div>

            {/* TABLA DE SOPORTE */}
            <div className="bg-white rounded-3xl shadow-sm border border-gray-100 overflow-hidden">
                <div className="p-6 border-b border-gray-50 flex items-center gap-2">
                    <ClipboardList className="text-blue-500" size={20} />
                    <h3 className="font-bold text-gray-800 tracking-tight">Tabla de Soporte</h3>
                </div>
                <div className="overflow-x-auto">
                    <table className="w-full text-left">
                        <thead className="bg-gray-50/50 text-gray-400 text-[10px] uppercase tracking-widest font-bold">
                            <tr>
                                <th className="px-6 py-4">Operario</th>
                                <th className="px-6 py-4 text-center">Pedidos Totales</th>
                                <th className="px-6 py-4 text-center">Dentro Umbral</th>
                                <th className="px-6 py-4 text-center">Fuera Umbral</th>
                                <th className="px-6 py-4 text-center">Tiempo Promedio</th>
                                <th className="px-6 py-4 text-center">Eficiencia</th>
                            </tr>
                        </thead>
                        <tbody className="divide-y divide-gray-50">
                            {datos.map((op, idx) => (
                                <tr key={idx} className="hover:bg-gray-50/30 transition-colors">
                                    <td className="px-6 py-4 font-bold text-gray-700 text-sm">{op.nombreOperario}</td>
                                    <td className="px-6 py-4 text-center text-gray-600 font-medium">{op.pedidosTotales}</td>
                                    <td className="px-6 py-4 text-center text-green-600 font-bold">{op.dentroUmbral}</td>
                                    <td className="px-6 py-4 text-center text-red-500 font-bold">{op.fueraUmbral}</td>
                                    <td className="px-6 py-4 text-center text-gray-500 text-xs italic">
                                        <div className="flex items-center justify-center gap-1">
                                            <Clock size={12} /> {op.tiempoPromedioMinutos} min
                                        </div>
                                    </td>
                                    <td className="px-6 py-4 text-center font-bold">
                                        <span className={`px-3 py-1 rounded-lg text-[11px] ${getEficienciaColor(op.porcentajeEficiencia)}`}>
                                            {op.porcentajeEficiencia}%
                                        </span>
                                    </td>
                                </tr>
                            ))}
                            <tr className="bg-gray-800 text-white font-bold">
                                <td className="px-6 py-5 rounded-bl-3xl uppercase tracking-tighter">Total / Promedio Gral.</td>
                                <td className="px-6 py-5 text-center">{totalPedidos}</td>
                                <td className="px-6 py-5 text-center text-green-400">{totalDentro}</td>
                                <td className="px-6 py-5 text-center text-red-400">{totalFuera}</td>
                                <td className="px-6 py-5 text-center text-gray-300 font-medium">{tiempoGralPromedio} min</td>
                                <td className="px-6 py-5 text-center rounded-br-3xl">
                                    <span className="bg-white/20 px-3 py-1 rounded-lg">
                                        {eficienciaGral}%
                                    </span>
                                </td>
                            </tr>
                        </tbody>
                    </table>
                </div>
            </div>
        </div>
    );
};

const KPICard = ({ title, value, icon: Icon, color }: { title: string, value: string | number, icon: any, color: string }) => {
    const colors: any = {
        blue: 'bg-blue-50 text-blue-600',
        purple: 'bg-purple-50 text-purple-600',
        green: 'bg-green-50 text-green-600',
        orange: 'bg-orange-50 text-orange-600'
    };
    return (
        <div className="bg-white p-6 rounded-3xl shadow-sm border border-gray-100 flex items-center gap-5">
            <div className={`p-4 rounded-2xl ${colors[color]}`}>
                <Icon size={24} />
            </div>
            <div>
                <p className="text-gray-400 text-[10px] font-bold uppercase tracking-widest">{title}</p>
                <p className="text-2xl font-black text-gray-800">{value}</p>
            </div>
        </div>
    );
};

export default ReporteOperarios;