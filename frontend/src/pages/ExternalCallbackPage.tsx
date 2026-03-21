import { useEffect, useState } from 'react';
import { useNavigate } from 'react-router-dom';
import { useAuthStore } from '../store/auth.store';
import Spinner from '../components/ui/Spinner';

export default function ExternalCallbackPage() {
  const navigate = useNavigate();
  const login = useAuthStore((s) => s.login);
  const [error, setError] = useState<string | null>(null);

  useEffect(() => {
    const handleCallback = async () => {
      try {
        // The backend redirects to /external-callback with tokens as query params
        const params = new URLSearchParams(window.location.search);
        let accessToken = params.get('accessToken');
        let refreshToken = params.get('refreshToken');

        // If tokens are not in query params, try to fetch from the current URL
        // (backend might return JSON at the callback URL)
        if (!accessToken || !refreshToken) {
          const apiBase = import.meta.env.VITE_API_URL || 'https://localhost:7286/api';
          const response = await fetch(`${apiBase}/Auth/external-callback${window.location.search}`, {
            credentials: 'include',
          });
          if (response.ok) {
            const data = await response.json();
            accessToken = data.accessToken;
            refreshToken = data.refreshToken;
          }
        }

        if (accessToken && refreshToken) {
          login(accessToken, refreshToken);
          navigate('/dashboard', { replace: true });
        } else {
          setError('Authentication failed. No tokens received.');
          setTimeout(() => navigate('/login', { replace: true }), 3000);
        }
      } catch {
        setError('Authentication failed. Please try again.');
        setTimeout(() => navigate('/login', { replace: true }), 3000);
      }
    };

    handleCallback();
  }, [login, navigate]);

  return (
    <div className="min-h-screen bg-[#000000] flex flex-col items-center justify-center">
      {error ? (
        <div className="text-center">
          <p className="text-[#EF4444] text-lg mb-2">{error}</p>
          <p className="text-[#555555] text-sm">Redirecting to login...</p>
        </div>
      ) : (
        <div className="flex flex-col items-center gap-4">
          <Spinner size={32} />
          <p className="text-[#AAAAAA] text-base">Completing sign in...</p>
        </div>
      )}
    </div>
  );
}
