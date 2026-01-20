import React, { useState } from 'react';
import { useAuth } from '@context/AuthContext';
import { Button } from '@components/common/Button';
import { Input } from '@components/common/Input';
import { Alert } from '@components/common/Alert';
import { Pill, Lock, User } from 'lucide-react';

export const Login: React.FC = () => {
    const { login } = useAuth();
    const [formData, setFormData] = useState({
        usuario: '',
        password: '',
    });
    const [selectedRole, setSelectedRole] = useState<string>('');
    const [error, setError] = useState<string>('');
    const [isLoading, setIsLoading] = useState(false);

    const roles = [
        { value: 'Administrador', label: 'Administrador', color: 'bg-blue-600 hover:bg-blue-700' },
        { value: 'Operario', label: 'Operario', color: 'bg-green-600 hover:bg-green-700' },
        { value: 'Cadete', label: 'Cadete', color: 'bg-purple-600 hover:bg-purple-700' },
    ];

    const handleSubmit = async (e: React.FormEvent) => {
        e.preventDefault();
        setError('');

        if (!selectedRole) {
            setError('Por favor seleccione el tipo de usuario');
            return;
        }

        if (!formData.usuario || !formData.password) {
            setError('Por favor complete todos los campos');
            return;
        }

        setIsLoading(true);

        try {
            await login(formData.password, formData.usuario);
            localStorage.setItem('farmacia_role', selectedRole);
        } catch (err: any) {
            setError(err.response?.data?.message || 'Usuario o contraseña incorrectos');
        } finally {
            setIsLoading(false);
        }
    };

    return (
        <div className="min-h-screen bg-gradient-to-br from-blue-50 via-white to-blue-50 flex items-center justify-center p-4">
            <div className="w-full max-w-md">
                <div className="text-center mb-8">
                    <div className="inline-flex items-center justify-center w-16 h-16 bg-blue-600 rounded-full mb-4">
                        <Pill className="w-8 h-8 text-white" />
                    </div>
                    <h1 className="text-3xl font-bold text-gray-900 mb-2">
                        Farmacia General Paz
                    </h1>
                    <p className="text-gray-600">Sistema de Gestión de Pedidos</p>
                </div>

                <div className="bg-white rounded-2xl shadow-xl p-8">
                    <h2 className="text-2xl font-semibold text-gray-900 mb-6 text-center">
                        Iniciar Sesión
                    </h2>

                    <div className="mb-6">
                        <label className="block text-sm font-medium text-gray-700 mb-3">
                            Seleccione el tipo de usuario
                        </label>
                        <div className="grid grid-cols-1 gap-3">
                            {roles.map((role) => (
                                <button
                                    key={role.value}
                                    type="button"
                                    onClick={() => setSelectedRole(role.value)}
                                    className={`p-4 rounded-lg border-2 transition-all ${
                                        selectedRole === role.value
                                            ? `${role.color} text-white border-transparent`
                                            : 'bg-white text-gray-700 border-gray-200 hover:border-gray-300'
                                    }`}
                                >
                                    <span className="font-medium">{role.label}</span>
                                </button>
                            ))}
                        </div>
                    </div>

                    {selectedRole && (
                        <form onSubmit={handleSubmit} className="space-y-4">
                            {error && (
                                <Alert type="error">
                                    {error}
                                </Alert>
                            )}

                            <div className="relative">
                                <User className="absolute left-3 top-1/2 transform -translate-y-1/2 text-gray-400 w-5 h-5" />
                                <Input
                                    type="text"
                                    placeholder="Usuario"
                                    value={formData.usuario}
                                    onChange={(e) => setFormData({ ...formData, usuario: e.target.value })}
                                    className="w-full pl-10 pr-4 py-3 border border-gray-300 rounded-lg focus:ring-2 focus:ring-blue-500 focus:border-transparent"
                                    disabled={isLoading}
                                />
                            </div>

                            <div className="relative">
                                <Lock className="absolute left-3 top-1/2 transform -translate-y-1/2 text-gray-400 w-5 h-5" />
                                <Input
                                    type="password"
                                    placeholder="Contraseña"
                                    value={formData.password}
                                    onChange={(e) => setFormData({ ...formData, password: e.target.value })}
                                    className="w-full pl-10 pr-4 py-3 border border-gray-300 rounded-lg focus:ring-2 focus:ring-blue-500 focus:border-transparent"
                                    disabled={isLoading}
                                />
                            </div>

                            <Button
                                type="submit"
                                variant="primary"
                                className="w-full py-3"
                                isLoading={isLoading}
                            >
                                Ingresar al Sistema
                            </Button>
                        </form>
                    )}
                </div>

                <div className="text-center mt-6 text-sm text-gray-600">
                    <p>© 2026 Farmacia General Paz - Córdoba, Argentina</p>
                </div>
            </div>
        </div>
    );
};