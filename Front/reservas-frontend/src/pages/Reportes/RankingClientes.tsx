import React, { useEffect, useState } from 'react';
import { Card } from '@components/common/Card'; // Usando Alias
import { getRankingClientes } from '@services/reporteService'; // Usando Alias
import { RankingClienteDTO } from '../../types/reporte.types';
import { Award, Calendar, TrendingUp } from 'lucide-react';

export const RankingClientes: React.FC = () => {
  const [ranking, setRanking] = useState<RankingClienteDTO[]>([]);
  const [loading, setLoading] = useState(true);

  useEffect(() => {
    const fetchData = async () => {
      try {
        const data = await getRankingClientes();
        setRanking(data);
      } catch (error) {
        console.error("Error al cargar el ranking:", error);
      } finally {
        setLoading(false);
      }
    };
    fetchData();
  }, []);

  if (loading) {
    return (
      <div className="flex justify-center items-center p-10 bg-white rounded-xl border border-gray-100">
        <div className="animate-spin rounded-full h-8 w-8 border-b-2 border-blue-600"></div>
        <span className="ml-3 text-gray-500 text-sm">Cargando reporte de volumen...</span>
      </div>
    );
  }

  return (
    <Card className="p-6 overflow-hidden bg-white shadow-sm border border-gray-100">
      <div className="flex items-center justify-between mb-6">
        <div className="flex items-center gap-2">
          <div className="p-2 bg-yellow-50 rounded-lg">
            <Award className="text-yellow-600" size={20} />
          </div>
          <div>
            <h3 className="text-sm font-bold text-gray-800">Ranking Clientes Frecuentes</h3>
            <p className="text-[10px] text-gray-400 uppercase">Top 10 por cantidad de pedidos</p>
          </div>
        </div>
        <TrendingUp size={18} className="text-gray-300" />
      </div>

      <div className="overflow-x-auto">
        <table className="w-full text-left">
          <thead>
            <tr className="text-gray-400 text-[10px] uppercase tracking-wider border-b border-gray-50">
              <th className="pb-3 font-semibold">Cliente</th>
              <th className="pb-3 font-semibold text-center">Pedidos</th>
              <th className="pb-3 font-semibold text-right">Gasto Total</th>
              <th className="pb-3 font-semibold text-right">Ticket Prom.</th>
              <th className="pb-3 font-semibold text-center uppercase">Últ. Compra</th>
            </tr>
          </thead>
          <tbody className="divide-y divide-gray-50 text-sm">
            {ranking.length > 0 ? (
              ranking.map((item, index) => (
                <tr key={index} className="hover:bg-blue-50/30 transition-all group">
                  <td className="py-4">
                    <div className="flex items-center gap-3">
                      <span className="text-[10px] font-bold text-gray-300 w-4">
                        {String(index + 1).padStart(2, '0')}
                      </span>
                      <span className="font-medium text-gray-700 group-hover:text-blue-600 transition-colors">
                        {item.nombreCliente}
                      </span>
                    </div>
                  </td>
                  <td className="py-4 text-center">
                    <span className="bg-gray-100 text-gray-600 px-2 py-0.5 rounded text-xs font-bold">
                      {item.cantidadPedidos}
                    </span>
                  </td>
                  <td className="py-4 text-right font-semibold text-gray-700">
                    ${item.gastoTotal.toLocaleString('es-AR')}
                  </td>
                  <td className="py-4 text-right text-green-600 font-medium">
                    ${item.ticketPromedio.toLocaleString('es-AR', { minimumFractionDigits: 2 })}
                  </td>
                  <td className="py-4 text-center">
                    <div className="flex items-center justify-center gap-1 text-[10px] text-gray-400">
                      <Calendar size={10} />
                      {new Date(item.ultimaCompra).toLocaleDateString('es-AR')}
                    </div>
                  </td>
                </tr>
              ))
            ) : (
              <tr>
                <td colSpan={5} className="py-10 text-center text-gray-400 text-xs">
                  Sin datos de entregas para mostrar.
                </td>
              </tr>
            )}
          </tbody>
        </table>
      </div>
    </Card>
  );
};