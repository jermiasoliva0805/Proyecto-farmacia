import React, { useEffect, useMemo, useRef, useState } from 'react';
import { Download, Users, BarChart3 } from 'lucide-react';
import { Card } from '../../components/common/Card';
import { ExportDialog } from '../../components/ExportDialog';
import { exportToExcel, exportToPDF } from '../../service/exportService';
import { getReporteEncuestaSatisfaccion } from '../../service/reporteService';
import { ReporteEncuestaSatisfaccionDTO } from '../../types/pedido.types';

interface MetricCardProps {
    title: string;
    value: string;
    icon: React.ReactNode;
    color?: string;
    bgColor?: string;
}

interface ColorConfig {
    bar: string;
    bg: string;
    text: string;
}

// Mapeo de números a nombres de satisfacción
const mapNumberToSatisfaction = (value: string | number): string => {
    const numValue = typeof value === 'string' ? parseFloat(value) : value;
    
    if (!isNaN(numValue)) {
        switch (Math.round(numValue)) {
            case 1: return 'Mala';
            case 2: return 'Regular';
            case 3: return 'Buena';
            case 4: return 'Muy buena';
            case 5: return 'Excelente';
            default: return value.toString();
        }
    }
    return value.toString();
};

// Mapeo de opciones a colores
const getColorForOption = (respuesta: string): ColorConfig => {
    const respuestaLower = respuesta.toLowerCase().trim();
    
    // Escala de satisfacción (rojo -> naranja -> amarillo -> verde claro -> verde)
    if (respuestaLower === 'mala') {
        return {
            bar: 'from-red-500 to-red-600',
            bg: 'bg-red-100',
            text: 'text-red-700'
        };
    }
    if (respuestaLower === 'regular') {
        return {
            bar: 'from-orange-500 to-orange-600',
            bg: 'bg-orange-100',
            text: 'text-orange-700'
        };
    }
    if (respuestaLower === 'buena') {
        return {
            bar: 'from-amber-500 to-amber-600',
            bg: 'bg-amber-100',
            text: 'text-amber-700'
        };
    }
    if (respuestaLower === 'muy buena') {
        return {
            bar: 'from-lime-500 to-lime-600',
            bg: 'bg-lime-100',
            text: 'text-lime-700'
        };
    }
    if (respuestaLower === 'excelente' || respuestaLower === 'si') {
        return {
            bar: 'from-green-500 to-green-600',
            bg: 'bg-green-100',
            text: 'text-green-700'
        };
    }
    if (respuestaLower === 'no') {
        return {
            bar: 'from-red-500 to-red-600',
            bg: 'bg-red-100',
            text: 'text-red-700'
        };
    }
    if (respuestaLower === 'tal vez') {
        return {
            bar: 'from-amber-500 to-amber-600',
            bg: 'bg-amber-100',
            text: 'text-amber-700'
        };
    }
    
    // Color por defecto
    return {
        bar: 'from-blue-500 to-blue-600',
        bg: 'bg-blue-100',
        text: 'text-blue-700'
    };
};

