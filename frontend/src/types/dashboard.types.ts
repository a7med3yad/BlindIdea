export interface DashboardStats {
  totalIdeas: number;
  totalRatings: number;
  averageRating: number;
  myIdeas: number;
  topIdeas: DashboardIdea[];
  recentIdeas: DashboardIdea[];
  team: DashboardTeam | null;
}

export interface DashboardIdea {
  id: string;
  title: string;
  averageRating: number;
  totalRatings: number;
  createdAt: string;
}

export interface DashboardTeam {
  id: string;
  name: string;
  inviteCode: string;
  memberCount: number;
}
