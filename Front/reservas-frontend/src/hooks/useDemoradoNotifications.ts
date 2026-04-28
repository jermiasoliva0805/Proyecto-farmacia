import { useState, useEffect, useCallback } from 'react';
import { api } from '@services/api';
import type { OrderSummaryDTO } from '@models/pedido.types';
 
interface UseDemoradoNotificationsReturn {
  notifications: OrderSummaryDTO[];
  loading: boolean;
  error: string | null;
  hasUnread: boolean;
  fetchNotifications: () => Promise<void>;
  markAsRead: () => void;
  count: number;
}
 
/**
 * Hook para obtener pedidos con subestado demorado.
 * El backend filtra automáticamente según el rol del usuario autenticado:
 *  - Encargado: todos los pedidos con esDemorado = true
 *  - Operario: solo los de su zona/asignados
 *  - Cadete: solo los de su zona de reparto
 *
 * Endpoint: GET /reporte/pedidos-demorados-usuario
 * Polling automático cada `interval` ms (default: 30s).
 */
export const useDemoradoNotifications = (
  interval: number = 30000
): UseDemoradoNotificationsReturn => {
  const [notifications, setNotifications] = useState<OrderSummaryDTO[]>([]);
  const [loading, setLoading] = useState(false);
  const [error, setError] = useState<string | null>(null);
  const [hasUnread, setHasUnread] = useState(false);
 
  const fetchNotifications = useCallback(async () => {
    try {
      setLoading(true);
      setError(null);
      const response = await api.get('/reporte/pedidos-demorados-usuario');
      const data = response.data as OrderSummaryDTO[];
      setNotifications(data);
      setHasUnread(data.length > 0);
    } catch (err: any) {
      const errorMessage =
        err.response?.data?.message || 'Error al obtener notificaciones';
      setError(errorMessage);
      console.error('Error fetching delayed orders:', err);
    } finally {
      setLoading(false);
    }
  }, []);
 
  useEffect(() => {
    fetchNotifications();
    const intervalId = setInterval(fetchNotifications, interval);
    return () => clearInterval(intervalId);
  }, [fetchNotifications, interval]);
 
  const markAsRead = useCallback(() => {
    setHasUnread(false);
    localStorage.setItem('lastNotificationCheck', new Date().toISOString());
  }, []);
 
  return {
    notifications,
    loading,
    error,
    hasUnread,
    fetchNotifications,
    markAsRead,
    count: notifications.length,
  };
};