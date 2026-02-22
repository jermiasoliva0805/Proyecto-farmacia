import React, { useEffect, useState } from 'react';
import { getRankingClientes } from '@services/reporteService';
import { RankingClienteDTO } from '../../types/reporte.types';
import { Users, Download, Calendar, MapPin, ChevronDown } from 'lucide-react';

export const RankingClientes: React.FC = () => {
  const [ranking, setRanking] = useState<RankingClienteDTO[]>([]);
  const [periodo, setPeriodo] = useState("30");
  const [sucursal, setSucursal] = useState("todas");
  const [loading, setLoading] = useState(true);

  useEffect(() => {
    const fetchData = async () => {
      try {
        setLoading(true);
        const data = await getRankingClientes();
        setRanking(data);
      } catch (error) {
        console.error("Error al cargar ranking:", error);
      } finally {
        setLoading(false);
      }
    };
    fetchData();
  }, [periodo, sucursal]); // Se recarga si cambian los filtros

  return (
    <div className="space-y-6">
      {/* Header del Reporte alineado con el estilo del equipo */}
      <div className="flex justify-between items-center mb-6">
        <div>
          <h1 className="text-xl font-bold text-gray-800 flex items-center gap-2">
            <Users size={22} className="text-blue-600" /> Ranking de Clientes Frecuentes
          </h1>
          <p className="text-sm text-gray-500">Visualiza el Top 10 de clientes por volumen de compra</p>
        </div>
        <button className="bg-black text-white px-5 py-2.5 rounded-xl font-bold text-sm flex items-center gap-2 hover:bg-gray-800 transition-all shadow-md">
          <Download size={18} />
          Exportar Reporte
        </button>
      </div>

      {/* Selectores dinámicos iguales a los de tus compañeras */}
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

      {/* Tarjetas de Resumen */}
      <div className="grid grid-cols-1 md:grid-cols-3 gap-4">
        <MetricSmall title="Top Clientes" value="10" sub="Ranking Actual" />
        <MetricSmall title="Ticket Promedio" value={`$${ranking.length > 0 ? (ranking.reduce((acc, curr) => acc + curr.ticketPromedio, 0) / ranking.length).toFixed(0) : 0}`} sub="Global" color="text-blue-600" />
        <MetricSmall title="Fidelización" value={`${ranking.length}`} sub="Clientes en Top" color="text-green-600" />
      </div>

      {/* Tabla Estilizada */}
      <div className="bg-white rounded-2xl overflow-hidden border border-gray-100">
        <table className="w-full text-left">
          <thead>
            <tr className="bg-gray-50/50 text-gray-400 text-[11px] uppercase tracking-wider border-b border-gray-100">
              <th className="px-6 py-4 font-bold text-center w-16">#</th>
              <th className="px-6 py-4 font-bold">Cliente</th>
              <th className="px-6 py-4 font-bold text-center">Pedidos</th>
              <th className="px-6 py-4 font-bold text-right">Gasto Total</th>
              <th className="px-6 py-4 font-bold text-right">Ticket Promedio</th>
              <th className="px-6 py-4 font-bold text-center">Última Compra</th>
            </tr>
          </thead>
          <tbody className="divide-y divide-gray-50 text-sm">
            {loading ? (
              <tr><td colSpan={6} className="py-10 text-center text-gray-400">Cargando datos...</td></tr>
            ) : ranking.map((item, index) => (
              <tr key={index} className="hover:bg-blue-50/30 transition-all group">
                <td className="px-6 py-4 text-center font-bold text-gray-300 group-hover:text-blue-400">
                  {index + 1}
                </td>
                <td className="px-6 py-4 font-semibold text-gray-700">
                  {item.nombreCliente}
                </td>
                <td className="px-6 py-4 text-center">
                  <span className="bg-gray-100 px-3 py-1 rounded-lg font-bold text-gray-600">
                    {item.cantidadPedidos}
                  </span>
                </td>
                <td className="px-6 py-4 text-right font-bold text-gray-800">
                  ${item.gastoTotal.toLocaleString('es-AR')}
                </td>
                <td className="px-6 py-4 text-right text-green-600 font-medium">
                  ${item.ticketPromedio.toLocaleString('es-AR', { minimumFractionDigits: 2 })}
                </td>
                <td className="px-6 py-4 text-center text-gray-400 text-xs font-medium">
                  {new Date(item.ultimaCompra).toLocaleDateString('es-AR')}
                </td>
              </tr>
            ))}
          </tbody>
        </table>
      </div>
    </div>
  );
};

// Componentes internos para mantener la estructura limpia
const Selector = ({ icon, label, value, options, onChange }: any) => (
  <div className="bg-white p-3 rounded-2xl border border-gray-100 flex items-center justify-between shadow-sm">
    <div className="flex items-center gap-2 text-gray-500 text-sm">
      {icon}
      <span className="font-medium">{label}</span>
      <div className="relative">
        <select
          value={value}
          onChange={onChange}
          className="appearance-none bg-transparent font-bold text-gray-800 pr-6 focus:outline-none cursor-pointer"
        >
          {options.map((opt: any) => (
            <option key={opt.value} value={opt.value}>{opt.label}</option>
          ))}
        </select>
        <ChevronDown size={14} className="absolute right-0 top-1/2 -translate-y-1/2 pointer-events-none text-gray-400" />
      </div>
    </div>
  </div>
);

const MetricSmall = ({ title, value, sub, color = "text-gray-800" }: any) => (
  <div className="bg-white p-5 rounded-3xl border border-gray-100 shadow-sm hover:shadow-md transition-shadow">
    <p className="text-[11px] text-gray-400 font-bold uppercase tracking-wider mb-2">{title}</p>
    <div className="flex items-baseline gap-2">
      <span className={`text-3xl font-black ${color}`}>{value}</span>
    </div>
    <p className="text-[11px] text-gray-400 mt-2 font-medium">{sub}</p>
  </div>
);