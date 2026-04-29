import { useState, useEffect, useCallback, useRef } from 'react';
import * as signalR from '@microsoft/signalr';
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
  const connectionRef = useRef<signalR.HubConnection | null>(null);
 
  const fetchNotifications = useCallback(async () => {
    try {
      setLoading(true);
      setError(null);
      // Este endpoint ya filtra estados finales (7=Entregado, 9=Cancelado)
      const response = await api.get('/reporte/pedidos-demorados-usuario');
      const data = response.data as OrderSummaryDTO[];
      setNotifications(data);
      setHasUnread(data.length > 0);
    } catch (err: any) {
      setError(err.response?.data?.message || 'Error al obtener notificaciones');
      console.error('Error fetching delayed orders:', err);
    } finally {
      setLoading(false);
    }
  }, []);
 
  // SignalR: cuando cualquier pedido cambia de estado, todos los roles refrescan
  useEffect(() => {
    const baseUrl = (import.meta.env.VITE_API_BASE_URL as string)?.replace('/api', '');
    const hubUrl = `${baseUrl}/hubs/pedidos`;
 
    const connection = new signalR.HubConnectionBuilder()
      .withUrl(hubUrl)
      .withAutomaticReconnect()
      .configureLogging(signalR.LogLevel.Warning)
      .build();
 
    connectionRef.current = connection;
 
    connection.on('PedidosDemoradosActualizados', () => {
      fetchNotifications();
    });
 
    connection.start().catch((err) =>
      console.error('SignalR connection error:', err)
    );
 
    return () => { connection.stop(); };
  }, [fetchNotifications]);
 
  // Polling como fallback
  useEffect(() => {
    fetchNotifications();
    const intervalId = setInterval(fetchNotifications, interval);
    return () => clearInterval(intervalId);
  }, [fetchNotifications, interval]);
 
  const markAsRead = useCallback(() => {
    setHasUnread(false);
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