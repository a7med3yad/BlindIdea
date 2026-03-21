import { useState, useRef, useEffect, type KeyboardEvent, type ClipboardEvent } from 'react';
import Button from '../../ui/Button';

interface OtpFormProps {
  onSubmit: (otp: string) => void;
  isLoading?: boolean;
  onResend?: () => void;
  countdownSeconds?: number;
}

export default function OtpForm({
  onSubmit,
  isLoading = false,
  onResend,
  countdownSeconds = 300,
}: OtpFormProps) {
  const [otp, setOtp] = useState<string[]>(Array(6).fill(''));
  const [countdown, setCountdown] = useState(countdownSeconds);
  const inputRefs = useRef<(HTMLInputElement | null)[]>([]);

  useEffect(() => {
    inputRefs.current[0]?.focus();
  }, []);

  useEffect(() => {
    if (countdown <= 0) return;
    const timer = setInterval(() => {
      setCountdown((prev) => prev - 1);
    }, 1000);
    return () => clearInterval(timer);
  }, [countdown]);

  const formatTime = (seconds: number) => {
    const m = Math.floor(seconds / 60);
    const s = seconds % 60;
    return `${m}:${s.toString().padStart(2, '0')}`;
  };

  const handleChange = (index: number, value: string) => {
    if (!/^\d*$/.test(value)) return;
    const newOtp = [...otp];
    newOtp[index] = value.slice(-1);
    setOtp(newOtp);

    if (value && index < 5) {
      inputRefs.current[index + 1]?.focus();
    }

    const fullOtp = newOtp.join('');
    if (fullOtp.length === 6) {
      onSubmit(fullOtp);
    }
  };

  const handleKeyDown = (index: number, e: KeyboardEvent<HTMLInputElement>) => {
    if (e.key === 'Backspace' && !otp[index] && index > 0) {
      inputRefs.current[index - 1]?.focus();
    }
  };

  const handlePaste = (e: ClipboardEvent<HTMLInputElement>) => {
    e.preventDefault();
    const pasted = e.clipboardData.getData('text').replace(/\D/g, '').slice(0, 6);
    if (pasted.length === 6) {
      const newOtp = pasted.split('');
      setOtp(newOtp);
      onSubmit(pasted);
    }
  };

  const handleResend = () => {
    setCountdown(countdownSeconds);
    setOtp(Array(6).fill(''));
    inputRefs.current[0]?.focus();
    onResend?.();
  };

  return (
    <div className="space-y-6">
      <div className="flex justify-center gap-3">
        {otp.map((digit, index) => (
          <input
            key={index}
            ref={(el) => { inputRefs.current[index] = el; }}
            type="text"
            inputMode="numeric"
            maxLength={1}
            value={digit}
            onChange={(e) => handleChange(index, e.target.value)}
            onKeyDown={(e) => handleKeyDown(index, e)}
            onPaste={index === 0 ? handlePaste : undefined}
            disabled={isLoading}
            className={`
              w-12 h-14 text-center text-xl font-bold rounded-xl
              bg-[#1A1A1A] border border-[#2A2A2A] text-white
              focus:border-[#E8003D] focus:ring-1 focus:ring-[#E8003D]/50
              focus:outline-none transition-all duration-200
              disabled:opacity-50
            `}
          />
        ))}
      </div>

      <div className="text-center">
        {countdown > 0 ? (
          <p className="text-sm text-[#555555]">
            Resend OTP in{' '}
            <span className="text-[#E8003D] font-semibold">
              {formatTime(countdown)}
            </span>
          </p>
        ) : (
          <Button variant="ghost" size="sm" onClick={handleResend}>
            Resend OTP
          </Button>
        )}
      </div>
    </div>
  );
}
