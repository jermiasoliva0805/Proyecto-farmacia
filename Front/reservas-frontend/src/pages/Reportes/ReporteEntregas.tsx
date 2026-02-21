import React, { useState, useEffect } from 'react';
import { Card } from '../../components/common/Card';
import { Package, CheckCircle2, XCircle, DollarSign, Calendar, MapPin } from 'lucide-react';

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

  // Filtros dinámicos
  const [periodo, setPeriodo] = useState("7"); // últimos 7 días por defecto
  const [sucursal, setSucursal] = useState("todas"); // todas las sucursales por defecto

  // Calculamos fechas desde/hasta según el periodo
  const fechaHasta = new Date();
  const fechaDesde = new Date();
  fechaDesde.setDate(fechaHasta.getDate() - parseInt(periodo));

  const fechaDesdeStr = fechaDesde.toISOString().split("T")[0];
  const fechaHastaStr = fechaHasta.toISOString().split("T")[0];

  useEffect(() => {
  const fetchReporte = async () => {
    try {
      setLoading(true);
      const url = `http://localhost:5000/api/Reporte/entregas-cadete?fechaDesde=${fechaDesdeStr}&fechaHasta=${fechaHastaStr}&sucursal=${sucursal}`;
      
      console.log("[LOG] Fetching reporte desde:", url); // 🔎 Log URL
      
      const response = await fetch(url);
      if (!response.ok) throw new Error("Error en la petición");
      
      const data = await response.json();
      console.log("[LOG] Respuesta del backend:", data); // 🔎 Log datos recibidos
      setReporte(data);
    } catch (error) {
      console.error("[LOG] Error en fetchReporte:", error);
    } finally {
      setLoading(false);
    }
  };



  fetchReporte();
}, [periodo, sucursal]); // Se ejecutará cada vez que cambies el tiempo o la sucursal
  // Totales
  const totalPedidos = reporte.reduce((acc, c) => acc + c.totalPedidosAsignados, 0);
  const totalExito = reporte.reduce((acc, c) => acc + c.entregasExitosas, 0);
  const totalFallidos = reporte.reduce((acc, c) => acc + c.entregasFallidas, 0);
  const tasaExitoGlobal = totalPedidos > 0 ? ((totalExito / totalPedidos) * 100).toFixed(1) : "0.0";
  const tasaFalloGlobal = totalPedidos > 0 ? ((totalFallidos / totalPedidos) * 100).toFixed(1) : "0.0";
  const ingresosTotales = reporte.reduce((acc, c) => acc + c.totalRecaudado, 0);
  const ingresoPromedio = reporte.length > 0 ? (ingresosTotales / reporte.length).toFixed(2) : "0.00";

  if (loading) {
    return <p className="p-6 text-gray-500">Cargando reporte...</p>;
  }

  return (
    <div className="p-6 bg-[#f8f9fa] min-h-screen">
      {/* Header */}
      <div className="flex justify-between items-center mb-6">
        <div>
          <h1 className="text-xl font-bold text-gray-800">Reportes y Análisis</h1>
          <p className="text-sm text-gray-500">Visualiza métricas y estadísticas de rendimiento</p>
        </div>
        <button className="bg-black text-white px-4 py-2 rounded-lg text-sm flex items-center gap-2">
          <DollarSign size={16} /> Exportar Reporte
        </button>
      </div>

      {/* Selectores dinámicos */}
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
          value={sucursal}
          options={[
            { value: "todas", label: "Todas las sucursales" },
            { value: "centro", label: "Sucursal Centro" },
            { value: "norte", label: "Sucursal Norte" },
          ]}
          onChange={(e: React.ChangeEvent<HTMLSelectElement>) => setSucursal(e.target.value)}
        />
      </div>

      {/* Métricas */}
      <div className="grid grid-cols-1 md:grid-cols-4 gap-4 mb-8">
        <MetricCard title="Total Pedidos" value={totalPedidos} sub="En progreso" icon={<Package className="text-gray-400" />} />
        <MetricCard title="Tasa de Éxito" value={`${tasaExitoGlobal}%`} sub={`${totalExito} entregados`} icon={<CheckCircle2 className="text-green-500" />} color="text-green-500" />
        <MetricCard title="Tasa de Fallo" value={`${tasaFalloGlobal}%`} sub={`${totalFallidos} fallidos`} icon={<XCircle className="text-red-500" />} color="text-red-500" />
        <MetricCard title="Ingresos Totales" value={`$${ingresosTotales}`} sub={`Promedio: $${ingresoPromedio}`} icon={<DollarSign className="text-gray-400" />} />
      </div>

      {/* Tabla */}
      <Card className="p-6">
        <h3 className="text-sm font-bold text-gray-700 mb-6">Rendimiento de Cadetes</h3>
        <table className="w-full text-left">
          <thead>
            <tr className="text-gray-400 text-xs uppercase border-b border-gray-100">
              <th className="pb-4 font-semibold">Cadete</th>
              <th className="pb-4 font-semibold text-center">Pedidos Asignados</th>
              <th className="pb-4 font-semibold text-center text-blue-500">Entregados</th>
              <th className="pb-4 font-semibold text-center text-red-400">Fallidos</th>
              <th className="pb-4 font-semibold text-right">Tasa de Éxito</th>
            </tr>
          </thead>
          <tbody className="divide-y divide-gray-50 text-sm">
            {reporte.map((item) => (
              <tr key={item.idCadete} className="hover:bg-gray-50 transition-all">
                <td className="py-4 font-medium text-gray-700">{item.nombreCadete}</td>
                <td className="py-4 text-center">{item.totalPedidosAsignados}</td>
                <td className="py-4 text-center text-blue-500 font-semibold">{item.entregasExitosas}</td>
                <td className="py-4 text-center text-red-400">{item.entregasFallidas}</td>
                <td className="py-4 text-right font-bold text-green-600">
                  {item.porcentajeEfectividad.toFixed(1)}%
                </td>
              </tr>
            ))}
          </tbody>
        </table>
      </Card>
    </div>
  );
};

// Componentes auxiliares
const MetricCard = ({ title, value, sub, icon, color = "text-gray-900" }: any) => (
  <Card className="p-5 flex justify-between items-start">
    <div>
      <p className="text-xs text-gray-500 mb-1">{title}</p>
      <h4 className={`text-xl font-bold ${color}`}>{value}</h4>
      <p className="text-[10px] text-gray-400 mt-2 uppercase">{sub}</p>
    </div>
    <div className="bg-gray-50 p-2 rounded-lg">{icon}</div>
  </Card>
);

const Selector = ({ icon, label, value, options, onChange }: any) => (
  <div className="bg-white p-3 rounded-xl border border-gray-200 flex items-center justify-between">
    <div className="flex items-center gap-2 text-gray-500 text-sm">
      {icon} <span>{label}</span>
    </div>
    <select
      value={value}
      onChange={onChange}
      className="ml-2 px-2 py-1 border border-gray-300 rounded-md text-sm focus:ring-2 focus:ring-blue-500"
    >
      {options.map((opt: any) => (
        <option key={opt.value} value={opt.value}>
          {opt.label}
        </option>
      ))}
    </select>
  </div>
  
);
