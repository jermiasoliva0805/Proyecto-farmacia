import React from 'react';
import { LabelData } from '../../service/orderService';

export const ShippingLabel = ({ data }: { data: LabelData }) => {
  if (!data) return null;

  const formatDate = (dateString?: string) => {
    if (!dateString) return '-';
    try {
      return new Date(dateString).toLocaleDateString('es-ES', {
        year: 'numeric',
        month: '2-digit',
        day: '2-digit',
      });
    } catch {
      return '-';
    }
  };

  return (
    <div className="hidden print:block p-8 bg-white text-black text-sm">
      {/* Header */}
      <div className="border-b-2 border-gray-800 pb-4 mb-6">
        <h1 className="text-2xl font-bold uppercase">Farmacias General Paz</h1>
        <p className="text-sm text-gray-600">Etiqueta de Envío</p>
      </div>

      {/* Número de Pedido */}
      <div className="mb-6 text-center">
        <p className="text-lg font-bold">Pedido #<span className="text-2xl">{data.idPedido}</span></p>
        <p className="text-xs text-gray-600">Fecha: {formatDate(data.fecha)}</p>
      </div>

      {/* Datos del Cliente */}
      <section className="mb-8 border border-gray-400 p-4 rounded">
        <h2 className="font-bold text-lg mb-3 bg-gray-100 px-2 py-1 -mx-4 -mt-4 -mb-3">Enviar A:</h2>
        
        <div className="mt-4">
          <p className="font-bold text-base">{data.clienteNombre || 'Cliente'}</p>
          
          {data.clienteLocalidadBarrio && (
            <p className="text-xs text-gray-600 mt-1">{data.clienteLocalidadBarrio}</p>
          )}
        </div>

        <div className="mt-4 border-t pt-4">
          <p className="font-bold text-sm mb-2">Dirección:</p>
          <p className="text-sm leading-relaxed break-words">
            {data.clienteDireccion || '-'}
          </p>
          
          {data.codigoPostal && (
            <p className="text-sm mt-2">
              <span className="font-semibold">CP:</span> {data.codigoPostal}
            </p>
          )}
          
          {data.referenciaEntrega && (
            <p className="text-sm mt-2">
              <span className="font-semibold">Referencia:</span> {data.referenciaEntrega}
            </p>
          )}
        </div>

        <div className="mt-4 border-t pt-4">
          <p className="text-xs mb-1">
            <span className="font-semibold">Teléfono:</span> {data.clienteTelefono || '-'}
          </p>
          <p className="text-xs">
            <span className="font-semibold">Email:</span> {data.clienteEmail || '-'}
          </p>
        </div>
      </section>

      {/* Método de Envío */}
      <section className="mb-8 border border-gray-400 p-4 rounded">
        <h2 className="font-bold text-lg mb-3 bg-gray-100 px-2 py-1 -mx-4 -mt-4 -mb-3">Detalles de Envío:</h2>
        
        <div className="mt-4">
          <p className="mb-3">
            <span className="font-semibold">Método de envío:</span> {data.metodoEnvio || '-'}
          </p>
          
          {data.puntoDeRetiro && data.metodoEnvio === 'Punto de retiro' && (
            <p className="mb-3">
              <span className="font-semibold">Punto de retiro:</span> {data.puntoDeRetiro}
            </p>
          )}
        </div>
      </section>

      {/* Firma / Observaciones */}
      <div className="mt-8 text-center">
        <p className="text-xs text-gray-500 mb-2">Firma del receptor:</p>
        <div className="border-t border-gray-400 mt-6 pt-2" style={{ minHeight: '40px' }}></div>
      </div>

      {/* QR o Info adicional */}
      <div className="mt-6 text-center border-t pt-4">
        <p className="text-xs text-gray-600">
          Para consultas: www.farmacias-generalpaz.com
        </p>
      </div>
    </div>
  );
};
