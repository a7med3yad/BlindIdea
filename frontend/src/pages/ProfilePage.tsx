import { useForm } from 'react-hook-form';
import { zodResolver } from '@hookform/resolvers/zod';
import { User, Lock, Mail, Shield, AlertTriangle } from 'lucide-react';
import { useAuthStore } from '../store/auth.store';
import { useChangePassword, useLogout } from '../hooks/useAuth';
import {
  changePasswordSchema,
  type ChangePasswordFormData,
} from '../schemas/auth.schema';

export default function ProfilePage() {
  const email = useAuthStore((s) => s.email);
  const role = useAuthStore((s) => s.role);
  const { mutate: changePassword, isPending } = useChangePassword();
  const { mutate: logout, isPending: isLoggingOut } = useLogout();

  const {
    register,
    handleSubmit,
    reset,
    formState: { errors, isValid },
  } = useForm<ChangePasswordFormData>({
    resolver: zodResolver(changePasswordSchema),
    mode: 'onChange',
  });

  const onSubmit = (data: ChangePasswordFormData) => {
    changePassword(
      { currentPassword: data.currentPassword, newPassword: data.newPassword },
      { onSuccess: () => reset() }
    );
  };

  return (
    <div className="min-h-screen p-8">
      <div className="max-w-2xl mx-auto">
        {/* Page header */}
        <div className="mb-8">
          <h1 className="text-3xl font-bold text-white mb-2">Profile</h1>
          <p className="text-[#AAAAAA]">Manage your account settings.</p>
        </div>

        {/* Account Information Card */}
        <div className="bg-[#0D0D0D] border border-[#2A2A2A] rounded-2xl p-6 mb-6">
          <div className="flex items-center gap-3 mb-6">
            <div className="w-10 h-10 bg-[#1A1A1A] rounded-xl flex items-center justify-center border border-[#2A2A2A]">
              <User className="w-5 h-5 text-[#E8003D]" />
            </div>
            <h2 className="text-lg font-bold text-white">
              Account Information
            </h2>
          </div>

          <div className="space-y-4">
            <div>
              <label className="text-xs font-medium text-[#555555] uppercase tracking-wider mb-1 block">
                Email
              </label>
              <div className="flex items-center gap-3 bg-[#1A1A1A] border border-[#2A2A2A] rounded-lg px-4 h-12">
                <Mail className="w-4 h-4 text-[#555555]" />
                <span className="text-white text-sm">{email}</span>
              </div>
            </div>

            <div>
              <label className="text-xs font-medium text-[#555555] uppercase tracking-wider mb-1 block">
                Role
              </label>
              <div className="flex items-center gap-3 bg-[#1A1A1A] border border-[#2A2A2A] rounded-lg px-4 h-12">
                <Shield className="w-4 h-4 text-[#555555]" />
                <span
                  className={`text-sm font-medium px-2 py-0.5 rounded-md ${
                    role === 'Admin'
                      ? 'bg-[#E8003D]/20 text-[#E8003D]'
                      : 'bg-[#1A1A1A] text-[#AAAAAA] border border-[#2A2A2A]'
                  }`}
                >
                  {role || 'Member'}
                </span>
              </div>
            </div>
          </div>
        </div>

        {/* Change Password Card */}
        <div className="bg-[#0D0D0D] border border-[#2A2A2A] rounded-2xl p-6 mb-6">
          <div className="flex items-center gap-3 mb-6">
            <div className="w-10 h-10 bg-[#1A1A1A] rounded-xl flex items-center justify-center border border-[#2A2A2A]">
              <Lock className="w-5 h-5 text-[#E8003D]" />
            </div>
            <h2 className="text-lg font-bold text-white">Change Password</h2>
          </div>

          <form onSubmit={handleSubmit(onSubmit)} className="space-y-4">
            <div>
              <label className="text-sm font-medium text-[#AAAAAA] mb-2 block">
                Current Password
              </label>
              <input
                type="password"
                placeholder="Enter current password"
                disabled={isPending}
                {...register('currentPassword')}
                className="w-full h-12 bg-[#1A1A1A] border border-[#2A2A2A] rounded-lg px-4 text-white placeholder-[#555555] focus:border-[#E8003D] focus:outline-none"
              />
              {errors.currentPassword && (
                <p className="text-xs text-[#EF4444] mt-1">
                  {errors.currentPassword.message}
                </p>
              )}
            </div>

            <div>
              <label className="text-sm font-medium text-[#AAAAAA] mb-2 block">
                New Password
              </label>
              <input
                type="password"
                placeholder="Min. 8 characters"
                disabled={isPending}
                {...register('newPassword')}
                className="w-full h-12 bg-[#1A1A1A] border border-[#2A2A2A] rounded-lg px-4 text-white placeholder-[#555555] focus:border-[#E8003D] focus:outline-none"
              />
              {errors.newPassword && (
                <p className="text-xs text-[#EF4444] mt-1">
                  {errors.newPassword.message}
                </p>
              )}
            </div>

            <div>
              <label className="text-sm font-medium text-[#AAAAAA] mb-2 block">
                Confirm New Password
              </label>
              <input
                type="password"
                placeholder="Re-enter new password"
                disabled={isPending}
                {...register('confirmPassword')}
                className="w-full h-12 bg-[#1A1A1A] border border-[#2A2A2A] rounded-lg px-4 text-white placeholder-[#555555] focus:border-[#E8003D] focus:outline-none"
              />
              {errors.confirmPassword && (
                <p className="text-xs text-[#EF4444] mt-1">
                  {errors.confirmPassword.message}
                </p>
              )}
            </div>

            <button
              type="submit"
              disabled={!isValid || isPending}
              className="w-full h-12 bg-[#E8003D] hover:bg-[#CC0035] text-white rounded-lg font-semibold text-base transition-colors duration-200 mt-2 disabled:opacity-50 disabled:cursor-not-allowed"
            >
              {isPending ? 'Updating...' : 'Update Password'}
            </button>
          </form>
        </div>

        {/* Danger Zone Card */}
        <div className="bg-[#0D0D0D] border border-[#EF4444]/30 rounded-2xl p-6">
          <div className="flex items-center gap-3 mb-2">
            <AlertTriangle className="w-5 h-5 text-[#EF4444]" />
            <h2 className="text-lg font-bold text-[#EF4444]">Danger Zone</h2>
          </div>
          <p className="text-[#AAAAAA] text-sm mb-6">
            Logging out will end your current session.
          </p>

          <button
            onClick={() => logout()}
            disabled={isLoggingOut}
            className="w-full h-12 bg-transparent border border-[#EF4444] text-[#EF4444] hover:bg-[#EF4444] hover:text-white rounded-lg font-semibold text-base transition-colors duration-200 disabled:opacity-50 disabled:cursor-not-allowed"
          >
            {isLoggingOut ? 'Signing out...' : 'Sign Out'}
          </button>
        </div>
      </div>
    </div>
  );
}
