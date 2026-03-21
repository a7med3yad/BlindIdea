import { Lightbulb } from 'lucide-react';
import { Link } from 'react-router-dom';

interface LogoProps {
  size?: 'sm' | 'md' | 'lg' | 'xl';
  linkTo?: string;
}

const sizes = {
  sm: { text: 'text-xl', icon: 'w-5 h-5' },
  md: { text: 'text-2xl', icon: 'w-6 h-6' },
  lg: { text: 'text-5xl', icon: 'w-10 h-10' },
  xl: { text: 'text-6xl', icon: 'w-14 h-14' },
};

export default function Logo({ size = 'md', linkTo }: LogoProps) {
  const s = sizes[size];

  const content = (
    <div className="flex items-center gap-0 select-none">
      <span
        className={`font-black ${s.text}`}
        style={{ color: '#E8003D' }}
      >
        Bl
      </span>
      <Lightbulb
        className={`${s.icon} mx-0.5`}
        style={{ color: '#E8003D' }}
        strokeWidth={2.5}
      />
      <span
        className={`font-black ${s.text}`}
        style={{ color: '#E8003D' }}
      >
        nd
      </span>
      <span
        className={`font-black ${s.text}`}
        style={{ color: '#FFFFFF' }}
      >
        Idea
      </span>
    </div>
  );

  if (linkTo) {
    return (
      <Link to={linkTo} className="no-underline">
        {content}
      </Link>
    );
  }

  return content;
}
