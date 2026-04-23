import React, { useState, useEffect } from 'react';
// Ensure the Navbar component is correctly imported
import { Navbar } from '../layout/Navbar.tsx'; // Ensure Navbar.tsx exists in the same directory
// If Navbar.tsx does not exist, create it or correct the import path
// Check if Sidebar.tsx exists in the same directory or adjust the path
import { Sidebar } from '../layout/Sidebar.tsx'; // Ensure Sidebar.tsx exists in the same directory

interface DashboardLayoutProps {
    children: React.ReactNode;
}

export const DashboardLayout: React.FC<DashboardLayoutProps> = ({ children }) => {
    const [sidebarOpen, setSidebarOpen] = useState(false);

    // En desktop (lg+) lo abre automático, en mobile se mantiene cerrado
    useEffect(() => {
        const handleResize = () => {
            if (window.innerWidth >= 1024) {
                setSidebarOpen(true);
            } else {
                setSidebarOpen(false);
            }
        };
        
        // Ejecutar al montar
        handleResize();
        
        window.addEventListener('resize', handleResize);
        return () => window.removeEventListener('resize', handleResize);
    }, []);

    return (
        <div className="min-h-screen bg-gray-50">
            <Navbar onMenuClick={() => setSidebarOpen(!sidebarOpen)} />
            {sidebarOpen && (
                <div
                    className="fixed inset-0 bg-black/50 z-30 lg:hidden mt-16"
                    onClick={() => setSidebarOpen(false)}
                />
            )}
            <div className="flex pt-16">
                <Sidebar isOpen={sidebarOpen} onClose={() => setSidebarOpen(false)} />
                <main className="w-full min-w-0 transition-all duration-300 lg:pl-64">
                    <div className="p-4 sm:p-6">
                        {children}
                    </div>
                </main>
            </div>
        </div>
    );
};
