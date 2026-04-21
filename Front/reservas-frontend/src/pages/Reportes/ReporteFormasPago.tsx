import React, { useEffect, useRef, useState } from 'react';
import { Calendar, CreditCard, Download, DollarSign, MapPin } from 'lucide-react';
import { PieChart, Pie, Cell, ResponsiveContainer, Tooltip } from 'recharts';
import { Card } from '../../components/common/Card';
import { ExportDialog } from '../../components/ExportDialog';
import { exportToExcel, exportToPDF } from '../../service/exportService';
import { getReporteFormasPago } from '../../service/reporteService';
import { ReporteFormasPagoDTO } from '../../types/pedido.types';

const COLORS = ['#3b82f6', '#8b5cf6', '#ec4899', '#f59e0b', '#10b981', '#06b6d4'];

interface SelectorOption {
    value: string;
    label: string;
}

interface SelectorProps {
    icon: React.ReactNode;
    label: string;
    value: string;
    options: SelectorOption[];
    onChange: (e: React.ChangeEvent<HTMLSelectElement>) => void;
}

interface MetricCardProps {
    title: string;
    value: string;
    sub: string;
    icon: React.ReactNode;
    color?: string;
}

export const ReporteFormasPago: React.FC = () => {
    const [reporte, setReporte] = useState<ReporteFormasPagoDTO | null>(null);
    const [loading, setLoading] = useState(true);
    const [periodo, setPeriodo] = useState('7');
    const [showExportDialog, setShowExportDialog] = useState(false);
    const [isExporting, setIsExporting] = useState(false);

    const contentRef = useRef<HTMLDivElement>(null);

    useEffect(() => {
        const cargarData = async () => {
            try {
                setLoading(true);
                const ahora = new Date();
                const desde = new Date(ahora);
                desde.setDate(desde.getDate() - parseInt(periodo, 10));

                const fechaDesde = desde.toISOString().split('T')[0];
                const fechaHasta = ahora.toISOString().split('T')[0];

                const res = await getReporteFormasPago(fechaDesde, fechaHasta);
                setReporte(res);
            } catch (error) {
                console.error('Error al cargar reporte de formas de pago:', error);
            } finally {
                setLoading(false);
            }
        };

        cargarData();
    }, [periodo]);

    const handleExportExcel = async () => {
        try {
            setIsExporting(true);
            if (!reporte) {
                alert('No hay datos para exportar');
                return;
            }

            const dataExport = reporte.distribucionFormasPago.map((item) => ({
                'Forma de pago': item.formaDePago,
                'Operaciones': item.cantidadOperaciones,
                'Porcentaje': `${item.porcentaje.toFixed(2)}%`,
                'Monto total': item.montoTotal
            }));

            exportToExcel(dataExport, {
                reportName: 'Formas de Pago',
                fileName: 'formas-de-pago'
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

            await exportToPDF(contentRef.current, {
                reportName: 'Formas de Pago',
                fileName: 'formas-de-pago'
            });
        } finally {
            setIsExporting(false);
            setShowExportDialog(false);
        }
    };

    if (loading) return <p className="p-6 text-gray-500 font-medium">Cargando reporte de formas de pago...</p>;
    if (!reporte) return <p className="p-6 text-gray-500 font-medium">No hay datos disponibles</p>;

    const pieData = reporte.distribucionFormasPago.map((item) => ({
        name: item.formaDePago,
        value: item.cantidadOperaciones
    }));

    const formaPrincipal = reporte.distribucionFormasPago.length > 0
        ? reporte.distribucionFormasPago[0]
        : null;
    const formaPrincipalLabel = formaPrincipal?.formaDePago ?? '—';
    const formaPrincipalSub = formaPrincipal
        ? `${formaPrincipal.porcentaje.toFixed(1)}% del total`
        : 'Sin datos';

    return (
        <div className="p-6 bg-[#f8f9fa] min-h-screen">
            <div className="flex justify-between items-center mb-6">
                <div>
                    <h1 className="text-xl font-bold text-gray-800 tracking-tight">Reporte de Formas de Pago</h1>
                    <p className="text-sm text-gray-500">Distribución y facturación por medio de pago</p>
                </div>
                <button
                    onClick={() => setShowExportDialog(true)}
                    className="bg-gray-700 hover:bg-gray-800 text-white px-5 py-2.5 rounded-xl font-bold text-sm flex items-center gap-2 transition-all shadow-md"
                >
                    <Download size={16} />
                    Exportar Reporte
                </button>
            </div>

            <div ref={contentRef}>
                <div className="grid grid-cols-1 md:grid-cols-2 gap-4 mb-6">
                    <Selector
                        icon={<Calendar size={18} />}
                        label="Periodo:"
                        value={periodo}
                        options={[
                            { value: '7', label: 'Últimos 7 días' },
                            { value: '30', label: 'Últimos 30 días' },
                            { value: '90', label: 'Últimos 90 días' }
                        ]}
                        onChange={(e: React.ChangeEvent<HTMLSelectElement>) => setPeriodo(e.target.value)}
                    />
                </div>

                <div className="grid grid-cols-1 md:grid-cols-3 gap-4 mb-8">
                    <MetricCard
                        title="Total Operaciones"
                        value={reporte.totalOperaciones.toString()}
                        sub="Transacciones registradas"
                        icon={<CreditCard className="text-blue-500" size={24} />}
                        color="text-blue-600"
                    />
                    <MetricCard
                        title="Monto Total"
                        value={`$${reporte.totalMonto.toLocaleString('es-AR')}`}
                        sub="Facturación del período"
                        icon={<DollarSign className="text-green-500" size={24} />}
                        color="text-green-600"
                    />
                    <MetricCard
                        title="Forma principal"
                        value={formaPrincipalLabel}
                        sub={formaPrincipalSub}
                        icon={<CreditCard className="text-purple-500" size={24} />}
                        color="text-purple-600"
                    />
                </div>

                <div className="grid grid-cols-1 lg:grid-cols-2 gap-6 mb-6">
                    <Card className="p-6">
                        <h3 className="text-sm font-bold text-gray-700 mb-6 uppercase tracking-wider">Distribución por operaciones</h3>
                        <ResponsiveContainer width="100%" height={320}>
                            <PieChart>
                                <Pie data={pieData} dataKey="value" nameKey="name" outerRadius={120}>
                                    {pieData.map((_, index) => (
                                        <Cell key={`cell-${index}`} fill={COLORS[index % COLORS.length]} />
                                    ))}
                                </Pie>
                                <Tooltip formatter={(value) => `${value} operaciones`} />
                            </PieChart>
                        </ResponsiveContainer>
                    </Card>

                    <Card className="p-6">
                        <h3 className="text-sm font-bold text-gray-700 mb-6 uppercase tracking-wider">Detalle de Formas de Pago</h3>
                        <div className="overflow-x-auto">
                            <table className="w-full text-left text-sm">
                                <thead>
                                    <tr className="text-gray-400 text-xs uppercase border-b border-gray-100">
                                        <th className="pb-4 font-semibold">Forma de pago</th>
                                        <th className="pb-4 font-semibold text-center">Operaciones</th>
                                        <th className="pb-4 font-semibold text-center">Porcentaje</th>
                                        <th className="pb-4 font-semibold text-right">Monto</th>
                                    </tr>
                                </thead>
                                <tbody className="divide-y divide-gray-50">
                                    {reporte.distribucionFormasPago.map((item, index) => (
                                        <tr key={index} className="hover:bg-gray-50 transition-all">
                                            <td className="py-4 font-medium text-gray-700">{item.formaDePago}</td>
                                            <td className="py-4 text-center text-gray-600">{item.cantidadOperaciones}</td>
                                            <td className="py-4 text-center text-gray-600">{item.porcentaje.toFixed(2)}%</td>
                                            <td className="py-4 text-right font-bold text-gray-900">
                                                ${item.montoTotal.toLocaleString('es-AR')}
                                            </td>
                                        </tr>
                                    ))}
                                </tbody>
                            </table>
                        </div>
                    </Card>
                </div>
            </div>

            <ExportDialog
                isOpen={showExportDialog}
                reportName="Formas de Pago"
                onExcelClick={handleExportExcel}
                onPdfClick={handleExportPDF}
                onCancel={() => setShowExportDialog(false)}
                isLoading={isExporting}
            />
        </div>
    );
};

const Selector = ({ icon, label, value, options, onChange }: SelectorProps) => (
    <div className="bg-white p-3 rounded-xl border border-gray-200 flex items-center justify-between shadow-sm">
        <div className="flex items-center gap-2 text-gray-500 text-sm font-medium">
            {icon} <span>{label}</span>
        </div>
        <select
            value={value}
            onChange={onChange}
            className="ml-2 px-2 py-1 border border-gray-300 rounded-md text-sm focus:ring-2 focus:ring-blue-500 outline-none transition-all cursor-pointer font-semibold text-gray-700"
        >
            {options.map((opt) => (
                <option key={opt.value} value={opt.value}>{opt.label}</option>
            ))}
        </select>
    </div>
);

const MetricCard = ({ title, value, sub, icon, color = 'text-gray-900' }: MetricCardProps) => (
    <Card className="p-5 flex justify-between items-start border-gray-100 shadow-sm">
        <div>
            <p className="text-xs text-gray-500 mb-1 font-medium uppercase tracking-wider">{title}</p>
            <h4 className={`text-2xl font-bold ${color}`}>{value}</h4>
            <p className="text-[10px] text-gray-400 mt-2 uppercase font-semibold italic">{sub}</p>
        </div>
        <div className="bg-gray-50 p-2 rounded-lg">{icon}</div>
    </Card>
);
