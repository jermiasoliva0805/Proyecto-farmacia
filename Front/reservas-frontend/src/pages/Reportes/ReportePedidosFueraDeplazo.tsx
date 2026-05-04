import React, { useState, useEffect, useRef } from 'react';
import { Card } from '../../components/common/Card';
import { AlertCircle, TrendingUp, Clock, Download } from 'lucide-react';
import { exportToExcel, exportToPDF } from '../../service/exportService';
import { ExportDialog } from '../../components/ExportDialog';

interface DetallePedidoFueraDeplazo {
  idPedido: number;
  clienteNombre: string;
  nombreCadete: string;
  fechaCreacion: string;
  fechaEstimada: string;
  fechaEntrega: string;
  retrasoDías: number;
  intentosEntregaFallida: number;
  esDemorado: boolean;
  fechaMarcadoDemorado: string | null;
}

interface PedidosFueraDeplazoDTO {
  totalEntregas: number;
  entregasTardías: number;
  retrasoPromedioDías: number;
  detalles: DetallePedidoFueraDeplazo[];
}

export const ReportePedidosFueraDeplazo: React.FC = () => {
  const [reporte, setReporte] = useState<PedidosFueraDeplazoDTO | null>(null);
  const [loading, setLoading] = useState(true);
  const [periodo, setPeriodo] = useState("30");
  const [showExportDialog, setShowExportDialog] = useState(false);
  const [isExporting, setIsExporting] = useState(false);
  
  const contentRef = useRef<HTMLDivElement>(null);

  useEffect(() => {
    const fetchReporte = async () => {
      setLoading(true);
      try {
        const hoy = new Date();
        const inicio = new Date();
        inicio.setDate(hoy.getDate() - parseInt(periodo));

        const fDesde = inicio.toISOString().split("T")[0];
        const fHasta = hoy.toISOString().split("T")[0];

        const API_BASE = import.meta.env.VITE_API_BASE_URL || 'http://localhost:5000/api';
        const url = `${API_BASE}/Reporte/entregas-fuera-de-plazo?fechaDesde=${fDesde}&fechaHasta=${fHasta}`;
        
        console.log("Solicitando reporte de entregas fuera de plazo:", { periodo, url });

        const response = await fetch(url);
        if (!response.ok) throw new Error("Error en la respuesta del servidor");
        
        const data = await response.json();
        setReporte(data);
      } catch (error) {
        console.error("Error al cargar reporte:", error);
        setReporte(null);
      } finally {
        setLoading(false);
      }
    };

    fetchReporte();
  }, [periodo]);

  const handleExportExcel = async () => {
    try {
      setIsExporting(true);
      if (!reporte || reporte.detalles.length === 0) {
        alert('No hay datos');
        return;
      }
      
      const dataExport = reporte.detalles.map(d => ({
        'ID Pedido': d.idPedido,
        'Cliente': d.clienteNombre,
        'Cadete': d.nombreCadete,
        'Fecha Creación': new Date(d.fechaCreacion).toLocaleDateString('es-AR'),
        'Fecha Estimada': new Date(d.fechaEstimada).toLocaleDateString('es-AR'),
        'Fecha Entrega': new Date(d.fechaEntrega).toLocaleDateString('es-AR'),
        'Demorado': d.esDemorado ? 'Sí' : 'No',
        'Retraso (Días)': d.retrasoDías,
        'Intentos Fallidos': d.intentosEntregaFallida,
      }));
      
      const periodoLabel = periodo === "7" ? "Últimos 7 días" : periodo === "30" ? "Últimos 30 días" : "Últimos 90 días";
      
      exportToExcel(dataExport, {
        reportName: 'Entregas Fuera de Plazo',
        fileName: 'entregas-fuera-de-plazo',
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
        reportName: 'Entregas Fuera de Plazo',
        fileName: 'entregas-fuera-de-plazo',
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

  const porcentajeTardío = reporte && reporte.totalEntregas > 0 
    ? ((reporte.entregasTardías / reporte.totalEntregas) * 100).toFixed(1)
    : "0.0";

  return (
    <div className="p-6 bg-[#f8f9fa] min-h-screen">
      <div className="flex justify-between items-center mb-6">
        <div>
          <h1 className="text-xl font-bold text-gray-800 tracking-tight">Entregas fuera de plazo</h1>
          <p className="text-sm text-gray-500">Pedidos que no se entregaron dentro del plazo estimado</p>
        </div>
        <button 
          onClick={handleExportClick}
          className="bg-gray-700 hover:bg-gray-800 text-white px-5 py-2.5 rounded-xl font-bold text-sm flex items-center gap-2 transition-all shadow-md"
        >
          <Download size={16} />
          Exportar Reporte
        </button>
      </div>

      <div ref={contentRef}>
        {/* Selector de período */}
        <div className="mb-6 flex">
          <div className="bg-white p-3 rounded-xl border border-gray-200 flex items-center justify-between shadow-sm w-full md:w-64">
            <div className="flex items-center gap-2 text-gray-500 text-sm font-medium">
              <Clock size={18} /> <span>Período:</span>
            </div>
            <select
              value={periodo}
              onChange={(e) => setPeriodo(e.target.value)}
              className="ml-2 px-2 py-1 border border-gray-300 rounded-md text-sm focus:ring-2 focus:ring-blue-500 outline-none font-semibold text-gray-700 cursor-pointer"
            >
              <option value="7">Últimos 7 días</option>
              <option value="30">Últimos 30 días</option>
              <option value="90">Últimos 90 días</option>
            </select>
          </div>
        </div>

        {loading ? (
          <div className="flex justify-center p-20">
            <p className="text-gray-400 font-medium animate-pulse">Actualizando resultados...</p>
          </div>
        ) : reporte ? (
          <>
            {/* Tarjetas de métricas */}
            <div className="grid grid-cols-1 md:grid-cols-3 gap-4 mb-8">
              <MetricCard 
                title="Total Entregas" 
                value={reporte.totalEntregas} 
                sub="Pedidos entregados" 
                icon={<TrendingUp className="text-gray-400" />} 
              />
              <MetricCard 
                title="Entregas Tardías" 
                value={reporte.entregasTardías} 
                sub={`${porcentajeTardío}% del total`}
                icon={<AlertCircle className="text-orange-500" />} 
                color="text-orange-600" 
              />
              <MetricCard 
                title="Retraso Promedio" 
                value={`${reporte.retrasoPromedioDías} días`} 
                sub="Horas hábiles" 
                icon={<Clock className="text-red-500" />} 
                color="text-red-600" 
              />
            </div>

            {/* Tabla de detalles */}
            <Card className="p-6">
              <table className="w-full text-left">
                <thead>
                  <tr className="text-gray-400 text-xs uppercase border-b border-gray-100">
                    <th className="pb-4 font-semibold px-2">Pedido</th>
                    <th className="pb-4 font-semibold">Cliente</th>
                    <th className="pb-4 font-semibold">Cadete</th>
                    <th className="pb-4 font-semibold text-center">Fecha Estimada</th>
                    <th className="pb-4 font-semibold text-center">Fecha Entrega</th>
                    <th className="pb-4 font-semibold text-center">Demorado</th>
                    <th className="pb-4 font-semibold text-right px-2">Retraso (días)</th>
                  </tr>
                </thead>
                <tbody className="divide-y divide-gray-50 text-sm">
                  {reporte.detalles.length > 0 ? (
                    reporte.detalles.map((item) => (
                      <tr key={item.idPedido} className="hover:bg-gray-50 transition-all">
                        <td className="py-4 px-2 font-medium text-blue-600">#{item.idPedido}</td>
                        <td className="py-4 text-gray-700">{item.clienteNombre}</td>
                        <td className="py-4 text-gray-700">{item.nombreCadete}</td>
                        <td className="py-4 text-center text-gray-600">
                          {new Date(item.fechaEstimada).toLocaleDateString('es-AR')}
                        </td>
                        <td className="py-4 text-center text-gray-600">
                          {new Date(item.fechaEntrega).toLocaleDateString('es-AR')}
                        </td>
                        <td className="py-4 text-center">
                          {item.esDemorado ? (
                            <span className="inline-block px-3 py-1 bg-red-100 text-red-700 rounded-full text-xs font-semibold">
                              Sí
                            </span>
                          ) : (
                            <span className="inline-block px-3 py-1 bg-green-100 text-green-700 rounded-full text-xs font-semibold">
                              No
                            </span>
                          )}
                        </td>
                        <td className="py-4 text-right px-2 font-bold text-orange-600">
                          +{item.retrasoDías}
                        </td>
                      </tr>
                    ))
                  ) : (
                    <tr>
                      <td colSpan={7} className="py-10 text-center text-gray-400">
                        No hay entregas fuera de plazo en este período
                      </td>
                    </tr>
                  )}
                </tbody>
              </table>
            </Card>
          </>
        ) : (
          <div className="text-center p-10 bg-white rounded-xl border border-gray-200">
            <AlertCircle className="w-12 h-12 text-gray-300 mx-auto mb-3" />
            <p className="text-gray-500">Error al cargar el reporte</p>
          </div>
        )}
      </div>

      {/* Export Dialog Modal */}
      <ExportDialog
        isOpen={showExportDialog}
        reportName="Entregas Fuera de Plazo"
        onExcelClick={handleExportExcel}
        onPdfClick={handleExportPDF}
        onCancel={() => setShowExportDialog(false)}
        isLoading={isExporting}
      />
    </div>
  );
};

// Componentes internos
const MetricCard = ({ title, value, sub, icon, color = "text-gray-900" }: any) => (
  <Card className="p-5 flex justify-between items-start">
    <div>
      <p className="text-xs text-gray-500 mb-1 font-medium">{title}</p>
      <h4 className={`text-xl font-bold ${color}`}>{value}</h4>
      <p className="text-xs text-gray-500 mt-1">{sub}</p>
    </div>
    <div className="ml-4 flex-shrink-0">
      {icon}
    </div>
  </Card>
);
