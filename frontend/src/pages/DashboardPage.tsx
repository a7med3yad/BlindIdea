import { useState, useEffect } from 'react';
import { Link } from 'react-router-dom';
import { motion } from 'framer-motion';
import {
  Lightbulb,
  Star,
  BarChart3,
  FileText,
  Users,
  ArrowRight,
  Trophy,
  Clock,
  Copy,
} from 'lucide-react';
import toast from 'react-hot-toast';
import { dashboardApi } from '../api/dashboard.api';
import { teamApi } from '../api/team.api';
import { useAuthStore } from '../store/auth.store';
import type { DashboardResponse } from '../types/dashboard.types';
import type { Team } from '../types/team.types';
import StatsCard from '../components/features/dashboard/StatsCard';
import Spinner from '../components/ui/Spinner';
import Button from '../components/ui/Button';

export default function DashboardPage() {
  const hasTeam = useAuthStore((s) => s.hasTeam);
  const [dashboard, setDashboard] = useState<DashboardResponse | null>(null);
  const [team, setTeam] = useState<Team | null>(null);
  const [isLoading, setIsLoading] = useState(true);

  useEffect(() => {
    if (!hasTeam) {
      setIsLoading(false);
      return;
    }

    const fetchData = async () => {
      try {
        setIsLoading(true);

        // Fetch dashboard and team sequentially to avoid connection pool issues
        const dashResponse = await dashboardApi.getDashboard();
        setDashboard(dashResponse.data);

        const teamResponse = await teamApi.getMyTeam();
        setTeam(teamResponse.data);
      } catch (error: any) {
        const msg =
          error.response?.data?.message || error.response?.data || '';
        if (!String(msg).toLowerCase().includes('team')) {
          toast.error('Failed to load dashboard');
        }
      } finally {
        setIsLoading(false);
      }
    };

    fetchData();
  }, [hasTeam]);

  const formatDate = (dateString: string) => {
    if (!dateString) return 'Unknown';
    try {
      return new Date(dateString).toLocaleDateString('en-US', {
        year: 'numeric',
        month: 'long',
        day: 'numeric',
      });
    } catch {
      return 'Unknown';
    }
  };

  if (isLoading && hasTeam) {
    return (
      <div className="min-h-screen p-8">
        <div className="max-w-5xl mx-auto">
          <div className="flex flex-col items-center justify-center min-h-[60vh]">
            <Spinner size={32} />
            <p className="text-sm text-[#555555] mt-3">Loading dashboard...</p>
          </div>
        </div>
      </div>
    );
  }

  if (!hasTeam || !dashboard) {
    return (
      <div className="min-h-screen p-8">
        <div className="max-w-5xl mx-auto">
          <motion.div
            initial={{ opacity: 0 }}
            animate={{ opacity: 1 }}
            className="flex flex-col items-center justify-center min-h-[60vh] text-center"
          >
            <div className="w-20 h-20 bg-[#1A1A1A] rounded-2xl flex items-center justify-center mb-6">
              <Users className="w-9 h-9 text-[#555555]" />
            </div>
            <h2 className="text-2xl font-bold text-white mb-3">
              Join a team to get started
            </h2>
            <p className="text-[#AAAAAA] text-base mb-8 max-w-md">
              You need to be part of a team to see your dashboard. Create or
              join a team to start sharing ideas.
            </p>
            <Link to="/team">
              <Button size="lg">
                Go to Team
                <ArrowRight className="w-4 h-4" />
              </Button>
            </Link>
          </motion.div>
        </div>
      </div>
    );
  }

  // Map EXACT backend field names
  const stats = [
    {
      label: 'Total Ideas',
      value: dashboard.ideas?.totalIdeas ?? 0,
      icon: Lightbulb,
    },
    {
      label: 'Total Ratings',
      value: dashboard.ideas?.totalRatings ?? 0,
      icon: Star,
    },
    {
      label: 'Avg Rating',
      value:
        dashboard.ideas?.overallAverageRating > 0
          ? dashboard.ideas.overallAverageRating.toFixed(1)
          : '—',
      icon: BarChart3,
    },
    {
      label: 'My Ideas',
      value: dashboard.ideas?.ideasSubmittedByMe ?? 0,
      icon: FileText,
    },
  ];

  return (
    <div className="min-h-screen p-8">
      <div className="max-w-5xl mx-auto">
        <motion.div
          initial={{ opacity: 0 }}
          animate={{ opacity: 1 }}
          transition={{ duration: 0.3 }}
          className="space-y-8"
        >
          <div className="mb-8">
            <h1 className="text-3xl font-bold text-white mb-1">Dashboard</h1>
            <p className="text-[#AAAAAA]">
              Your team's idea insights at a glance.
            </p>
          </div>

          {/* Stats row */}
          <div className="grid grid-cols-2 lg:grid-cols-4 gap-4 mb-8">
            {stats.map((stat, i) => (
              <StatsCard key={stat.label} {...stat} index={i} />
            ))}
          </div>

          {/* Top Rated + Recent Ideas */}
          <div className="grid grid-cols-1 lg:grid-cols-2 gap-6">
            {/* Top Rated Ideas */}
            <div className="bg-[#0D0D0D] border border-[#2A2A2A] rounded-xl p-6">
              <div className="flex items-center gap-2 mb-4">
                <Trophy className="w-5 h-5 text-amber-500" />
                <h2 className="text-lg font-bold text-white">
                  Top Rated Ideas
                </h2>
              </div>
              {dashboard.topIdeas?.length > 0 ? (
                <div className="space-y-3">
                  {dashboard.topIdeas.map((idea, i) => (
                    <div
                      key={idea.id}
                      className="flex items-center justify-between bg-[#1A1A1A] rounded-lg px-4 py-3"
                    >
                      <div className="flex items-center gap-3">
                        <span
                          className={`w-7 h-7 rounded-full flex items-center justify-center text-xs font-bold ${
                            i === 0
                              ? 'bg-amber-500 text-black'
                              : 'bg-[#2A2A2A] text-[#AAAAAA]'
                          }`}
                        >
                          {i + 1}
                        </span>
                        <span className="text-white text-sm font-medium">
                          {idea.title}
                        </span>
                      </div>
                      <div className="flex items-center gap-1">
                        <Star className="w-4 h-4 text-amber-400 fill-amber-400" />
                        <span className="text-amber-400 text-sm font-bold">
                          {idea.averageRating?.toFixed(1)}
                        </span>
                      </div>
                    </div>
                  ))}
                </div>
              ) : (
                <p className="text-[#555555] text-sm text-center py-8">
                  No rated ideas yet
                </p>
              )}
            </div>

            {/* Recent Ideas */}
            <div className="bg-[#0D0D0D] border border-[#2A2A2A] rounded-xl p-6">
              <div className="flex items-center gap-2 mb-4">
                <Clock className="w-5 h-5 text-[#AAAAAA]" />
                <h2 className="text-lg font-bold text-white">Recent Ideas</h2>
              </div>
              {dashboard.recentIdeas?.length > 0 ? (
                <div className="space-y-3">
                  {dashboard.recentIdeas.map((idea) => (
                    <div
                      key={idea.id}
                      className="flex items-center justify-between bg-[#1A1A1A] rounded-lg px-4 py-3"
                    >
                      <span className="text-white text-sm font-medium">
                        {idea.title}
                      </span>
                      <span className="text-[#555555] text-xs">
                        {new Date(idea.createdAt).toLocaleDateString()}
                      </span>
                    </div>
                  ))}
                </div>
              ) : (
                <p className="text-[#555555] text-sm text-center py-8">
                  No ideas yet
                </p>
              )}
            </div>
          </div>

          {/* Your Team Card */}
          {team && (
            <div className="bg-[#0D0D0D] border border-[#2A2A2A] rounded-xl p-6">
              <h2 className="text-lg font-bold text-white mb-6">Your Team</h2>

              <div className="flex items-start gap-4 mb-6">
                <div className="w-12 h-12 bg-[#1A1A1A] rounded-xl flex items-center justify-center border border-[#2A2A2A] flex-shrink-0">
                  <Users className="w-6 h-6 text-[#E8003D]" />
                </div>
                <div>
                  <h3 className="text-xl font-bold text-white mb-1">
                    {team.name}
                  </h3>
                  <p className="text-[#AAAAAA] text-sm">
                    {team.memberCount ?? 0} member
                    {team.memberCount !== 1 ? 's' : ''}
                    {' · '}
                    Created {formatDate(team.createdAt)}
                  </p>
                </div>
              </div>

              {/* Invite Code with copy button */}
              <div>
                <label className="text-xs font-medium text-[#555555] uppercase tracking-wider mb-2 block">
                  Invite Code
                </label>
                <div className="flex items-center gap-3 bg-[#1A1A1A] border border-[#2A2A2A] rounded-lg px-4 h-12">
                  <code className="text-[#E8003D] font-mono font-bold text-lg flex-1 tracking-widest">
                    {team.inviteCode}
                  </code>
                  <button
                    onClick={() => {
                      navigator.clipboard.writeText(team.inviteCode);
                      toast.success('Invite code copied!');
                    }}
                    className="text-[#555555] hover:text-white transition-colors p-1 cursor-pointer"
                  >
                    <Copy className="w-4 h-4" />
                  </button>
                </div>
              </div>
            </div>
          )}
        </motion.div>
      </div>
    </div>
  );
}
