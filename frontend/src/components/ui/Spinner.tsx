import { Loader2 } from 'lucide-react';

interface SpinnerProps {
  size?: number;
  className?: string;
}

export default function Spinner({ size = 24, className = '' }: SpinnerProps) {
  return (
    <div className={`flex items-center justify-center ${className}`}>
      <Loader2 size={size} className="animate-spin text-[#E8003D]" />
    </div>
  );
}

export function PageSpinner() {
  return (
    <div className="flex flex-col items-center justify-center min-h-[400px]">
      <Loader2 className="w-8 h-8 animate-spin text-[#E8003D]" />
      <p className="text-sm text-[#555555] mt-3">Loading...</p>
    </div>
  );
}
