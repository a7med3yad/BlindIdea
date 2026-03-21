import { Lightbulb } from 'lucide-react';
import IdeaCard from './IdeaCard';
import Spinner from '../../ui/Spinner';
import type { Idea } from '../../../types/idea.types';

interface IdeaListProps {
  ideas: Idea[] | undefined;
  isLoading: boolean;
}

export default function IdeaList({ ideas, isLoading }: IdeaListProps) {
  if (isLoading) {
    return (
      <div className="flex flex-col items-center justify-center py-20">
        <Spinner size={32} />
        <p className="text-[#AAAAAA] text-sm mt-3">Loading...</p>
      </div>
    );
  }

  if (!ideas || ideas.length === 0) {
    return (
      <div className="flex flex-col items-center justify-center py-20 text-center">
        <div className="w-16 h-16 bg-[#1A1A1A] rounded-2xl flex items-center justify-center mb-4">
          <Lightbulb className="w-8 h-8 text-[#555555]" />
        </div>
        <h3 className="text-lg font-bold text-white mb-2">No ideas yet</h3>
        <p className="text-[#AAAAAA] text-sm max-w-sm">
          Be the first to share an idea with your team! All submissions are anonymous.
        </p>
      </div>
    );
  }

  return (
    <div className="grid grid-cols-1 xl:grid-cols-2 gap-4">
      {ideas.map((idea, index) => (
        <IdeaCard key={idea.id} idea={idea} index={index} />
      ))}
    </div>
  );
}
