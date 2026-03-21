import { Lock, Trash2, Star } from 'lucide-react';
import { motion } from 'framer-motion';
import StarRating from '../../ui/StarRating';
import { useRateIdea, useDeleteIdea, useRemoveRating } from '../../../hooks/useIdeas';
import type { Idea } from '../../../types/idea.types';

interface IdeaCardProps {
  idea: Idea;
  index?: number;
}

export default function IdeaCard({ idea, index = 0 }: IdeaCardProps) {
  const { mutate: rateIdea } = useRateIdea();
  const { mutate: deleteIdea, isPending: isDeleting } = useDeleteIdea();
  const { mutate: removeRating } = useRemoveRating();

  const handleRate = (score: number) => {
    if (score === idea.userRating) {
      removeRating(idea.id);
    } else {
      rateIdea({ ideaId: idea.id, data: { score } });
    }
  };

  return (
    <motion.div
      initial={{ opacity: 0, y: 20 }}
      animate={{ opacity: 1, y: 0 }}
      transition={{ duration: 0.3, delay: index * 0.05 }}
    >
      <div className="bg-[#0D0D0D] border border-[#2A2A2A] rounded-xl p-6 hover:border-[#3A3A3A] transition-colors space-y-3">
        <div className="flex items-start justify-between gap-3">
          <h3 className="text-base font-bold text-white leading-tight flex-1">
            {idea.title}
          </h3>
          {idea.isOwner && (
            <button
              onClick={() => deleteIdea(idea.id)}
              disabled={isDeleting}
              className="p-1.5 text-[#555555] hover:text-[#EF4444] rounded-lg hover:bg-[#EF4444]/10 transition-colors shrink-0 disabled:opacity-50 cursor-pointer"
              title="Delete idea"
            >
              <Trash2 className="w-4 h-4" />
            </button>
          )}
        </div>

        <p className="text-[#AAAAAA] text-sm leading-relaxed line-clamp-3">
          {idea.content}
        </p>

        <div className="flex items-center justify-between pt-3 border-t border-[#2A2A2A]">
          <div className="flex items-center gap-4">
            <div className="flex items-center gap-1">
              <Star className="w-3.5 h-3.5 text-[#F59E0B]" fill="#F59E0B" />
              <span className="text-sm font-semibold text-white">
                {idea.averageRating > 0 ? idea.averageRating.toFixed(1) : '—'}
              </span>
              <span className="text-xs text-[#555555]">
                ({idea.totalRatings})
              </span>
            </div>
            <span className="text-xs text-[#555555]">
              {new Date(idea.createdAt).toLocaleDateString()}
            </span>
          </div>

          {idea.isOwner ? (
            <div className="flex items-center gap-1.5 text-[#555555]">
              <Lock className="w-3.5 h-3.5" />
              <span className="text-xs">Your idea</span>
            </div>
          ) : (
            <StarRating
              value={idea.userRating ?? 0}
              onChange={handleRate}
              size={16}
            />
          )}
        </div>
      </div>
    </motion.div>
  );
}
