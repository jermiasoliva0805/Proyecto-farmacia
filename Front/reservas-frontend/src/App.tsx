import React from 'react';
import { BrowserRouter, Routes, Route, Navigate, Outlet } from 'react-router-dom';
import { AuthProvider } from './context/AuthContext';
import { ProtectedRoute } from './components/auth/ProtectedRoute';
import { Sidebar } from './components/layout/Sidebar'; 

// Importación de Páginas
import { Login } from './pages/Login';
import { DashboardAdmin } from './pages/DashboardAdmin';
import { DashboardOperario } from './pages/DashboardOperario';
import { DashboardCadete } from './pages/DashboardCadete';
import { SeguimientoPedidos } from './pages/SeguimientoPedidos';
import { AsignarOperarioPage } from './pages/AsignarOperario';
import { AsignarCadetePage } from './pages/AsignarCadete';
import OrderFormPage from './pages/orders/OrderFormPage';
import { PanelReportes } from './pages/Reportes/PanelReportes'; // Importamos el Panel único

// Componente Layout para mantener el Sidebar a la izquierda
const MainLayout = () => {
  return (
    <div className="flex min-h-screen bg-gray-50">
      <Sidebar isOpen={true} />
      <main className="flex-1 lg:ml-64 p-8">
        <Outlet />
      </main>
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
            
            {/* ADMINISTRADOR */}
            <Route
              path="/dashboard/admin"
              element={
                <ProtectedRoute allowedRoles={['Administrador']}>
                  <DashboardAdmin />
                </ProtectedRoute>
              }
            />
            
            {/* RUTA ÚNICA DE REPORTES (Aquí dentro están tus pestañas) */}
            <Route
              path="/reportes"
              element={
                <ProtectedRoute allowedRoles={['Administrador']}>
                  <PanelReportes />
                </ProtectedRoute>
              }
            />

            <Route
              path="/asignar-operario"
              element={
                <ProtectedRoute allowedRoles={['Administrador']}>
                  <AsignarOperarioPage />
                </ProtectedRoute>
              }
            />

            <Route
              path="/asignar-cadete"
              element={
                <ProtectedRoute allowedRoles={['Administrador']}>
                  <AsignarCadetePage />
                </ProtectedRoute>
              }
            />

            <Route
              path="/seguimiento"
              element={
                <ProtectedRoute allowedRoles={['Administrador']}>
                  <SeguimientoPedidos />
                </ProtectedRoute>
              }
            />

            <Route
              path="/usuarios"
              element={
                <ProtectedRoute allowedRoles={['Administrador']}>
                  <div>Página de Usuarios</div> 
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

            <Route
              path="/pedidos"
              element={
                <ProtectedRoute allowedRoles={['Administrador', 'Operario']}>
                  <OrderFormPage />
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