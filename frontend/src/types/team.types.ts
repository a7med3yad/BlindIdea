export interface Team {
  id: string;
  name: string;
  inviteCode: string;
  createdAt: string;
  memberCount?: number;
  adminId?: string;
}

export interface TeamMember {
  id: string;
  email: string;
  role: string;
  joinedAt: string;
}

export interface CreateTeamRequest {
  name: string;
}

export interface JoinTeamRequest {
  inviteCode: string;
}
