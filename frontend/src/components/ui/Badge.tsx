interface BadgeProps {
  children: React.ReactNode;
  variant?: 'default' | 'primary' | 'success' | 'warning' | 'error';
}

const variants = {
  default: 'bg-[#1A1A1A] text-[#AAAAAA] border-[#2A2A2A]',
  primary: 'bg-[#E8003D]/10 text-[#E8003D] border-[#E8003D]/30',
  success: 'bg-[#22C55E]/10 text-[#22C55E] border-[#22C55E]/30',
  warning: 'bg-[#F59E0B]/10 text-[#F59E0B] border-[#F59E0B]/30',
  error: 'bg-[#EF4444]/10 text-[#EF4444] border-[#EF4444]/30',
};

export default function Badge({ children, variant = 'default' }: BadgeProps) {
  return (
    <span
      className={`inline-flex items-center px-2.5 py-0.5 text-xs font-semibold border rounded-full ${variants[variant]}`}
    >
      {children}
    </span>
  );
}
