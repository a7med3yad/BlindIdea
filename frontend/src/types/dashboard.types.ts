export interface DashboardResponse {
  team: {
    teamName: string;
    memberCount: number;
    createdAt: string;
  };
  ideas: {
    totalIdeas: number;
    totalRatings: number;
    overallAverageRating: number;
    ideasSubmittedByMe: number;
    ideasRatedByMe: number;
  };
  topIdeas: DashboardIdea[];
  recentIdeas: DashboardIdea[];
}

export interface DashboardIdea {
  id: string;
  title: string;
  averageRating: number;
  totalRatings: number;
  createdAt: string;
}

// Keep old name as alias for backward compat
export type DashboardStats = DashboardResponse;
