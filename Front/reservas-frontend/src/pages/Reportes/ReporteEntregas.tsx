import React, { useState, useEffect, useRef } from 'react';
import { Card } from '../../components/common/Card';
import { Package, CheckCircle2, XCircle, DollarSign, Calendar, MapPin, Download } from 'lucide-react';
import { exportToExcel, exportToPDF } from '../../service/exportService';
import { ExportDialog } from '../../components/ExportDialog';

interface EntregaPorCadeteDTO {
  idCadete: number;
  nombreCadete: string;
  totalPedidosAsignados: number;
  entregasExitosas: number;
  entregasFallidas: number;
  totalRecaudado: number;
  porcentajeEfectividad: number;
}

export const ReporteEntregas: React.FC = () => {
  const [reporte, setReporte] = useState<EntregaPorCadeteDTO[]>([]);
  const [loading, setLoading] = useState(true);

  // Estados de los filtros
  const [periodo, setPeriodo] = useState("7"); 
  const [idSucursal, setIdSucursal] = useState<number | null>(null);
  const [showExportDialog, setShowExportDialog] = useState(false);
  const [isExporting, setIsExporting] = useState(false);
  
  const contentRef = useRef<HTMLDivElement>(null); 

  useEffect(() => {
    // Definimos la función de carga DENTRO para asegurar que tome los valores frescos de 'periodo' e 'idSucursal'
    const fetchReporte = async () => {
      setLoading(true);
      try {
        // 1. Calculamos las fechas en el momento del click/cambio
        const hoy = new Date();
        const inicio = new Date();
        inicio.setDate(hoy.getDate() - parseInt(periodo));

        const fDesde = inicio.toISOString().split("T")[0];
        const fHasta = hoy.toISOString().split("T")[0];

        // 2. Construimos la URL con parámetros correctos
        const API_BASE = import.meta.env.VITE_API_BASE_URL || 'http://localhost:5000/api';
        let url = `${API_BASE}/Reporte/entregas-cadete?fechaDesde=${fDesde}&fechaHasta=${fHasta}`;
        
        if (idSucursal !== null && idSucursal > 0) {
          url += `&idSucursal=${idSucursal}`;
        }
        
        console.log("Solicitando datos para:", { periodo, idSucursal, url });

        const response = await fetch(url);
        if (!response.ok) throw new Error("Error en la respuesta del servidor");
        
        const data = await response.json();
        setReporte(data);
      } catch (error) {
        console.error("Error al filtrar reporte:", error);
      } finally {
        setLoading(false);
      }
    };

    fetchReporte();
  }, [periodo, idSucursal]); // <--- Estos son los disparadores. Si cambian, se ejecuta fetchReporte.

  const handleExportExcel = async () => {
    try {
      setIsExporting(true);
      if (!reporte || reporte.length === 0) {
        alert('No hay datos');
        return;
      }
      const dataExport = reporte.map(c => ({
        'Cadete': c.nombreCadete,
        'Pedidos Asignados': c.totalPedidosAsignados,
        'Entregas Exitosas': c.entregasExitosas,
        'Entregas Fallidas': c.entregasFallidas,
        'Total Recaudado': c.totalRecaudado,
        'Efectividad': `${c.porcentajeEfectividad.toFixed(2)}%`,
      }));
      
      const periodoLabel = periodo === "7" ? "Últimos 7 días" : periodo === "30" ? "Últimos 30 días" : "Últimos 90 días";
      
      exportToExcel(dataExport, {
        reportName: 'Entregas por Cadete',
        fileName: 'entregas-cadete',
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
        reportName: 'Entregas por Cadete',
        fileName: 'entregas-cadete',
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

  // Cálculos de totales (se recalculan automáticamente al cambiar el estado 'reporte')
  const totalPedidos = reporte.reduce((acc, c) => acc + c.totalPedidosAsignados, 0);
  const totalExito = reporte.reduce((acc, c) => acc + c.entregasExitosas, 0);
  const totalFallidos = reporte.reduce((acc, c) => acc + c.entregasFallidas, 0);
  const tasaExitoGlobal = totalPedidos > 0 ? ((totalExito / totalPedidos) * 100).toFixed(1) : "0.0";
  const ingresosTotales = reporte.reduce((acc, c) => acc + c.totalRecaudado, 0);

  return (
    <div className="p-6 bg-[#f8f9fa] min-h-screen">
      <div className="flex justify-between items-center mb-6">
        <div>
          <h1 className="text-xl font-bold text-gray-800 tracking-tight">Reportes y Análisis</h1>
          <p className="text-sm text-gray-500">Métricas de rendimiento de cadetes</p>
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

      {loading ? (
        <div className="flex justify-center p-20">
            <p className="text-gray-400 font-medium animate-pulse">Actualizando resultados...</p>
        </div>
      ) : (
        <>
          <div className="grid grid-cols-1 md:grid-cols-4 gap-4 mb-8">
            <MetricCard title="Total Pedidos" value={totalPedidos} sub="Asignados" icon={<Package className="text-gray-400" />} />
            <MetricCard title="Tasa de Éxito" value={`${tasaExitoGlobal}%`} sub="Entregas completadas" icon={<CheckCircle2 className="text-green-500" />} color="text-green-600" />
            <MetricCard title="Pedidos Fallidos" value={totalFallidos} sub="No entregados" icon={<XCircle className="text-red-500" />} color="text-red-600" />
            <MetricCard title="Recaudación" value={`$${ingresosTotales.toLocaleString()}`} sub="Total periodo" icon={<DollarSign className="text-blue-500" />} />
          </div>

          <Card className="p-6">
            <table className="w-full text-left">
              <thead>
                <tr className="text-gray-400 text-xs uppercase border-b border-gray-100">
                  <th className="pb-4 font-semibold px-2">Cadete</th>
                  <th className="pb-4 font-semibold text-center">Asignados</th>
                  <th className="pb-4 font-semibold text-center">Exitosos</th>
                  <th className="pb-4 font-semibold text-right px-2">Efectividad</th>
                </tr>
              </thead>
              <tbody className="divide-y divide-gray-50 text-sm">
                {reporte.length > 0 ? (
                    reporte.map((item) => (
                    <tr key={item.idCadete} className="hover:bg-gray-50 transition-all">
                        <td className="py-4 px-2 font-medium text-gray-700">{item.nombreCadete}</td>
                        <td className="py-4 text-center">{item.totalPedidosAsignados}</td>
                        <td className="py-4 text-center text-blue-600">{item.entregasExitosas}</td>
                        <td className="py-4 text-right px-2 font-bold text-green-600">
                        {item.porcentajeEfectividad.toFixed(1)}%
                        </td>
                    </tr>
                    ))
                ) : (
                    <tr>
                        <td colSpan={4} className="py-10 text-center text-gray-400">No hay datos para este periodo o sucursal</td>
                    </tr>
                )}
              </tbody>
            </table>
          </Card>
        </>
      )}
      </div>
    {/* Export Dialog Modal */}
    <ExportDialog
        isOpen={showExportDialog}
        reportName="Entregas por Cadete"
        onExcelClick={handleExportExcel}
        onPdfClick={handleExportPDF}
        onCancel={() => setShowExportDialog(false)}
        isLoading={isExporting}
    />
    </div>
  );
};

// Componentes internos limpios
const Selector = ({ icon, label, value, options, onChange }: any) => (
  <div className="bg-white p-3 rounded-xl border border-gray-200 flex items-center justify-between shadow-sm">
    <div className="flex items-center gap-2 text-gray-500 text-sm font-medium">
      {icon} <span>{label}</span>
    </div>
    <select
      value={value}
      onChange={onChange}
      className="ml-2 px-2 py-1 border border-gray-300 rounded-md text-sm focus:ring-2 focus:ring-blue-500 outline-none font-semibold text-gray-700 cursor-pointer"
    >
      {options.map((o: any) => <option key={o.value} value={o.value}>{o.label}</option>)}
    </select>
  </div>
);

const MetricCard = ({ title, value, sub, icon, color = "text-gray-900" }: any) => (
  <Card className="p-5 flex justify-between items-start">
    <div>
      <p className="text-xs text-gray-500 mb-1 font-medium">{title}</p>
      <h4 className={`text-xl font-bold ${color}`}>{value}</h4>
      <p className="text-[10px] text-gray-400 mt-2 uppercase font-semibold">{sub}</p>
    </div>
    <div className="bg-gray-50 p-2 rounded-lg">{icon}</div>
  </Card>
);