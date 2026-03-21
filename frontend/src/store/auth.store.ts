import { create } from 'zustand';
import api from '../api/axios';

interface AuthState {
  accessToken: string | null;
  refreshToken: string | null;
  email: string | null;
  role: string | null;
  isAuthenticated: boolean;

  login: (accessToken: string, refreshToken: string) => void;
  logout: () => void;
  initialize: () => void;
}

const decodeToken = (token: string) => {
  try {
    const payload = JSON.parse(atob(token.split('.')[1]));
    const email =
      payload['http://schemas.xmlsoap.org/ws/2005/05/identity/claims/emailaddress'] ||
      payload.email ||
      null;
    const role =
      payload['http://schemas.microsoft.com/ws/2008/06/identity/claims/role'] ||
      payload.role ||
      null;
    return { email, role };
  } catch {
    return { email: null, role: null };
  }
};

export const useAuthStore = create<AuthState>((set) => ({
  accessToken: null,
  refreshToken: null,
  email: null,
  role: null,
  isAuthenticated: false,

  login: (accessToken: string, refreshToken: string) => {
    localStorage.setItem('accessToken', accessToken);
    localStorage.setItem('refreshToken', refreshToken);
    const { email, role } = decodeToken(accessToken);
    api.defaults.headers.common['Authorization'] = `Bearer ${accessToken}`;
    set({ accessToken, refreshToken, email, role, isAuthenticated: true });
  },

  logout: () => {
    localStorage.removeItem('accessToken');
    localStorage.removeItem('refreshToken');
    delete api.defaults.headers.common['Authorization'];
    set({
      accessToken: null,
      refreshToken: null,
      email: null,
      role: null,
      isAuthenticated: false,
    });
  },

  initialize: () => {
    const accessToken = localStorage.getItem('accessToken');
    const refreshToken = localStorage.getItem('refreshToken');
    if (accessToken && refreshToken) {
      const { email, role } = decodeToken(accessToken);
      api.defaults.headers.common['Authorization'] = `Bearer ${accessToken}`;
      set({ accessToken, refreshToken, email, role, isAuthenticated: true });
    }
  },
}));
