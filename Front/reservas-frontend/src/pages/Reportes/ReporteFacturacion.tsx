import React, { useEffect, useState, useRef } from 'react';
import { BarChart, Bar, XAxis, YAxis, CartesianGrid, Tooltip, ResponsiveContainer, Cell } from 'recharts';
import { Calendar, MapPin, DollarSign, TrendingUp, Download } from 'lucide-react';
import { Card } from '../../components/common/Card';
import { getRankingClientesFacturacion } from '../../service/reporteService';
import { ClienteFacturacionDTO } from '../../types/pedido.types';
import { exportToExcel, exportToPDF } from '../../service/exportService';
import { ExportDialog } from '../../components/ExportDialog';

export const ReporteFacturacion = () => {
    const [datos, setDatos] = useState<ClienteFacturacionDTO[]>([]);
    const [loading, setLoading] = useState(true);
    const [periodo, setPeriodo] = useState("7");
    const [idSucursal, setIdSucursal] = useState<number | null>(null);
    const [showExportDialog, setShowExportDialog] = useState(false);
    const [isExporting, setIsExporting] = useState(false);
    
    const contentRef = useRef<HTMLDivElement>(null);

   useEffect(() => {
    const cargarData = async () => {
        try {
            setLoading(true);
            // Pasamos las dos variables de estado aquí
            const res = await getRankingClientesFacturacion(periodo, idSucursal);
            setDatos(res);
        } catch (error) {
            console.error("Error al cargar facturación:", error);
        } finally {
            setLoading(false);
        }
    };
    cargarData();
}, [periodo, idSucursal]); // Esto detecta cuando cambias el selector y recarga

    const totalGeneral = datos.reduce((acc, curr) => acc + curr.totalFacturado, 0);
    const promedioGeneral = datos.length > 0 ? totalGeneral / datos.length : 0;

    const handleExportExcel = async () => {
        try {
            setIsExporting(true);
            if (!datos || datos.length === 0) {
                alert('No hay datos');
                return;
            }
            const dataExport = datos.map((c, i) => ({
                'Posición': i + 1,
                'Cliente': c.nombreCliente,
                'Pedidos': c.cantidadPedidos,
                'Facturación': c.totalFacturado,
            }));
            
            const periodoLabel = periodo === "7" ? "Últimos 7 días" : periodo === "30" ? "Últimos 30 días" : "Últimos 90 días";
            
            exportToExcel(dataExport, {
                reportName: 'Ranking de Facturación',
                fileName: 'ranking-facturacion',
                filters: { periodo: periodoLabel }
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
            const periodoLabel = periodo === "7" ? "Últimos 7 días" : periodo === "30" ? "Últimos 30 días" : "Últimos 90 días";
            
            await exportToPDF(contentRef.current, {
                reportName: 'Ranking de Facturación',
                fileName: 'ranking-facturacion',
                filters: { periodo: periodoLabel }
            });
        } finally {
            setIsExporting(false);
            setShowExportDialog(false);
        }
    };

    const handleExportClick = () => {
        setShowExportDialog(true);
    };

    if (loading) return <p className="p-6 text-gray-500 font-medium">Cargando reporte de facturación...</p>;

    return (
        <div className="p-6 bg-[#f8f9fa] min-h-screen">
            {/* Header unificado */}
            <div className="flex justify-between items-center mb-6">
                <div>
                    <h1 className="text-xl font-bold text-gray-800 tracking-tight">Reporte de Facturación</h1>
                    <p className="text-sm text-gray-500">Visualiza métricas y estadísticas de facturación por cliente</p>
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
            {/* Selectores idénticos a los otros reportes */}
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

            {/* Métricas unificadas */}
            <div className="grid grid-cols-1 md:grid-cols-2 gap-4 mb-8">
                <MetricCard 
                    title="Ingresos Totales (Top 10)" 
                    value={`$${totalGeneral.toLocaleString('es-AR')}`} 
                    sub="Suma de pedidos entregados" 
                    icon={<DollarSign className="text-green-500" />} 
                    color="text-green-600"
                />
                <MetricCard 
                    title="Gasto Promedio" 
                    value={`$${promedioGeneral.toLocaleString('es-AR')}`} 
                    sub="Promedio por cliente en el top" 
                    icon={<TrendingUp className="text-blue-500" />} 
                    color="text-blue-600"
                />
            </div>

          

            {/* Tabla unificada */}
            <Card className="p-6">
                <h3 className="text-sm font-bold text-gray-700 mb-6 uppercase tracking-wider">Ranking de Facturación</h3>
                <table className="w-full text-left">
                    <thead>
                        <tr className="text-gray-400 text-xs uppercase border-b border-gray-100">
                            <th className="pb-4 font-semibold w-12 text-center">Ranking</th>
                            <th className="pb-4 font-semibold px-4">Cliente</th>
                            <th className="pb-4 font-semibold text-center px-4">Pedidos</th>
                            <th className="pb-4 font-semibold text-right px-4">Total Facturado</th>
                        </tr>
                    </thead>
                    <tbody className="divide-y divide-gray-50 text-sm">
                        {datos.map((cliente, index) => (
                            <tr key={index} className="hover:bg-gray-50 transition-all">
                                <td className="py-4 text-center font-bold text-gray-300">{index + 1}</td>
                                <td className="py-4 px-4 font-semibold text-gray-700">{cliente.nombreCliente}</td>
                                <td className="py-4 px-4 text-center font-medium text-gray-600">{cliente.cantidadPedidos}</td>
                                <td className="py-4 px-4 text-right font-bold text-gray-900">
                                    ${cliente.totalFacturado.toLocaleString('es-AR')}
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
            reportName="Ranking de Facturación"
            onExcelClick={handleExportExcel}
            onPdfClick={handleExportPDF}
            onCancel={() => setShowExportDialog(false)}
            isLoading={isExporting}
        />
        </div>
    );
};

// Componente Selector (Consistente en todos los reportes)
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

// Componente MetricCard (Consistente en todos los reportes)
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