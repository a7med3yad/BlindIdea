import { useState } from 'react';
import { Link, useLocation } from 'react-router-dom';
import { motion, AnimatePresence } from 'framer-motion';
import {
  LayoutDashboard,
  Lightbulb,
  Users,
  User,
  LogOut,
  Menu,
  X,
} from 'lucide-react';
import Logo from '../ui/Logo';
import { useLogout } from '../../hooks/useAuth';
import { useAuthStore } from '../../store/auth.store';

const navLinks = [
  { to: '/dashboard', label: 'Dashboard', icon: LayoutDashboard },
  { to: '/ideas', label: 'Ideas', icon: Lightbulb },
  { to: '/team', label: 'Team', icon: Users },
  { to: '/profile', label: 'Profile', icon: User },
];

export default function Navbar() {
  const [mobileOpen, setMobileOpen] = useState(false);
  const location = useLocation();
  const { mutate: logout, isPending } = useLogout();
  const email = useAuthStore((s) => s.email);

  return (
    <nav className="sticky top-0 z-40 bg-[#0D0D0D]/90 backdrop-blur-xl border-b border-[#1A1A1A]">
      <div className="relative px-8">
        <div className="flex items-center justify-between h-16">
          {/* Logo — slightly right */}
          <div className="pl-8 shrink-0">
            <Logo size="md" linkTo="/dashboard" />
          </div>

          {/* Desktop nav — centered */}
          <div className="hidden md:flex items-center gap-8 absolute left-1/2 -translate-x-1/2">
            {navLinks.map((link) => {
              const isActive = location.pathname === link.to;
              return (
                <Link
                  key={link.to}
                  to={link.to}
                  className={`
                    flex items-center gap-1.5 text-sm font-medium
                    transition-colors duration-200 no-underline
                    ${
                      isActive
                        ? 'text-[#E8003D]'
                        : 'text-[#AAAAAA] hover:text-white'
                    }
                  `}
                >
                  <link.icon className="w-4 h-4" />
                  <span>{link.label}</span>
                </Link>
              );
            })}
          </div>

          {/* Right side — far right */}
          <div className="hidden md:flex items-center gap-4 pr-6">
            <span className="text-xs text-[#555555] truncate max-w-[160px]">
              {email}
            </span>
            <button
              onClick={() => logout()}
              disabled={isPending}
              className="flex items-center gap-1.5 h-9 px-4 rounded-lg text-sm font-medium text-[#AAAAAA] hover:text-white hover:bg-white/5 transition-all cursor-pointer"
            >
              <LogOut className="w-4 h-4" />
              Logout
            </button>
          </div>

          {/* Mobile menu button */}
          <button
            onClick={() => setMobileOpen(!mobileOpen)}
            className="md:hidden p-2 text-[#AAAAAA] hover:text-white cursor-pointer"
          >
            {mobileOpen ? <X className="w-5 h-5" /> : <Menu className="w-5 h-5" />}
          </button>
        </div>
      </div>

      {/* Mobile menu */}
      <AnimatePresence>
        {mobileOpen && (
          <motion.div
            initial={{ opacity: 0, height: 0 }}
            animate={{ opacity: 1, height: 'auto' }}
            exit={{ opacity: 0, height: 0 }}
            className="md:hidden border-t border-[#1A1A1A] bg-[#0D0D0D]"
          >
            <div className="px-4 py-3 space-y-1">
              {navLinks.map((link) => {
                const isActive = location.pathname === link.to;
                return (
                  <Link
                    key={link.to}
                    to={link.to}
                    onClick={() => setMobileOpen(false)}
                    className={`
                      flex items-center gap-2 px-4 py-3 rounded-lg text-sm font-medium
                      transition-colors duration-200 no-underline
                      ${
                        isActive
                          ? 'text-[#E8003D] bg-[#E8003D]/10'
                          : 'text-[#AAAAAA] hover:text-white hover:bg-white/5'
                      }
                    `}
                  >
                    <link.icon className="w-4 h-4" />
                    <span>{link.label}</span>
                  </Link>
                );
              })}
              <button
                onClick={() => {
                  logout();
                  setMobileOpen(false);
                }}
                disabled={isPending}
                className="flex items-center gap-2 w-full px-4 py-3 rounded-lg text-sm font-medium text-[#AAAAAA] hover:text-white hover:bg-white/5 transition-all cursor-pointer"
              >
                <LogOut className="w-4 h-4" />
                <span>Logout</span>
              </button>
            </div>
          </motion.div>
        )}
      </AnimatePresence>
    </nav>
  );
}
