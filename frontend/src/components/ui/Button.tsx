import { type ButtonHTMLAttributes, forwardRef } from 'react';
import { Loader2 } from 'lucide-react';

interface ButtonProps extends ButtonHTMLAttributes<HTMLButtonElement> {
  variant?: 'primary' | 'secondary' | 'danger' | 'ghost';
  size?: 'sm' | 'md' | 'lg';
  isLoading?: boolean;
  fullWidth?: boolean;
}

const variantClasses = {
  primary:
    'bg-[#E8003D] text-white hover:bg-[#CC0035] active:bg-[#B00030]',
  secondary:
    'bg-transparent text-white border border-[#2A2A2A] hover:border-[#E8003D]',
  danger:
    'bg-transparent text-[#EF4444] border border-[#EF4444]/40 hover:bg-[#EF4444]/10',
  ghost:
    'bg-transparent text-[#AAAAAA] hover:text-white hover:bg-white/5',
};

const sizeClasses = {
  sm: 'h-9 px-4 text-sm rounded-lg',
  md: 'h-12 px-6 text-base rounded-lg',
  lg: 'h-12 px-8 text-base rounded-lg',
};

const Button = forwardRef<HTMLButtonElement, ButtonProps>(
  (
    {
      variant = 'primary',
      size = 'md',
      isLoading = false,
      fullWidth = false,
      disabled,
      children,
      className = '',
      ...props
    },
    ref
  ) => {
    return (
      <button
        ref={ref}
        disabled={disabled || isLoading}
        className={`
          inline-flex items-center justify-center gap-2 font-semibold
          min-w-[120px] transition-all duration-200 cursor-pointer
          disabled:opacity-50 disabled:cursor-not-allowed
          ${variantClasses[variant]}
          ${sizeClasses[size]}
          ${fullWidth ? 'w-full' : ''}
          ${className}
        `}
        {...props}
      >
        {isLoading && <Loader2 className="w-4 h-4 animate-spin" />}
        {children}
      </button>
    );
  }
);

Button.displayName = 'Button';
export default Button;
