import React, { useState } from 'react';
import { useAuth } from '../context/AuthContext'; 
import { useNavigate } from 'react-router-dom';
import { User, Lock, Loader2, ChevronRight, ArrowLeft } from 'lucide-react';

// CONFIGURACIÓN VISUAL Y LÓGICA DE LOS ROLES
const ROLES_CONFIG = [
    { 
        value: 'Encargado', 
        label: 'Encargado', 
        // Estilos visuales (Tema Azul)
        colorTheme: {
            border: 'border-blue-200',
            borderFocus: 'focus-within:border-blue-500',
            bgBadge: 'bg-blue-100', // Fondo suave para etiquetas
            textMain: 'text-blue-700',
            textLabel: 'text-blue-900',
            button: 'bg-blue-600 hover:bg-blue-700 shadow-blue-200'
        }
    },
    { 
        value: 'Operario', 
        label: 'Operario', 
        // Estilos visuales (Tema Amarillo/Ocre)
        colorTheme: {
            border: 'border-yellow-300',
            borderFocus: 'focus-within:border-yellow-600',
            bgBadge: 'bg-yellow-100',
            textMain: 'text-yellow-700',
            textLabel: 'text-yellow-800',
            button: 'bg-yellow-500 hover:bg-yellow-600 shadow-yellow-200'
        }
    },
    { 
        value: 'Cadete', 
        label: 'Cadete', 
        // Estilos visuales (Tema Verde)
        colorTheme: {
            border: 'border-emerald-200',
            borderFocus: 'focus-within:border-emerald-500',
            bgBadge: 'bg-emerald-100',
            textMain: 'text-emerald-700',
            textLabel: 'text-emerald-900',
            button: 'bg-emerald-600 hover:bg-emerald-700 shadow-emerald-200'
        }
    }
];

