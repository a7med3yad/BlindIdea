import { useState, useEffect } from 'react';
import { BrowserRouter, Routes, Route } from 'react-router-dom';
import { QueryClient, QueryClientProvider } from '@tanstack/react-query';
import { Toaster } from 'react-hot-toast';
import { AnimatePresence } from 'framer-motion';

import { useAuthStore } from './store/auth.store';
import { teamApi } from './api/team.api';
import ProtectedRoute from './components/layout/ProtectedRoute';
import Sidebar from './components/layout/Sidebar';
import Spinner from './components/ui/Spinner';

import LandingPage from './pages/LandingPage';
import LoginPage from './pages/LoginPage';
import RegisterPage from './pages/RegisterPage';
import VerifyEmailPage from './pages/VerifyEmailPage';
import ForgotPasswordPage from './pages/ForgotPasswordPage';
import DashboardPage from './pages/DashboardPage';
import IdeasPage from './pages/IdeasPage';
import TeamPage from './pages/TeamPage';
import ProfilePage from './pages/ProfilePage';
import NotFoundPage from './pages/NotFoundPage';
import ExternalCallbackPage from './pages/ExternalCallbackPage';

const queryClient = new QueryClient({
  defaultOptions: {
    queries: {
      retry: 1,
      refetchOnWindowFocus: false,
      staleTime: 5 * 60_000, // 5 minutes — avoid redundant refetches
    },
  },
});

function FullPageSpinner() {
  return (
    <div className="min-h-screen bg-[#000000] flex flex-col items-center justify-center">
      <Spinner size={32} />
      <p className="text-sm text-[#555555] mt-3">Loading...</p>
    </div>
  );
}

function AppRoutes() {
  return (
    <AnimatePresence mode="wait">
      <Routes>
        {/* Public routes */}
        <Route path="/" element={<LandingPage />} />
        <Route path="/login" element={<LoginPage />} />
        <Route path="/register" element={<RegisterPage />} />
        <Route path="/verify-email" element={<VerifyEmailPage />} />
        <Route path="/forgot-password" element={<ForgotPasswordPage />} />
        <Route path="/external-callback" element={<ExternalCallbackPage />} />

        {/* Protected routes */}
        <Route element={<ProtectedRoute />}>
          <Route element={<Sidebar />}>
            <Route path="/dashboard" element={<DashboardPage />} />
            <Route path="/ideas" element={<IdeasPage />} />
            <Route path="/team" element={<TeamPage />} />
            <Route path="/profile" element={<ProfilePage />} />
          </Route>
        </Route>

        {/* 404 */}
        <Route path="*" element={<NotFoundPage />} />
      </Routes>
    </AnimatePresence>
  );
}

export default function App() {
  const [appReady, setAppReady] = useState(false);

  useEffect(() => {
    const initApp = async () => {
      // 1. Restore auth (tokens + email + role) from localStorage
      useAuthStore.getState().initialize();

      const isLoggedIn = useAuthStore.getState().isAuthenticated;

      if (isLoggedIn) {
        // 2. Fetch team ONCE on app startup — store in Zustand
        //    Never re-fetch on every page navigation
        try {
          const response = await teamApi.getMyTeam();
          useAuthStore.getState().setTeam(response.data.id);
        } catch {
          // Not in a team — that's fine
          useAuthStore.getState().setTeam(null);
        }
      }

      setAppReady(true);
    };

    initApp();
  }, []); // ✅ Empty deps — runs ONCE only

  if (!appReady) return <FullPageSpinner />;

  return (
    <QueryClientProvider client={queryClient}>
      <BrowserRouter>
        <AppRoutes />
        <Toaster
          position="top-center"
          toastOptions={{
            duration: 4000,
            style: {
              background: '#0D0D0D',
              color: '#FFFFFF',
              border: '1px solid #2A2A2A',
              borderRadius: '12px',
              fontSize: '14px',
            },
            success: {
              iconTheme: {
                primary: '#22C55E',
                secondary: '#FFFFFF',
              },
            },
            error: {
              iconTheme: {
                primary: '#EF4444',
                secondary: '#FFFFFF',
              },
            },
          }}
        />
      </BrowserRouter>
    </QueryClientProvider>
  );
}
