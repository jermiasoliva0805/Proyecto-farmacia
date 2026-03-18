import React from 'react';
import { BrowserRouter, Routes, Route, Navigate, Outlet } from 'react-router-dom';
import { AuthProvider } from './context/AuthContext';
import { ProtectedRoute } from './components/auth/ProtectedRoute';
import { Sidebar } from './components/layout/Sidebar'; 
import { Navbar } from './components/layout/Navbar';

// Importación de Páginas
import { Login } from './pages/Login';
import { DashboardAdmin } from './pages/DashboardAdmin';
import { DashboardOperario } from './pages/DashboardOperario';
import { DashboardCadete } from './pages/DashboardCadete';
import { SeguimientoPedidos } from './pages/SeguimientoPedidos';
import { AsignarOperarioPage } from './pages/AsignarOperario';
import { AsignarCadetePage } from './pages/AsignarCadete';
import OrderFormPage from './pages/orders/OrderFormPage';
import { PanelReportes } from './pages/Reportes/PanelReportes';
import MisEntregas from './pages/MisEntregasCadete';
import EntregasFallidas from './pages/EntregasFallidasCadete';
import MisPedidosOperario from './pages/MisPedidosOperario';
import UsuariosPage from './pages/UsuariosPage'; 

// Componente Layout para mantener el Navbar, Sidebar y contenido
const MainLayout = () => {
  const [sidebarOpen, setSidebarOpen] = React.useState(true);

  return (
    <div className="min-h-screen bg-gray-50">
      <Navbar onMenuClick={() => setSidebarOpen(!sidebarOpen)} />
      <div className="flex pt-16">
        <Sidebar isOpen={sidebarOpen} />
        <main className={`flex-1 transition-all duration-300 ${sidebarOpen ? 'ml-64' : 'ml-0'}`}>
          <Outlet />
        </main>
      </div>
    </div>
  );
};

function App() {
  return (
    <BrowserRouter>
      <AuthProvider>
        <Routes>
          {/* --- RUTA PÚBLICA --- */}
          <Route path="/login" element={<Login />} />

          {/* --- RUTAS CON SIDEBAR (ADMIN, OPERARIO, CADETE) --- */}
          <Route element={<MainLayout />}>
            
            {/* ENCARGADO */}
            <Route
              path="/dashboard/admin"
              element={
                <ProtectedRoute allowedRoles={['Encargado']}>
                  <DashboardAdmin />
                </ProtectedRoute>
              }
            />
            
            {/* CREAR NUEVO PEDIDO */}
            <Route
              path="/pedidos/nuevo"
              element={
                <ProtectedRoute allowedRoles={['Encargado']}>
                  <OrderFormPage />
                </ProtectedRoute>
              }
            />

            {/* GESTIÓN DE PEDIDOS */}
            <Route
              path="/pedidos"
              element={
                <ProtectedRoute allowedRoles={['Encargado']}>
                  <OrderFormPage />
                </ProtectedRoute>
              }
            />
            
            {/* RUTA ÚNICA DE REPORTES */}
            <Route
              path="/reportes"
              element={
                <ProtectedRoute allowedRoles={['Encargado']}>
                  <PanelReportes />
                </ProtectedRoute>
              }
            />

            <Route
              path="/asignar-operario"
              element={
                <ProtectedRoute allowedRoles={['Encargado']}>
                  <AsignarOperarioPage />
                </ProtectedRoute>
              }
            />

            <Route
              path="/asignar-cadete"
              element={
                <ProtectedRoute allowedRoles={['Encargado']}>
                  <AsignarCadetePage />
                </ProtectedRoute>
              }
            />

            <Route
              path="/seguimiento"
              element={
                <ProtectedRoute allowedRoles={['Encargado']}>
                  <SeguimientoPedidos />
                </ProtectedRoute>
              }
            />

            <Route
              path="/usuarios"
              element={
                <ProtectedRoute allowedRoles={['Encargado']}>
                  <UsuariosPage />
                </ProtectedRoute>
              }
            />
            
            {/* OPERARIO */}
            <Route
              path="/dashboard/operario"
              element={
                <ProtectedRoute allowedRoles={['Operario']}>
                  <DashboardOperario />
                </ProtectedRoute>
              }
            />

            {/* OPERARIO */}
            <Route
              path="/dashboard/operario"
              element={
                <ProtectedRoute allowedRoles={['Operario']}>
                  <DashboardOperario />
                </ProtectedRoute>
              }
            />


            {/* Mis Pedidos Operario - RUTA SEPARADA */}
            <Route
              path="/mis-pedidos"
              element={
                <ProtectedRoute allowedRoles={['Operario']}>
                  <MisPedidosOperario />
                </ProtectedRoute>
              }
            />

            {/* CADETE */}
            <Route
              path="/dashboard/cadete"
              element={
                <ProtectedRoute allowedRoles={['Cadete']}>
                  <DashboardCadete />
                </ProtectedRoute>
              }
            />

            <Route
              path="/mis-entregas"
              element={
                <ProtectedRoute allowedRoles={['Cadete']}>
                  <MisEntregas />
                </ProtectedRoute>
              }
            />

            <Route
              path="/intentos-fallidos"
              element={
                <ProtectedRoute allowedRoles={['Cadete']}>
                  <EntregasFallidas />
                </ProtectedRoute>
              }
            />

          </Route>

          {/* --- ERRORES Y REDIRECCIONES --- */}
          <Route path="/unauthorized" element={<div>No autorizado</div>} />
          <Route path="/" element={<Navigate to="/login" replace />} />
          <Route path="*" element={<Navigate to="/login" replace />} />
          
        </Routes>
      </AuthProvider>
    </BrowserRouter>
  );
}

export default App;