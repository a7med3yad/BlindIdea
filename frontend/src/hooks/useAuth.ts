import { useMutation } from '@tanstack/react-query';
import { useNavigate } from 'react-router-dom';
import toast from 'react-hot-toast';
import { authApi } from '../api/auth.api';
import { useAuthStore } from '../store/auth.store';
import type {
  RegisterRequest,
  LoginRequest,
  VerifyEmailRequest,
  VerifyResetRequest,
  ChangePasswordRequest,
} from '../types/auth.types';

export const useRegister = () => {
  const navigate = useNavigate();

  return useMutation({
    mutationFn: (data: RegisterRequest) => authApi.register(data),
    onSuccess: (_, variables) => {
      toast.success('Account created! Check your email for verification code.');
      navigate('/verify-email', { state: { email: variables.email } });
    },
  });
};

export const useVerifyEmail = () => {
  const navigate = useNavigate();
  const login = useAuthStore((s) => s.login);

  return useMutation({
    mutationFn: (data: VerifyEmailRequest) => authApi.verifyEmail(data),
    onSuccess: (response) => {
      const { accessToken, refreshToken } = response.data;
      login(accessToken, refreshToken);
      toast.success('Email verified successfully!');
      navigate('/dashboard');
    },
  });
};

export const useLogin = () => {
  const navigate = useNavigate();
  const login = useAuthStore((s) => s.login);

  return useMutation({
    mutationFn: (data: LoginRequest) => authApi.login(data),
    onSuccess: (response) => {
      const { accessToken, refreshToken } = response.data;
      login(accessToken, refreshToken);
      toast.success('Welcome back!');
      navigate('/dashboard');
    },
  });
};

export const useForgotPassword = () => {
  return useMutation({
    mutationFn: (email: string) => authApi.forgotPassword(email),
    onSuccess: () => {
      toast.success('OTP sent to your email!');
    },
  });
};

export const useVerifyReset = () => {
  return useMutation({
    mutationFn: (data: VerifyResetRequest) => authApi.verifyReset(data),
    onSuccess: () => {
      toast.success('Password reset successful!');
    },
  });
};

export const useChangePassword = () => {
  return useMutation({
    mutationFn: (data: ChangePasswordRequest) => authApi.changePassword(data),
    onSuccess: () => {
      toast.success('Password changed successfully!');
    },
  });
};

export const useLogout = () => {
  const navigate = useNavigate();
  const authStore = useAuthStore();

  return useMutation({
    mutationFn: async () => {
      const refreshToken = authStore.refreshToken;
      if (refreshToken) {
        await authApi.logout(refreshToken);
      }
    },
    onSuccess: () => {
      authStore.logout();
      toast.success('Logged out successfully');
      navigate('/login');
    },
    onError: () => {
      authStore.logout();
      navigate('/login');
    },
  });
};
