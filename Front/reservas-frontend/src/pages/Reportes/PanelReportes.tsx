import React, { useState } from 'react';
import { ReporteEntregas } from './ReporteEntregas';
import ReporteOperarios from './ReporteOperarios';
import { RankingClientes } from './RankingClientes';
import { ReporteFacturacion } from './ReporteFacturacion';
import { ReporteCancelados } from './ReporteCancelados';
import { ReporteCancelacionesPorMotivos } from './ReporteCancelacionesPorMotivos'; 

export const PanelReportes = () => {
    const [tabActiva, setTabActiva] = useState('entregas');

    return (
        <div className="p-6">
            <h1 className="text-2xl font-bold text-gray-800 mb-6">Panel de Reportes</h1>

            {/* Selectores de Pestaña - Sin iconos ni emojis */}
            <div className="flex gap-2 mb-6 bg-gray-100 p-1.5 rounded-2xl w-fit flex-wrap">
                <button
                    onClick={() => setTabActiva('entregas')}
                    className={`px-6 py-2 rounded-xl font-bold text-sm transition-all ${
                        tabActiva === 'entregas' ? 'bg-white text-blue-600 shadow-sm' : 'text-gray-500'
                    }`}
                >
                    Entregas por Cadete
                </button>

                <button
                    onClick={() => setTabActiva('operarios')}
                    className={`px-6 py-2 rounded-xl font-bold text-sm transition-all ${
                        tabActiva === 'operarios' ? 'bg-white text-blue-600 shadow-sm' : 'text-gray-500'
                    }`}
                >
                    Rendimiento Operarios
                </button>

                <button
                    onClick={() => setTabActiva('ranking')}
                    className={`px-6 py-2 rounded-xl font-bold text-sm transition-all ${
                        tabActiva === 'ranking' ? 'bg-white text-blue-600 shadow-sm' : 'text-gray-500'
                    }`}
                >
                    Clientes Por Volumen
                </button>
                
                <button
                    onClick={() => setTabActiva('facturacion')}
                    className={`px-6 py-2 rounded-xl font-bold text-sm transition-all ${
                        tabActiva === 'facturacion' ? 'bg-white text-blue-600 shadow-sm' : 'text-gray-500'
                    }`}
                >
                    Clientes por Facturación
                </button>

                <button
                    onClick={() => setTabActiva('cancelados')}
                    className={`px-6 py-2 rounded-xl font-bold text-sm transition-all ${
                        tabActiva === 'cancelados' ? 'bg-white text-blue-600 shadow-sm' : 'text-gray-500'
                    }`}
                >
                    Pedidos Cancelados
                </button>

                <button
                    onClick={() => setTabActiva('motivosCancelacion')}
                    className={`px-6 py-2 rounded-xl font-bold text-sm transition-all ${
                        tabActiva === 'motivosCancelacion' ? 'bg-white text-blue-600 shadow-sm' : 'text-gray-500'
                    }`}
                >
                    Cancelaciones por Motivo
                </button>
            </div>

            {/* Contenido Dinámico */}
            <div className="bg-white rounded-3xl shadow-sm border border-gray-100 p-6">
                {tabActiva === 'entregas' && <ReporteEntregas />}
                {tabActiva === 'operarios' && <ReporteOperarios />}
                {tabActiva === 'ranking' && <RankingClientes />}
                {tabActiva === 'facturacion' && <ReporteFacturacion />}
                {tabActiva === 'cancelados' && <ReporteCancelados />}
                {tabActiva === 'motivosCancelacion' && <ReporteCancelacionesPorMotivos />}
            </div>
        </div>
    );
};