export const ReporteEncuestaSatisfaccion: React.FC = () => {
    const [reporte, setReporte] = useState<ReporteEncuestaSatisfaccionDTO | null>(null);
    const [loading, setLoading] = useState(true);
    const [showExportDialog, setShowExportDialog] = useState(false);
    const [isExporting, setIsExporting] = useState(false);
    const contentRef = useRef<HTMLDivElement>(null);

    // Función para filtrar preguntas vacías o inválidas
    const isValidQuestion = (pregunta: string): boolean => {
        const preguntaLower = pregunta.toLowerCase().trim();
        // Filtrar preguntas vacías, columnas genéricas, mails y otros campos inválidos
        const invalidPatterns = ['columna', 'mail', 'email', 'sin contenido', 'timestamps'];
        return !invalidPatterns.some(pattern => preguntaLower.includes(pattern)) && pregunta.trim().length > 0;
    };

    const preguntasValidas = useMemo(() => {
        return reporte?.preguntas.filter(p => isValidQuestion(p.pregunta)).map(pregunta => {
            // Consolidar opciones duplicadas (ej: "5" y "Excelente" -> "Excelente")
            const opcionesConsolidadas = new Map();
            
            pregunta.opciones.forEach(opcion => {
                const nombreMapeado = mapNumberToSatisfaction(opcion.respuesta);
                const clave = nombreMapeado.toLowerCase();
                
                if (opcionesConsolidadas.has(clave)) {
                    // Sumar si ya existe
                    const existing = opcionesConsolidadas.get(clave);
                    existing.cantidad += opcion.cantidad;
                    existing.porcentaje += opcion.porcentaje;
                } else {
                    opcionesConsolidadas.set(clave, {
                        respuesta: nombreMapeado,
                        cantidad: opcion.cantidad,
                        porcentaje: opcion.porcentaje
                    });
                }
            });
            
            return {
                ...pregunta,
                opciones: Array.from(opcionesConsolidadas.values())
            };
        }) || [];
    }, [reporte]);

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

    const handleExportExcel = async () => {
        try {
            setIsExporting(true);
            if (!reporte) {
                alert('No hay datos para exportar');
                return;
            }

            const dataExport = preguntasValidas.flatMap((pregunta) =>
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
            <div className="flex justify-between items-center mb-8">
                <div>
                    <h1 className="text-2xl font-bold text-gray-900 tracking-tight">Encuesta de Satisfacción</h1>
                    <p className="text-sm text-gray-500 mt-1">Análisis de respuestas de clientes</p>
                </div>
                <button
                    onClick={() => setShowExportDialog(true)}
                    className="bg-gradient-to-r from-blue-600 to-blue-700 hover:from-blue-700 hover:to-blue-800 text-white px-6 py-2.5 rounded-lg font-semibold text-sm flex items-center gap-2 transition-all shadow-md hover:shadow-lg"
                >
                    <Download size={18} />
                    Exportar Reporte
                </button>
            </div>

            <div ref={contentRef}>
                <div className="grid grid-cols-1 md:grid-cols-2 gap-6 mb-10">
                    <MetricCard
                        title="Clientes que respondieron"
                        value={reporte.cantidadClientesRespondieron.toString()}
                        icon={<Users className="w-8 h-8" />}
                        color="text-blue-600"
                        bgColor="bg-blue-50"
                    />
                    <MetricCard
                        title="Cantidad de encuestas enviadas"
                        value={(reporte.cantidadEncuestasEnviadas ?? reporte.cantidadTotalRespuestas ?? 0).toString()}
                        icon={<BarChart3 className="w-8 h-8" />}
                        color="text-emerald-600"
                        bgColor="bg-emerald-50"
                    />
                </div>

                <div className="space-y-6">
                    {preguntasValidas.map((pregunta, index) => (
                        <Card key={`${pregunta.pregunta}-${index}`} className="p-6 border border-gray-200">
                            <div className="mb-6">
                                <h3 className="text-base font-bold text-gray-900">{pregunta.pregunta}</h3>
                                <p className="text-xs text-gray-500 mt-1">
                                    {pregunta.totalRespuestas} respuestas
                                </p>
                            </div>
                            <div className="space-y-3">
                                {pregunta.opciones.map((opcion, opcionIndex) => {
                                    const colorConfig = getColorForOption(opcion.respuesta);
                                    return (
                                        <div key={`${opcion.respuesta}-${opcionIndex}`} className="flex items-center gap-3">
                                            <div className="flex-1">
                                                <div className="flex justify-between mb-2 items-center">
                                                    <span className={`text-sm font-semibold ${colorConfig.text} ${colorConfig.bg} px-3 py-1 rounded-lg`}>
                                                        {opcion.respuesta}
                                                    </span>
                                                    <div className="flex gap-6 text-right">
                                                        <div>
                                                            <p className="text-xs text-gray-500 font-semibold">Cantidad</p>
                                                            <p className="text-sm font-bold text-gray-900">{opcion.cantidad}</p>
                                                        </div>
                                                        <div>
                                                            <p className="text-xs text-gray-500 font-semibold">Porcentaje</p>
                                                            <p className="text-sm font-bold text-gray-900">{opcion.porcentaje.toFixed(1)}%</p>
                                                        </div>
                                                    </div>
                                                </div>
                                                <div className="w-full bg-gray-200 rounded-full h-3">
                                                    <div 
                                                        className={`bg-gradient-to-r ${colorConfig.bar} h-3 rounded-full transition-all`}
                                                        style={{ width: `${opcion.porcentaje}%` }}
                                                    ></div>
                                                </div>
                                            </div>
                                        </div>
                                    );
                                })}
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

const MetricCard = ({ title, value, icon, color = 'text-gray-900', bgColor = 'bg-gray-50' }: MetricCardProps) => (
    <Card className={`p-6 border-0 shadow-sm ${bgColor}`}>
        <div className="flex items-start justify-between gap-4">
            <div className="flex-1">
                <p className="text-xs text-gray-600 font-semibold uppercase tracking-wider mb-2">{title}</p>
                <h4 className={`text-4xl font-bold ${color}`}>{value}</h4>
            </div>
            <div className={`${color} opacity-20`}>
                {icon}
            </div>
        </div>
    </Card>
);
