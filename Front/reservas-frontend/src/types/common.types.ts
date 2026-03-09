export interface ClientDTO {
    id: number;                // IDCliente
    nombre: string;
    apellido: string;
    dni?: string;
    email?: string;
    telefono?: string;
    idLocalidad?: number;
}

export interface ProductDTO {
    id: number;                // IDProducto del backend
    nombre: string;            // NombreProducto
    descripcion: string;       // Descripcion
    precio: number;            // PrecioProducto (decimal en backend)
    stock: number;             // CantidadProducto
    categoria: string;         // Categoria
}
export interface LocalidadDTO {
    id: number;
    nombre: string;
    provincia: string;
    codigoPostal: string;
}
export interface EstadoPedido {
    id: number;
    nombre: string;
    color: string; // Para UI
    icono: string; // Para UI
}

export interface ApiError {
    message: string;
    detail?: string;
    statusCode?: number;
}

export interface ApiResponse<T> {
    data?: T;
    error?: ApiError;
    success: boolean;
}