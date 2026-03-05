import React, { useState, useEffect } from 'react';
import { Plus, Trash2, User, Package, Save, ArrowLeft, ShoppingCart, MapPin, Phone, Mail, CreditCard } from 'lucide-react';
import { SearchableSelect } from '@/components/common/SearchableSelect';
import { useNavigate } from 'react-router-dom';
import { pedidosService } from '@/service/PedidosService';
import { catalogoService, SucursalDTO } from '@/service/catalogoService';
import { ClientDTO, ProductDTO } from '@/types/common.types';

const OrderFormPage: React.FC = () => {
    const navigate = useNavigate();
    
    // Estado del cliente
    const [tipoCliente, setTipoCliente] = useState<'existente' | 'nuevo'>('existente');
    const [clienteId, setClienteId] = useState<string>('');
    const [nuevoCliente, setNuevoCliente] = useState({
        nombre: '',
        apellido: '',
        telefono: '',
        email: '',
        direccion: ''
    });
    
    // Datos del pedido
    const [items, setItems] = useState([{ tempId: Date.now(), productId: '', quantity: 1 }]);
    const [medioPago, setMedioPago] = useState('Efectivo');
    const [puntoRetiro, setPuntoRetiro] = useState('');
    const [sucursalId, setSucursalId] = useState<string>('');
    
    // Datos cargados
    const [clientes, setClientes] = useState<ClientDTO[]>([]);
    const [productos, setProductos] = useState<ProductDTO[]>([]);
    const [sucursales, setSucursales] = useState<SucursalDTO[]>([]);
    const [loading, setLoading] = useState(true);
    const [error, setError] = useState<string>('');

    useEffect(() => {
        loadData();
    }, []);

    const loadData = async () => {
        try {
            setLoading(true);
            setError('');
            
            console.log('🔄 Iniciando carga de datos...');
            
            const [clientesData, productosData, sucursalesData] = await Promise.all([
                catalogoService.getClientes().catch(err => {
                    console.error('❌ Error al cargar clientes:', err);
                    return [];
                }),
                catalogoService.getProductos().catch(err => {
                    console.error('❌ Error al cargar productos:', err);
                    return [];
                }),
                catalogoService.getSucursales().catch(err => {
                    console.error('❌ Error al cargar sucursales:', err);
                    return [];
                })
            ]);
            
            // Asegurar que siempre sean arrays (no undefined)
            const clientesSeguro = Array.isArray(clientesData) ? clientesData : [];
            const productosSeguro = Array.isArray(productosData) ? productosData : [];
            const sucursalesSeguro = Array.isArray(sucursalesData) ? sucursalesData : [];
            
            setClientes(clientesSeguro);
            setProductos(productosSeguro);
            setSucursales(sucursalesSeguro);
            
            // Pre-seleccionar la sucursal del usuario autenticado si es posible
            const userDataJson = localStorage.getItem('farmacia_user');
            const userData = userDataJson ? JSON.parse(userDataJson) : null;
            if (userData?.idSucursal) {
                setSucursalId(String(userData.idSucursal));
            }
            
            if (clientesSeguro.length === 0) {
                console.warn('⚠️ No hay clientes disponibles');
            }
            if (productosSeguro.length === 0) {
                setError('⚠️ No hay productos disponibles. Verifica la conexión con el servidor.');
            }
            if (sucursalesSeguro.length === 0) {
                console.warn('⚠️ No hay sucursales disponibles');
            }
        } catch (error) {
            console.error('❌ Error al cargar datos:', error);
            setError('Error al cargar datos del servidor. Verifica tu conexión.');
        } finally {
            setLoading(false);
        }
    };

    const addProduct = () => {
        setItems([...items, { tempId: Date.now(), productId: '', quantity: 1 }]);
    };

    const removeProduct = (tempId: number) => {
        if (items.length > 1) {
            setItems(items.filter(item => item.tempId !== tempId));
        }
    };

    const updateProductInList = (index: number, prodId: string) => {
        const newItems = [...items];
        newItems[index].productId = prodId;
        setItems(newItems);
    };

    const updateQuantity = (index: number, qty: number) => {
        const newItems = [...items];
        newItems[index].quantity = qty;
        setItems(newItems);
    };

    const handleSave = async () => {
        // Validaciones
        if (tipoCliente === 'existente' && !clienteId) {
            return alert("Por favor selecciona un cliente");
        }
        
        if (tipoCliente === 'nuevo') {
            if (!nuevoCliente.nombre.trim()) return alert("El nombre del cliente es requerido");
            if (!nuevoCliente.telefono.trim()) return alert("El teléfono es requerido");
            if (!nuevoCliente.email.trim()) return alert("El email es requerido");
            if (!nuevoCliente.direccion.trim()) return alert("La dirección es requerida");
        }
        
        const detallesValidos = items.filter(i => i.productId !== '');
        if (detallesValidos.length === 0) return alert("Agrega al menos un producto");
        
        if (!sucursalId) return alert("Por favor selecciona una sucursal");

        // Obtener datos del usuario autenticado
        const userDataJson = localStorage.getItem('farmacia_user');
        const userData = userDataJson ? JSON.parse(userDataJson) : null;
        
        if (!userData) {
            return alert("No se pudo obtener los datos de tu usuario. Por favor inicia sesión nuevamente.");
        }

        try {
            // Obtener datos del cliente
            let clienteData = null;
            if (tipoCliente === 'existente') {
                clienteData = clientes.find(c => c.id === parseInt(clienteId));
                if (!clienteData) return alert("Cliente no encontrado");
            }

            const direccionEntrega = tipoCliente === 'nuevo' 
                ? nuevoCliente.direccion 
                : (clienteData?.email || '');
            
            const puntoRetiroFinal = puntoRetiro || direccionEntrega;

            const pedido: any = {
                IDSucursal: parseInt(sucursalId),
                IDUsuario: userData.id,
                FormaDePago: medioPago,
                PuntoRetiro: puntoRetiroFinal,
                Detalles: detallesValidos
                    .map(i => {
                        const producto = productos.find(p => String(p.id) === i.productId);
                        return {
                            IDProducto: parseInt(i.productId),
                            Cantidad: i.quantity,
                            PrecioUnitario: producto?.precio || 0
                        };
                    })
            };

            // Agregar datos según tipo de cliente
            if (tipoCliente === 'existente') {
                pedido.IDCliente = parseInt(clienteId);
            } else {
                // Cliente nuevo - NO incluir IDCliente, solo datos del cliente
                pedido.NombreCliente = `${nuevoCliente.nombre} ${nuevoCliente.apellido}`;
                pedido.Telefono = nuevoCliente.telefono;
                pedido.Email = nuevoCliente.email;
                pedido.Direccion = nuevoCliente.direccion;
            }

            console.log('📤 Enviando pedido:', pedido);

            const resultado = await pedidosService.createOrder(pedido);
            console.log('✅ Pedido creado:', resultado);
            alert(`Pedido creado con éxito. ID: ${resultado.pedidoId}`);
            navigate('/pedidos'); 
        } catch (error) {
            console.error("❌ Error al guardar:", error);
            alert("Error al guardar el pedido");
        }
    };

    const sucursalSeleccionada = sucursales.find(s => s.id === parseInt(sucursalId));

    return (
        <div className="min-h-screen bg-gray-50 p-4 lg:p-8">
            <div className="max-w-5xl mx-auto">
                <div className="flex items-center gap-4 mb-6">
                    <button onClick={() => navigate(-1)} className="p-2 hover:bg-gray-200 rounded-full text-gray-600">
                        <ArrowLeft size={20} />
                    </button>
                    <h1 className="text-2xl font-bold text-gray-800">Crear Nuevo Pedido</h1>
                </div>

                <div className="bg-white rounded-2xl shadow-sm border border-gray-200 overflow-hidden">
                    <div className="bg-blue-600 px-6 py-4 text-white font-semibold flex items-center gap-2">
                        <Plus size={20} /> Datos del Pedido
                    </div>

                    {error && (
                        <div className="px-6 py-3 bg-red-50 border-l-4 border-red-500 text-red-700">
                            {error}
                        </div>
                    )}

                    {loading ? (
                        <div className="p-8 text-center">
                            <div className="inline-block animate-spin rounded-full h-12 w-12 border-4 border-gray-200 border-t-blue-600"></div>
                            <p className="mt-4 text-gray-600">Cargando datos...</p>
                        </div>
                    ) : (
                        <div className="p-6 space-y-8">
                            {/* Detalles de Facturación */}
                            <div className="bg-gray-50 p-6 rounded-xl border border-gray-200">
                                <h3 className="text-lg font-semibold text-gray-800 mb-4 flex items-center gap-2">
                                    <User size={20} /> Detalles de Facturación
                                </h3>
                                
                                {/* Tipo de Cliente */}
                                <div className="mb-4">
                                    <div className="flex gap-4">
                                        <label className="flex items-center gap-2 cursor-pointer">
                                            <input 
                                                type="radio" 
                                                value="existente" 
                                                checked={tipoCliente === 'existente'}
                                                onChange={(e) => setTipoCliente(e.target.value as 'existente' | 'nuevo')}
                                                className="w-4 h-4"
                                            />
                                            <span className="text-sm font-medium">Cliente Existente</span>
                                        </label>
                                        <label className="flex items-center gap-2 cursor-pointer">
                                            <input 
                                                type="radio" 
                                                value="nuevo" 
                                                checked={tipoCliente === 'nuevo'}
                                                onChange={(e) => setTipoCliente(e.target.value as 'existente' | 'nuevo')}
                                                className="w-4 h-4"
                                            />
                                            <span className="text-sm font-medium">Cliente Nuevo</span>
                                        </label>
                                    </div>
                                </div>

                                {/* Cliente Existente */}
                                {tipoCliente === 'existente' && (
                                    <div className="mb-4 max-w-md">
                                        <SearchableSelect 
                                            label="Seleccionar Cliente" 
                                            options={clientes.map(c => ({
                                                id: c.id,
                                                label: `${c.nombre || ''} ${c.apellido || ''}`.trim(),
                                                subtext: c.telefono
                                            }))}
                                            onSelect={(opt: any) => {
                                                setClienteId(String(opt.id));
                                                console.log('✅ Cliente seleccionado:', opt.id);
                                            }}
                                            icon={User} 
                                            placeholder="Seleccione un cliente..."
                                        />
                                    </div>
                                )}

                                {/* Cliente Nuevo */}
                                {tipoCliente === 'nuevo' && (
                                    <div className="grid grid-cols-1 md:grid-cols-2 gap-4">
                                        <input
                                            type="text"
                                            placeholder="Nombre"
                                            value={nuevoCliente.nombre}
                                            onChange={(e) => setNuevoCliente({...nuevoCliente, nombre: e.target.value})}
                                            className="px-4 py-2 border border-gray-300 rounded-lg focus:ring-2 focus:ring-blue-500"
                                        />
                                        <input
                                            type="text"
                                            placeholder="Apellido"
                                            value={nuevoCliente.apellido}
                                            onChange={(e) => setNuevoCliente({...nuevoCliente, apellido: e.target.value})}
                                            className="px-4 py-2 border border-gray-300 rounded-lg focus:ring-2 focus:ring-blue-500"
                                        />
                                        <input
                                            type="tel"
                                            placeholder="Teléfono"
                                            value={nuevoCliente.telefono}
                                            onChange={(e) => setNuevoCliente({...nuevoCliente, telefono: e.target.value})}
                                            className="px-4 py-2 border border-gray-300 rounded-lg focus:ring-2 focus:ring-blue-500"
                                        />
                                        <input
                                            type="email"
                                            placeholder="Email"
                                            value={nuevoCliente.email}
                                            onChange={(e) => setNuevoCliente({...nuevoCliente, email: e.target.value})}
                                            className="px-4 py-2 border border-gray-300 rounded-lg focus:ring-2 focus:ring-blue-500"
                                        />
                                        <input
                                            type="text"
                                            placeholder="Dirección"
                                            value={nuevoCliente.direccion}
                                            onChange={(e) => setNuevoCliente({...nuevoCliente, direccion: e.target.value})}
                                            className="px-4 py-2 border border-gray-300 rounded-lg focus:ring-2 focus:ring-blue-500 md:col-span-2"
                                        />
                                    </div>
                                )}
                            </div>

                            {/* Detalles de Envío */}
                            <div className="bg-gray-50 p-6 rounded-xl border border-gray-200">
                                <h3 className="text-lg font-semibold text-gray-800 mb-4 flex items-center gap-2">
                                    <MapPin size={20} /> Detalles de Envío
                                </h3>
                                
                                <div className="grid grid-cols-1 md:grid-cols-2 gap-4">
                                    {/* Sucursal */}
                                    <SearchableSelect 
                                        label="Sucursal" 
                                        options={sucursales.map(s => ({
                                            id: s.id,
                                            label: s.nombre,
                                            subtext: s.direccion
                                        }))}
                                        onSelect={(opt: any) => {
                                            setSucursalId(String(opt.id));
                                            console.log('✅ Sucursal seleccionada:', opt.id);
                                        }}
                                        placeholder="Seleccione sucursal..."
                                    />

                                    {/* Medio de Pago */}
                                    <div>
                                        <label className="block text-sm font-medium text-gray-700 mb-1">
                                            <CreditCard size={16} className="inline mr-2" />
                                            Medio de Pago
                                        </label>
                                        <select
                                            value={medioPago}
                                            onChange={(e) => setMedioPago(e.target.value)}
                                            className="w-full px-4 py-2 border border-gray-300 rounded-lg focus:ring-2 focus:ring-blue-500"
                                        >
                                            <option value="Efectivo">Efectivo</option>
                                            <option value="Tarjeta Débito">Tarjeta Débito</option>
                                            <option value="Tarjeta Crédito">Tarjeta Crédito</option>
                                            <option value="Transferencia">Transferencia</option>
                                        </select>
                                    </div>

                                    {/* Punto de Retiro */}
                                    <div className="md:col-span-2">
                                        <label className="block text-sm font-medium text-gray-700 mb-1">
                                            <MapPin size={16} className="inline mr-2" />
                                            Punto de Retiro (opcional)
                                        </label>
                                        <input
                                            type="text"
                                            placeholder="Ej: Farmacia Centro, Av. Colon 123"
                                            value={puntoRetiro}
                                            onChange={(e) => setPuntoRetiro(e.target.value)}
                                            className="w-full px-4 py-2 border border-gray-300 rounded-lg focus:ring-2 focus:ring-blue-500"
                                        />
                                        <p className="text-xs text-gray-500 mt-1">Si no especificas un punto, se usará la dirección del cliente</p>
                                    </div>
                                </div>

                                {sucursalSeleccionada && (
                                    <div className="mt-4 p-3 bg-blue-50 rounded-lg border border-blue-200">
                                        <p className="text-sm text-blue-900">
                                            <strong>Sucursal:</strong> {sucursalSeleccionada.nombre}
                                        </p>
                                        <p className="text-sm text-blue-800"><strong>Dirección:</strong> {sucursalSeleccionada.direccion}</p>
                                        <p className="text-sm text-blue-800"><strong>Teléfono:</strong> {sucursalSeleccionada.telefono}</p>
                                    </div>
                                )}
                            </div>

                            {/* Listado de Productos */}
                            <div className="space-y-4">
                                <h3 className="text-lg font-medium text-gray-700 flex items-center gap-2">
                                    <Package size={18} /> Productos
                                </h3>
                                
                                {items.map((item, index) => (
                                    <div key={item.tempId} className="flex items-end gap-3 p-4 bg-gray-50 rounded-xl border border-gray-100">
                                        <div className="flex-1">
                                            <SearchableSelect 
                                                label={`Producto #${index + 1}`}
                                                options={productos.map(p => ({
                                                    id: p.id,
                                                    label: p.nombre || 'Sin nombre',
                                                    subtext: `Stock: ${p.stock || 0} - $${p.precio || 0}`
                                                }))}
                                                onSelect={(opt: any) => {
                                                    updateProductInList(index, String(opt.id));
                                                    console.log('✅ Producto seleccionado:', opt.id, opt.label);
                                                }}
                                                icon={ShoppingCart}
                                                placeholder={productos.length === 0 ? "No hay productos" : `Seleccionar producto...`}
                                            />
                                        </div>

                                        <div className="w-24">
                                            <label className="block text-sm font-medium text-gray-700 mb-1">Cantidad</label>
                                            <input
                                                type="number"
                                                min="1"
                                                max="999"
                                                value={item.quantity}
                                                onChange={(e) => updateQuantity(index, parseInt(e.target.value) || 1)}
                                                className="w-full px-3 py-2 border border-gray-300 rounded-lg focus:ring-2 focus:ring-blue-500"
                                            />
                                        </div>

                                        <button
                                            onClick={() => removeProduct(item.tempId)}
                                            className="p-2 hover:bg-red-50 rounded-lg text-red-600"
                                            disabled={items.length === 1}
                                        >
                                            <Trash2 size={20} />
                                        </button>
                                    </div>
                                ))}

                                <button
                                    onClick={addProduct}
                                    className="w-full py-3 border-2 border-dashed border-gray-300 rounded-lg text-gray-600 hover:border-blue-500 hover:text-blue-600 flex items-center justify-center gap-2 transition-colors"
                                >
                                    <Plus size={20} /> Agregar otro producto
                                </button>
                            </div>

                            {/* Botones de Acción */}
                            <div className="flex gap-3 pt-6 border-t">
                                <button
                                    onClick={() => navigate(-1)}
                                    className="flex-1 px-6 py-3 border border-gray-300 text-gray-700 rounded-lg hover:bg-gray-50 font-medium"
                                >
                                    <ArrowLeft size={18} className="inline mr-2" /> Cancelar
                                </button>
                                <button
                                    onClick={handleSave}
                                    className="flex-1 px-6 py-3 bg-blue-600 text-white rounded-lg hover:bg-blue-700 font-medium flex items-center justify-center gap-2"
                                >
                                    <Save size={18} /> Guardar Pedido
                                </button>
                            </div>
                        </div>
                    )}
                </div>
            </div>
        </div>
    );
};

export default OrderFormPage;