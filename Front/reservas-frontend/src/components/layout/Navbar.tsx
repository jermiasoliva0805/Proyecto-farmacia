import React from 'react';
import { useAuth } from '@context/AuthContext';
import { Menu, Bell, User } from 'lucide-react';
// import { Badge } from './Badge'; // Descomenta si usas el Badge separado

interface NavbarProps {
    onMenuClick: () => void;
}

export const Navbar: React.FC<NavbarProps> = ({ onMenuClick }) => {
    const { user, logout } = useAuth();

    return (
        <nav className="fixed top-0 left-0 right-0 bg-[#1e3a8a] text-white z-50 h-16 shadow-md">
            <div className="px-4 h-full">
                <div className="flex items-center justify-between h-full">
                    {/* Sección Izquierda: Menú y Logo */}
                    <div className="flex items-center gap-4">
                        <button
                            onClick={onMenuClick}
                            className="p-2 rounded-lg hover:bg-blue-800 transition-colors lg:hidden"
                        >
                            <Menu className="w-6 h-6 text-white" />
                        </button>
                        
                        <div className="flex items-center gap-2 sm:gap-3">
                            {/* Logo con fondo blanco circular para resaltar sobre el azul */}
                            <div className="bg-white p-1 rounded-full w-9 h-9 sm:w-10 sm:h-10 flex items-center justify-center flex-shrink-0">
                                <img 
                                    src="/LogofarmaciaCirculo.png" 
                                    alt="Logo Farmacia" 
                                    className="w-7 h-7 sm:w-8 sm:h-8 object-contain" 
                                />
                            </div>
                            <div className="hidden sm:block">
                                <h1 className="text-base sm:text-lg font-bold leading-none">
                                    Farmacia General Paz
                                </h1>
                                <p className="text-[9px] sm:text-[10px] text-blue-200 uppercase tracking-wider mt-0.5">
                                    Sistema de Gestión de Pedidos
                                </p>
                            </div>
                        </div>
                    </div>

                    {/* Sección Derecha: Notificaciones y Usuario */}
                    <div className="flex items-center gap-2 sm:gap-4">
                        <button className="relative p-2 rounded-lg hover:bg-blue-800 transition-colors">
                            <Bell className="w-5 sm:w-6 h-5 sm:h-6 text-white" />
                            <span className="absolute top-1.5 right-1.5 w-2 h-2 bg-yellow-400 rounded-full"></span>
                        </button>

                        <div className="h-8 w-px bg-blue-700 mx-1 hidden sm:block"></div>

                        <div className="flex items-center gap-2 sm:gap-3">
                            <div className="w-8 h-8 sm:w-9 sm:h-9 bg-yellow-400 rounded-full flex items-center justify-center text-blue-900 font-bold text-sm border-2 border-white/20 flex-shrink-0">
                                {user?.nombreCompleto?.charAt(0) || 'A'}
                            </div>
                            
                            <div className="hidden sm:block text-right mr-2">
                                <p className="font-medium text-sm leading-none">{user?.nombreCompleto || 'Encargado'}</p>
                                <p className="text-[11px] text-blue-200 mt-0.5">{user?.rol || 'Encargado'}</p>
                            </div>

                            <button
                                onClick={logout}
                                className="px-2 sm:px-4 py-1.5 bg-red-600 hover:bg-red-700 text-white text-xs sm:text-sm font-medium rounded-md transition-colors shadow-sm whitespace-nowrap"
                            >
                                Salir
                            </button>
                        </div>
                    </div>
                </div>
            </div>
        </nav>
    );
};