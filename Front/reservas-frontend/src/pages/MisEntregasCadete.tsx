import { PedidosCadeteTemplate } from '../pages/PedidosCadeteTemplate';
export const MisEntregas = () => (
    <PedidosCadeteTemplate 
        titulo="Entregas Activas"
        estadosIds={[5, 6]} 
        mensajeVacio="No tienes repartos pendientes ahora mismo."
        colorIcono="bg-blue-500"
    />
);