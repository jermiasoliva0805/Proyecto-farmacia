import React, { useState } from 'react';
import { useAuth } from '@context/AuthContext';
import { Menu, Bell, User, LogOut, Settings } from 'lucide-react';
import { UserProfileModal } from '../profile/UserProfileModal';
import { NotificationsModal } from '../common/NotificationsModal';
import { useDemoradoNotifications } from '@hooks/useDemoradoNotifications';
// import { Badge } from './Badge'; // Descomenta si usas el Badge separado

interface NavbarProps {
    onMenuClick: () => void;
}

export const Navbar: React.FC<NavbarProps> = ({ onMenuClick }) => {
    const { user, logout } = useAuth();
    const [dropdownOpen, setDropdownOpen] = useState(false);
    const [profileModalOpen, setProfileModalOpen] = useState(false);
    const [notificationsOpen, setNotificationsOpen] = useState(false);
    
    // Hook para obtener notificaciones
    const { notifications, loading, hasUnread, markAsRead } = useDemoradoNotifications();

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
                        <button 
                            onClick={() => {
                                setNotificationsOpen(true);
                                markAsRead();
                            }}
                            className="relative p-2 rounded-lg hover:bg-blue-800 transition-colors"
                            title={`${notifications.length} pedido(s) demorado(s)`}
                        >
                            <Bell className="w-5 sm:w-6 h-5 sm:h-6 text-white" />
                            {hasUnread && (
                                <span className="absolute top-1.5 right-1.5 w-2.5 h-2.5 bg-yellow-400 rounded-full animate-pulse"></span>
                            )}
                            {notifications.length > 0 && (
                                <span className="absolute -top-1 -right-1 bg-red-500 text-white text-xs rounded-full w-5 h-5 flex items-center justify-center font-bold">
                                    {notifications.length}
                                </span>
                            )}
                        </button>

                        <div className="h-8 w-px bg-blue-700 mx-1 hidden sm:block"></div>

                        {/* Dropdown Usuario */}
                        <div className="relative">
                            <button
                                onClick={() => setDropdownOpen(!dropdownOpen)}
                                className="flex items-center gap-2 sm:gap-3 hover:bg-blue-800 px-2 sm:px-3 py-1.5 rounded-lg transition-colors"
                            >
                                <div className="w-8 h-8 sm:w-9 sm:h-9 bg-yellow-400 rounded-full flex items-center justify-center text-blue-900 font-bold text-sm border-2 border-white/20 flex-shrink-0">
                                    {user?.nombreCompleto?.charAt(0) || 'A'}
                                </div>
                                
                                <div className="hidden sm:block text-right">
                                    <p className="font-medium text-sm leading-none">{user?.nombreCompleto || 'Usuario'}</p>
                                    <p className="text-[11px] text-blue-200 mt-0.5">{user?.rol || 'Encargado'}</p>
                                </div>
                            </button>

                            {/* Dropdown Menu */}
                            {dropdownOpen && (
                                <div className="absolute right-0 mt-2 w-48 bg-white rounded-lg shadow-xl border border-gray-200 z-40 overflow-hidden">
                                    {/* Header con info */}
                                    <div className="px-4 py-3 bg-gray-50 border-b border-gray-200">
                                        <p className="text-sm font-medium text-gray-900">{user?.nombreCompleto}</p>
                                        <p className="text-xs text-gray-500">{user?.email}</p>
                                    </div>

                                    {/* Opciones */}
                                    <div className="py-1">
                                        <button
                                            onClick={() => {
                                                setProfileModalOpen(true);
                                                setDropdownOpen(false);
                                            }}
                                            className="w-full px-4 py-2.5 text-sm text-gray-700 hover:bg-blue-50 hover:text-blue-600 flex items-center gap-2 transition-colors"
                                        >
                                            <Settings className="w-4 h-4" />
                                            Mi Perfil
                                        </button>

                                        <button
                                            onClick={() => {
                                                logout();
                                                setDropdownOpen(false);
                                            }}
                                            className="w-full px-4 py-2.5 text-sm text-red-600 hover:bg-red-50 flex items-center gap-2 transition-colors border-t border-gray-200"
                                        >
                                            <LogOut className="w-4 h-4" />
                                            Salir
                                        </button>
                                    </div>
                                </div>
                            )}
                        </div>
                    </div>

                    {/* Modal Mi Perfil */}
                    <UserProfileModal 
                        isOpen={profileModalOpen}
                        onClose={() => setProfileModalOpen(false)}
                    />

                    {/* Modal Notificaciones */}
                    <NotificationsModal 
                        isOpen={notificationsOpen}
                        onClose={() => setNotificationsOpen(false)}
                        notifications={notifications}
                        loading={loading}
                    />
                </div>
            </div>
        </nav>
    );
};