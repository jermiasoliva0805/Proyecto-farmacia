import React, { useState } from 'react';
import { Truck, Clock, Users } from 'lucide-react';
import { ReporteEntregas } from './ReporteEntregas';
import ReporteOperarios from './ReporteOperarios';
import { RankingClientes } from './RankingClientes';

export const PanelReportes = () => {
    const [tabActiva, setTabActiva] = useState('entregas');

    return (
        <div className="p-6">
            <h1 className="text-2xl font-bold text-gray-800 mb-6">Panel de Reportes</h1>

            {/* Selectores de Pestaña */}
            <div className="flex gap-2 mb-6 bg-gray-100 p-1.5 rounded-2xl w-fit">
                <button
                    onClick={() => setTabActiva('entregas')}
                    className={`flex items-center gap-2 px-6 py-2 rounded-xl font-bold text-sm transition-all ${
                        tabActiva === 'entregas' ? 'bg-white text-blue-600 shadow-sm' : 'text-gray-500'
                    }`}
                >
                    <Truck size={18} /> Entregas por Cadete
                </button>
                <button
                    onClick={() => setTabActiva('operarios')}
                    className={`flex items-center gap-2 px-6 py-2 rounded-xl font-bold text-sm transition-all ${
                        tabActiva === 'operarios' ? 'bg-white text-blue-600 shadow-sm' : 'text-gray-500'
                    }`}
                >
                    <Clock size={18} /> Rendimiento Operarios
                </button>
                <button
                    onClick={() => setTabActiva('ranking')}
                    className={`flex items-center gap-2 px-6 py-2 rounded-xl font-bold text-sm transition-all ${
                        tabActiva === 'ranking' ? 'bg-white text-blue-600 shadow-sm' : 'text-gray-500'
                    }`}
                >
                    <Users size={18} /> Ranking Clientes
                </button>
            </div>

            {/* Contenido Dinámico */}
            <div className="bg-white rounded-3xl shadow-sm border border-gray-100 p-6">
                {tabActiva === 'entregas' && <ReporteEntregas />}
                {tabActiva === 'operarios' && <ReporteOperarios />}
                {tabActiva === 'ranking' && <RankingClientes />}
            </div>
        </div>
    );
};