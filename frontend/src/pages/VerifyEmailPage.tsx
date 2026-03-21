import { useLocation, Navigate } from 'react-router-dom';
import { motion } from 'framer-motion';
import { Mail } from 'lucide-react';
import Logo from '../components/ui/Logo';
import OtpForm from '../components/features/auth/OtpForm';
import { useVerifyEmail } from '../hooks/useAuth';
import { authApi } from '../api/auth.api';
import toast from 'react-hot-toast';

export default function VerifyEmailPage() {
  const location = useLocation();
  const email = location.state?.email as string | undefined;
  const { mutate, isPending } = useVerifyEmail();

  if (!email) {
    return <Navigate to="/register" replace />;
  }

  const handleSubmit = (otp: string) => {
    mutate({ email, otp });
  };

  const handleResend = async () => {
    try {
      await authApi.register({ email, password: '' });
    } catch {
      toast.error('Could not resend OTP. Please try registering again.');
    }
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
        </div>

        <div className="bg-[#0D0D0D] border border-[#2A2A2A] rounded-2xl p-8 text-center">
          <div className="w-16 h-16 rounded-2xl bg-[#E8003D]/10 flex items-center justify-center mx-auto mb-5">
            <Mail className="w-7 h-7 text-[#E8003D]" />
          </div>
          <h2 className="text-xl font-bold text-white mb-2">
            Verify your email
          </h2>
          <p className="text-sm text-[#555555] mb-6">
            We sent a 6-digit code to{' '}
            <span className="text-white font-medium">{email}</span>
          </p>

          <OtpForm
            onSubmit={handleSubmit}
            isLoading={isPending}
            onResend={handleResend}
            countdownSeconds={300}
          />
        </div>
      </motion.div>
    </div>
  );
}