export const Login: React.FC = () => {
    const navigate = useNavigate();
    const { login } = useAuth();
    
    // Estados
    const [formData, setFormData] = useState({ usuario: '', password: '' });
    const [selectedRole, setSelectedRole] = useState<typeof ROLES_CONFIG[0] | null>(null);
    const [error, setError] = useState<string>('');
    const [isLoading, setIsLoading] = useState(false);

    // Manejar selección de rol (Paso 1 -> Paso 2)
    const handleRoleSelect = (role: typeof ROLES_CONFIG[0]) => {
        setError('');
        setFormData({ usuario: '', password: '' }); 
        setSelectedRole(role);
    };

    // Volver atrás
    const handleBack = () => {
        setSelectedRole(null);
        setError('');
    };

    // Lógica de Envío
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
            await login(formData.usuario, formData.password);
            
            localStorage.setItem('farmacia_role', selectedRole.value);

            switch (selectedRole.value) {
                case 'Encargado':
                    navigate('/dashboard/admin');
                    break;
                case 'Operario':
                    navigate('/dashboard/operario');
                    break;
                case 'Cadete':
                    navigate('/dashboard/cadete');
                    break;
                default:
                    navigate('/'); 
                    break;
            }
        } catch (err: any) {
            const msg = err.response?.data?.message || 'Usuario o contraseña incorrectos';
            setError(msg);
        } finally {
            setIsLoading(false);
        }
    };

    const handleChange = (e: React.ChangeEvent<HTMLInputElement>) => {
        setFormData({ ...formData, [e.target.name]: e.target.value });
    };

    return (
        <div className="min-h-screen flex items-center justify-center bg-gray-50 p-4 font-sans">
            <div className="max-w-md w-full bg-white rounded-3xl shadow-xl overflow-hidden transition-all duration-500">
                
                {/* --- HEADER --- */}
                <div className="pt-10 pb-6 text-center px-8">
                    <div className={`mx-auto w-32 h-32 bg-white rounded-full flex items-center justify-center border-4 shadow-sm mb-4 relative z-10 overflow-hidden transition-colors duration-300 ${selectedRole ? selectedRole.colorTheme.border : 'border-gray-200'}`}>
                        {/* Fondo del logo sutilmente coloreado según selección */}
                        <div className={`absolute inset-0 rounded-full scale-90 -z-10 transition-colors duration-300 ${selectedRole ? selectedRole.colorTheme.bgBadge : 'bg-gray-50'}`}></div>
                        <img src="/Logofarmacia.png" alt="Logo" className="w-full h-full object-contain p-2"/>
                    </div>
                    
                    <h1 className="text-3xl font-bold text-gray-900 tracking-tight">Farmacia General Paz</h1>
                    
                    {!selectedRole ? (
                        <p className="text-gray-500 mt-2 font-medium">Sistema de Gestión de Pedidos</p>
                    ) : (
                        <div className="flex items-center justify-center gap-2 mt-2">
                             <span className={`px-4 py-1 rounded-full text-sm font-bold uppercase tracking-wider ${selectedRole.colorTheme.bgBadge} ${selectedRole.colorTheme.textMain}`}>
                                {selectedRole.label}
                            </span>
                        </div>
                    )}
                </div>

                <div className="px-8 pb-10">
                    
                    {/* --- PASO 1: SELECCIÓN DE ROL (Sin Iconos) --- */}
                    {!selectedRole ? (
                        <div className="space-y-4 animate-in fade-in slide-in-from-bottom-4 duration-500">
                            <p className="text-center text-sm text-gray-400 mb-4">Seleccione su perfil para ingresar</p>
                            
                            {ROLES_CONFIG.map((role) => (
                                <button
                                    key={role.value}
                                    onClick={() => handleRoleSelect(role)}
                                    className={`w-full group relative flex items-center justify-between p-5 border-2 rounded-xl bg-white transition-all duration-300 cursor-pointer ${role.colorTheme.border} hover:shadow-md hover:scale-[1.02]`}
                                >
                                    <div className="flex flex-col text-left">
                                        <span className={`block text-lg font-bold ${role.colorTheme.textLabel}`}>
                                            {role.label}
                                        </span>
                                        <span className="text-xs text-gray-400 font-medium">Haga clic para ingresar</span>
                                    </div>
                                    <ChevronRight className={`w-6 h-6 opacity-40 group-hover:opacity-100 transition-opacity transform group-hover:translate-x-1 ${role.colorTheme.textMain}`} />
                                </button>
                            ))}
                        </div>
                    ) : (
                        
                    /* --- PASO 2: FORMULARIO --- */
                        <form onSubmit={handleSubmit} className="space-y-5 animate-in fade-in slide-in-from-right-8 duration-500">
                            
                            <button 
                                type="button" 
                                onClick={handleBack}
                                className="flex items-center gap-1 text-sm text-gray-400 hover:text-gray-600 mb-2 transition-colors"
                            >
                                <ArrowLeft className="w-4 h-4" /> Cambiar de perfil
                            </button>

                            {error && (
                                <div className="p-3 bg-red-50 text-red-600 text-sm font-medium rounded-lg border border-red-200 text-center animate-pulse">
                                    {error}
                                </div>
                            )}

                            {/* INPUT USUARIO */}
                            <label className={`group relative flex items-center gap-4 p-4 border-2 rounded-xl bg-white focus-within:shadow-lg transition-all duration-300 cursor-text ${selectedRole.colorTheme.border} ${selectedRole.colorTheme.borderFocus}`}>
                                <div className={`w-10 h-10 rounded-lg flex items-center justify-center ${selectedRole.colorTheme.bgBadge} ${selectedRole.colorTheme.textMain}`}>
                                    <User className="w-5 h-5" />
                                </div>
                                <div className="flex-1">
                                    <span className={`block text-xs font-bold uppercase tracking-wider mb-0.5 ${selectedRole.colorTheme.textLabel}`}>
                                        Usuario
                                    </span>
                                    <input
                                        name="usuario" 
                                        type="text"
                                        autoComplete="username"
                                        required
                                        autoFocus
                                        disabled={isLoading}
                                        className={`w-full bg-transparent outline-none font-semibold text-lg placeholder-gray-300 ${selectedRole.colorTheme.textLabel}`}
                                        placeholder={`Usuario ${selectedRole.label}`}
                                        value={formData.usuario}
                                        onChange={handleChange}
                                    />
                                </div>
                            </label>

                            {/* INPUT CONTRASEÑA */}
                            <label className={`group relative flex items-center gap-4 p-4 border-2 rounded-xl bg-white focus-within:shadow-lg transition-all duration-300 cursor-text ${selectedRole.colorTheme.border} ${selectedRole.colorTheme.borderFocus}`}>
                                <div className={`w-10 h-10 rounded-lg flex items-center justify-center ${selectedRole.colorTheme.bgBadge} ${selectedRole.colorTheme.textMain}`}>
                                    <Lock className="w-5 h-5" />
                                </div>
                                <div className="flex-1">
                                    <span className={`block text-xs font-bold uppercase tracking-wider mb-0.5 ${selectedRole.colorTheme.textLabel}`}>
                                        Contraseña
                                    </span>
                                    <input
                                        name="password"
                                        type="password"
                                        autoComplete="current-password"
                                        required
                                        disabled={isLoading}
                                        className={`w-full bg-transparent outline-none font-semibold text-lg placeholder-gray-300 ${selectedRole.colorTheme.textLabel}`}
                                        placeholder="••••••••"
                                        value={formData.password}
                                        onChange={handleChange}
                                    />
                                </div>
                            </label>

                            <button
                                type="submit"
                                disabled={isLoading}
                                className={`w-full text-white text-lg font-bold py-4 rounded-xl transition-all transform hover:scale-[1.02] active:scale-[0.98] shadow-lg flex justify-center items-center gap-2 mt-6 ${selectedRole.colorTheme.button}`}
                            >
                                {isLoading ? (
                                    <Loader2 className="animate-spin w-6 h-6" />
                                ) : (
                                    <>
                                        <span>INGRESAR AL SISTEMA</span>
                                        <ChevronRight className="w-5 h-5 opacity-80" />
                                    </>
                                )}
                            </button>
                        </form>
                    )}
                </div>
                
                <div className="bg-gray-50 py-4 text-center border-t border-gray-100">
                    <p className="text-xs text-gray-400">© 2026 Farmacia General Paz - Córdoba</p>
                </div>
            </div>
        </div>
    );
};