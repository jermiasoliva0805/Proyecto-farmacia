import React, { useEffect, useState } from 'react';
import { PieChart, Pie, Cell, ResponsiveContainer, Legend, Tooltip } from 'recharts';
import { Calendar, MapPin, ShoppingCart, TrendingUp, Package } from 'lucide-react';
import { Card } from '../../components/common/Card';
import { getTop10Productos } from '../../service/reporteService';
import { TopProductosDTO } from '../../types/pedido.types';

const COLORS = [
    '#3b82f6', '#8b5cf6', '#ec4899', '#f59e0b', '#10b981',
    '#06b6d4', '#6366f1', '#f43f5e', '#14b8a6', '#f97316'
];

export const ReporteProductos = () => {
    const [datos, setDatos] = useState<TopProductosDTO[]>([]);
    const [loading, setLoading] = useState(true);
    const [periodo, setPeriodo] = useState("7");
    const [idSucursal, setIdSucursal] = useState<number | null>(null);
    const [activeIndex, setActiveIndex] = useState<number | null>(null);

    useEffect(() => {
        const cargarData = async () => {
            try {
                setLoading(true);
                const res = await getTop10Productos(periodo, idSucursal);
                setDatos(res);
            } catch (error) {
                console.error("Error al cargar productos:", error);
            } finally {
                setLoading(false);
            }
        };
        cargarData();
    }, [periodo, idSucursal]);

    const totalUnidades = datos.reduce((acc, curr) => acc + curr.unidadesVendidas, 0);
    const productoTop = datos.length > 0 ? datos[0] : null;

    if (loading) return <p className="p-6 text-gray-500 font-medium">Cargando reporte de productos...</p>;

    // Prepare data for charts
    const pieData = datos.map(p => ({
        name: p.nombreProducto,
        value: p.unidadesVendidas,
        fill: COLORS[datos.indexOf(p) % COLORS.length]
    }));

    return (
        <div className="p-6 bg-[#f8f9fa] min-h-screen">
            {/* Header */}
            <div className="flex justify-between items-center mb-6">
                <div>
                    <h1 className="text-xl font-bold text-gray-800 tracking-tight">Top 10 Productos Más Vendidos</h1>
                    <p className="text-sm text-gray-500">Análisis de productos con mayor volumen de ventas</p>
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

            {/* Métricas */}
            <div className="grid grid-cols-1 md:grid-cols-3 gap-4 mb-8">
                <MetricCard 
                    title="Total Unidades Vendidas" 
                    value={totalUnidades.toString()} 
                    sub="Suma del período seleccionado" 
                    icon={<Package className="text-blue-500" />} 
                    color="text-blue-600"
                />
                {productoTop && (
                    <>
                        <MetricCard 
                            title="Producto Top #1" 
                            value={productoTop.nombreProducto} 
                            sub={`${productoTop.unidadesVendidas} unidades vendidas`} 
                            icon={<ShoppingCart className="text-green-500" />} 
                            color="text-green-600"
                        />
                        <MetricCard 
                            title="Precio Promedio (Top 1)" 
                            value={`$${productoTop.precioPromedio.toLocaleString('es-AR')}`} 
                            sub={`${productoTop.porcentaje.toFixed(1)}% del total` }
                            icon={<TrendingUp className="text-orange-500" />} 
                            color="text-orange-600"
                        />
                    </>
                )}
            </div>

            {/* Gráficos */}
            <div className="grid grid-cols-1 gap-6 mb-8">
                {/* Gráfico de Pastel */}
                <Card className="p-6">
                    <h3 className="text-sm font-bold text-gray-700 mb-6 uppercase tracking-wider">Distribución Porcentual</h3>
                    <ResponsiveContainer width="100%" height={450}>
                        <PieChart>
                            <Pie
                                data={pieData}
                                cx="50%"
                                cy="50%"
                                outerRadius={80}
                                fill="#8884d8"
                                dataKey="value"
                                onMouseEnter={(_, index) => setActiveIndex(index)}
                                onMouseLeave={() => setActiveIndex(null)}
                                activeIndex={activeIndex}
                                activeShape={{
                                    outerRadius: 100,
                                    filter: 'drop-shadow(0 0 8px rgba(0,0,0,0.3))'
                                }}
                            >
                                {pieData.map((entry, index) => (
                                    <Cell 
                                        key={`cell-${index}`} 
                                        fill={entry.fill}
                                    />
                                ))}
                            </Pie>
                            <Tooltip 
                                formatter={(value) => `${value} unidades`}
                                labelFormatter={(label) => `Producto: ${label}`}
                                contentStyle={{ backgroundColor: '#fff', border: '1px solid #ccc', borderRadius: '8px' }}
                            />
                        </PieChart>
                    </ResponsiveContainer>
                </Card>
            </div>

            {/* Tabla de Detalles */}
            <Card className="p-6">
                <h3 className="text-sm font-bold text-gray-700 mb-6 uppercase tracking-wider">Detalles del Ranking</h3>
                <table className="w-full text-left text-sm">
                    <thead>
                        <tr className="text-gray-400 text-xs uppercase border-b border-gray-100">
                            <th className="pb-4 font-semibold w-12 text-center">Ranking</th>
                            <th className="pb-4 font-semibold px-4">Producto</th>
                            <th className="pb-4 font-semibold text-center px-4">Unidades</th>
                            <th className="pb-4 font-semibold text-center px-4">Porcentaje</th>
                            <th className="pb-4 font-semibold text-right px-4">Precio Promedio</th>
                        </tr>
                    </thead>
                    <tbody className="divide-y divide-gray-50">
                        {datos.map((producto, index) => (
                            <tr key={index} className="hover:bg-gray-50 transition-all">
                                <td className="py-4 text-center font-bold text-gray-300">{index + 1}</td>
                                <td className="py-4 px-4 font-semibold text-gray-700">{producto.nombreProducto}</td>
                                <td className="py-4 px-4 text-center font-medium text-gray-600">{producto.unidadesVendidas}</td>
                                <td className="py-4 px-4 text-center font-medium text-gray-600">{producto.porcentaje.toFixed(1)}%</td>
                                <td className="py-4 px-4 text-right font-bold text-gray-900">
                                    ${producto.precioPromedio.toLocaleString('es-AR')}
                                </td>
                            </tr>
                        ))}
                    </tbody>
                </table>
            </Card>
        </div>
    );
};

