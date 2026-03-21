import { useQuery, useMutation, useQueryClient } from '@tanstack/react-query';
import toast from 'react-hot-toast';
import { ideaApi } from '../api/idea.api';
import type { SubmitIdeaRequest, RateIdeaRequest } from '../types/idea.types';

export const useTeamIdeas = () => {
  return useQuery({
    queryKey: ['team-ideas'],
    queryFn: async () => {
      const response = await ideaApi.getTeamIdeas();
      return response.data;
    },
  });
};

export const useSubmitIdea = () => {
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: (data: SubmitIdeaRequest) => ideaApi.submit(data),
    onSuccess: () => {
      toast.success('Idea submitted anonymously!');
      queryClient.invalidateQueries({ queryKey: ['team-ideas'] });
      queryClient.invalidateQueries({ queryKey: ['dashboard'] });
    },
  });
};

export const useDeleteIdea = () => {
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: (ideaId: string) => ideaApi.delete(ideaId),
    onSuccess: () => {
      toast.success('Idea deleted');
      queryClient.invalidateQueries({ queryKey: ['team-ideas'] });
      queryClient.invalidateQueries({ queryKey: ['dashboard'] });
    },
  });
};

export const useRateIdea = () => {
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: ({ ideaId, data }: { ideaId: string; data: RateIdeaRequest }) =>
      ideaApi.rate(ideaId, data),
    onSuccess: () => {
      toast.success('Rating submitted!');
      queryClient.invalidateQueries({ queryKey: ['team-ideas'] });
      queryClient.invalidateQueries({ queryKey: ['dashboard'] });
    },
  });
};

export const useRemoveRating = () => {
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: (ideaId: string) => ideaApi.removeRating(ideaId),
    onSuccess: () => {
      toast.success('Rating removed');
      queryClient.invalidateQueries({ queryKey: ['team-ideas'] });
    },
  });
};
