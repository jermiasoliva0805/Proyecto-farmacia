import React, { useEffect, useState, useRef } from 'react';
import { ResponsiveContainer, Tooltip, BarChart, Bar, XAxis, YAxis, CartesianGrid } from 'recharts';
import { Calendar, MapPin, Package, TrendingUp, Download } from 'lucide-react';
import { Card } from '../../components/common/Card';
import { getPedidosPorZona } from '../../service/reporteService';
import { PedidosPorZonaDTO } from '../../types/pedido.types';
import { exportToExcel, exportToPDF } from '../../service/exportService';
import { ExportDialog } from '../../components/ExportDialog';

<<<<<<< HEAD
interface ZonaDTO {
    id: number;
    nombre: string;
}

=======
>>>>>>> 7b1011c84020762d4fa19d19571d6e6256869dbd
const COLORS = [
    '#3b82f6', '#8b5cf6', '#ec4899', '#f59e0b', '#10b981',
    '#06b6d4', '#6366f1', '#f43f5e', '#14b8a6', '#f97316'
];

export const ReportePedidosPorZona = () => {
    const [datos, setDatos] = useState<PedidosPorZonaDTO[]>([]);
<<<<<<< HEAD
    const [zonas, setZonas] = useState<ZonaDTO[]>([]);
    const [loading, setLoading] = useState(true);
    const [fechaDesde, setFechaDesde] = useState<string>('');
    const [fechaHasta, setFechaHasta] = useState<string>('');
    const [idZona, setIdZona] = useState<number | null>(null);
=======
    const [loading, setLoading] = useState(true);
    const [fechaDesde, setFechaDesde] = useState<string>('');
    const [fechaHasta, setFechaHasta] = useState<string>('');
    const [idSucursal, setIdSucursal] = useState<number | null>(null);
>>>>>>> 7b1011c84020762d4fa19d19571d6e6256869dbd

    const [showExportDialog, setShowExportDialog] = useState(false);
    const [isExporting, setIsExporting] = useState(false);
    
    const contentRef = useRef<HTMLDivElement>(null);

    // Inicializar fechas por defecto (últimos 30 días)
    useEffect(() => {
        const hoy = new Date();
        const hace30 = new Date(hoy);
        hace30.setDate(hoy.getDate() - 30);
        
        setFechaDesde(hace30.toISOString().split('T')[0]);
        setFechaHasta(hoy.toISOString().split('T')[0]);
<<<<<<< HEAD

        // Cargar zonas
        const cargarZonas = async () => {
            try {
                const response = await fetch('/api/localidades/zonas');
                const data = await response.json();
                setZonas(data || []);
            } catch (error) {
                console.error('Error al cargar zonas:', error);
            }
        };
        
        cargarZonas();
=======
>>>>>>> 7b1011c84020762d4fa19d19571d6e6256869dbd
    }, []);

    useEffect(() => {
        const cargarData = async () => {
            if (!fechaDesde || !fechaHasta) return;
            
            try {
                setLoading(true);
<<<<<<< HEAD
                const res = await getPedidosPorZona(fechaDesde, fechaHasta, idZona);
=======
                const res = await getPedidosPorZona(fechaDesde, fechaHasta, idSucursal);
>>>>>>> 7b1011c84020762d4fa19d19571d6e6256869dbd
                setDatos(res || []);
            } catch (error) {
                console.error("Error al cargar pedidos por zona:", error);
            } finally {
                setLoading(false);
            }
        };
        cargarData();
<<<<<<< HEAD
    }, [fechaDesde, fechaHasta, idZona]);
=======
    }, [fechaDesde, fechaHasta, idSucursal]);
