export type PrintProduct = {
  cantidad: number;
  productoNombre: string;
  sku?: string;
  precioUnitario: number;
  subtotal?: number;
  descuento?: number;
};

export type PrintData = {
  idPedido: number;
  fecha?: string;
  formaPago?: string;
  metodoEnvio?: string;
  puntoDeRetiro?: string;
  clienteNombre: string;
  clienteTelefono?: string;
  clienteEmail?: string;
  clienteDireccion: string;
  productos: Array<PrintProduct>;
  total: number;
};

export type LabelData = {
  idPedido: number;
  fecha?: string;
  metodoEnvio?: string;
  puntoDeRetiro?: string;
  clienteNombre: string;
  clienteTelefono?: string;
  clienteEmail?: string;
  clienteDireccion: string;
  codigoPostal?: string;
  referenciaEntrega?: string;
  clienteLocalidadBarrio?: string;
};

const API_BASE = (import.meta.env.VITE_API_BASE_URL ?? 'http://localhost:5000/api').replace(/\/$/, '');
const PRINT_PATH = (import.meta.env.VITE_ORDER_PRINT_PATH ?? '/orders/{id}/print-data').trim();
const USE_COOKIES = (import.meta.env.VITE_USE_COOKIES ?? 'false').toLowerCase() === 'true';

function buildUrlFromPath(path: string, idPedido: number): string {
  const normalized = path.startsWith('/') ? path : `/${path}`;
  return `${API_BASE}${normalized.replace('{id}', String(idPedido))}`;
}

// Busca el token en distintas keys comunes
function getAuthToken(): string | null {
  const envToken = import.meta.env.VITE_API_TOKEN;
  const keys = ['token', 'accessToken', 'access_token', 'authToken', 'jwt','farmacia_token'];
  const storageToken =
    localStorage.getItem(keys.find((k) => localStorage.getItem(k)) || '') ||
    sessionStorage.getItem(keys.find((k) => sessionStorage.getItem(k)) || '') ||
    null;
  return envToken || storageToken || null;
}

function isJwtExpired(token: string): boolean {
  try {
    const [, payloadB64] = token.split('.');
    if (!payloadB64) return false; // Si no es JWT “clásico”, no evaluamos expiración
    const payload = JSON.parse(atob(payloadB64));
    return typeof payload.exp === 'number' ? payload.exp * 1000 < Date.now() : false;
  } catch {
    return false;
  }
}

async function fetchJson(url: string) {
  const token = getAuthToken();
  if (!USE_COOKIES && !token) {
    // Si no usamos cookies y no hay token, avisamos pronto para que el error sea claro
    throw new Error('Token de autenticación ausente. Define VITE_API_TOKEN o guarda el token en localStorage/sessionStorage.');
  }
  if (token && isJwtExpired(token)) {
    throw new Error('El token JWT está expirado. Obtén uno nuevo e inténtalo nuevamente.');
  }

  console.log('[orderService] GET', url);
  const res = await fetch(url, {
    method: 'GET',
    headers: {
      Accept: 'application/json',
      ...(token ? { Authorization: `Bearer ${token}` } : {}),
    },
    credentials: USE_COOKIES ? 'include' : 'same-origin',
  });

  const ok = res.ok;
  const status = res.status;
  const text = ok ? '' : await res.text().catch(() => '');
  const json = ok ? await res.json() : null;

  return { ok, status, json, text };
}

