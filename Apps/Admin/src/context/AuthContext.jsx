import { createContext, useContext, useState, useEffect } from 'react';
import { authApi } from '@/lib/api';

const AuthContext = createContext(null);

export function AuthProvider({ children }) {
  const [usuario, setUsuario] = useState(null);
  const [loading, setLoading] = useState(true);

  useEffect(() => {
    // Verificar se há token salvo
    const token = localStorage.getItem('token');
    const usuarioSalvo = localStorage.getItem('usuario');

    if (token && usuarioSalvo) {
      try {
        setUsuario(JSON.parse(usuarioSalvo));
        // Verificar se o token ainda é válido
        authApi.me()
          .then((res) => {
            setUsuario(res.data);
            localStorage.setItem('usuario', JSON.stringify(res.data));
          })
          .catch(() => {
            // Token inválido, limpar
            logout();
          })
          .finally(() => setLoading(false));
      } catch (error) {
        logout();
        setLoading(false);
      }
    } else {
      setLoading(false);
    }
  }, []);

  const login = async (email, senha) => {
    try {
      const response = await authApi.login({ email, senha });
      const { token, refreshToken, usuario: usuarioData } = response.data;

      localStorage.setItem('token', token);
      localStorage.setItem('refreshToken', refreshToken);
      localStorage.setItem('usuario', JSON.stringify(usuarioData));

      setUsuario(usuarioData);
      return { success: true };
    } catch (error) {
      console.error('Erro no login:', error);
      const errorMessage = error.response?.data?.message || 
                          error.response?.data?.error ||
                          error.message ||
                          'Email ou senha inválidos';
      return {
        success: false,
        message: errorMessage,
      };
    }
  };

  const logout = () => {
    localStorage.removeItem('token');
    localStorage.removeItem('refreshToken');
    localStorage.removeItem('usuario');
    setUsuario(null);
  };

  const atualizarUsuario = (usuarioData) => {
    setUsuario(usuarioData);
    localStorage.setItem('usuario', JSON.stringify(usuarioData));
  };

  const isAuthenticated = !!usuario;

  return (
    <AuthContext.Provider
      value={{
        usuario,
        loading,
        login,
        logout,
        atualizarUsuario,
        isAuthenticated,
      }}
    >
      {children}
    </AuthContext.Provider>
  );
}

export function useAuth() {
  const context = useContext(AuthContext);
  if (!context) {
    throw new Error('useAuth deve ser usado dentro de AuthProvider');
  }
  return context;
}

