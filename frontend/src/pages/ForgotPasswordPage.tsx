import { useState } from 'react';
import { useNavigate } from 'react-router-dom';
import { motion } from 'framer-motion';
import { useForm } from 'react-hook-form';
import { zodResolver } from '@hookform/resolvers/zod';
import { KeyRound } from 'lucide-react';
import Logo from '../components/ui/Logo';
import ForgotPasswordForm from '../components/features/auth/ForgotPasswordForm';
import OtpForm from '../components/features/auth/OtpForm';
import Input from '../components/ui/Input';
import Button from '../components/ui/Button';
import { useForgotPassword, useVerifyReset } from '../hooks/useAuth';
import {
  resetPasswordSchema,
  type ResetPasswordFormData,
} from '../schemas/auth.schema';
import { authApi } from '../api/auth.api';

export default function ForgotPasswordPage() {
  const [step, setStep] = useState<1 | 2 | 3>(1);
  const [email, setEmail] = useState('');
  const [otp, setOtp] = useState('');
  const navigate = useNavigate();

  const { mutate: forgotPassword, isPending: isSending } = useForgotPassword();
  const { mutate: verifyReset, isPending: isVerifying } = useVerifyReset();

  const {
    register,
    handleSubmit,
    formState: { errors, isValid },
  } = useForm<ResetPasswordFormData>({
    resolver: zodResolver(resetPasswordSchema),
    mode: 'onChange',
  });

  const handleEmailSubmit = (emailValue: string) => {
    setEmail(emailValue);
    forgotPassword(emailValue, {
      onSuccess: () => setStep(2),
    });
  };

  const handleOtpSubmit = (otpValue: string) => {
    setOtp(otpValue);
    setStep(3);
  };

  const handleResetPassword = (_data: ResetPasswordFormData) => {
    verifyReset(
      { email, otp },
      {
        onSuccess: () => {
          navigate('/login');
        },
      }
    );
  };

  const handleResendOtp = () => {
    authApi.forgotPassword(email);
  };

  return (
    <div className="min-h-screen bg-[#000000] flex items-center justify-center p-6">
      <motion.div
        initial={{ opacity: 0, y: 20 }}
        animate={{ opacity: 1, y: 0 }}
        transition={{ duration: 0.5 }}
        className="w-full max-w-md"
      >
        <div className="text-center mb-8">
          <div className="flex justify-center mb-4">
            <Logo size="lg" linkTo="/" />
          </div>
          <p className="text-sm text-[#555555]">Reset your password</p>
        </div>

        <div className="bg-[#0D0D0D] border border-[#2A2A2A] rounded-2xl p-8">
          {/* Step indicator */}
          <div className="flex items-center justify-center gap-2 mb-8">
            {[1, 2, 3].map((s) => (
              <div
                key={s}
                className={`h-1.5 rounded-full transition-all duration-300 ${
                  s === step
                    ? 'w-8 bg-[#E8003D]'
                    : s < step
                    ? 'w-6 bg-[#E8003D]/50'
                    : 'w-6 bg-[#2A2A2A]'
                }`}
              />
            ))}
          </div>

          {step === 1 && (
            <motion.div
              key="step1"
              initial={{ opacity: 0, x: -20 }}
              animate={{ opacity: 1, x: 0 }}
            >
              <div className="text-center mb-6">
                <KeyRound className="w-7 h-7 text-[#E8003D] mx-auto mb-3" />
                <h2 className="text-lg font-bold text-white">Enter your email</h2>
                <p className="text-xs text-[#555555] mt-1">
                  We'll send a verification code
                </p>
              </div>
              <ForgotPasswordForm
                onSubmit={handleEmailSubmit}
                isLoading={isSending}
              />
            </motion.div>
          )}

          {step === 2 && (
            <motion.div
              key="step2"
              initial={{ opacity: 0, x: -20 }}
              animate={{ opacity: 1, x: 0 }}
            >
              <div className="text-center mb-6">
                <h2 className="text-lg font-bold text-white">
                  Enter verification code
                </h2>
                <p className="text-xs text-[#555555] mt-1">
                  Sent to <span className="text-white">{email}</span>
                </p>
              </div>
              <OtpForm
                onSubmit={handleOtpSubmit}
                isLoading={isVerifying}
                onResend={handleResendOtp}
                countdownSeconds={300}
              />
            </motion.div>
          )}

          {step === 3 && (
            <motion.div
              key="step3"
              initial={{ opacity: 0, x: -20 }}
              animate={{ opacity: 1, x: 0 }}
            >
              <div className="text-center mb-6">
                <h2 className="text-lg font-bold text-white">
                  Set new password
                </h2>
              </div>
              <form
                onSubmit={handleSubmit(handleResetPassword)}
                className="space-y-4"
              >
                <Input
                  label="New Password"
                  type="password"
                  placeholder="Min. 8 characters"

                  error={errors.newPassword?.message}
                  disabled={isVerifying}
                  {...register('newPassword')}
                />
                <Input
                  label="Confirm Password"
                  type="password"
                  placeholder="Re-enter password"

                  error={errors.confirmPassword?.message}
                  disabled={isVerifying}
                  {...register('confirmPassword')}
                />
                <Button
                  type="submit"
                  fullWidth
                  size="lg"
                  isLoading={isVerifying}
                  disabled={!isValid || isVerifying}
                >
                  Reset Password
                </Button>
              </form>
            </motion.div>
          )}
        </div>
      </motion.div>
    </div>
  );
}