export async function getPrintData(idPedido: number): Promise<PrintData> {
  const url = buildUrlFromPath(PRINT_PATH, idPedido);
  const { ok, status, json, text } = await fetchJson(url);

  if (status === 401) {
    throw new Error('No autorizado (401). Verifica que envías el token/cookies correctas y que no está expirado.');
  }
  if (status === 403) {
    throw new Error('Acceso prohibido (403). Tu token no tiene permisos suficientes.');
  }
  if (status === 404) {
    throw new Error(`Endpoint no encontrado: ${url}`);
  }
  if (!ok) {
    throw new Error(`Fallo al obtener datos de impresión (${status}): ${text}`);
  }

  const productosOrigen: any[] = Array.isArray(json.productos ?? json.items) ? (json.productos ?? json.items) : [];
  const productos: PrintProduct[] = productosOrigen.map((p: any) => ({
    cantidad: Number(p.cantidad ?? p.qty ?? 0),
    productoNombre: String(p.productoNombre ?? p.nombre ?? p.producto?.nombre ?? ''),
    sku: p.sku ? String(p.sku) : undefined,
    precioUnitario: Number(p.precioUnitario ?? p.precio ?? p.unitPrice ?? 0),
    subtotal: p.subtotal != null ? Number(p.subtotal) : undefined,
    descuento: p.descuento != null ? Number(p.descuento) : undefined,
  }));

  const subtotalCalculado =
    productos.reduce((acc, it) => acc + (it.subtotal != null ? it.subtotal : it.cantidad * it.precioUnitario - (it.descuento ?? 0)), 0);

  const formaPagoValue = json.formaPago || json.formaDePago || json.FormaDePago;
  const data: PrintData = {
    idPedido: Number(json.idPedido ?? json.id ?? idPedido),
    fecha: json.fecha ? String(json.fecha) : undefined,
    formaPago: formaPagoValue ? String(formaPagoValue) : undefined,
    metodoEnvio: json.metodoEnvio ? String(json.metodoEnvio) : undefined,
    puntoDeRetiro: json.puntoDeRetiro ? String(json.puntoDeRetiro) : undefined,
    clienteNombre: String(json.clienteNombre ?? json.cliente?.nombre ?? ''),
    clienteTelefono: json.clienteTelefono ? String(json.clienteTelefono) : undefined,
    clienteEmail: json.clienteEmail ? String(json.clienteEmail) : undefined,
    clienteDireccion: String(json.clienteDireccion ?? json.cliente?.direccion ?? ''),
    productos,
    total: Number(json.total ?? json.montoTotal ?? subtotalCalculado ?? 0),
  };

  return data;
}

export async function getLabelData(idPedido: number): Promise<LabelData> {
  const LABEL_PATH = (import.meta.env.VITE_ORDER_LABEL_PATH ?? '/orders/{id}/label-data').trim();
  const url = buildUrlFromPath(LABEL_PATH, idPedido);
  const { ok, status, json, text } = await fetchJson(url);

  if (status === 401) {
    throw new Error('No autorizado (401). Verifica que envías el token/cookies correctas y que no está expirado.');
  }
  if (status === 403) {
    throw new Error('Acceso prohibido (403). Tu token no tiene permisos suficientes.');
  }
  if (status === 404) {
    throw new Error(`Endpoint no encontrado: ${url}`);
  }
  if (!ok) {
    throw new Error(`Fallo al obtener datos de etiqueta (${status}): ${text}`);
  }

  const data: LabelData = {
    idPedido: Number(json.idPedido ?? json.id ?? idPedido),
    fecha: json.fecha ? String(json.fecha) : undefined,
    metodoEnvio: json.metodoEnvio ? String(json.metodoEnvio) : undefined,
    puntoDeRetiro: json.puntoDeRetiro ? String(json.puntoDeRetiro) : undefined,
    clienteNombre: String(json.clienteNombre ?? ''),
    clienteTelefono: json.clienteTelefono ? String(json.clienteTelefono) : undefined,
    clienteEmail: json.clienteEmail ? String(json.clienteEmail) : undefined,
    clienteDireccion: String(json.clienteDireccion ?? ''),
    codigoPostal: json.codigoPostal ? String(json.codigoPostal) : undefined,
    referenciaEntrega: json.referenciaEntrega ? String(json.referenciaEntrega) : undefined,
    clienteLocalidadBarrio: json.clienteLocalidadBarrio ? String(json.clienteLocalidadBarrio) : undefined,
  };

  return data;
}