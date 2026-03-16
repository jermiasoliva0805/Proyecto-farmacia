import React, { useEffect, useState } from 'react';
import { Calendar, MapPin, AlertCircle, TrendingDown } from 'lucide-react';
import { Card } from '../../components/common/Card';
import { getPedidosCancelados } from '../../service/reporteService';
import { ReportePedidosCanceladosDTO } from '../../types/pedido.types';

export const ReporteCancelados = () => {
    const [reporte, setReporte] = useState<ReportePedidosCanceladosDTO | null>(null);
    const [loading, setLoading] = useState(true);
    const [periodo, setPeriodo] = useState("7");
    const [idSucursal, setIdSucursal] = useState<number | null>(null);

    useEffect(() => {
        const cargarData = async () => {
            try {
                setLoading(true);
                const ahora = new Date();
                const desde = new Date(ahora);
                desde.setDate(desde.getDate() - parseInt(periodo));

                const fechaDesde = desde.toISOString().split('T')[0];
                const fechaHasta = ahora.toISOString().split('T')[0];

                const res = await getPedidosCancelados(fechaDesde, fechaHasta, idSucursal);
                setReporte(res);
            } catch (error) {
                console.error("Error al cargar reporte de cancelados:", error);
            } finally {
                setLoading(false);
            }
        };
        cargarData();
    }, [periodo, idSucursal]);

    if (loading) return <p className="p-6 text-gray-500 font-medium">Cargando reporte de pedidos cancelados...</p>;

    if (!reporte) return <p className="p-6 text-gray-500 font-medium">No hay datos disponibles</p>;

    return (
        <div className="p-6 bg-[#f8f9fa] min-h-screen">
            {/* Header */}
            <div className="flex justify-between items-center mb-6">
                <div>
                    <h1 className="text-xl font-bold text-gray-800 tracking-tight">Reporte de Pedidos Cancelados</h1>
                    <p className="text-sm text-gray-500">Visualiza la cantidad total de pedidos cancelados por intentos de entrega fallida</p>
                </div>
                <button className="bg-black text-white px-5 py-2.5 rounded-xl font-bold text-sm hover:bg-gray-800 transition-all shadow-md">
                    Exportar Reporte
                </button>
            </div>

            {/* Selector de Sucursal */}
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
                    onChange={(e: React.ChangeEvent<HTMLSelectElement>) => 
                        setIdSucursal(e.target.value === "" ? null : parseInt(e.target.value))
                    }
                />
            </div>

            {/* Métricas principales */}
            <div className="grid grid-cols-1 md:grid-cols-3 gap-4 mb-8">
                <MetricCard 
                    title="Total de Cancelados"
                    value={reporte.totalPedidosCancelados.toString()}
                    sub="Cantidad de pedidos"
                    icon={<AlertCircle className="text-red-500" size={24} />}
                    color="text-red-600"
                />
                <MetricCard 
                    title="Porcentaje del Total"
                    value={`${reporte.porcentajeDelTotal.toFixed(2)}%`}
                    sub="Del total de pedidos"
                    icon={<TrendingDown className="text-orange-500" size={24} />}
                    color="text-orange-600"
                />
                <MetricCard 
                    title="Monto Total Cancelado"
                    value={`$${reporte.montoTotalCancelado.toLocaleString('es-AR')}`}
                    sub="Dinero en cancelaciones"
                    icon={<AlertCircle className="text-yellow-500" size={24} />}
                    color="text-yellow-600"
                />
            </div>

            {/* Información adicional */}
            <Card className="p-6">
                <h3 className="text-sm font-bold text-gray-700 mb-4 uppercase tracking-wider">Información</h3>
                <p className="text-gray-600 text-sm">
                    Este reporte muestra los pedidos cancelados <strong>automáticamente después de 3 intentos de entrega fallida</strong>.
                    Para ver cancelaciones manuales por motivo, ir a "Cancelaciones por Motivo".
                </p>
            </Card>
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
