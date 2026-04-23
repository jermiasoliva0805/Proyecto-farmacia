import React, { useEffect, useState } from 'react';
import { DashboardLayout } from '@components/layout/DashboardLayout';
import { usuariosService, RegisterDTO } from '../service/usuariosService';
import { api } from '../service/api';
import { UserDTO } from '../types/auth.types';
import { Plus, Pencil, Trash2, UserCircle } from 'lucide-react';

const ROLE_THEMES: Record<string, any> = {
    'Encargado': {
        bgBadge: 'bg-blue-100',
        textMain: 'text-blue-700',
        border: 'border-blue-200',
        button: 'bg-blue-600 hover:bg-blue-700'
    },
    'Operario': {
        bgBadge: 'bg-yellow-100',
        textMain: 'text-yellow-700',
        border: 'border-yellow-300',
        button: 'bg-yellow-500 hover:bg-yellow-600'
    },
    'Cadete': {
        bgBadge: 'bg-emerald-100',
        textMain: 'text-emerald-700',
        border: 'border-emerald-200',
        button: 'bg-emerald-600 hover:bg-emerald-700'
    }
};

const UsuariosPage = () => {
    const [usuarios, setUsuarios] = useState<UserDTO[]>([]);
    const [loading, setLoading] = useState(true);

    const [isModalOpen, setIsModalOpen] = useState(false);
    const [isDeleteModalOpen, setIsDeleteModalOpen] = useState(false);
    const [userToDelete, setUserToDelete] = useState<any>(null);
    const [editingId, setEditingId] = useState<number | null>(null);
    const [formData, setFormData] = useState<RegisterDTO>({
        nombre: '',
        apellido: '',
        usuarioNombre: '',
        contraseña: '',
        rol: 'Encargado',
        mail: '',
        idSucursal: 1,
        zonaId: undefined
    });

    const [zonas, setZonas] = useState<Array<{ id: number; nombre: string }>>([]);
    const [loadingZonas, setLoadingZonas] = useState(false);
    const [errors, setErrors] = useState<Record<string, string>>({}); const [toast, setToast] = useState<{ message: string; type: 'success' | 'error' } | null>(null);

    // Auto-cerrar toast después de 3 segundos
    useEffect(() => {
        if (toast) {
            const timer = setTimeout(() => {
                setToast(null);
            }, 3000);
            return () => clearTimeout(timer);
        }
    }, [toast]);

    // Validaciones
    const validateForm = () => {
        const newErrors: Record<string, string> = {};

        // En edición, solo validamos los campos que se pueden editar
        if (editingId) {
            // En edición no validamos nombre/apellido/usuarioNombre a menos que estén siendo editados
            // Solo validamos los que se pueden cambiar en edición
            if (formData.nombre && formData.nombre.trim().length > 100) {
                newErrors.nombre = 'El nombre no puede exceder 100 caracteres';
            }

            if (formData.apellido && formData.apellido.trim().length > 100) {
                newErrors.apellido = 'El apellido no puede exceder 100 caracteres';
            }

            if (formData.mail && !formData.mail.trim()) {
                newErrors.mail = 'El correo es obligatorio';
            } else if (formData.mail && !/^[^\s@]+@[^\s@]+\.[^\s@]+$/.test(formData.mail)) {
                newErrors.mail = 'El formato del correo no es válido';
            }

            // Validar contraseña solo si la proporciona
            if (formData.contraseña && formData.contraseña.length < 6) {
                newErrors.contraseña = 'La contraseña debe tener al menos 6 caracteres';
            }

            setErrors(newErrors);
            return Object.keys(newErrors).length === 0;
        }

        // En creación, validamos todos los campos
        if (!formData.nombre.trim()) {
            newErrors.nombre = 'El nombre es obligatorio';
        } else if (formData.nombre.length > 100) {
            newErrors.nombre = 'El nombre no puede exceder 100 caracteres';
        }

        if (!formData.apellido.trim()) {
            newErrors.apellido = 'El apellido es obligatorio';
        } else if (formData.apellido.length > 100) {
            newErrors.apellido = 'El apellido no puede exceder 100 caracteres';
        }

        if (!formData.usuarioNombre.trim()) {
            newErrors.usuarioNombre = 'El usuario es obligatorio';
        } else if (formData.usuarioNombre.length < 3 || formData.usuarioNombre.length > 50) {
            newErrors.usuarioNombre = 'El usuario debe tener entre 3 y 50 caracteres';
        }

        if (!formData.mail.trim()) {
            newErrors.mail = 'El correo es obligatorio';
        } else if (!/^[^\s@]+@[^\s@]+\.[^\s@]+$/.test(formData.mail)) {
            newErrors.mail = 'El formato del correo no es válido';
        }

        if (!editingId && !formData.contraseña.trim()) {
            newErrors.contraseña = 'La contraseña es obligatoria';
        } else if (!editingId && formData.contraseña.length < 6) {
            newErrors.contraseña = 'La contraseña debe tener al menos 6 caracteres';
        }

        setErrors(newErrors);
        return Object.keys(newErrors).length === 0;
    };

    const validateField = (field: string, value: string | number) => {
        const newErrors = { ...errors };

        switch (field) {
            case 'nombre':
                if (!value) {
                    newErrors.nombre = 'El nombre es obligatorio';
                } else if (String(value).length > 100) {
                    newErrors.nombre = 'El nombre no puede exceder 100 caracteres';
                } else {
                    delete newErrors.nombre;
                }
                break;
            case 'apellido':
                if (!value) {
                    newErrors.apellido = 'El apellido es obligatorio';
                } else if (String(value).length > 100) {
                    newErrors.apellido = 'El apellido no puede exceder 100 caracteres';
                } else {
                    delete newErrors.apellido;
                }
                break;
            case 'usuarioNombre':
                if (!value) {
                    newErrors.usuarioNombre = 'El usuario es obligatorio';
                } else if (String(value).length < 3 || String(value).length > 50) {
                    newErrors.usuarioNombre = 'El usuario debe tener entre 3 y 50 caracteres';
                } else {
                    delete newErrors.usuarioNombre;
                }
                break;
            case 'mail':
                if (!value) {
                    newErrors.mail = 'El correo es obligatorio';
                } else if (!/^[^\s@]+@[^\s@]+\.[^\s@]+$/.test(String(value))) {
                    newErrors.mail = 'El formato del correo no es válido';
                } else {
                    delete newErrors.mail;
                }
                break;
            case 'contraseña':
                if (!editingId) {
                    if (!value) {
                        newErrors.contraseña = 'La contraseña es obligatoria';
                    } else if (String(value).length < 6) {
                        newErrors.contraseña = 'La contraseña debe tener al menos 6 caracteres';
                    } else {
                        delete newErrors.contraseña;
                    }
                } else {
                    delete newErrors.contraseña;
                }
                break;
        }

        setErrors(newErrors);
    };

    useEffect(() => {
        fetchUsuarios();
        fetchZonas();
    }, []);

    const fetchZonas = async () => {
        try {
            setLoadingZonas(true);
            const response = await api.get('/localidades/zonas');
            setZonas(response.data || []);
        } catch (error) {
            console.error("Error al cargar zonas:", error);
        } finally {
            setLoadingZonas(false);
        }
    };

    const abrirModalEdicion = async (id: number) => {
        try {
            const usuario = await usuariosService.getUsuarioById(id);
            setFormData({
                nombre: usuario.nombreCompleto.split(' ')[0] || '',
                apellido: usuario.nombreCompleto.split(' ').slice(1).join(' ') || '',
                usuarioNombre: usuario.usuario,
                contraseña: '',
                rol: usuario.rol,
                mail: usuario.email,
                idSucursal: 1,
                zonaId: usuario.zonaId
            });
            setEditingId(id);
            setErrors({});
            setIsModalOpen(true);
        } catch (error) {
            setToast({ message: "Error al cargar el usuario", type: 'error' });
        }
    };

    const abrirModalNuevo = () => {
        setFormData({
            nombre: '',
            apellido: '',
            usuarioNombre: '',
            contraseña: '',
            rol: 'Encargado',
            mail: '',
            idSucursal: 1,
            zonaId: undefined
        });
        setEditingId(null);
        setErrors({});
        setIsModalOpen(true);
    };

    const fetchUsuarios = async () => {
        try {
            setLoading(true);
            const data = await usuariosService.getAllUsuarios();
            setUsuarios(data);
        } catch (error) {
            console.error("Error al cargar usuarios:", error);
        } finally {
            setLoading(false);
        }
    };

    const handleGuardar = async () => {
        if (!validateForm()) {
            return;
        }

        try {
            if (editingId) {
                // Editar usuario
                const updateData: any = {
                    nombre: formData.nombre,
                    apellido: formData.apellido,
                    rol: formData.rol,
                    mail: formData.mail,
                    idSucursal: formData.idSucursal
                };
                
                // Incluir zonaId si es cadete
                if (formData.rol === 'Cadete') {
                    updateData.zonaId = formData.zonaId || null;
                }
                
                // Solo incluir contraseña si se proporciona
                if (formData.contraseña.trim()) {
                    updateData.contraseña = formData.contraseña;
                }

                await usuariosService.updateUsuario(editingId, updateData);
                setToast({ message: "Usuario actualizado correctamente", type: 'success' });
            } else {
                // Crear nuevo usuario
                const createData = { ...formData };
                
                // Si no es cadete, eliminar zonaId
                if (createData.rol !== 'Cadete') {
                    createData.zonaId = undefined;
                }
                
                await usuariosService.createUsuario(createData);
                setToast({ message: "Usuario registrado correctamente", type: 'success' });
            }
            
            setIsModalOpen(false);
            await fetchUsuarios();
            // Limpiamos el formulario
            setFormData({ nombre: '', apellido: '', usuarioNombre: '', contraseña: '', rol: 'Encargado', mail: '', idSucursal: 1, zonaId: undefined });
            setEditingId(null);
            setErrors({});
        } catch (error: any) {
            const msg = error.response?.data?.message || "Error al guardar el usuario";
            setToast({ message: msg, type: 'error' });
        }
    };

    const handleEliminar = (usuario: any) => {
        setUserToDelete(usuario);
        setIsDeleteModalOpen(true);
    };

    const confirmDelete = async () => {
        if (!userToDelete) return;
        
        try {
            await usuariosService.deleteUsuario(userToDelete.id);
            setUsuarios(prev => prev.filter(u => u.id !== userToDelete.id));
            setToast({ message: "Usuario eliminado correctamente", type: 'success' });
            setIsDeleteModalOpen(false);
            setUserToDelete(null);
        } catch (error: any) {
            const msg = error.response?.data?.message || "Error al eliminar el usuario";
            setToast({ message: msg, type: 'error' });
        }
    };

    const cancelDelete = () => {
        setIsDeleteModalOpen(false);
        setUserToDelete(null);
    };

    return (
        <DashboardLayout>
            <div className="p-6 bg-gray-50 min-h-screen">
                {/* Header */}
                <div className="flex justify-between items-center mb-6">
                    <div>
                        <h1 className="text-2xl font-bold text-gray-800">Gestión de Personal</h1>
                    </div>
                    <button
                        onClick={() => abrirModalNuevo()}
                        className="flex items-center gap-2 bg-blue-600 text-white px-4 py-2 rounded-lg hover:bg-blue-700 transition-colors shadow-lg shadow-blue-200"
                    >
                        <Plus size={20} />
                        Nuevo Usuario
                    </button>
                </div>

                {/* Tabla Principal */}
                <div className="bg-white rounded-xl shadow-sm border border-gray-100 overflow-hidden">
                <table className="w-full text-left">
                    <thead className="bg-gray-50 border-b border-gray-100">
                        <tr>
                            <th className="px-6 py-4 text-sm font-semibold text-gray-600">Personal</th>
                            <th className="px-6 py-4 text-sm font-semibold text-gray-600">Usuario</th>
                            <th className="px-6 py-4 text-sm font-semibold text-gray-600">Rol</th>
                            <th className="px-6 py-4 text-sm font-semibold text-gray-600">Sucursal</th>
                            <th className="px-6 py-4 text-sm font-semibold text-gray-600 text-right">Acciones</th>
                        </tr>
                    </thead>
                    <tbody className="divide-y divide-gray-50">
                        {loading ? (
                            <tr><td colSpan={5} className="text-center py-10 text-gray-400">Cargando personal...</td></tr>
                        ) : usuarios.map((user) => {
                            const theme = ROLE_THEMES[user.rol] || ROLE_THEMES['Encargado'];
                            return (
                                <tr key={user.id} className="hover:bg-gray-50 transition-colors">
                                    <td className="px-6 py-4">
                                        <div className="flex items-center gap-3">
                                            <div className={`p-2 rounded-full ${theme.bgBadge} ${theme.textMain}`}>
                                                <UserCircle size={24} />
                                            </div>
                                            <div>
                                                <div className="font-medium text-gray-800">{user.nombreCompleto}</div>
                                                <div className="text-xs text-gray-400">{user.email}</div>
                                            </div>
                                        </div>
                                    </td>
                                    <td className="px-6 py-4 text-sm text-gray-600">{user.usuario}</td>
                                    <td className="px-6 py-4">
                                        <span className={`px-3 py-1 rounded-full text-xs font-bold border ${theme.bgBadge} ${theme.textMain} ${theme.border}`}>
                                            {user.rol.toUpperCase()}
                                        </span>
                                    </td>
                                    <td className="px-6 py-4 text-sm text-gray-600">{user.nombreSucursal}</td>
                                    <td className="px-6 py-4 text-right">
                                        <div className="flex justify-end gap-2">
                                            <button 
                                                onClick={() => abrirModalEdicion(user.id)}
                                                className="p-2 text-gray-400 hover:text-blue-600 transition-colors"
                                            >
                                                <Pencil size={18} />
                                            </button>
                                            <button 
                                                onClick={() => handleEliminar(user)}
                                                className="p-2 text-gray-400 hover:text-red-600 transition-colors"
                                            >
                                                <Trash2 size={18} />
                                            </button>
                                        </div>
                                    </td>
                                </tr>
                            );
                        })}
                    </tbody>
                </table>
                </div>

                {/* Modal de Registro/Edición */}
                {isModalOpen && (
                    <div className="fixed inset-0 bg-black/40 backdrop-blur-sm flex items-center justify-center z-50 p-4">
                    <div className="bg-white rounded-2xl shadow-2xl w-full max-w-md overflow-hidden animate-in fade-in zoom-in duration-200 max-h-[90vh] overflow-y-auto">
                        <div className={`p-6 border-b ${ROLE_THEMES[formData.rol].border} bg-gray-50`}>
                            <h2 className={`text-xl font-bold ${ROLE_THEMES[formData.rol].textMain}`}>
                                {editingId ? `Editar ${formData.rol}` : `Registrar ${formData.rol}`}
                            </h2>
                        </div>
                        <div className="p-6 space-y-4">
                            <div className="grid grid-cols-2 gap-4">
                                <div>
                                    <input 
                                        placeholder="Nombre" 
                                        value={formData.nombre}
                                        className={`w-full p-2 border rounded-lg outline-none focus:ring-2 focus:ring-blue-500 ${errors.nombre ? 'border-red-500' : ''}`}
                                        onChange={(e) => {
                                            setFormData({...formData, nombre: e.target.value});
                                            validateField('nombre', e.target.value);
                                        }} 
                                    />
                                    {errors.nombre && <p className="text-red-500 text-xs mt-1">{errors.nombre}</p>}
                                </div>
                                <div>
                                    <input 
                                        placeholder="Apellido"
                                        value={formData.apellido}
                                        className={`w-full p-2 border rounded-lg outline-none focus:ring-2 focus:ring-blue-500 ${errors.apellido ? 'border-red-500' : ''}`}
                                        onChange={(e) => {
                                            setFormData({...formData, apellido: e.target.value});
                                            validateField('apellido', e.target.value);
                                        }} 
                                    />
                                    {errors.apellido && <p className="text-red-500 text-xs mt-1">{errors.apellido}</p>}
                                </div>
                            </div>
                            <div>
                                <input 
                                    placeholder="Email"
                                    value={formData.mail}
                                    className={`w-full p-2 border rounded-lg outline-none focus:ring-2 focus:ring-blue-500 ${errors.mail ? 'border-red-500' : ''}`}
                                    onChange={(e) => {
                                        setFormData({...formData, mail: e.target.value});
                                        validateField('mail', e.target.value);
                                    }} 
                                />
                                {errors.mail && <p className="text-red-500 text-xs mt-1">{errors.mail}</p>}
                            </div>
                            <div className="grid grid-cols-2 gap-4">
                                <div>
                                    <input 
                                        placeholder="Usuario"
                                        value={formData.usuarioNombre}
                                        className={`w-full p-2 border rounded-lg outline-none focus:ring-2 focus:ring-blue-500 ${errors.usuarioNombre ? 'border-red-500' : ''}`}
                                        onChange={(e) => {
                                            setFormData({...formData, usuarioNombre: e.target.value});
                                            validateField('usuarioNombre', e.target.value);
                                        }} 
                                    />
                                    {errors.usuarioNombre && <p className="text-red-500 text-xs mt-1">{errors.usuarioNombre}</p>}
                                </div>
                                <div>
                                    <input 
                                        placeholder={editingId ? "Contraseña (dejar vacío para mantener)" : "Contraseña"}
                                        type="password"
                                        value={formData.contraseña}
                                        className={`w-full p-2 border rounded-lg outline-none focus:ring-2 focus:ring-blue-500 ${errors.contraseña ? 'border-red-500' : ''}`}
                                        onChange={(e) => {
                                            setFormData({...formData, contraseña: e.target.value});
                                            validateField('contraseña', e.target.value);
                                        }} 
                                    />
                                    {errors.contraseña && <p className="text-red-500 text-xs mt-1">{errors.contraseña}</p>}
                                    {editingId && <p className="text-gray-400 text-xs mt-1">Opcional: Déjalo vacío para no cambiarla</p>}
                                </div>
                            </div>
                            <div className="space-y-1">
                                <label className="text-xs font-bold text-gray-400 uppercase">Rol Asignado</label>
                                <select 
                                    value={formData.rol}
                                    onChange={(e) => setFormData({...formData, rol: e.target.value})}
                                    className={`w-full p-2 border rounded-lg outline-none transition-colors ${ROLE_THEMES[formData.rol].border}`}
                                >
                                    <option value="Encargado">Encargado</option>
                                    <option value="Operario">Operario</option>
                                    <option value="Cadete">Cadete</option>
                                </select>
                            </div>

                            {/* Selector de Zona - Solo para Cadetes */}
                            {formData.rol === 'Cadete' && (
                                <div className="space-y-1">
                                    <label className="text-xs font-bold text-gray-400 uppercase">Zona de Reparto</label>
                                    <select 
                                        value={formData.zonaId || ''}
                                        onChange={(e) => setFormData({...formData, zonaId: e.target.value ? parseInt(e.target.value) : undefined})}
                                        className="w-full p-2 border border-emerald-300 rounded-lg outline-none focus:ring-2 focus:ring-emerald-500 transition-colors"
                                        disabled={loadingZonas || zonas.length === 0}
                                    >
                                        <option value="">-- Seleccionar Zona --</option>
                                        {zonas.map(zona => (
                                            <option key={zona.id} value={zona.id}>
                                                {zona.nombre}
                                            </option>
                                        ))}
                                    </select>
                                    {zonas.length === 0 && !loadingZonas && (
                                        <p className="text-red-500 text-xs mt-1">No hay zonas disponibles</p>
                                    )}
                                    {loadingZonas && (
                                        <p className="text-gray-500 text-xs mt-1">Cargando zonas...</p>
                                    )}
                                </div>
                            )}
                        </div>
                        <div className="p-6 bg-gray-50 flex justify-end gap-3 border-t">
                            <button 
                                onClick={() => {
                                    setIsModalOpen(false);
                                    setEditingId(null);
                                    setErrors({});
                                }} 
                                className="px-4 py-2 text-gray-500 font-medium hover:text-gray-700"
                            >
                                Cancelar
                            </button>
                            <button 
                                onClick={handleGuardar}
                                disabled={Object.keys(errors).length > 0}
                                className={`px-6 py-2 text-white rounded-lg font-bold shadow-md transition-all ${ROLE_THEMES[formData.rol].button} ${Object.keys(errors).length > 0 ? 'opacity-50 cursor-not-allowed' : ''}`}
                            >
                                {editingId ? 'Actualizar' : 'Guardar'} Personal
                            </button>
                        </div>
                    </div>
                    </div>
                )}

                {/* Modal de Confirmación de Eliminación */}
                {isDeleteModalOpen && userToDelete && (
                    <div className="fixed inset-0 bg-black/40 backdrop-blur-sm flex items-center justify-center z-50 p-4">
                    <div className="bg-white rounded-2xl shadow-2xl w-full max-w-md overflow-hidden animate-in fade-in zoom-in duration-200">
                        <div className="p-6 border-b border-red-200 bg-red-50">
                            <h2 className="text-xl font-bold text-red-700">
                                Confirmar Eliminación
                            </h2>
                        </div>
                        <div className="p-6 space-y-4">
                            <p className="text-gray-700">
                                ¿Estás seguro de que deseas dar de baja a <span className="font-bold">{userToDelete.nombreCompleto}</span>?
                            </p>
                            <p className="text-sm text-red-600 font-medium">
                                Esta acción no se puede deshacer.
                            </p>
                        </div>
                        <div className="p-6 bg-gray-50 flex justify-end gap-3 border-t">
                            <button 
                                onClick={cancelDelete}
                                className="px-4 py-2 text-gray-500 font-medium hover:text-gray-700 transition-colors"
                            >
                                Cancelar
                            </button>
                            <button 
                                onClick={confirmDelete}
                                className="px-6 py-2 bg-red-600 hover:bg-red-700 text-white rounded-lg font-bold shadow-md transition-all"
                            >
                                Eliminar
                            </button>
                        </div>
                    </div>
                    </div>
                )}

                {/* Toast Notification */}
                {toast && (
                    <div className={`fixed bottom-6 right-6 px-6 py-4 rounded-lg shadow-2xl text-white font-medium animate-in fade-in slide-in-from-right-5 duration-300 z-[60] ${
                        toast.type === 'success'
                            ? 'bg-green-500 hover:bg-green-600'
                            : 'bg-red-500 hover:bg-red-600'
                    } transition-all`}>
                        {toast.type === 'success' ? '✓ ' : '✕ '}
                        {toast.message}
                    </div>
                )}
            </div>
        </DashboardLayout>
    );
};

export default UsuariosPage;
