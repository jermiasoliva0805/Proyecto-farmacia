import React, { useState, useEffect } from 'react';
import { useAuth } from '@context/AuthContext';
import { usuariosService, UpdateUserDTO } from '@services/usuariosService';
import { X, Eye, EyeOff } from 'lucide-react';

interface UserProfileModalProps {
    isOpen: boolean;
    onClose: () => void;
}

export const UserProfileModal: React.FC<UserProfileModalProps> = ({ isOpen, onClose }) => {
    const { user } = useAuth();
    const [loading, setLoading] = useState(false);
    const [showPassword, setShowPassword] = useState(false);
    const [toast, setToast] = useState<{ message: string; type: 'success' | 'error' } | null>(null);
    const [errors, setErrors] = useState<Record<string, string>>({});

    const [formData, setFormData] = useState({
        mail: '',
        contraseña: ''
    });

    // Cargar datos del usuario al abrir modal
    useEffect(() => {
        if (isOpen && user) {
            setFormData({
                mail: user.email,
                contraseña: ''
            });
            setErrors({});
        }
    }, [isOpen, user]);

    // Auto-cerrar toast
    useEffect(() => {
        if (toast) {
            const timer = setTimeout(() => setToast(null), 3000);
            return () => clearTimeout(timer);
        }
    }, [toast]);

    const validateForm = () => {
        const newErrors: Record<string, string> = {};

        // Validar email (obligatorio)
        if (!formData.mail.trim()) {
            newErrors.mail = 'El correo es obligatorio';
        } else if (!/^[^\s@]+@[^\s@]+\.[^\s@]+$/.test(formData.mail)) {
            newErrors.mail = 'El formato del correo no es válido';
        }

        // Validar contraseña (opcional pero si la proporciona debe cumplir)
        if (formData.contraseña && formData.contraseña.length < 6) {
            newErrors.contraseña = 'La contraseña debe tener al menos 6 caracteres';
        }

        setErrors(newErrors);
        return Object.keys(newErrors).length === 0;
    };

    const validateField = (field: string, value: string) => {
        const newErrors = { ...errors };

        if (field === 'mail') {
            if (!value.trim()) {
                newErrors.mail = 'El correo es obligatorio';
            } else if (!/^[^\s@]+@[^\s@]+\.[^\s@]+$/.test(value)) {
                newErrors.mail = 'El formato del correo no es válido';
            } else {
                delete newErrors.mail;
            }
        } else if (field === 'contraseña') {
            if (value && value.length < 6) {
                newErrors.contraseña = 'La contraseña debe tener al menos 6 caracteres';
            } else {
                delete newErrors.contraseña;
            }
        }

        setErrors(newErrors);
    };

    const handleInputChange = (field: string, value: string) => {
        setFormData(prev => ({ ...prev, [field]: value }));
        validateField(field, value);
    };

    const handleGuardar = async () => {
        if (!validateForm() || !user) return;

        try {
            setLoading(true);
            
            // Construir objeto de actualización
            const updateData: UpdateUserDTO = {
                mail: formData.mail
            };

            // Solo incluir contraseña si se proporciona
            if (formData.contraseña.trim()) {
                updateData.contraseña = formData.contraseña;
            }

            await usuariosService.updateUsuario(user.id, updateData);
            setToast({ message: 'Perfil actualizado correctamente', type: 'success' });
            
            // Limpiar formulario y cerrar modal después de 1.5s
            setTimeout(() => {
                setFormData({ mail: user.email, contraseña: '' });
                onClose();
            }, 1500);
        } catch (error: any) {
            const errorMsg = error.response?.data?.message || 'Error al actualizar perfil';
            setToast({ message: errorMsg, type: 'error' });
        } finally {
            setLoading(false);
        }
    };

    if (!isOpen) return null;

    return (
        <>
            {/* Overlay */}
            <div
                className="fixed inset-0 bg-black/50 z-40"
                onClick={onClose}
            />

            {/* Modal */}
            <div className="fixed inset-0 z-50 flex items-center justify-center p-4 sm:p-0">
                <div className="bg-white rounded-xl shadow-2xl w-full max-w-md max-h-[90vh] overflow-y-auto">
                    {/* Header */}
                    <div className="flex items-center justify-between p-6 border-b border-gray-200 bg-white">
                        <h2 className="text-lg font-bold text-gray-900">Mi Perfil</h2>
                        <button
                            onClick={onClose}
                            className="p-1 hover:bg-gray-100 rounded-lg transition-colors text-gray-600"
                        >
                            <X className="w-5 h-5" />
                        </button>
                    </div>

                    {/* Content */}
                    <div className="p-6 space-y-5 bg-white">
                        {/* Información no editable */}
                        <div className="space-y-2">
                            <label className="text-sm font-semibold text-gray-900">Nombre Completo</label>
                            <div className="px-4 py-2.5 bg-gray-100 rounded-lg text-gray-900 border border-gray-300 font-medium text-sm">
                                {user?.nombreCompleto}
                            </div>
                        </div>

                        <div className="space-y-2">
                            <label className="text-sm font-semibold text-gray-900">Usuario</label>
                            <div className="px-4 py-2.5 bg-gray-100 rounded-lg text-gray-900 border border-gray-300 font-medium text-sm">
                                {user?.usuario}
                            </div>
                        </div>

                        <div className="space-y-2">
                            <label className="text-sm font-semibold text-gray-900">Rol</label>
                            <div className="px-4 py-2.5 bg-gray-100 rounded-lg text-gray-900 border border-gray-300 font-medium text-sm">
                                {user?.rol}
                            </div>
                        </div>

                        {/* Email (editable) */}
                        <div className="space-y-2">
                            <label className="text-sm font-semibold text-gray-900">
                                Email <span className="text-red-500">*</span>
                            </label>
                            <input
                                type="email"
                                value={formData.mail}
                                onChange={(e) => handleInputChange('mail', e.target.value)}
                                className={`w-full px-4 py-2.5 border rounded-lg text-black placeholder-gray-400 transition-all font-medium text-sm bg-white ${
                                    errors.mail
                                        ? 'border-red-500 focus:ring-red-500'
                                        : 'border-gray-300 focus:ring-blue-500'
                                } focus:ring-1 focus:outline-none`}
                                placeholder="tu@email.com"
                            />
                            {errors.mail && (
                                <p className="text-sm text-red-600 mt-1">{errors.mail}</p>
                            )}
                        </div>

                        {/* Contraseña (editable, opcional) */}
                        <div className="space-y-2">
                            <label className="text-sm font-semibold text-gray-900">
                                Nueva Contraseña <span className="text-gray-500 text-xs font-normal">(opcional)</span>
                            </label>
                            <div className="relative">
                                <input
                                    type={showPassword ? 'text' : 'password'}
                                    value={formData.contraseña}
                                    onChange={(e) => handleInputChange('contraseña', e.target.value)}
                                    className={`w-full px-4 py-2.5 pr-10 border rounded-lg text-black placeholder-gray-400 transition-all font-medium text-sm bg-white ${
                                        errors.contraseña
                                            ? 'border-red-500 focus:ring-red-500'
                                            : 'border-gray-300 focus:ring-blue-500'
                                    } focus:ring-1 focus:outline-none`}
                                    placeholder="Mínimo 6 caracteres"
                                />
                                <button
                                    type="button"
                                    onClick={() => setShowPassword(!showPassword)}
                                    className="absolute right-3 top-1/2 -translate-y-1/2 text-gray-600 hover:text-gray-900"
                                >
                                    {showPassword ? (
                                        <EyeOff className="w-4 h-4" />
                                    ) : (
                                        <Eye className="w-4 h-4" />
                                    )}
                                </button>
                            </div>
                            {errors.contraseña && (
                                <p className="text-sm text-red-600 mt-1">{errors.contraseña}</p>
                            )}
                            {!errors.contraseña && formData.contraseña && (
                                <p className="text-sm text-green-600 mt-1">✓ Contraseña válida</p>
                            )}
                        </div>
                    </div>

                    {/* Toast */}
                    {toast && (
                        <div className={`px-6 py-3 border-t text-sm font-medium ${
                            toast.type === 'success'
                                ? 'bg-green-50 border-green-200 text-green-700'
                                : 'bg-red-50 border-red-200 text-red-700'
                        }`}>
                            {toast.message}
                        </div>
                    )}

                    {/* Footer */}
                    <div className="flex gap-3 p-6 border-t border-gray-200 bg-gray-50">
                        <button
                            onClick={onClose}
                            className="flex-1 px-4 py-2.5 text-gray-900 bg-white border border-gray-300 rounded-lg hover:bg-gray-50 font-semibold transition-colors text-sm"
                        >
                            Cancelar
                        </button>
                        <button
                            onClick={handleGuardar}
                            disabled={loading}
                            className="flex-1 px-4 py-2.5 bg-blue-600 text-white rounded-lg hover:bg-blue-700 disabled:bg-blue-400 font-semibold transition-colors text-sm"
                        >
                            {loading ? 'Guardando...' : 'Guardar Cambios'}
                        </button>
                    </div>
                </div>
            </div>
        </>
    );
};
