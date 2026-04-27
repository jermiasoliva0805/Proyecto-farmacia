import React, { useEffect, useState, useRef } from 'react';
import { PieChart, Pie, Cell, ResponsiveContainer, Legend, Tooltip } from 'recharts';
import { Calendar, MapPin, AlertCircle, TrendingDown, DollarSign, Download } from 'lucide-react';
import { Card } from '../../components/common/Card';
import { getCancelacionesPorMotivo } from '../../service/reporteService';
import { ReporteCancelacionesPorMotivoDTO } from '../../types/pedido.types';
import { exportToExcel, exportToPDF } from '../../service/exportService';
import { ExportDialog } from '../../components/ExportDialog';

const COLORS = ['#ef4444', '#f97316', '#eab308', '#ec4899', '#8b5cf6', '#3b82f6', '#06b6d4', '#10b981'];

export const ReporteCancelacionesPorMotivos = () => {
    const [reporte, setReporte] = useState<ReporteCancelacionesPorMotivoDTO | null>(null);
    const [loading, setLoading] = useState(true);
    const [periodo, setPeriodo] = useState("7");
    const [showExportDialog, setShowExportDialog] = useState(false);
    const [isExporting, setIsExporting] = useState(false);
    
    // Referencias para exportación
    const contentRef = useRef<HTMLDivElement>(null);
    const tableRef = useRef<HTMLTableElement>(null);

    useEffect(() => {
        const cargarData = async () => {
            try {
                setLoading(true);
                const ahora = new Date();
                const desde = new Date(ahora);
                desde.setDate(desde.getDate() - parseInt(periodo));

                const fechaDesde = desde.toISOString().split('T')[0];
                const fechaHasta = ahora.toISOString().split('T')[0];

                const res = await getCancelacionesPorMotivo(fechaDesde, fechaHasta);
                setReporte(res);
            } catch (error) {
                console.error("Error al cargar reporte:", error);
            } finally {
                setLoading(false);
            }
        };
        cargarData();
    }, [periodo]);

    // Funciones de exportación
    const handleExportExcel = async () => {
        try {
            setIsExporting(true);
            if (!reporte) {
                alert('No hay datos para exportar');
                return;
            }

            const dataExport = reporte.detalleMotivos.map((motivo, index) => ({
                '#': index + 1,
                'Motivo': motivo.motivo,
                'Cantidad': motivo.cantidad,
                'Porcentaje': `${motivo.porcentaje.toFixed(2)}%`,
                'Monto Perdido': motivo.montoPerdido,
            }));

            const periodoLabel = periodo === "7" ? "Últimos 7 días" : 
                                 periodo === "30" ? "Últimos 30 días" : "Últimos 90 días";

            exportToExcel(dataExport, {
                reportName: 'Cancelaciones por Motivo',
                fileName: 'cancelaciones-por-motivo',
                filters: {
                    periodo: periodoLabel,
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
                alert('No hay contenido para exportar');
                return;
            }

            const periodoLabel = periodo === "7" ? "Últimos 7 días" : 
                                 periodo === "30" ? "Últimos 30 días" : "Últimos 90 días";

            await exportToPDF(contentRef.current, {
                reportName: 'Cancelaciones por Motivo',
                fileName: 'cancelaciones-por-motivo',
                filters: {
                    periodo: periodoLabel,
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

    if (loading) return <p className="p-6 text-gray-500 font-medium">Cargando reporte...</p>;
    if (!reporte) return <p className="p-6 text-gray-500 font-medium">No hay datos disponibles</p>;

    const dataPie = reporte.detalleMotivos.map(item => ({
        name: item.motivo,
        value: item.cantidad
    }));

    return (
        <div className="p-6 bg-[#f8f9fa] min-h-screen">
            {/* Header */}
            <div className="flex justify-between items-center mb-6">
                <div>
                    <h1 className="text-xl font-bold text-gray-800 tracking-tight">Cancelaciones por Motivo</h1>
                    <p className="text-sm text-gray-500">Análisis de motivos de <strong>cancelación manual</strong> realizada por el encargado</p>
                </div>
                <button 
                    onClick={handleExportClick}
                    className="bg-gray-700 hover:bg-gray-800 text-white px-5 py-2.5 rounded-xl font-bold text-sm flex items-center gap-2 transition-all shadow-md"
                >
                    <Download size={16} />
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
            </div>

            {/* Contenido a exportar */}
            <div ref={contentRef}>
            {/* Métricas principales */}
            <div className="grid grid-cols-1 md:grid-cols-3 gap-4 mb-8">
                <MetricCard 
                    title="Total Pedidos"
                    value={reporte.totalPedidos.toString()}
                    sub="En el período"
                    icon={<AlertCircle className="text-gray-500" size={24} />}
                    color="text-gray-600"
                />
                <MetricCard 
                    title="Cancelados"
                    value={`${reporte.totalCancelados}`}
                    sub="Del total de pedidos"
                    icon={<TrendingDown className="text-red-500" size={24} />}
                    color="text-red-600"
                />
                <MetricCard 
                    title="Ingresos Perdidos"
                    value={`$${reporte.ingresosPerdidos.toLocaleString('es-AR')}`}
                    sub="Por cancelaciones"
                    icon={<DollarSign className="text-yellow-500" size={24} />}
                    color="text-yellow-600"
                />
            </div>

            {/* Información principal motivo */}
            <Card className="p-6 mb-8 bg-white border-l-4 border-red-500">
                <div className="flex items-start justify-between">
                    <div>
                        <p className="text-xs text-gray-500 font-medium uppercase tracking-wider mb-1">Principal Motivo</p>
                        <h3 className="text-2xl font-bold text-gray-800">{reporte.principalMotivo}</h3>
                    </div>
                    <div className="text-right">
                        <p className="text-sm text-gray-600">Más frecuente en cancelaciones</p>
                    </div>
                </div>
            </Card>

            {/* Gráfico de Pie */}
            <Card className="p-6 mb-8">
                <h3 className="text-sm font-bold text-gray-700 mb-6 uppercase tracking-wider">Distribución Porcentual de Cancelaciones por Motivo</h3>
                <ResponsiveContainer width="100%" height={300}>
                    <PieChart>
                        <Pie
                            data={dataPie}
                            cx="50%"
                            cy="50%"
                            labelLine={false}
                            label={(entry) => `${entry.name}: ${((entry.value / reporte.totalCancelados) * 100).toFixed(1)}%`}
                            outerRadius={80}
                            fill="#8884d8"
                            dataKey="value"
                        >
                            {dataPie.map((entry, index) => (
                                <Cell key={`cell-${index}`} fill={COLORS[index % COLORS.length]} />
                            ))}
                        </Pie>
                        <Tooltip 
                            formatter={(value) => `${value} pedidos`}
                            contentStyle={{ backgroundColor: '#fff', border: '1px solid #ccc' }}
                        />
                        <Legend />
                    </PieChart>
                </ResponsiveContainer>
            </Card>

            {/* Tabla de Detalle */}
            <Card className="p-6">
                <h3 className="text-sm font-bold text-gray-700 mb-6 uppercase tracking-wider">Detalle por Motivo</h3>
                <table className="w-full text-left">
                    <thead>
                        <tr className="text-gray-400 text-xs uppercase border-b border-gray-100">
                            <th className="pb-4 font-semibold w-12 text-center">#</th>
                            <th className="pb-4 font-semibold px-4">Motivo</th>
                            <th className="pb-4 font-semibold text-center px-4">Cantidad</th>
                            <th className="pb-4 font-semibold text-right px-4">Porcentaje</th>
                            <th className="pb-4 font-semibold text-right px-4">Monto Perdido</th>
                        </tr>
                    </thead>
                    <tbody className="divide-y divide-gray-50 text-sm">
                        {reporte.detalleMotivos.map((motivo, index) => (
                            <tr key={index} className="hover:bg-gray-50 transition-all">
                                <td className="py-4 text-center font-bold text-gray-300">{index + 1}</td>
                                <td className="py-4 px-4 font-semibold text-gray-700">{motivo.motivo}</td>
                                <td className="py-4 px-4 text-center font-medium text-gray-600">{motivo.cantidad}</td>
                                <td className="py-4 px-4 text-right font-bold text-gray-900">
                                    {motivo.porcentaje.toFixed(2)}%
                                </td>
                                <td className="py-4 px-4 text-right font-bold text-gray-900">
                                    ${motivo.montoPerdido.toLocaleString('es-AR')}
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
            reportName="Cancelaciones por Motivo"
            onExcelClick={handleExportExcel}
            onPdfClick={handleExportPDF}
            onCancel={() => setShowExportDialog(false)}
            isLoading={isExporting}
        />
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
            <h4 className={`text-2xl font-bold ${color}`}>{value}</h4>
            <p className="text-[10px] text-gray-400 mt-2 uppercase font-semibold italic">{sub}</p>
        </div>
        <div className="bg-gray-50 p-2 rounded-lg">{icon}</div>
    </Card>
);
