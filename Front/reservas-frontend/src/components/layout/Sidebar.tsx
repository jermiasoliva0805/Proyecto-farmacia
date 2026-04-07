import React from 'react';
import { NavLink } from 'react-router-dom';
import { useAuth } from '@context/AuthContext';
import {
    LayoutDashboard,
    Package,
    Users,
    MapPin,
    BarChart3,
    ClipboardList,
    FileText,
    LucideIcon
} from 'lucide-react';

interface SidebarProps {
    isOpen: boolean;
}

interface MenuItem {
    path: string;
    icon: LucideIcon;
    label: string;
}

type UserRole = 'Encargado' | 'Operario' | 'Cadete';

export const Sidebar: React.FC<SidebarProps> = ({ isOpen }) => {
    const { user } = useAuth();

    const menuItems: Record<UserRole, MenuItem[]> = {
        Encargado: [
            { path: '/dashboard/admin', icon: LayoutDashboard, label: 'Dashboard' },
            { path: '/pedidos', icon: Package, label: 'Gestión de Pedidos' },
            { path: '/asignar-operario', icon: Users, label: 'Asignar Operarios' },
            { path: '/asignar-cadete', icon: MapPin, label: 'Asignar Cadetes' },
            { path: '/seguimiento', icon: ClipboardList, label: 'Seguimiento' },
            { path: '/reportes', icon: BarChart3, label: 'Reportes' },
            { path: '/usuarios', icon: Users, label: 'Usuarios' },
        ],
        Operario: [
            { path: '/dashboard/operario', icon: LayoutDashboard, label: 'Dashboard' },
            { path: '/mis-pedidos', icon: Package, label: 'Mis Pedidos' },
        ],
        Cadete: [
            { path: '/dashboard/cadete', icon: LayoutDashboard, label: 'Dashboard' },
            { path: '/mis-entregas', icon: MapPin, label: 'Mis Entregas' },
            { path: '/intentos-fallidos', icon: FileText, label: 'Entregas Fallidas' },
        ],
    };

    const userRole = (user?.rol as UserRole) || 'Encargado';
    const currentMenu = menuItems[userRole] || [];

    const visibilityClass = isOpen ? 'translate-x-0' : '-translate-x-full lg:translate-x-0';

    return (
        <aside 
            className={`fixed left-0 top-16 h-[calc(100vh-4rem)] w-64 bg-white border-r border-gray-200 overflow-y-auto z-50 lg:z-40 transition-transform duration-300 ease-in-out shadow-lg lg:shadow-none ${visibilityClass}`}
        >
            <nav className="p-4 space-y-2">
                {currentMenu.map((item) => (
                    <NavLink
                        key={item.path}
                        to={item.path}
                        className={({ isActive }) =>
                            `flex items-center gap-3 px-3 sm:px-4 py-2.5 sm:py-3 rounded-lg transition-all font-medium text-sm sm:text-base ${
                                isActive
                                    ? 'bg-blue-600 text-white shadow-md shadow-blue-200'
                                    : 'text-gray-600 hover:bg-gray-50 hover:text-blue-600'
                            }`
                        }
                    >
                        <item.icon className="w-5 h-5 flex-shrink-0" />
                        <span className="truncate">{item.label}</span>
                    </NavLink>
                ))}
            </nav>

            <div className="absolute bottom-0 left-0 right-0 p-4 bg-gray-50 border-t border-gray-100">
                <div className="flex items-center gap-2 text-gray-500 text-xs">
                    <div className="w-2 h-2 rounded-full bg-green-500"></div>
                    <p>Sistema Online</p>
                </div>
            </div>
        </aside>
    );
};