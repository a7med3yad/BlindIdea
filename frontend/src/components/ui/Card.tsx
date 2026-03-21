import type { ReactNode } from 'react';

interface CardProps {
  children: ReactNode;
  className?: string;
  hover?: boolean;
  onClick?: () => void;
}

export default function Card({
  children,
  className = '',
  hover = false,
  onClick,
}: CardProps) {
  return (
    <div
      onClick={onClick}
      className={`
        bg-[#0D0D0D] border border-[#2A2A2A] rounded-xl p-6
        transition-all duration-200
        ${hover ? 'hover:border-[#3A3A3A] hover:bg-[#111111] cursor-pointer' : ''}
        ${className}
      `}
    >
      {children}
    </div>
  );
}
