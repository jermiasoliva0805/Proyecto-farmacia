import React from 'react';
import { Search, Calendar } from 'lucide-react';

interface OrderFiltersProps {
    activeTab: string;
    onTabChange: (tab: string) => void;
    searchTerm: string;
    onSearchChange: (val: string) => void;
    dateRange: { from: string; to: string };
    onDateChange: (range: { from: string; to: string }) => void;
}

const OrderFilters: React.FC<OrderFiltersProps> = ({
    activeTab,
    onTabChange,
    searchTerm,
    onSearchChange,
    dateRange,
    onDateChange,
}) => {
    // Lista de estados sincronizada con pedidosService.ts
    const tabs = [
        'Todos',
        'Sin preparar',
        'Preparar pedido',
        'Demorado',
        'Listo para despachar',
        'Despachando',
        'En camino',
        'Entregado',
        'Entrega fallida',
        'Cancelado'
    ];

    return (
        <div className="bg-white p-4 rounded-lg shadow-sm border border-gray-100 space-y-4">
            {/* Mantén tu buscador y filtros avanzados aquí */}
            <div className="flex flex-col md:flex-row gap-4">
                <div className="relative flex-1">
                    <Search className="absolute left-3 top-1/2 -translate-y-1/2 text-gray-400 w-5 h-5" />
                    <input
                        type="text"
                        placeholder="Buscar por ID, cliente, cadete u operario..."
                        className="w-full pl-10 pr-4 py-2 border border-gray-200 rounded-lg focus:ring-2 focus:ring-blue-500 outline-none"
                        value={searchTerm}
                        onChange={(e) => onSearchChange(e.target.value)}
                    />
                </div>

                <div className="flex items-center gap-2">
                    <input
                        type="date"
                        className="px-3 py-2 border border-gray-200 rounded-lg text-sm"
                        value={dateRange.from}
                        onChange={(e) => onDateChange({ ...dateRange, from: e.target.value })}
                    />
                    <span className="text-gray-400">a</span>
                    <input
                        type="date"
                        className="px-3 py-2 border border-gray-200 rounded-lg text-sm"
                        value={dateRange.to}
                        onChange={(e) => onDateChange({ ...dateRange, to: e.target.value })}
                    />
                </div>
            </div>

            {/* Tabs de Estados: Esto es lo que garantiza que el filtro por estado funcione con el backend */}
            <div className="flex flex-wrap gap-2 pt-2 border-t border-gray-50">
                {tabs.map((tab) => (
                    <button
                        key={tab}
                        onClick={() => onTabChange(tab)}
                        className={`px-4 py-1.5 rounded-full text-sm font-medium transition-all ${
                            activeTab === tab
                                ? 'bg-blue-600 text-white shadow-md'
                                : 'bg-gray-100 text-gray-600 hover:bg-gray-200'
                        }`}
                    >
                        {tab}
                    </button>
                ))}
            </div>
        </div>
    );
};

export default OrderFilters;