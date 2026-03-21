import { useQuery, useMutation, useQueryClient } from '@tanstack/react-query';
import toast from 'react-hot-toast';
import { teamApi } from '../api/team.api';
import { useTeamStore } from '../store/team.store';
import type { CreateTeamRequest, JoinTeamRequest } from '../types/team.types';

export const useMyTeam = () => {
  const setTeam = useTeamStore((s) => s.setTeam);

  return useQuery({
    queryKey: ['my-team'],
    queryFn: async () => {
      const response = await teamApi.getMyTeam();
      setTeam(response.data);
      return response.data;
    },
    retry: false,
  });
};

export const useTeamMembers = () => {
  return useQuery({
    queryKey: ['team-members'],
    queryFn: async () => {
      const response = await teamApi.getMembers();
      return response.data;
    },
  });
};

export const useCreateTeam = () => {
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: (data: CreateTeamRequest) => teamApi.create(data),
    onSuccess: () => {
      toast.success('Team created successfully!');
      queryClient.invalidateQueries({ queryKey: ['my-team'] });
    },
  });
};

export const useJoinTeam = () => {
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: (data: JoinTeamRequest) => teamApi.join(data),
    onSuccess: () => {
      toast.success('Joined team successfully!');
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
