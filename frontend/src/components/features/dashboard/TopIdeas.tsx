import { Trophy, Star } from 'lucide-react';
import type { DashboardIdea } from '../../../types/dashboard.types';

interface TopIdeasProps {
  ideas: DashboardIdea[];
}

export default function TopIdeas({ ideas }: TopIdeasProps) {
  return (
    <div className="bg-[#0D0D0D] border border-[#2A2A2A] rounded-xl p-6">
      <h3 className="text-lg font-bold text-white mb-4 flex items-center gap-2">
        <Trophy className="w-4 h-4 text-[#F59E0B]" />
        Top Rated Ideas
      </h3>
      {ideas.length === 0 ? (
        <p className="text-sm text-[#555555] text-center py-6">
          No rated ideas yet
        </p>
      ) : (
        <div className="space-y-2">
          {ideas.map((idea, i) => (
            <div
              key={idea.id}
              className="flex items-center gap-3 p-3 rounded-lg bg-[#1A1A1A]/50 hover:bg-[#1A1A1A] transition-colors"
            >
              <span
                className={`w-6 h-6 rounded-full flex items-center justify-center text-xs font-bold ${
                  i === 0
                    ? 'bg-[#F59E0B]/20 text-[#F59E0B]'
                    : i === 1
                    ? 'bg-[#AAAAAA]/20 text-[#AAAAAA]'
                    : 'bg-[#8B5E3C]/20 text-[#CD7F32]'
                }`}
              >
                {i + 1}
              </span>
              <span className="flex-1 text-sm text-white truncate">
                {idea.title}
              </span>
              <div className="flex items-center gap-1 text-sm">
                <Star className="w-3 h-3 text-[#F59E0B]" fill="#F59E0B" />
                <span className="text-white font-semibold">
                  {idea.averageRating.toFixed(1)}
                </span>
              </div>
            </div>
          ))}
        </div>
      )}
    </div>
  );
}
