import React, { useState } from 'react';
import { Search, Filter } from 'lucide-react';

interface OrderFiltersProps {
    userRole: string;
    onFilterChange: (filters: any) => void;
}

export const OrderFilters: React.FC<OrderFiltersProps> = ({ userRole, onFilterChange }) => {
    const [activeFilter, setActiveFilter] = useState('Todos');
    const [searchTerm, setSearchTerm] = useState('');

    // 1. Definimos la lista maestra de filtros
    const allFilters = [
        { label: 'Todos', value: 'Todos' },
        { label: 'Sin preparar', value: 'Sin preparar' },
        { label: 'Preparar pedido', value: 'Preparar pedido' },
        { label: 'Demorado', value: 'Demorado' },
        { label: 'Listo para despachar', value: 'Listo para despachar' },
        { label: 'Despachado', value: 'Despachado' },
        { label: 'En camino', value: 'En camino' },
        { label: 'Entregado', value: 'Entregado' },
        { label: 'Entrega fallida', value: 'Entrega fallida' },
        { label: 'Cancelado', value: 'Cancelado' },
    ];

    // 2. Lógica de visibilidad por Rol
    const getFilteredOptions = () => {
        if (userRole === 'Administrador' || userRole === 'Admin') return allFilters;

        if (userRole === 'Operario') {
            return allFilters.filter(f => 
                ['Todos',  'Preparar pedido', 'Listo para despachar', 'Demorado'].includes(f.value)
            );
        }

        if (userRole === 'Cadete') {
            return allFilters.filter(f => 
                ['Todos', 'Despachado', 'En camino', 'Entregado', 'Entrega fallida', 'Cancelado'].includes(f.value)
            );
        }

        return [{ label: 'Todos', value: 'Todos' }]; // Por defecto
    };

    const filters = getFilteredOptions();

    const handleFilterClick = (value: string) => {
        setActiveFilter(value);
        onFilterChange({ estado: value === 'Todos' ? undefined : value, search: searchTerm });
    };

    const handleSearch = (e: React.ChangeEvent<HTMLInputElement>) => {
        const value = e.target.value;
        setSearchTerm(value);
        onFilterChange({ estado: activeFilter === 'Todos' ? undefined : activeFilter, search: value });
    };

    return (
        <div className="flex flex-col lg:flex-row justify-between items-center gap-4 p-2">
            {/* Lista de Filtros Dinámica */}
            <div className="flex flex-wrap gap-2 items-center flex-1 overflow-x-auto pb-2 lg:pb-0 w-full scrollbar-hide">
                {filters.map((filter) => (
                    <button
                        key={filter.value}
                        onClick={() => handleFilterClick(filter.value)}
                        className={`
                            px-4 py-1.5 rounded-full text-sm font-medium transition-all duration-200 whitespace-nowrap
                            ${activeFilter === filter.value
                                ? 'bg-yellow-400 text-blue-900 shadow-md shadow-yellow-200 scale-105 font-bold'
                                : 'bg-gray-100 text-gray-600 hover:bg-gray-200 hover:text-blue-600'
                            }
                        `}
                    >
                        {filter.label}
                    </button>
                ))}
            </div>

            {/* Buscador (Visible para todos) */}
            <div className="flex items-center gap-3 w-full lg:w-auto mt-2 lg:mt-0">
                <div className="relative w-full lg:w-64">
                    <Search className="absolute left-3 top-1/2 transform -translate-y-1/2 text-gray-400 w-4 h-4" />
                    <input
                        type="text"
                        placeholder="Buscar ID o Cliente..."
                        value={searchTerm}
                        onChange={handleSearch}
                        className="w-full pl-10 pr-4 py-2 border border-gray-200 rounded-lg text-sm focus:outline-none focus:ring-2 focus:ring-blue-500/20 focus:border-blue-500 transition-all"
                    />
                </div>
                
                {/* Botón de filtros extra solo para el Admin */}
                {userRole === 'Administrador' && (
                    <button className="flex items-center gap-2 px-4 py-2 border border-gray-200 rounded-lg text-sm font-medium text-gray-700 hover:bg-gray-50 transition-colors">
                        <Filter className="w-4 h-4" />
                        <span>Filtros Avanzados</span>
                    </button>
                )}
            </div>
        </div>
    );
};