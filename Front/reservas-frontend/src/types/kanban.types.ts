import { OrderSummaryDTO } from './pedido.types';

// ========== MAPEO DE ESTADOS ==========
// El estado 3 "Demorado" ya NO es un estado principal navegable.
// "Demorado" ahora es un subestado/flag (estaDemorado: boolean) que convive con cualquier estado principal.
export const ESTADO_MAP: Record<number, string> = {
  1: 'Sin preparar',
  2: 'Preparar pedido',
  // 3 eliminado como estado principal — ahora es subestado flag
  4: 'Listo para despachar',
  5: 'Despachando',
  6: 'En camino',
  7: 'Entregado',
  8: 'Entrega fallida',
  9: 'Cancelado',
};

// ========== CONSTANTES DE COLUMNAS POR ROL ==========
// Se elimina el estado 3 de todas las columnas.
export const COLUMNAS_POR_ROL: Record<string, number[]> = {
  'Encargado': [1, 2, 4, 5, 6, 7, 8, 9],
  'Operario':  [2, 4],          // Preparar, Listo
  'Cadete':    [5, 6, 7, 8],    // Despachando, En camino, Entregado, Entrega fallida
};

// ========== REGLAS DE TRANSICIÓN ==========
// Se eliminan todas las referencias al estado 3 (Demorado) como estado destino/origen.
// Un pedido demorado sigue en su estado principal; el flag esDemorado se gestiona automáticamente por el backend.
export const TRANSICIONES_PERMITIDAS: Record<number, number[]> = {
  1: [2, 9],     // Sin preparar → Preparar o Cancelado
  2: [4, 9],     // Preparar → Listo o Cancelado (ya no puede ir a 3)
  4: [5, 9],     // Listo → Despachando o Cancelado
  5: [6, 9],     // Despachando → En camino o Cancelado
  6: [7, 8, 9],  // En camino → Entregado, Fallo o Cancelado
  7: [],         // Entregado → Final
  8: [6, 9],     // Entrega fallida → Reintentando o Cancelado
  9: [],         // Cancelado → Final
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
    // Solo puede mover dentro de [2, 4]
    if (![2, 4].includes(origen) || ![2, 4].includes(destino)) {
      return false;
    }
    return TRANSICIONES_PERMITIDAS[origen]?.includes(destino) ?? false;
  },
  puedeLlegarAlEstado: (estado: number) => [2, 4, 9].includes(estado),
};

export const VALIDACIONES_CADETE: ValidationRuleSet = {
  puedeMoverHacia: (origen: number, destino: number) => {
    if (![5, 6, 7, 8].includes(origen)) return false;
    if (destino === 9) return ![7, 9].includes(origen);
    if (![5, 6, 7, 8, 9].includes(destino)) return false;
    if (destino === origen) return false;
    return TRANSICIONES_PERMITIDAS[origen]?.includes(destino) ?? false;
  },
  puedeLlegarAlEstado: (estado: number) => [5, 6, 7, 8, 9].includes(estado),
};

export const VALIDACIONES_ENCARGADO: ValidationRuleSet = {
  puedeMoverHacia: (origen: number, destino: number) => {
    if (destino < 1 || destino > 9 || origen < 1 || origen > 9) return false;
    if ([7, 9].includes(origen)) return false;

    const estadosOperario = [1, 2, 4];
    const estadosCadete = [5, 6, 8];

    if (estadosOperario.includes(origen)) {
      if (estadosOperario.includes(destino) || destino === 9) {
        return TRANSICIONES_PERMITIDAS[origen]?.includes(destino) ?? false;
      }
      if (origen === 4 && destino === 5) return true;
      return false;
    }

    if (estadosCadete.includes(origen)) {
      return destino === 9;
    }

    return false;
  },
  puedeLlegarAlEstado: (estado: number) => [1, 2, 4, 5, 9].includes(estado),
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