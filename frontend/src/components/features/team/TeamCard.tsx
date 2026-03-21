import { Copy, RefreshCw, Users } from 'lucide-react';
import toast from 'react-hot-toast';
import Badge from '../../ui/Badge';
import type { Team } from '../../../types/team.types';
import { useRegenerateInvite } from '../../../hooks/useTeam';
import { useAuthStore } from '../../../store/auth.store';

interface TeamCardProps {
  team: Team;
}

export default function TeamCard({ team }: TeamCardProps) {
  const role = useAuthStore((s) => s.role);
  const isAdmin = role === 'Admin';
  const { mutate: regenerate, isPending } = useRegenerateInvite();

  const copyCode = async () => {
    try {
      await navigator.clipboard.writeText(team.inviteCode);
      toast.success('Invite code copied!');
    } catch {
      toast.error('Failed to copy');
    }
  };

  return (
    <div className="bg-[#0D0D0D] border border-[#2A2A2A] rounded-xl p-6 space-y-4">
      <div className="flex items-center justify-between">
        <div className="flex items-center gap-3">
          <div className="w-10 h-10 rounded-xl bg-[#E8003D]/10 flex items-center justify-center">
            <Users className="w-5 h-5 text-[#E8003D]" />
          </div>
          <div>
            <h3 className="text-lg font-bold text-white">{team.name}</h3>
            <p className="text-xs text-[#555555]">
              Created {new Date(team.createdAt).toLocaleDateString()}
            </p>
          </div>
        </div>
        {isAdmin && <Badge variant="primary">Admin</Badge>}
      </div>

      <div className="flex items-center gap-2 bg-[#1A1A1A] rounded-lg px-4 py-3">
        <span className="text-xs text-[#555555] mr-2">Invite Code:</span>
        <code className="text-sm font-mono text-white tracking-wider flex-1">
          {team.inviteCode}
        </code>
        <button
          onClick={copyCode}
          className="p-1.5 text-[#555555] hover:text-white rounded-lg hover:bg-white/5 transition-colors cursor-pointer"
          title="Copy invite code"
        >
          <Copy className="w-4 h-4" />
        </button>
        {isAdmin && (
          <button
            onClick={() => regenerate()}
            disabled={isPending}
            className="p-1.5 text-[#555555] hover:text-[#E8003D] rounded-lg hover:bg-white/5 transition-colors disabled:opacity-50 cursor-pointer"
            title="Regenerate code"
          >
            <RefreshCw className={`w-4 h-4 ${isPending ? 'animate-spin' : ''}`} />
          </button>
        )}
      </div>
    </div>
  );
}
