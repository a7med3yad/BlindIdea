import { Clock } from 'lucide-react';
import type { DashboardIdea } from '../../../types/dashboard.types';

interface RecentIdeasProps {
  ideas: DashboardIdea[];
}

export default function RecentIdeas({ ideas }: RecentIdeasProps) {
  return (
    <div className="bg-[#0D0D0D] border border-[#2A2A2A] rounded-xl p-6">
      <h3 className="text-lg font-bold text-white mb-4 flex items-center gap-2">
        <Clock className="w-4 h-4 text-[#AAAAAA]" />
        Recent Ideas
      </h3>
      {ideas.length === 0 ? (
        <p className="text-sm text-[#555555] text-center py-6">
          No ideas submitted yet
        </p>
      ) : (
        <div className="space-y-2">
          {ideas.map((idea) => (
            <div
              key={idea.id}
              className="flex items-center justify-between p-3 rounded-lg bg-[#1A1A1A]/50 hover:bg-[#1A1A1A] transition-colors"
            >
              <span className="text-sm text-white truncate flex-1 mr-3">
                {idea.title}
              </span>
              <span className="text-xs text-[#555555] whitespace-nowrap">
                {new Date(idea.createdAt).toLocaleDateString()}
              </span>
            </div>
          ))}
        </div>
      )}
    </div>
  );
}
