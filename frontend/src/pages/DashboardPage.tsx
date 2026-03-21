import { useQuery } from '@tanstack/react-query';
import { Link } from 'react-router-dom';
import { motion } from 'framer-motion';
import { Lightbulb, Star, BarChart3, FileText, Users, ArrowRight } from 'lucide-react';
import { dashboardApi } from '../api/dashboard.api';
import StatsCard from '../components/features/dashboard/StatsCard';
import TopIdeas from '../components/features/dashboard/TopIdeas';
import RecentIdeas from '../components/features/dashboard/RecentIdeas';
import TeamCard from '../components/features/team/TeamCard';
import Spinner from '../components/ui/Spinner';
import Button from '../components/ui/Button';

export default function DashboardPage() {
  const { data, isLoading, isError } = useQuery({
    queryKey: ['dashboard'],
    queryFn: async () => {
      const response = await dashboardApi.getStats();
      return response.data;
    },
  });

  if (isLoading) {
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

  if (isError || !data) {
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
              You need to be part of a team to see your dashboard. Create or join a team to start sharing ideas.
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

  const stats = [
    { label: 'Total Ideas', value: data.totalIdeas, icon: Lightbulb },
    { label: 'Total Ratings', value: data.totalRatings, icon: Star },
    {
      label: 'Avg Rating',
      value: data.averageRating > 0 ? data.averageRating.toFixed(1) : '—',
      icon: BarChart3,
    },
    { label: 'My Ideas', value: data.myIdeas, icon: FileText },
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

          {/* Content grid */}
          <div className="grid grid-cols-1 lg:grid-cols-2 gap-6">
            <TopIdeas ideas={data.topIdeas || []} />
            <RecentIdeas ideas={data.recentIdeas || []} />
          </div>

          {/* Team info */}
          {data.team && (
            <div>
              <h2 className="text-xl font-bold text-white mb-4">Your Team</h2>
              <TeamCard
                team={{
                  id: data.team.id,
                  name: data.team.name,
                  inviteCode: data.team.inviteCode,
                  createdAt: '',
                }}
              />
            </div>
          )}
        </motion.div>
      </div>
    </div>
  );
}
