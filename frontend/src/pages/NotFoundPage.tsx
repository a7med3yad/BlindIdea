import { Link } from 'react-router-dom';
import { motion } from 'framer-motion';
import { Home } from 'lucide-react';
import Logo from '../components/ui/Logo';
import Button from '../components/ui/Button';

export default function NotFoundPage() {
  return (
    <div className="min-h-screen bg-[#000000] flex items-center justify-center p-6">
      <motion.div
        initial={{ opacity: 0, y: 20 }}
        animate={{ opacity: 1, y: 0 }}
        transition={{ duration: 0.5 }}
        className="text-center"
      >
        <div className="flex justify-center mb-6">
          <Logo size="lg" />
        </div>

        <div className="relative mb-8">
          <span className="text-[120px] sm:text-[180px] font-bold text-[#0D0D0D] leading-none select-none">
            404
          </span>
          <span className="absolute inset-0 flex items-center justify-center text-[120px] sm:text-[180px] font-bold text-[#E8003D]/10 leading-none select-none">
            404
          </span>
        </div>

        <h1 className="text-xl font-bold text-white mb-2">Page not found</h1>
        <p className="text-[#AAAAAA] text-base mb-8 max-w-sm mx-auto">
          The page you're looking for doesn't exist or has been moved.
        </p>

        <Link to="/">
          <Button size="lg">
            <Home className="w-4 h-4" />
            Go Home
          </Button>
        </Link>
      </motion.div>
    </div>
  );
}
