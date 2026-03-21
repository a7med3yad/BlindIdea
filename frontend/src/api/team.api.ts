import api from './axios';
import type { Team, TeamMember, CreateTeamRequest, JoinTeamRequest } from '../types/team.types';

export const teamApi = {
  create: (data: CreateTeamRequest) =>
    api.post<Team>('/Team/create', data),

  join: (data: JoinTeamRequest) =>
    api.post<Team>('/Team/join', data),

  getMyTeam: () =>
    api.get<Team>('/Team/my-team'),

  getMembers: () =>
    api.get<TeamMember[]>('/Team/members'),

  leave: () =>
    api.post('/Team/leave'),

  deleteTeam: () =>
    api.delete('/Team/delete'),

  regenerateInvite: () =>
    api.post<{ inviteCode: string }>('/Team/regenerate-invite'),

  removeMember: (memberId: string) =>
    api.delete(`/Team/remove-member/${memberId}`),
};