>>>>>>> 7b1011c84020762d4fa19d19571d6e6256869dbd

    const handleExportExcel = async () => {
        try {
            setIsExporting(true);
            if (!datos || datos.length === 0) {
                alert('No hay datos para exportar');
                return;
            }
            const dataExport = datos.map((z, i) => ({
                'Posición': i + 1,
                'Zona': z.nombreZona,
                'Cantidad de Pedidos': z.cantidadPedidos,
                'Porcentaje': `${z.porcentaje.toFixed(2)}%`,
                'Total Recaudado': `$${z.totalRecaudado.toLocaleString('es-AR')}`,
            }));
            
            exportToExcel(dataExport, {
                reportName: 'Pedidos por Zona',
                fileName: 'pedidos-por-zona',
                filters: { 
                    desde: fechaDesde,
                    hasta: fechaHasta,
<<<<<<< HEAD
                    zona: idZona === null ? "Todas" : zonas.find(z => z.id === idZona)?.nombre || "N/A"
=======
                    sucursal: idSucursal === null ? "Todas" : idSucursal === 1 ? "Centro" : "Norte"
>>>>>>> 7b1011c84020762d4fa19d19571d6e6256869dbd
                }
            });
        } finally {
            setIsExporting(false);
            setShowExportDialog(false);
        }
    };

    const handleExportPDF = async () => {
        try {
            setIsExporting(true);
            if (!contentRef.current) {
                alert('No hay contenido');
                return;
            }
            
            await exportToPDF(contentRef.current, {
                reportName: 'Pedidos por Zona',
                fileName: 'pedidos-por-zona',
                filters: { 
                    desde: fechaDesde,
                    hasta: fechaHasta,
<<<<<<< HEAD
                    zona: idZona === null ? "Todas" : zonas.find(z => z.id === idZona)?.nombre || "N/A"
=======
                    sucursal: idSucursal === null ? "Todas" : idSucursal === 1 ? "Centro" : "Norte"
>>>>>>> 7b1011c84020762d4fa19d19571d6e6256869dbd
                }
            });
        } finally {
            setIsExporting(false);
            setShowExportDialog(false);
        }
    };

    const handleExportClick = () => {
        setShowExportDialog(true);
    };

    const totalPedidos = datos.reduce((acc, curr) => acc + curr.cantidadPedidos, 0);
    const totalRecaudado = datos.reduce((acc, curr) => acc + curr.totalRecaudado, 0);
    const zonaTop = datos.length > 0 ? datos[0] : null;

    if (loading) return <p className="p-6 text-gray-500 font-medium">Cargando reporte de pedidos por zona...</p>;

    // Prepare data for charts
    const pieData = datos.map(z => ({
        name: z.nombreZona,
        value: z.cantidadPedidos,
        fill: COLORS[datos.indexOf(z) % COLORS.length]
    }));

    const barData = datos.map(z => ({
        nombre: z.nombreZona,
        pedidos: z.cantidadPedidos,
        recaudado: Math.round(z.totalRecaudado)
    }));

    return (
        <div className="p-6 bg-[#f8f9fa] min-h-screen">
            {/* Header */}
            <div className="flex justify-between items-center mb-6">
                <div>
                    <h1 className="text-xl font-bold text-gray-800 tracking-tight">Pedidos por Zona</h1>
                    <p className="text-sm text-gray-500">Análisis de distribución de pedidos según zonas de cobertura</p>
                </div>
                <button 
                    onClick={handleExportClick}
                    className="bg-gray-700 hover:bg-gray-800 text-white px-5 py-2.5 rounded-xl font-bold text-sm flex items-center gap-2 transition-all shadow-md"
                >
                    <Download size={16} />
                    Exportar Reporte
                </button>
            </div>

            {/* Contenido a exportar */}
            <div ref={contentRef}>
                {/* Selectores */}
                <div className="grid grid-cols-1 md:grid-cols-3 gap-4 mb-6">
                    <Selector
                        icon={<Calendar size={18} />}
                        label="Desde:"
                        type="date"
                        value={fechaDesde}
                        onChange={(e: React.ChangeEvent<HTMLInputElement>) => setFechaDesde(e.target.value)}
                    />
                    <Selector
                        icon={<Calendar size={18} />}
                        label="Hasta:"
                        type="date"
                        value={fechaHasta}
                        onChange={(e: React.ChangeEvent<HTMLInputElement>) => setFechaHasta(e.target.value)}
                    />
                    <Selector
                        icon={<MapPin size={18} />}
<<<<<<< HEAD
                        label="Zona:"
                        type="select"
                        value={idZona !== null ? idZona.toString() : ""}
                        options={[
                            { value: "", label: "Todas las zonas" },
                            ...zonas.map(z => ({ value: z.id.toString(), label: z.nombre }))
                        ]}
                        onChange={(e: React.ChangeEvent<HTMLSelectElement>) => setIdZona(e.target.value === "" ? null : parseInt(e.target.value))}
=======
                        label="Sucursal:"
                        type="select"
                        value={idSucursal !== null ? idSucursal.toString() : ""}
                        options={[
                            { value: "", label: "Todas las sucursales" },
                            { value: "1", label: "Sucursal Centro" },
                            { value: "2", label: "Sucursal Norte" },
                        ]}
                        onChange={(e: React.ChangeEvent<HTMLSelectElement>) => setIdSucursal(e.target.value === "" ? null : parseInt(e.target.value))}
>>>>>>> 7b1011c84020762d4fa19d19571d6e6256869dbd
                    />
                </div>

                {/* Métricas */}
                <div className="grid grid-cols-1 md:grid-cols-3 gap-4 mb-8">
                    <MetricCard 
                        title="Total Pedidos" 
                        value={totalPedidos.toString()} 
                        sub="En el período seleccionado" 
                        icon={<Package className="text-blue-500" />} 
                        color="text-blue-600"
                    />
                    {zonaTop && (
                        <>
                            <MetricCard 
                                title="Zona Principal" 
                                value={zonaTop.nombreZona} 
                                sub={`${zonaTop.cantidadPedidos} pedidos (${zonaTop.porcentaje.toFixed(1)}%)`} 
                                icon={<MapPin className="text-green-500" />} 
                                color="text-green-600"
                            />
                            <MetricCard 
                                title="Total Recaudado" 
                                value={`$${totalRecaudado.toLocaleString('es-AR')}`} 
                                sub={`Promedio por zona: $${(totalRecaudado / datos.length).toLocaleString('es-AR')}`}
                                icon={<TrendingUp className="text-orange-500" />} 
                                color="text-orange-600"
                            />
                        </>
                    )}
                </div>

                {/* Gráficos */}
<<<<<<< HEAD
                <div className="mb-8">
                    {/* Gráfico de Barras Horizontal - Pedidos */}
                    <Card className="p-6">
                        <h3 className="text-sm font-bold text-gray-700 mb-6 uppercase tracking-wider">Pedidos por Zona</h3>
=======
                <div className="grid grid-cols-1 lg:grid-cols-2 gap-6 mb-8">
                    {/* Gráfico de Barras Horizontal - Pedidos */}
                    <Card className="p-6">
                        <h3 className="text-sm font-bold text-gray-700 mb-6 uppercase tracking-wider">Pedidos por Zona (Top 10)</h3>
>>>>>>> 7b1011c84020762d4fa19d19571d6e6256869dbd
                        <ResponsiveContainer width="100%" height={300}>
                            <BarChart data={barData} layout="vertical" margin={{ left: 120, right: 20 }}>
                                <CartesianGrid strokeDasharray="3 3" stroke="#e5e7eb" />
                                <XAxis type="number" tick={{ fontSize: 12 }} />
                                <YAxis dataKey="nombre" type="category" tick={{ fontSize: 12 }} />
                                <Tooltip 
                                    contentStyle={{ backgroundColor: '#fff', border: '1px solid #ccc', borderRadius: '8px' }}
                                    formatter={(value: any) => value.toLocaleString('es-AR')}
                                />
                                <Bar dataKey="pedidos" fill="#3b82f6" name="Cantidad de Pedidos" />
                            </BarChart>
                        </ResponsiveContainer>
                    </Card>
<<<<<<< HEAD
=======

                    {/* Gráfico de Barras Horizontal - Ingresos */}
                    <Card className="p-6">
                        <h3 className="text-sm font-bold text-gray-700 mb-6 uppercase tracking-wider">Ingresos por Zona (Top 10)</h3>
                        <ResponsiveContainer width="100%" height={300}>
                            <BarChart data={barData} layout="vertical" margin={{ left: 120, right: 20 }}>
                                <CartesianGrid strokeDasharray="3 3" stroke="#e5e7eb" />
                                <XAxis type="number" tick={{ fontSize: 12 }} />
                                <YAxis dataKey="nombre" type="category" tick={{ fontSize: 12 }} />
                                <Tooltip 
                                    contentStyle={{ backgroundColor: '#fff', border: '1px solid #ccc', borderRadius: '8px' }}
                                    formatter={(value: any) => value.toLocaleString('es-AR')}
                                />
                                <Bar dataKey="recaudado" fill="#10b981" name="Total Recaudado" />
                            </BarChart>
                        </ResponsiveContainer>
                    </Card>
>>>>>>> 7b1011c84020762d4fa19d19571d6e6256869dbd
                </div>

                {/* Leyenda de Zonas */}
                <Card className="p-6 mb-8">
                    <h3 className="text-sm font-bold text-gray-700 mb-4 uppercase tracking-wider">Resumen de Zonas</h3>
                    <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-3 gap-3">
                        {datos.map((zona, index) => (
                            <div
                                key={index}
                                className="flex items-center gap-3 p-3 rounded-lg hover:bg-gray-50 transition-all border border-gray-100"
                            >
                                <div
                                    className="w-4 h-4 rounded-full flex-shrink-0"
                                    style={{ backgroundColor: COLORS[index % COLORS.length] }}
                                />
                                <div className="flex-1 min-w-0">
                                    <p className="text-sm font-medium text-gray-700">{zona.nombreZona}</p>
                                    <p className="text-xs text-gray-500">{zona.cantidadPedidos} pedidos ({zona.porcentaje.toFixed(1)}%)</p>
                                </div>
                            </div>
                        ))}
                    </div>
                </Card>

                {/* Tabla de Detalles */}
                <Card className="p-6">
                    <h3 className="text-sm font-bold text-gray-700 mb-6 uppercase tracking-wider">Detalles por Zona</h3>
                    <table className="w-full text-left text-sm">
                        <thead>
                            <tr className="text-gray-400 text-xs uppercase border-b border-gray-100">
                                <th className="pb-4 font-semibold w-12 text-center">Ranking</th>
                                <th className="pb-4 font-semibold px-4">Zona</th>
                                <th className="pb-4 font-semibold text-center px-4">Cantidad de Pedidos</th>
                                <th className="pb-4 font-semibold text-center px-4">Porcentaje</th>
                                <th className="pb-4 font-semibold text-right px-4">Total Recaudado</th>
                            </tr>
                        </thead>
                        <tbody className="divide-y divide-gray-50">
                            {datos.map((zona, index) => (
                                <tr key={index} className="hover:bg-gray-50 transition-all">
                                    <td className="py-4 text-center font-bold text-gray-300">{index + 1}</td>
                                    <td className="py-4 px-4 font-semibold text-gray-700">{zona.nombreZona}</td>
                                    <td className="py-4 px-4 text-center font-medium text-gray-600">{zona.cantidadPedidos}</td>
                                    <td className="py-4 px-4 text-center font-medium text-gray-600">{zona.porcentaje.toFixed(1)}%</td>
                                    <td className="py-4 px-4 text-right font-bold text-gray-900">
                                        ${zona.totalRecaudado.toLocaleString('es-AR')}
                                    </td>
                                </tr>
                            ))}
                        </tbody>
                    </table>
                </Card>
            </div>

            {/* Export Dialog Modal */}
            <ExportDialog
                isOpen={showExportDialog}
                reportName="Pedidos por Zona"
                onExcelClick={handleExportExcel}
                onPdfClick={handleExportPDF}
                onCancel={() => setShowExportDialog(false)}
                isLoading={isExporting}
            />
        </div>
    );
};

// Componente Selector
const Selector = ({ icon, label, type, value, options, onChange }: any) => {
    if (type === 'select') {
        return (
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
    }

    return (
        <div className="bg-white p-3 rounded-xl border border-gray-200 flex items-center justify-between shadow-sm">
            <div className="flex items-center gap-2 text-gray-500 text-sm font-medium">
                {icon} <span>{label}</span>
            </div>
            <input
                type={type}
                value={value}
                onChange={onChange}
                className="ml-2 px-2 py-1 border border-gray-300 rounded-md text-sm focus:ring-2 focus:ring-blue-500 outline-none transition-all cursor-pointer font-semibold text-gray-700"
            />
        </div>
    );
};

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
