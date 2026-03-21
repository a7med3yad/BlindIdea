import api from './axios';
import type {
  RegisterRequest,
  LoginRequest,
  VerifyEmailRequest,
  VerifyResetRequest,
  RefreshTokenRequest,
  ChangePasswordRequest,
  AuthResponse,
  UserProfile,
} from '../types/auth.types';

export const authApi = {
  register: (data: RegisterRequest) =>
    api.post('/Auth/register', data),

  verifyEmail: (data: VerifyEmailRequest) =>
    api.post<AuthResponse>('/Auth/Verify-email', data),

  login: (data: LoginRequest) =>
    api.post<AuthResponse>('/Auth/login', data),

  forgotPassword: (email: string) =>
    api.post(`/Auth/forgot-password?email=${encodeURIComponent(email)}`),

  verifyReset: (data: VerifyResetRequest) =>
    api.post('/Auth/verify-reset', data),

  refreshToken: (data: RefreshTokenRequest) =>
    api.post<AuthResponse>('/Auth/refresh-token', data),

  logout: (refreshToken: string) =>
    api.post('/Auth/logout', { refreshToken }),

  changePassword: (data: ChangePasswordRequest) =>
    api.post('/Auth/change-password', data),

  getProfile: () =>
    api.get<UserProfile>('/Auth/profile'),

  loginWithGoogle: () => {
    const apiBase = import.meta.env.VITE_API_URL || 'https://localhost:7286/api';
    const returnUrl = `${window.location.origin}/external-callback`;
    window.location.href = `${apiBase}/Auth/login/google?returnUrl=${encodeURIComponent(returnUrl)}`;
  },

  loginWithGithub: () => {
    const apiBase = import.meta.env.VITE_API_URL || 'https://localhost:7286/api';
    const returnUrl = `${window.location.origin}/external-callback`;
    window.location.href = `${apiBase}/Auth/login/github?returnUrl=${encodeURIComponent(returnUrl)}`;
  },
};
