import React, { useState, useRef, useEffect } from 'react';
import { Search, ChevronDown, LucideIcon } from 'lucide-react';

interface Option {
    id: string | number;
    label: string;
    subtext?: string;
}

interface SearchableSelectProps {
    label?: string;
    placeholder: string;
    options: Option[];
    onSelect: (option: Option) => void;
    icon?: LucideIcon;
    error?: string;
}

export const SearchableSelect: React.FC<SearchableSelectProps> = ({
    label,
    placeholder,
    options,
    onSelect,
    icon: Icon,
    error
}) => {
    const [isOpen, setIsOpen] = useState(false);
    const [searchTerm, setSearchTerm] = useState('');
    const [selected, setSelected] = useState<Option | null>(null);
    const containerRef = useRef<HTMLDivElement>(null);

    const filteredOptions = options.filter(opt => {
        const optLabel = String(opt.label || '').toLowerCase();
        const optSubtext = opt.subtext ? String(opt.subtext).toLowerCase() : '';
        const term = searchTerm.toLowerCase();
        return optLabel.includes(term) || optSubtext.includes(term);
    });

    useEffect(() => {
        const handleClickOutside = (event: MouseEvent) => {
            if (containerRef.current && !containerRef.current.contains(event.target as Node)) {
                setIsOpen(false);
            }
        };
        document.addEventListener('mousedown', handleClickOutside);
        return () => document.removeEventListener('mousedown', handleClickOutside);
    }, []);

    return (
        <div className="relative w-full" ref={containerRef}>
            {label && <label className="block text-sm font-medium text-gray-700 mb-1">{label}</label>}
            
            <div
                onClick={() => setIsOpen(!isOpen)}
                className={`flex items-center gap-2 w-full p-2.5 border rounded-lg bg-white cursor-pointer transition
                    ${isOpen ? 'border-blue-500 ring-2 ring-blue-100' : 'border-gray-300 hover:border-gray-400'}
                    ${error ? 'border-red-500' : ''}`}
            >
                {Icon && <Icon size={18} className="text-gray-400" />}
                <span className={`text-sm flex-1 truncate ${selected ? 'text-gray-900' : 'text-gray-400'}`}>
                    {selected ? selected.label : placeholder}
                </span>
                <ChevronDown size={18} className={`text-gray-400 transition-transform ${isOpen ? 'rotate-180' : ''}`} />
            </div>

            {isOpen && (
                <div className="absolute z-50 w-full mt-1 bg-white border border-gray-200 rounded-lg shadow-xl overflow-hidden flex flex-col">
                    <div className="p-2 border-b bg-gray-50">
                        <div className="relative">
                            <Search className="absolute left-2 top-2.5 text-gray-400" size={14} />
                            <input
                                autoFocus
                                type="text"
                                className="w-full pl-8 pr-4 py-2 border border-gray-300 rounded-md text-sm outline-none focus:border-blue-500"
                                placeholder="Filtrar opciones..."
                                value={searchTerm}
                                onChange={(e) => setSearchTerm(e.target.value)}
                                onClick={(e) => e.stopPropagation()}
                            />
                        </div>
                    </div>
                    <div className="max-h-60 overflow-y-auto">
                        {filteredOptions.length > 0 ? (
                            filteredOptions.map((opt, index) => (
                                <div
                                    key={`${opt.id}-${index}`}
                                    className="p-3 hover:bg-blue-50 cursor-pointer text-sm border-b border-gray-50 last:border-0"
                                    onClick={(e) => {
                                        e.stopPropagation();
                                        setSelected(opt);
                                        onSelect(opt);
                                        setIsOpen(false);
                                        setSearchTerm('');
                                    }}
                                >
                                    <div className="font-medium text-gray-800">{opt.label}</div>
                                    {opt.subtext && <div className="text-xs text-gray-500">{opt.subtext}</div>}
                                </div>
                            ))
                        ) : (
                            <div className="p-4 text-center text-sm text-gray-400">Sin resultados</div>
                        )}
                    </div>
                </div>
            )}
            {error && <p className="mt-1 text-xs text-red-500">{error}</p>}
        </div>
    );
};