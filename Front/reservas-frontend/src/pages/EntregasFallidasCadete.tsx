import { PedidosCadeteTemplate } from './PedidosCadeteTemplate';
export const EntregasFallidas = () => (
    <PedidosCadeteTemplate 
        titulo="Incidentes y Fallidos"
        estadosIds={[8, 9]} 
        mensajeVacio="¡Todo en orden! No hay pedidos fallidos."
        colorIcono="bg-red-500"
    />
);