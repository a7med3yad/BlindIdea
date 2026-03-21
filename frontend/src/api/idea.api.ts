import api from './axios';
import type { Idea, SubmitIdeaRequest, RateIdeaRequest } from '../types/idea.types';

export const ideaApi = {
  submit: (data: SubmitIdeaRequest) =>
    api.post<Idea>('/Idea/submit', data),

  getTeamIdeas: () =>
    api.get<Idea[]>('/Idea/team-ideas'),

  getById: (ideaId: string) =>
    api.get<Idea>(`/Idea/${ideaId}`),

  delete: (ideaId: string) =>
    api.delete(`/Idea/${ideaId}`),

  rate: (ideaId: string, data: RateIdeaRequest) =>
    api.post(`/Idea/${ideaId}/rate`, data),

  removeRating: (ideaId: string) =>
    api.delete(`/Idea/${ideaId}/rate`),
};
