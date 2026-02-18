import React, { useState, useEffect } from 'react';
import { Calendar, MapPin, Download, Clock, User, CheckCircle } from 'lucide-react';

interface ReporteOperario {
    nombreOperario: string;
    totalPedidosArmados: number;
    tiempoPromedioMinutos: number;
}

const ReporteOperarios = () => {
    const [datos, setDatos] = useState<ReporteOperario[]>([]);
    const [loading, setLoading] = useState(true);

    useEffect(() => {
        const fetchDatos = async () => {
            try {
                const response = await fetch('http://localhost:5000/api/Orders/reporte-tiempos-operarios', {
                    headers: {
                        'Authorization': `Bearer ${localStorage.getItem('token')}`
                    }
                });
                if (response.ok) {
                    const data = await response.json();
                    setDatos(data);
                }
            } catch (error) {
                console.error("Error al cargar reporte:", error);
            } finally {
                setLoading(false);
            }
        };

        fetchDatos();
    }, []);

    return (
        <div>
            {/* SECCIÓN DE FILTROS (Lo que querías agregar) */}
            <div className="flex flex-col md:flex-row justify-between items-end mb-8 gap-4">
                <div className="grid grid-cols-1 md:grid-cols-2 gap-4 flex-1 w-full">
                    {/* Filtro Periodo */}
                    <div className="flex items-center gap-3 bg-gray-50 p-3 rounded-2xl border border-gray-100">
                        <Calendar className="text-blue-500" size={20} />
                        <div className="flex flex-col">
                            <span className="text-[10px] text-gray-400 uppercase font-bold">Periodo</span>
                            <select className="bg-transparent font-bold text-sm focus:outline-none cursor-pointer">
                                <option>Últimos 7 días</option>
                                <option>Últimos 30 días</option>
                                <option>Mes actual</option>
                            </select>
                        </div>
                    </div>
                    {/* Filtro Sucursal */}
                    <div className="flex items-center gap-3 bg-gray-50 p-3 rounded-2xl border border-gray-100">
                        <MapPin className="text-purple-500" size={20} />
                        <div className="flex flex-col">
                            <span className="text-[10px] text-gray-400 uppercase font-bold">Sucursal</span>
                            <select className="bg-transparent font-bold text-sm focus:outline-none cursor-pointer">
                                <option>Todas las sucursales</option>
                                <option>Sucursal Centro</option>
                                <option>Sucursal Norte</option>
                            </select>
                        </div>
                    </div>
                </div>
                
                <button className="bg-black text-white p-3 rounded-xl hover:bg-gray-800 transition-all active:scale-95 shadow-lg">
                    <Download size={20} />
                </button>
            </div>

            {/* TABLA DE DATOS (Tu lógica original) */}
            {loading ? (
                <div className="flex justify-center py-10">
                    <div className="animate-spin rounded-full h-8 w-8 border-b-2 border-blue-600"></div>
                </div>
            ) : (
                <div className="overflow-x-auto">
                    <table className="w-full text-left border-separate border-spacing-y-2">
                        <thead>
                            <tr className="text-gray-400 text-sm uppercase tracking-wider">
                                <th className="px-4 py-2 font-medium">Operario</th>
                                <th className="px-4 py-2 font-medium">Pedidos Armados</th>
                                <th className="px-4 py-2 font-medium">Promedio de Armado</th>
                            </tr>
                        </thead>
                        <tbody>
                            {datos.map((op, index) => (
                                <tr key={index} className="bg-white border border-gray-100 rounded-xl shadow-sm hover:bg-gray-50 transition-colors">
                                    <td className="px-4 py-4 rounded-l-xl">
                                        <div className="flex items-center gap-3">
                                            <div className="bg-blue-100 p-2 rounded-full">
                                                <User size={16} className="text-blue-600" />
                                            </div>
                                            <span className="font-bold text-gray-700">{op.nombreOperario}</span>
                                        </div>
                                    </td>
                                    <td className="px-4 py-4">
                                        <div className="flex items-center gap-2">
                                            <CheckCircle size={16} className="text-green-500" />
                                            <span className="font-semibold text-gray-600">{op.totalPedidosArmados}</span>
                                        </div>
                                    </td>
                                    <td className="px-4 py-4 rounded-r-xl">
                                        <div className="flex items-center gap-2">
                                            <Clock size={16} className="text-orange-500" />
                                            <span className={`px-3 py-1 rounded-lg font-bold ${
                                                op.tiempoPromedioMinutos > 20 ? 'bg-red-50 text-red-600' : 'bg-green-50 text-green-600'
                                            }`}>
                                                {op.tiempoPromedioMinutos} min
                                            </span>
                                        </div>
                                    </td>
                                </tr>
                            ))}
                        </tbody>
                    </table>
                </div>
            )}
        </div>
    );
};

export default ReporteOperarios;