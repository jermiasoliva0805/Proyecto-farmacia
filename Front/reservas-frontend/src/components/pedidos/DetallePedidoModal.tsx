import React, { useState } from 'react';
import { X, AlertTriangle, Printer, History, Ban, Calendar } from 'lucide-react';
import { OrderSummaryDTO } from '../../types/pedido.types';
import { Badge } from '../common/Badge';
import { Modal } from '../common/Modal';
import { TrackingTimeline } from '../seguimiento/TrackingTimeline';
import { trackingService } from '../../service/trackingService';
import { OrderTrackingDTO } from '../../types/tracking.types';
import { getPrintData, type PrintData } from '../../service/orderService';
import { useAuth } from '../../context/AuthContext';

interface Props {
  isOpen: boolean;
  onClose: () => void;
  pedido: OrderSummaryDTO | null;
}

export const DetallePedidoModal: React.FC<Props> = ({ isOpen, onClose, pedido }) => {
  const { user } = useAuth();
  const [selectedTracking, setSelectedTracking] = useState<OrderTrackingDTO | null>(null);
  const [modalTrackingOpen, setModalTrackingOpen] = useState(false);
  const [loadingTracking, setLoadingTracking] = useState(false);

  if (!isOpen || !pedido) return null;

  // Roles para validaciones
  const esAdmin = user?.rol === 'Administrador';
  const esOperario = user?.rol === 'Operario';
  const esCadete = user?.rol === 'Cadete';

  const handleVerHistorial = async () => {
    setLoadingTracking(true);
    try {
      const tracking = await trackingService.getSeguimiento(pedido.idPedido);
      setSelectedTracking(tracking);
      setModalTrackingOpen(true);
    } catch (error) {
      console.error('Error al obtener seguimiento:', error);
    } finally {
      setLoadingTracking(false);
    }
  };

  const formatCurrency = (value: number) =>
    new Intl.NumberFormat('es-AR', { style: 'currency', currency: 'ARS', minimumFractionDigits: 2 }).format(value);

  const formatDate = (iso?: string) => {
    try {
      return iso ? new Date(iso).toLocaleDateString('es-AR', { year: 'numeric', month: '2-digit', day: '2-digit' }) : '';
    } catch {
      return '';
    }
  };

  const buildPrintHTML = (data: PrintData) => {
    const lineSubtotals = (Array.isArray(data.productos) ? data.productos : []).map((p: PrintData['productos'][number]) => {
      const desc = p.descuento ?? 0;
      const sub = p.subtotal != null ? p.subtotal : p.cantidad * p.precioUnitario - desc;
      return { subtotal: sub, descuento: desc };
    });
    const subtotal = lineSubtotals.reduce((acc, x) => acc + x.subtotal, 0);
    const recargoMedioPago = 0;
    const total = data.total || subtotal + recargoMedioPago;

    return `
      <html>
        <head>
          <meta charset="utf-8" />
          <title>Hoja de Pedido #${data.idPedido}</title>
          <style>
            @page { size: A4; margin: 16mm; }
            :root { --border:#d9d9d9; --text:#111; --muted:#666; --brand:#000; --bg:#fff; }
            * { box-sizing: border-box; }
            body { font-family: Arial, Helvetica, sans-serif; color: var(--text); background: var(--bg); }
            .sheet { width: 100%; border: 1px solid var(--border); border-radius: 6px; padding: 16px 18px; }
            .header { display:flex; justify-content:space-between; align-items:flex-start; margin-bottom:6px; }
            .brand { font-size:20px; font-weight:800; color:var(--brand); }
            .order-id { font-size:12px; color:var(--muted); margin-top:4px; }
            .info { display:grid; grid-template-columns:1fr 1fr; gap:16px; margin-top:8px; }
            .info h4 { font-size:12px; margin:0 0 6px; font-weight:700; }
            .block { border:1px solid var(--border); border-radius:4px; padding:10px; }
            .row { display:grid; grid-template-columns:140px 1fr; gap:8px; font-size:12px; margin:2px 0; }
            .label { color:var(--muted); }
            .value { color:var(--text); }
            .section-title { margin-top:14px; border-top:2px solid var(--border); padding-top:8px; font-size:13px; font-weight:700; }
            table { width:100%; border-collapse:collapse; margin-top:8px; font-size:12px; }
            thead th { text-align:left; background:#f6f6f6; border:1px solid var(--border); padding:6px; font-weight:700; }
            tbody td { border:1px solid var(--border); padding:6px; vertical-align:top; }
            .td-center { text-align:center; }
            .td-right { text-align:right; }
            .product-name { font-weight:600; }
            .product-note { color:var(--muted); font-size:11px; margin-top:2px; }
            .totals { margin-top:10px; display:grid; grid-template-columns:1fr 240px; gap:16px; align-items:start; }
            .totals-box { border:1px solid var(--border); border-radius:4px; padding:10px; }
            .totals-row { display:flex; justify-content:space-between; font-size:12px; margin:4px 0; }
            .totals-row.total { font-weight:800; border-top:1px dashed var(--border); padding-top:6px; }
            .signatures { margin-top:18px; display:grid; grid-template-columns:repeat(4,1fr); gap:16px; text-align:center; font-size:12px; color:var(--muted); }
            .sig-line { margin-top:14px; border-top:1px dotted var(--border); height:18px; }
            @media print { .sheet { border-color: transparent; } }
          </style>
        </head>
        <body>
          <div class="sheet">
            <div class="header">
              <div>
                <div class="brand">Farmacias General Paz</div>
                <div class="order-id">Pedido #${data.idPedido}</div>
              </div>
              <div class="order-id">Fecha: ${formatDate(data.fecha)}</div>
            </div>
            <div class="info">
              <div class="block">
                <h4>Detalles de Facturación</h4>
                <div class="row"><div class="label">Cliente:</div><div class="value">${data.clienteNombre}</div></div>
                <div class="row"><div class="label">Dirección:</div><div class="value">${data.clienteDireccion}</div></div>
                <div class="row"><div class="label">Teléfono:</div><div class="value">${data.clienteTelefono ?? '-'}</div></div>
                <div class="row"><div class="label">Email:</div><div class="value">${data.clienteEmail ?? '-'}</div></div>
                <div class="row"><div class="label">Medio de pago:</div><div class="value">${data.formaPago ?? '-'}</div></div>
              </div>
              <div class="block">
                <h4>Detalles de Envío</h4>
                <div class="row"><div class="label">Método de envío:</div><div class="value">${data.metodoEnvio ?? '-'}</div></div>
                <div class="row"><div class="label">Punto de retiro:</div><div class="value">${data.puntoDeRetiro ?? '-'}</div></div>
                <div class="row"><div class="label">Estado de entrega:</div><div class="value">-</div></div>
              </div>
            </div>
            <div class="section-title">Productos/Servicios</div>
            <table>
              <thead>
                <tr>
                  <th class="td-center" style="width:70px">Cant.</th>
                  <th>Producto</th>
                  <th class="td-right" style="width:120px">Precio unitario</th>
                  <th class="td-right" style="width:110px">Descuento</th>
                  <th class="td-right" style="width:120px">Subtotal</th>
                </tr>
              </thead>
              <tbody>
                ${
                  Array.isArray(data.productos) && data.productos.length
                    ? data.productos
                        .map((p: PrintData['productos'][number]) => {
                          const desc = p.descuento ?? 0;
                          const sub = p.subtotal != null ? p.subtotal : p.cantidad * p.precioUnitario - desc;
                          const sku = p.sku ?? '';
                          return `
                            <tr>
                              <td class="td-center">${p.cantidad}</td>
                              <td>
                                <div class="product-name">${p.productoNombre}</div>
                                ${sku ? `<div class="product-note">SKU: ${sku}</div>` : ''}
                              </td>
                              <td class="td-right">${formatCurrency(p.precioUnitario)}</td>
                              <td class="td-right">${desc ? '-' + formatCurrency(desc) : formatCurrency(0)}</td>
                              <td class="td-right">${formatCurrency(sub)}</td>
                            </tr>
                          `;
                        })
                        .join('')
                    : `<tr><td colspan="5" class="td-center">Sin detalle disponible</td></tr>`
                }
              </tbody>
            </table>
            <div class="totals">
              <div></div>
              <div class="totals-box">
                <div class="totals-row"><span>Subtotal:</span><span>${formatCurrency(subtotal)}</span></div>
                <div class="totals-row"><span>Recargo:</span><span>${formatCurrency(recargoMedioPago)}</span></div>
                <div class="totals-row total"><span>Total:</span><span>${formatCurrency(total)}</span></div>
              </div>
            </div>
            <div class="signatures">
              <div>Firma<div class="sig-line"></div></div>
              <div>Aclaración<div class="sig-line"></div></div>
              <div>DNI<div class="sig-line"></div></div>
              <div>Fecha<div class="sig-line"></div></div>
            </div>
          </div>
          <script>window.addEventListener('load', () => window.print());</script>
        </body>
      </html>
    `;
  };

  const handleImprimirHoja = async () => {
    try {
      const data = await getPrintData(pedido.idPedido);
      const ventana = window.open('', '_blank');
      if (!ventana) return;
      ventana.document.open();
      ventana.document.write(buildPrintHTML(data));
      ventana.document.close();
      ventana.focus();
    } catch (error) {
      console.error('Error al imprimir:', error);
    }
  };

  return (
    <>
      <div className="fixed inset-0 z-50 flex items-center justify-center bg-black bg-opacity-50 p-4">
        <div className="bg-white rounded-xl shadow-xl w-full max-w-lg overflow-hidden flex flex-col max-h-[90vh]">
          {/* Header */}
          <div className="p-4 border-b flex justify-between items-start">
            <div>
              <h2 className="text-xl font-bold text-gray-900">Detalle del Pedido</h2>
              <p className="text-sm text-gray-500">Información completa del pedido #{pedido.idPedido}</p>
            </div>
            <button onClick={onClose} className="text-gray-400 hover:text-gray-600">
              <X className="w-6 h-6" />
            </button>
          </div>

          <div className="p-4 overflow-y-auto space-y-4">
            {pedido.estaDemorado && (
              <div className="bg-orange-50 border border-orange-200 p-3 rounded-lg flex gap-3">
                <AlertTriangle className="w-5 h-5 text-orange-500 shrink-0" />
                <div>
                  <p className="text-sm font-bold text-orange-800">Pedido demorado</p>
                  <p className="text-xs text-orange-700">Pedido con más de 48hs hábiles sin completarse.</p>
                </div>
              </div>
            )}

            <div className="grid grid-cols-2 gap-4 text-sm">
              <div><p className="text-gray-500">N° de Pedido</p><p className="font-semibold">{pedido.idPedido}</p></div>
              <div><p className="text-gray-500">Estado</p><Badge variant={pedido.estaDemorado ? 'warning' : 'info'}>{pedido.estadoNombre}</Badge></div>
              <div><p className="text-gray-500">Cliente</p><p className="font-semibold">{pedido.clienteNombre}</p></div>
              <div><p className="text-gray-500">Sucursal</p><p className="font-semibold">Casa Central</p></div>
            </div>

            <div>
              <p className="text-sm font-bold text-gray-900 mb-2">Resumen Económico</p>
              <div className="bg-gray-50 p-3 rounded-lg flex justify-between items-center">
                <p className="text-sm font-medium">Items del pedido</p>
                <p className="text-sm font-bold">${pedido.total.toFixed(2)}</p>
              </div>
              <div className="mt-4 pt-2 border-t flex justify-between items-center text-blue-600">
                <p className="font-bold">Total del Pedido</p>
                <p className="text-lg font-bold">${pedido.total.toFixed(2)}</p>
              </div>
            </div>

            <div className="space-y-2 pt-4">
              {/* Acciones para todos los roles permitidos */}
              {(esAdmin || esOperario || esCadete) && (
                <div className="grid grid-cols-1 gap-2">
                  <button onClick={handleImprimirHoja} className="flex items-center justify-center gap-2 border p-2 rounded-lg text-sm font-medium hover:bg-gray-50">
                    <Printer className="w-4 h-4" /> Imprimir Hoja
                  </button>
                  <button onClick={handleVerHistorial} disabled={loadingTracking} className="flex items-center justify-center gap-2 border p-2 rounded-lg text-sm font-medium hover:bg-gray-50 disabled:opacity-50">
                    <History className={`w-4 h-4 ${loadingTracking ? 'animate-spin' : ''}`} />
                    {loadingTracking ? 'Cargando...' : 'Ver Historial de Estados'}
                  </button>
                </div>
              )}

              {/* Botón de Cancelar: Comentado lógicamente para el Administrador */}
              {esAdmin && (
                <div className="pt-2 border-t">
                  <button 
                    disabled 
                    className="w-full flex items-center justify-center gap-2 p-2 rounded-lg text-sm font-bold bg-gray-200 text-gray-400 cursor-not-allowed"
                  >
                    <Ban className="w-4 h-4" /> Cancelar Pedido (Proximamente)
                  </button>
                </div>
              )}
            </div>
          </div>
        </div>
      </div>

      {selectedTracking && (
        <Modal isOpen={modalTrackingOpen} onClose={() => setModalTrackingOpen(false)} title={`Historial #${pedido.idPedido}`} size="lg">
          <div className="space-y-4">
            <div className="bg-blue-50 border border-blue-200 rounded-lg p-4 flex items-center justify-between">
              <div>
                <p className="text-sm text-blue-800">Estado Actual</p>
                <p className="text-xl font-bold text-blue-900">{selectedTracking.estadoActual}</p>
              </div>
              <Calendar className="w-8 h-8 text-blue-600" />
            </div>
            <div className="max-h-[60vh] overflow-y-auto pr-2">
              <TrackingTimeline tracking={selectedTracking} />
            </div>
          </div>
        </Modal>
      )}
    </>
  );
};