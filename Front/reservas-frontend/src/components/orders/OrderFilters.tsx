import React, { useState, useEffect } from 'react';
import { Search, Filter, X } from 'lucide-react';

interface OrderFiltersProps {
    userRole: 'Administrador' | 'Operario' | 'Cadete';
    onFilterChange: (filters: any) => void;
}

export const OrderFilters: React.FC<OrderFiltersProps> = ({ userRole, onFilterChange }) => {
    // 1. Definimos los estados posibles (deben coincidir con el switch del service)
    const statuses = [
        'Todos', 
        'Sin preparar', 
        'Preparar pedido', 
        'Listo para despachar', 
        'En camino', 
        'Entregado', 
        'Cancelado'
    ];

    // 2. Estado local de los filtros
    const [filters, setFilters] = useState({
        search: '',
        estado: 'Todos',
        idOperario: '',
        idCadete: '',
        fechaDesde: '',
        fechaHasta: ''
    });

    // 3. Efecto para avisar al Dashboard cada vez que un filtro cambie
    useEffect(() => {
        // Usamos un pequeño delay para la búsqueda por texto (debounce)
        const handler = setTimeout(() => {
            onFilterChange(filters);
        }, 300);

        return () => clearTimeout(handler);
    }, [filters]);

    const handleStatusClick = (selectedStatus: string) => {
        setFilters(prev => ({ ...prev, estado: selectedStatus }));
    };

    const handleInputChange = (e: React.ChangeEvent<HTMLInputElement | HTMLSelectElement>) => {
        const { name, value } = e.target;
        setFilters(prev => ({ ...prev, [name]: value }));
    };

    return (
        <div className="bg-white rounded-xl shadow-sm border border-gray-200 p-4 mb-6">
            <div className="flex flex-col gap-4">
                
                {/* FILA 1: BOTONES DE ESTADO */}
                <div className="flex items-center gap-2 overflow-x-auto pb-2 scrollbar-hide">
                    {statuses.map(status => (
                        <button
                            type="button"
                            key={status}
                            onClick={() => handleStatusClick(status)}
                            className={`px-4 py-2 rounded-full text-sm font-medium whitespace-nowrap transition-colors
                                ${filters.estado === status 
                                    ? 'bg-blue-600 text-white shadow-md' 
                                    : 'bg-gray-100 text-gray-600 hover:bg-gray-200'}`}
                        >
                            {status}
                        </button>
                    ))}
                </div>

                {/* FILA 2: BARRA DE BÚSQUEDA Y OTROS FILTROS */}
                <div className="flex flex-col sm:flex-row gap-3">
                    <div className="relative flex-1">
                        <Search className="absolute left-3 top-2.5 text-gray-400" size={18} />
                        <input 
                            type="text"
                            name="search"
                            value={filters.search}
                            onChange={handleInputChange}
                            placeholder="Buscar por ID de pedido o cliente..." 
                            className="w-full pl-10 pr-4 py-2 border border-gray-300 rounded-lg focus:ring-2 focus:ring-blue-500 outline-none"
                        />
                    </div>
                    
                    {/* Filtros de Fecha (Opcionales pero útiles) */}
                    <div className="flex gap-2">
                        <input 
                            type="date" 
                            name="fechaDesde"
                            value={filters.fechaDesde}
                            onChange={handleInputChange}
                            className="p-2 border border-gray-300 rounded-lg text-sm outline-none"
                        />
                        <input 
                            type="date" 
                            name="fechaHasta"
                            value={filters.fechaHasta}
                            onChange={handleInputChange}
                            className="p-2 border border-gray-300 rounded-lg text-sm outline-none"
                        />
                    </div>
                </div>
            </div>
        </div>
    );
};