import React, { useState, useEffect } from 'react';
import { Card } from '../../components/common/Card';
import { 
 BarChart, 
Bar, 
XAxis, 
YAxis, 
CartesianGrid, 
Tooltip, 
ResponsiveContainer 
} from 'recharts'; // Asegúrate de instalar recharts: npm install recharts
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

  useEffect(() => {
    // Simulación de carga de datos desde tu API
    const fetchReporte = async () => {
      try {
        const response = await fetch('https://localhost:7164/api/Reporte/entregas-cadete');
        const data = await response.json();
        setReporte(data);
      } catch (error) {
        console.error("Error:", error);
      } finally {
        setLoading(false);
      }
    };
    fetchReporte();
  }, []);

  // Totales para las tarjetas superiores
  const totalPedidos = reporte.reduce((acc, c) => acc + c.totalPedidosAsignados, 0);
  const totalExito = reporte.reduce((acc, c) => acc + c.entregasExitosas, 0);
  const tasaExitoGlobal = totalPedidos > 0 ? ((totalExito / totalPedidos) * 100).toFixed(1) : 0;

  return (
    <div className="p-6 bg-[#f8f9fa] min-h-screen">
      <div className="flex justify-between items-center mb-6">
        <div>
          <h1 className="text-xl font-bold text-gray-800">Reportes y Análisis</h1>
          <p className="text-sm text-gray-500">Visualiza métricas y estadísticas de rendimiento</p>
        </div>
        <button className="bg-black text-white px-4 py-2 rounded-lg text-sm flex items-center gap-2">
           <DollarSign size={16} /> Exportar Reporte
        </button>
      </div>

      {/* Selectores de Periodo y Sucursal */}
      <div className="grid grid-cols-1 md:grid-cols-2 gap-4 mb-6">
        <div className="bg-white p-3 rounded-xl border border-gray-200 flex items-center justify-between">
          <div className="flex items-center gap-2 text-gray-500 text-sm">
            <Calendar size={18} /> <span>Periodo:</span>
            <span className="font-semibold text-gray-800">Últimos 7 días</span>
          </div>
        </div>
        <div className="bg-white p-3 rounded-xl border border-gray-200 flex items-center justify-between">
          <div className="flex items-center gap-2 text-gray-500 text-sm">
            <MapPin size={18} /> <span>Sucursal:</span>
            <span className="font-semibold text-gray-800">Todas las sucursales</span>
          </div>
        </div>
      </div>

      {/* Tarjetas de Métricas */}
      <div className="grid grid-cols-1 md:grid-cols-4 gap-4 mb-8">
        <MetricCard title="Total Pedidos" value={totalPedidos} sub="En progreso" icon={<Package className="text-gray-400" />} />
        <MetricCard title="Tasa de Éxito" value={`${tasaExitoGlobal}%`} sub="Entregados" icon={<CheckCircle2 className="text-green-500" />} color="text-green-500" />
        <MetricCard title="Tasa de Fallo" value="0.0%" sub="0 fallidos" icon={<XCircle className="text-red-500" />} color="text-red-500" />
        <MetricCard title="Ingresos Totales" value={`$${reporte.reduce((acc, c) => acc + c.totalRecaudado, 0)}`} sub="Promedio: $5745.38" icon={<DollarSign className="text-gray-400" />} />
      </div>

      {/* Gráfico de Barras Horizontales */}
      <Card className="p-6 mb-8">
        <h3 className="text-sm font-bold text-gray-700 mb-6 flex items-center gap-2">
          <TrendingUp size={18} /> Pedidos por Cadete (Top 10)
        </h3>
        <div className="h-64 w-full">
          <ResponsiveContainer width="100%" height="100%">
            <BarChart layout="vertical" data={reporte} margin={{ left: 40, right: 30 }}>
              <CartesianGrid strokeDasharray="3 3" horizontal={false} />
              <XAxis type="number" hide />
              <YAxis dataKey="nombreCadete" type="category" width={100} tick={{fontSize: 12}} />
              <Tooltip cursor={{fill: 'transparent'}} />
              <Bar dataKey="totalPedidosAsignados" fill="#8b5cf6" radius={[0, 4, 4, 0]} barSize={40} />
            </BarChart>
          </ResponsiveContainer>
        </div>
      </Card>

      {/* Tabla de Rendimiento */}
      <Card className="p-6">
        <h3 className="text-sm font-bold text-gray-700 mb-6">Rendimiento de Cadetes</h3>
        <table className="w-full text-left">
          <thead>
            <tr className="text-gray-400 text-xs uppercase border-b border-gray-100">
              <th className="pb-4 font-semibold">Cadete</th>
              <th className="pb-4 font-semibold text-center">Pedidos Asignados</th>
              <th className="pb-4 font-semibold text-center">Entregados</th>
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

// Componente pequeño para las tarjetas de arriba
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

const TrendingUp = ({ size }: { size: number }) => (
  <svg width={size} height={size} viewBox="0 0 24 24" fill="none" stroke="currentColor" strokeWidth="2" strokeLinecap="round" strokeLinejoin="round"><polyline points="23 6 13.5 15.5 8.5 10.5 1 18"></polyline><polyline points="17 6 23 6 23 12"></polyline></svg>
);