import { useState } from 'react';
import { motion } from 'framer-motion';
import { Users, Plus, KeyRound, Trash2, LogOut } from 'lucide-react';
import Button from '../components/ui/Button';
import Modal from '../components/ui/Modal';
import Spinner from '../components/ui/Spinner';
import CreateTeamForm from '../components/features/team/CreateTeamForm';
import JoinTeamForm from '../components/features/team/JoinTeamForm';
import TeamCard from '../components/features/team/TeamCard';
import MemberList from '../components/features/team/MemberList';
import { useMyTeam, useTeamMembers, useLeaveTeam, useDeleteTeam } from '../hooks/useTeam';
import { useAuthStore } from '../store/auth.store';

export default function TeamPage() {
  const [showDeleteModal, setShowDeleteModal] = useState(false);
  const hasTeam = useAuthStore((s) => s.hasTeam);
  const { data: team, isLoading: isLoadingTeam } = useMyTeam();
  const { data: members, isLoading: isLoadingMembers } = useTeamMembers();
  const { mutate: leaveTeam, isPending: isLeaving } = useLeaveTeam();
  const { mutate: deleteTeam, isPending: isDeleting } = useDeleteTeam();
  const role = useAuthStore((s) => s.role);
  const isAdmin = role === 'Admin';

  if (isLoadingTeam && hasTeam) {
    return (
      <div className="min-h-screen p-8">
        <div className="max-w-4xl mx-auto">
          <div className="flex flex-col items-center justify-center min-h-[60vh]">
            <Spinner size={32} />
            <p className="text-sm text-[#555555] mt-3">Loading...</p>
          </div>
        </div>
      </div>
    );
  }

  if (!hasTeam || !team) {
    return (
      <div className="min-h-screen p-8">
        <div className="max-w-4xl mx-auto">
          <motion.div
            initial={{ opacity: 0 }}
            animate={{ opacity: 1 }}
            transition={{ duration: 0.3 }}
          >
            <div className="text-center mb-12">
              <div className="w-20 h-20 bg-[#1A1A1A] rounded-2xl flex items-center justify-center mx-auto mb-6">
                <Users className="w-9 h-9 text-[#555555]" />
              </div>
              <h1 className="text-2xl font-bold text-white mb-3">
                Join or Create a Team
              </h1>
              <p className="text-[#AAAAAA] text-base">
                You need a team to start sharing and rating ideas.
              </p>
            </div>

            <div className="grid grid-cols-1 md:grid-cols-2 gap-6 mx-auto">
              {/* Create Team */}
              <div className="bg-[#0D0D0D] border border-[#2A2A2A] rounded-2xl p-8">
                <div className="flex items-center gap-3 mb-6">
                  <div className="w-10 h-10 bg-[#1A1A1A] border border-[#2A2A2A] rounded-xl flex items-center justify-center">
                    <Plus className="w-5 h-5 text-[#E8003D]" />
                  </div>
                  <h2 className="text-lg font-bold text-white">Create Team</h2>
                </div>
                <CreateTeamForm />
              </div>

              {/* Join Team */}
              <div className="bg-[#0D0D0D] border border-[#2A2A2A] rounded-2xl p-8">
                <div className="flex items-center gap-3 mb-6">
                  <div className="w-10 h-10 bg-[#1A1A1A] border border-[#2A2A2A] rounded-xl flex items-center justify-center">
                    <KeyRound className="w-5 h-5 text-[#E8003D]" />
                  </div>
                  <h2 className="text-lg font-bold text-white">Join Team</h2>
                </div>
                <JoinTeamForm />
              </div>
            </div>
          </motion.div>
        </div>
      </div>
    );
  }

  // In a team
  return (
    <div className="min-h-screen p-8">
      <div className="max-w-4xl mx-auto">
        <motion.div
          initial={{ opacity: 0 }}
          animate={{ opacity: 1 }}
          transition={{ duration: 0.3 }}
          className="space-y-8"
        >
          <div>
            <h1 className="text-3xl font-bold text-white mb-2">Team</h1>
            <p className="text-[#AAAAAA] text-base">Manage your team and members.</p>
          </div>

          <TeamCard team={team} />

          <div>
            <h2 className="text-xl font-bold text-white mb-4 flex items-center gap-2">
              <Users className="w-4 h-4 text-[#AAAAAA]" />
              Members
              {members && (
                <span className="text-sm text-[#555555] font-normal">
                  ({members.length})
                </span>
              )}
            </h2>
            {isLoadingMembers ? (
              <div className="flex items-center justify-center py-12">
                <Spinner size={24} />
              </div>
            ) : (
              <MemberList members={members || []} />
            )}
          </div>

          {/* Actions */}
          <div className="flex flex-col sm:flex-row gap-3 pt-4 border-t border-[#2A2A2A]">
            {!isAdmin && (
              <Button
                variant="danger"
                onClick={() => leaveTeam()}
                isLoading={isLeaving}
              >
                <LogOut className="w-4 h-4" />
                Leave Team
              </Button>
            )}
            {isAdmin && (
              <Button
                variant="danger"
                onClick={() => setShowDeleteModal(true)}
              >
                <Trash2 className="w-4 h-4" />
                Delete Team
              </Button>
            )}
          </div>

          {/* Delete confirmation modal */}
          <Modal
            isOpen={showDeleteModal}
            onClose={() => setShowDeleteModal(false)}
            title="Delete Team"
          >
            <p className="text-sm text-[#AAAAAA] mb-6">
              Are you sure you want to delete this team? This action cannot be
              undone. All ideas and ratings will be permanently deleted.
            </p>
            <div className="flex gap-3">
              <Button
                variant="secondary"
                onClick={() => setShowDeleteModal(false)}
                fullWidth
              >
                Cancel
              </Button>
              <Button
                variant="danger"
                onClick={() => {
                  deleteTeam();
                  setShowDeleteModal(false);
                }}
                isLoading={isDeleting}
                fullWidth
              >
                Delete
              </Button>
            </div>
          </Modal>
        </motion.div>
      </div>
    </div>
  );
}
