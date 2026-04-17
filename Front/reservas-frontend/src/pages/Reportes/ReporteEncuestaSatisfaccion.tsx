import React, { useEffect, useMemo, useRef, useState } from 'react';
import { Download, MessageSquare, ListChecks } from 'lucide-react';
import { Card } from '../../components/common/Card';
import { ExportDialog } from '../../components/ExportDialog';
import { exportToExcel, exportToPDF } from '../../service/exportService';
import { getReporteEncuestaSatisfaccion } from '../../service/reporteService';
import { ReporteEncuestaSatisfaccionDTO } from '../../types/pedido.types';

interface MetricCardProps {
    title: string;
    value: string;
    sub: string;
    icon: React.ReactNode;
    color?: string;
}

export const ReporteEncuestaSatisfaccion: React.FC = () => {
    const [reporte, setReporte] = useState<ReporteEncuestaSatisfaccionDTO | null>(null);
    const [loading, setLoading] = useState(true);
    const [showExportDialog, setShowExportDialog] = useState(false);
    const [isExporting, setIsExporting] = useState(false);
    const contentRef = useRef<HTMLDivElement>(null);

    useEffect(() => {
        const cargarData = async () => {
            try {
                setLoading(true);
                const data = await getReporteEncuestaSatisfaccion();
                setReporte(data);
            } catch (error) {
                console.error('Error al cargar reporte de encuesta:', error);
            } finally {
                setLoading(false);
            }
        };

        cargarData();
    }, []);

    const totalOpciones = useMemo(
        () => reporte?.preguntas.reduce((acc, p) => acc + p.opciones.length, 0) ?? 0,
        [reporte]
    );

    const handleExportExcel = async () => {
        try {
            setIsExporting(true);
            if (!reporte) {
                alert('No hay datos para exportar');
                return;
            }

            const dataExport = reporte.preguntas.flatMap((pregunta) =>
                pregunta.opciones.map((opcion) => ({
                    Pregunta: pregunta.pregunta,
                    Respuesta: opcion.respuesta,
                    Cantidad: opcion.cantidad,
                    Porcentaje: `${opcion.porcentaje.toFixed(2)}%`
                }))
            );

            exportToExcel(dataExport, {
                reportName: 'Encuesta de Satisfacción',
                fileName: 'encuesta-satisfaccion'
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
                reportName: 'Encuesta de Satisfacción',
                fileName: 'encuesta-satisfaccion'
            });
        } finally {
            setIsExporting(false);
            setShowExportDialog(false);
        }
    };

    if (loading) return <p className="p-6 text-gray-500 font-medium">Cargando reporte de encuesta de satisfacción...</p>;
    if (!reporte) return <p className="p-6 text-gray-500 font-medium">No hay datos disponibles</p>;

    return (
        <div className="p-6 bg-[#f8f9fa] min-h-screen">
            <div className="flex justify-between items-center mb-6">
                <div>
                    <h1 className="text-xl font-bold text-gray-800 tracking-tight">Reporte de Encuesta de Satisfacción</h1>
                    <p className="text-sm text-gray-500">Resultados consolidados por pregunta de Google Forms</p>
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
                <div className="grid grid-cols-1 md:grid-cols-3 gap-4 mb-8">
                    <MetricCard
                        title="Respuestas recibidas"
                        value={reporte.totalRespuestas.toString()}
                        sub="Formularios contestados"
                        icon={<MessageSquare className="text-blue-500" size={24} />}
                        color="text-blue-600"
                    />
                    <MetricCard
                        title="Preguntas analizadas"
                        value={reporte.preguntas.length.toString()}
                        sub="Columnas mapeadas"
                        icon={<ListChecks className="text-purple-500" size={24} />}
                        color="text-purple-600"
                    />
                    <MetricCard
                        title="Opciones registradas"
                        value={totalOpciones.toString()}
                        sub="Respuestas distintas"
                        icon={<ListChecks className="text-green-500" size={24} />}
                        color="text-green-600"
                    />
                </div>

                <div className="space-y-4">
                    {reporte.preguntas.map((pregunta, index) => (
                        <Card key={`${pregunta.pregunta}-${index}`} className="p-6">
                            <h3 className="text-sm font-bold text-gray-700 mb-4 uppercase tracking-wider">{pregunta.pregunta}</h3>
                            <p className="text-xs text-gray-500 mb-4">
                                Total respuestas: <strong>{pregunta.totalRespuestas}</strong>
                            </p>
                            <div className="overflow-x-auto">
                                <table className="w-full text-left text-sm">
                                    <thead>
                                        <tr className="text-gray-400 text-xs uppercase border-b border-gray-100">
                                            <th className="pb-3 font-semibold">Respuesta</th>
                                            <th className="pb-3 font-semibold text-center">Cantidad</th>
                                            <th className="pb-3 font-semibold text-right">Porcentaje</th>
                                        </tr>
                                    </thead>
                                    <tbody className="divide-y divide-gray-50">
                                        {pregunta.opciones.map((opcion, opcionIndex) => (
                                            <tr key={`${opcion.respuesta}-${opcionIndex}`} className="hover:bg-gray-50 transition-all">
                                                <td className="py-3 font-medium text-gray-700">{opcion.respuesta}</td>
                                                <td className="py-3 text-center text-gray-600">{opcion.cantidad}</td>
                                                <td className="py-3 text-right font-bold text-gray-900">{opcion.porcentaje.toFixed(2)}%</td>
                                            </tr>
                                        ))}
                                    </tbody>
                                </table>
                            </div>
                        </Card>
                    ))}
                </div>
            </div>

            <ExportDialog
                isOpen={showExportDialog}
                reportName="Encuesta de Satisfacción"
                onExcelClick={handleExportExcel}
                onPdfClick={handleExportPDF}
                onCancel={() => setShowExportDialog(false)}
                isLoading={isExporting}
            />
        </div>
    );
};

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