// Componente Selector
const Selector = ({ icon, label, value, options, onChange }: any) => (
    <div className="bg-white p-3 rounded-xl border border-gray-200 flex items-center justify-between shadow-sm">
        <div className="flex items-center gap-2 text-gray-500 text-sm font-medium">
            {icon} <span>{label}</span>
        </div>
        <select
            value={value}
            onChange={onChange}
            className="ml-2 px-2 py-1 border border-gray-300 rounded-md text-sm focus:ring-2 focus:ring-blue-500 outline-none transition-all cursor-pointer font-semibold text-gray-700"
        >
            {options.map((opt: any) => (
                <option key={opt.value} value={opt.value}>{opt.label}</option>
            ))}
        </select>
    </div>
);

// Componente MetricCard
const MetricCard = ({ title, value, sub, icon, color = "text-gray-900" }: any) => (
    <Card className="p-5 flex justify-between items-start border-gray-100 shadow-sm">
        <div>
            <p className="text-xs text-gray-500 mb-1 font-medium uppercase tracking-wider">{title}</p>
            <h4 className={`text-2xl font-bold ${color} truncate`}>{value}</h4>
            <p className="text-[10px] text-gray-400 mt-2 uppercase font-semibold italic">{sub}</p>
        </div>
        <div className="bg-gray-50 p-2 rounded-lg">{icon}</div>
    </Card>
);
