import React, { useState } from 'react';
import { Search, Clock } from 'lucide-react';

interface OrderFiltersProps {
  userRole: string;
  onFilterChange: (filters: {
    estado?: string;
    search?: string;
    soloDemorados?: boolean;
  }) => void;
}

/**
 * Filtros de pedidos.
 *
 * CAMBIO: "Demorado" ya NO es un estado principal de filtrado.
 * Se agrega un toggle "Solo demorados" independiente del filtro de estado,
 * de modo que ambos pueden aplicarse simultáneamente (ej: "En camino" + demorados).
 */
export const OrderFilters: React.FC<OrderFiltersProps> = ({
  userRole,
  onFilterChange,
}) => {
  const [activeFilter, setActiveFilter] = useState('Todos');
  const [searchTerm, setSearchTerm] = useState('');
  const [soloDemorados, setSoloDemorados] = useState(false);

  // Filtros de estado principal (sin "Demorado")
  const allFilters = [
    { label: 'Todos', value: 'Todos' },
    { label: 'Sin preparar', value: 'Sin preparar' },
    { label: 'Preparar pedido', value: 'Preparar pedido' },
    { label: 'Listo para despachar', value: 'Listo para despachar' },
    { label: 'Despachando', value: 'Despachando' },
    { label: 'En camino', value: 'En camino' },
    { label: 'Entregado', value: 'Entregado' },
    { label: 'Entrega fallida', value: 'Entrega fallida' },
    { label: 'Cancelado', value: 'Cancelado' },
  ];

  const getFilteredOptions = () => {
    if (userRole === 'Encargado') return allFilters;
    if (userRole === 'Operario') {
      return allFilters.filter((f) =>
        ['Todos', 'Preparar pedido', 'Listo para despachar'].includes(f.value)
      );
    }
    if (userRole === 'Cadete') {
      return allFilters.filter((f) =>
        ['Todos', 'Despachando', 'En camino', 'Entregado', 'Entrega fallida', 'Cancelado'].includes(
          f.value
        )
      );
    }
    return [{ label: 'Todos', value: 'Todos' }];
  };

  const filters = getFilteredOptions();

  const emitChange = (estado: string, search: string, demorados: boolean) => {
    onFilterChange({
      estado: estado === 'Todos' ? undefined : estado,
      search,
      soloDemorados: demorados,
    });
  };

  const handleFilterClick = (value: string) => {
    setActiveFilter(value);
    emitChange(value, searchTerm, soloDemorados);
  };

  const handleSearch = (e: React.ChangeEvent<HTMLInputElement>) => {
    const value = e.target.value;
    setSearchTerm(value);
    emitChange(activeFilter, value, soloDemorados);
  };

  const handleDemoradosToggle = () => {
    const next = !soloDemorados;
    setSoloDemorados(next);
    emitChange(activeFilter, searchTerm, next);
  };

  return (
    <div className="flex flex-col gap-3 p-2">
      {/* Fila 1: filtros de estado + toggle demorados */}
      <div className="flex flex-wrap gap-2 items-center overflow-x-auto pb-1 scrollbar-hide">
        {filters.map((filter) => (
          <button
            key={filter.value}
            onClick={() => handleFilterClick(filter.value)}
            className={`
              px-4 py-1.5 rounded-full text-sm font-medium transition-all duration-200 whitespace-nowrap
              ${
                activeFilter === filter.value
                  ? 'bg-yellow-400 text-blue-900 shadow-md shadow-yellow-200 scale-105 font-bold'
                  : 'bg-gray-100 text-gray-600 hover:bg-gray-200 hover:text-blue-600'
              }
            `}
          >
            {filter.label}
          </button>
        ))}

        {/* Separador */}
        <span className="w-px h-5 bg-gray-200 mx-1 hidden sm:block" />

        {/* Toggle: Solo demorados */}
        <button
          onClick={handleDemoradosToggle}
          className={`
            flex items-center gap-1.5 px-4 py-1.5 rounded-full text-sm font-medium transition-all duration-200 whitespace-nowrap border
            ${
              soloDemorados
                ? 'bg-orange-100 text-orange-700 border-orange-300 shadow-sm scale-105 font-bold'
                : 'bg-gray-100 text-gray-600 border-transparent hover:bg-orange-50 hover:text-orange-600'
            }
          `}
          title="Filtrar solo pedidos con subestado demorado"
        >
          <Clock className="w-3.5 h-3.5" />
          Demorados
        </button>
      </div>

      {/* Fila 2: buscador */}
      <div className="flex items-center gap-3 w-full lg:w-auto">
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
      </div>
    </div>
  );
};

