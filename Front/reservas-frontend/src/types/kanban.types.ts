import { OrderSummaryDTO } from './pedido.types';

// ========== MAPEO DE ESTADOS ==========
export const ESTADO_MAP: Record<number, string> = {
    1: 'Sin preparar',
    2: 'Preparar pedido',
    3: 'Demorado',
    4: 'Listo para despachar',
    5: 'Despachando',
    6: 'En camino',
    7: 'Entregado',
    8: 'Entrega fallida',
    9: 'Cancelado'
};

// ========== CONSTANTES DE COLUMNAS POR ROL ==========
export const COLUMNAS_POR_ROL: Record<string, number[]> = {
    'Encargado': [1, 2, 3, 4, 5, 6, 7, 8, 9],
    'Operario': [2, 3, 4],           // Preparar, Demorado, Listo
    'Cadete': [5, 6, 7, 8]            // Despachando, En camino, Entregado, Entrega fallida
};

// ========== REGLAS DE TRANSICIÓN ==========
// Define qué estados se pueden alcanzar desde cada estado
export const TRANSICIONES_PERMITIDAS: Record<number, number[]> = {
    1: [2, 9],                 // Sin preparar → Preparar o CANCELADO
    2: [3, 4, 9],              // Preparar → Demorado, Listo o CANCELADO
    3: [2, 4, 9],              // Demorado → Preparar, Listo o CANCELADO
    4: [5, 9],                 // Listo → Despachando o CANCELADO
    5: [6, 9],                 // Despachando → En camino o CANCELADO
    6: [7, 8, 9],              // En camino → Entregado, Fallo o CANCELADO
    7: [],                     // Entregado → Final (no puede cambiar)
    8: [6, 9],                 // Entrega fallida → Reintentando o CANCELADO
    9: []                      // Cancelado → Final (no puede cambiar)
};

// ========== VALIDACIONES POR ROL ==========
export interface ValidationRuleSet {
    puedeMoverHacia: (estadoOrigen: number, estadoDestino: number) => boolean;
    puedeLlegarAlEstado: (estadoDestino: number) => boolean;
}

export const VALIDACIONES_OPERARIO: ValidationRuleSet = {
    puedeMoverHacia: (origen: number, destino: number) => {
        // Permitir cancelación desde cualquier estado (excepto finales)
        if (destino === 9) {
            return ![7, 9].includes(origen);
        }
        // Solo puede mover dentro de [2, 3, 4]
        if (![2, 3, 4].includes(origen) || ![2, 3, 4].includes(destino)) {
            return false;
        }
        // No puede retroceder
        if (destino < origen) {
            return false;
        }
        // Validar transición específica
        return TRANSICIONES_PERMITIDAS[origen].includes(destino);
    },
    puedeLlegarAlEstado: (estado: number) => [2, 3, 4, 9].includes(estado)
};

export const VALIDACIONES_CADETE: ValidationRuleSet = {
    puedeMoverHacia: (origen: number, destino: number) => {
        // Cadete solo puede ver y mover en [5, 6, 7, 8]
        if (![5, 6, 7, 8].includes(origen)) {
            return false;
        }
        
        // Permitir cancelación desde cualquier estado (excepto finales)
        if (destino === 9) {
            return ![7, 9].includes(origen);
        }
        
        // Destino debe estar en rango permitido
        if (![5, 6, 7, 8, 9].includes(destino)) {
            return false;
        }
        
        // Permanecer en el mismo estado no es válido (excepto para reintento 8→6)
        if (destino === origen) {
            return false;
        }
        
        // Validar que la transición está permitida en TRANSICIONES_PERMITIDAS
        return TRANSICIONES_PERMITIDAS[origen].includes(destino);
    },
    puedeLlegarAlEstado: (estado: number) => [5, 6, 7, 8, 9].includes(estado)
};

export const VALIDACIONES_ENCARGADO: ValidationRuleSet = {
    puedeMoverHacia: (origen: number, destino: number) => {
        // Encargado puede mover entre cualquier estado lógicamente válido
        if (destino < 1 || destino > 9 || origen < 1 || origen > 9) {
            return false;
        }
        // Los estados finales (7, 9) no pueden cambiar
        if ([7, 9].includes(origen)) {
            return false;
        }
        return true;
    },
    puedeLlegarAlEstado: (estado: number) => estado >= 1 && estado <= 9
};

// ========== TIPOS KANBAN ==========
export interface KanbanColumn {
    id: number;
    nombre: string;
    color: string;
    bgColor: string;
    textColor: string;
}

export interface KanbanDragData {
    pedidoId: number;
    estadoOrigen: number;
    clienteNombre: string;
    total: number;
}

export interface KanbanState {
    columnasActivas: number[];
    pedidosPorEstado: Record<number, OrderSummaryDTO[]>;
    isLoading: boolean;
    dragBlockedPedidoId?: number;
}
