import { useState } from 'react';
import { useForm } from 'react-hook-form';
import { zodResolver } from '@hookform/resolvers/zod';
import { useNavigate } from 'react-router-dom';
import { AlertCircle, Github } from 'lucide-react';
import toast from 'react-hot-toast';

import Input from '../../ui/Input';
import Button from '../../ui/Button';
import { loginSchema, type LoginFormData } from '../../../schemas/auth.schema';
import { authApi } from '../../../api/auth.api';
import { useAuthStore } from '../../../store/auth.store';

export default function LoginForm() {
  const navigate = useNavigate();
  const login = useAuthStore((s) => s.login);
  const [isLoading, setIsLoading] = useState(false);
  const [oAuthOnlyError, setOAuthOnlyError] = useState(false);

  const {
    register,
    handleSubmit,
    formState: { errors, isValid },
  } = useForm<LoginFormData>({
    resolver: zodResolver(loginSchema),
    mode: 'onChange',
  });

  const onSubmit = async (data: LoginFormData) => {
    try {
      setIsLoading(true);
      setOAuthOnlyError(false);
      const response = await authApi.login(data);
      const { accessToken, refreshToken } = response.data;
      login(accessToken, refreshToken);
      toast.success('Welcome back!');
      navigate('/dashboard');
    } catch (error: any) {
      const message =
        error.response?.data?.message || error.response?.data || '';

      // Detect OAuth-only account (null password hash)
      if (
        String(message).toLowerCase().includes('password') ||
        String(message).toLowerCase().includes('invalid') ||
        error.response?.status === 401
      ) {
        setOAuthOnlyError(true);
      } else {
        toast.error(String(message) || 'Login failed');
      }
    } finally {
      setIsLoading(false);
    }
  };

  const handleInputChange = () => {
    if (oAuthOnlyError) setOAuthOnlyError(false);
  };

  return (
    <form onSubmit={handleSubmit(onSubmit)} className="space-y-5">
      {/* OAuth-only account warning */}
      {oAuthOnlyError && (
        <div className="bg-[#1A1A1A] border border-[#E8003D]/40 rounded-xl p-4 text-center">
          <div className="w-10 h-10 bg-[#E8003D]/10 rounded-full flex items-center justify-center mx-auto mb-3">
            <AlertCircle className="w-5 h-5 text-[#E8003D]" />
          </div>
          <p className="text-white font-semibold mb-1">
            This account uses Google or GitHub login
          </p>
          <p className="text-[#AAAAAA] text-sm mb-4">
            You signed up with a social account. Please login using the same
            method.
          </p>
          <div className="flex gap-3 justify-center">
            <button
              type="button"
              onClick={() => authApi.loginWithGoogle()}
              className="flex items-center gap-2 bg-white text-black h-10 px-4 rounded-lg text-sm font-semibold hover:bg-gray-100 transition-colors cursor-pointer"
            >
              <svg width="16" height="16" viewBox="0 0 24 24">
                <path
                  d="M22.56 12.25c0-.78-.07-1.53-.2-2.25H12v4.26h5.92a5.06 5.06 0 01-2.2 3.32v2.77h3.57c2.08-1.92 3.28-4.74 3.28-8.1z"
                  fill="#4285F4"
                />
                <path
                  d="M12 23c2.97 0 5.46-.98 7.28-2.66l-3.57-2.77c-.98.66-2.23 1.06-3.71 1.06-2.86 0-5.29-1.93-6.16-4.53H2.18v2.84C3.99 20.53 7.7 23 12 23z"
                  fill="#34A853"
                />
                <path
                  d="M5.84 14.09c-.22-.66-.35-1.36-.35-2.09s.13-1.43.35-2.09V7.07H2.18C1.43 8.55 1 10.22 1 12s.43 3.45 1.18 4.93l2.85-2.22.81-.62z"
                  fill="#FBBC05"
                />
                <path
                  d="M12 5.38c1.62 0 3.06.56 4.21 1.64l3.15-3.15C17.45 2.09 14.97 1 12 1 7.7 1 3.99 3.47 2.18 7.07l3.66 2.84c.87-2.6 3.3-4.53 6.16-4.53z"
                  fill="#EA4335"
                />
              </svg>
              Google
            </button>
            <button
              type="button"
              onClick={() => authApi.loginWithGithub()}
              className="flex items-center gap-2 bg-[#24292F] text-white h-10 px-4 rounded-lg text-sm font-semibold hover:bg-[#3d444d] transition-colors cursor-pointer"
            >
              <Github className="w-4 h-4" />
              GitHub
            </button>
          </div>
        </div>
      )}

      <Input
        label="Email"
        type="email"
        placeholder="you@example.com"
        error={errors.email?.message}
        disabled={isLoading}
        {...register('email', { onChange: handleInputChange })}
      />

      <Input
        label="Password"
        type="password"
        placeholder="Enter your password"
        error={errors.password?.message}
        disabled={isLoading}
        {...register('password', { onChange: handleInputChange })}
      />

      <Button
        type="submit"
        fullWidth
        size="lg"
        isLoading={isLoading}
        disabled={!isValid || isLoading}
      >
        Sign In
      </Button>
    </form>
  );
}
