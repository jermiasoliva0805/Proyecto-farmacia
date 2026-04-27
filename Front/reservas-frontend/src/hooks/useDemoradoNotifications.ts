import { useState, useEffect, useCallback } from 'react';
import { api } from '@service/api';
import { OrderSummaryDTO } from '@types/pedido.types';

/**
 * Hook personalizado para obtener pedidos demorados del usuario logueado
 * Se filtra automáticamente según el rol:
 * - Encargado: Ve todos los pedidos demorados
 * - Operario: Ve solo sus pedidos
 * - Cadete: Ve solo los de su zona
 */
export const useDemoradoNotifications = (interval: number = 30000) => {
  const [notifications, setNotifications] = useState<OrderSummaryDTO[]>([]);
  const [loading, setLoading] = useState(false);
  const [error, setError] = useState<string | null>(null);
  const [hasUnread, setHasUnread] = useState(false);

  // Obtener notificaciones
  const fetchNotifications = useCallback(async () => {
    try {
      setLoading(true);
      setError(null);
      const response = await api.get('/reporte/pedidos-demorados-usuario');
      const data = response.data as OrderSummaryDTO[];
      
      setNotifications(data);
      // Mostrar indicador si hay notificaciones
      setHasUnread(data.length > 0);
    } catch (err: any) {
      const errorMessage = err.response?.data?.message || 'Error al obtener notificaciones';
      setError(errorMessage);
      console.error('Error fetching delayed orders:', err);
    } finally {
      setLoading(false);
    }
  }, []);

  // Polling automático
  useEffect(() => {
    // Cargar inmediatamente
    fetchNotifications();

    // Configurar polling
    const intervalId = setInterval(fetchNotifications, interval);

    return () => clearInterval(intervalId);
  }, [fetchNotifications, interval]);

  // Marcar notificación como leída (opcional)
  const markAsRead = useCallback(() => {
    // Si quieres persistencia, puedes guardar en localStorage
    localStorage.setItem('lastNotificationCheck', new Date().toISOString());
  }, []);

  return {
    notifications,
    loading,
    error,
    hasUnread,
    fetchNotifications,
    markAsRead,
    count: notifications.length
  };
};
