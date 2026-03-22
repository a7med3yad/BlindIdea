import { useQuery, useMutation, useQueryClient } from '@tanstack/react-query';
import toast from 'react-hot-toast';
import { teamApi } from '../api/team.api';
import { useTeamStore } from '../store/team.store';
import { useAuthStore } from '../store/auth.store';
import type { CreateTeamRequest, JoinTeamRequest } from '../types/team.types';

export const useMyTeam = () => {
  const setTeam = useTeamStore((s) => s.setTeam);
  const hasTeam = useAuthStore((s) => s.hasTeam);

  return useQuery({
    queryKey: ['my-team'],
    queryFn: async () => {
      const response = await teamApi.getMyTeam();
      setTeam(response.data);
      // Keep auth store in sync
      useAuthStore.getState().setTeam(response.data.id);
      return response.data;
    },
    retry: false,
    // Only fetch if user actually has a team (known from app startup)
    enabled: hasTeam,
    staleTime: 10 * 60_000, // 10 minutes — team data rarely changes
  });
};

export const useTeamMembers = () => {
  const hasTeam = useAuthStore((s) => s.hasTeam);

  return useQuery({
    queryKey: ['team-members'],
    queryFn: async () => {
      const response = await teamApi.getMembers();
      return response.data;
    },
    enabled: hasTeam,
    staleTime: 10 * 60_000,
  });
};

export const useCreateTeam = () => {
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: (data: CreateTeamRequest) => teamApi.create(data),
    onSuccess: (response) => {
      toast.success('Team created successfully!');
      // ✅ Update store directly — no refetch needed
      useAuthStore.getState().setTeam(response.data.id);
      useTeamStore.getState().setTeam(response.data);
      queryClient.invalidateQueries({ queryKey: ['my-team'] });
    },
  });
};

export const useJoinTeam = () => {
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: (data: JoinTeamRequest) => teamApi.join(data),
    onSuccess: (response) => {
      toast.success('Joined team successfully!');
      // ✅ Update store directly — no refetch needed
      useAuthStore.getState().setTeam(response.data.id);
      useTeamStore.getState().setTeam(response.data);
      queryClient.invalidateQueries({ queryKey: ['my-team'] });
    },
  });
};

export const useLeaveTeam = () => {
  const queryClient = useQueryClient();
  const clearTeam = useTeamStore((s) => s.clearTeam);

  return useMutation({
    mutationFn: () => teamApi.leave(),
    onSuccess: () => {
      clearTeam();
      // ✅ Update store directly — no refetch needed
      useAuthStore.getState().setTeam(null);
      toast.success('Left team successfully');
      queryClient.invalidateQueries({ queryKey: ['my-team'] });
      queryClient.invalidateQueries({ queryKey: ['team-members'] });
    },
  });
};

export const useDeleteTeam = () => {
  const queryClient = useQueryClient();
  const clearTeam = useTeamStore((s) => s.clearTeam);

  return useMutation({
    mutationFn: () => teamApi.deleteTeam(),
    onSuccess: () => {
      clearTeam();
      // ✅ Update store directly — no refetch needed
      useAuthStore.getState().setTeam(null);
      toast.success('Team deleted');
      queryClient.invalidateQueries({ queryKey: ['my-team'] });
    },
  });
};

export const useRegenerateInvite = () => {
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: () => teamApi.regenerateInvite(),
    onSuccess: () => {
      toast.success('Invite code regenerated!');
      queryClient.invalidateQueries({ queryKey: ['my-team'] });
    },
  });
};

export const useRemoveMember = () => {
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: (memberId: string) => teamApi.removeMember(memberId),
    onSuccess: () => {
      toast.success('Member removed');
      queryClient.invalidateQueries({ queryKey: ['team-members'] });
    },
  });
};
