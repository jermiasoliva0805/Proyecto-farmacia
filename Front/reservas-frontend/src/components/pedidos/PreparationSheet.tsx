import React from 'react';

export const PreparationSheet = ({ data }: { data: any }) => {
if (!data) return null;

return (
    <div className="hidden print:block p-10 bg-white text-black text-sm">
      {/* Header estilo Farmacia */}
    <div className="flex justify-between items-start border-b-2 border-gray-800 pb-4 mb-6">
        <div>
        <h1 className="text-2xl font-bold uppercase">Farmacias General Paz</h1>
        <p className="text-lg font-semibold">Pedido #{data.idPedido}</p>
        </div>
        <div className="text-right">
        <p>Fecha de Pedido: <strong>{new Date(data.fecha).toLocaleDateString()}</strong></p>
        </div>
    </div>

    <div className="grid grid-cols-2 gap-10 mb-8">
        {/* Detalles de Facturación */}
        <section>
        <h2 className="bg-gray-100 font-bold px-2 py-1 mb-2">Detalles de Facturación</h2>
        <div className="space-y-1 ml-2">
            <p className="font-bold">{data.clienteNombre}</p>
            <p>{data.clienteDireccion}</p>
            <p>{data.clienteLocalidadBarrio}</p>
            <p>DNI: {data.clienteDNI}</p>
            <p>Tel: {data.clienteTelefono}</p>
            <p>Email: {data.clienteEmail}</p>
        </div>
        </section>

        {/* Detalles de Pago y Envío */}
        <section>
        <h2 className="bg-gray-100 font-bold px-2 py-1 mb-2">Información del Pedido</h2>
        <div className="space-y-1 ml-2">
            <p><strong>Medio de Pago:</strong> {data.formaDePago}</p>
            <p><strong>Método de Envío:</strong> {data.metodoEnvio}</p>
            {data.puntoRetiro && <p><strong>Punto de Retiro:</strong> {data.puntoRetiro}</p>}
        </div>
        </section>
    </div>

      {/* Tabla de Productos */}
    <table className="w-full border-collapse mb-8">
        <thead>
        <tr className="border-b-2 border-black">
            <th className="text-left p-2">Cant.</th>
            <th className="text-left p-2">Producto / SKU</th>
            <th className="text-right p-2">Unitario</th>
            <th className="text-right p-2">Subtotal</th>
        </tr>
        </thead>
        <tbody>
        {data.productos.map((item: any, idx: number) => (
            <tr key={idx} className="border-b border-gray-300">
            <td className="p-2 font-bold">{item.cantidad}</td>
            <td className="p-2">
                <p>{item.productoNombre}</p>
                <span className="text-xs text-gray-500">SKU: {item.sku}</span>
            </td>
            <td className="p-2 text-right">${item.precioUnitario.toLocaleString()}</td>
            <td className="p-2 text-right">${item.subtotal.toLocaleString()}</td>
            </tr>
        ))}
        </tbody>
    </table>

    <div className="flex justify-end">
        <div className="w-64 space-y-2">
        <div className="flex justify-between border-t border-black pt-2">
            <span className="font-bold text-lg">Total Pagado:</span>
            <span className="font-bold text-lg">${data.total.toLocaleString()}</span>
        </div>
        </div>
    </div>

    <footer className="mt-20 border-t border-dotted border-gray-400 pt-4 text-center text-gray-500 italic">
        Hoja de preparación interna - Operario: ____________________
    </footer>
    </div>
    );
};