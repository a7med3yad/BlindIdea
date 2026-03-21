export interface Idea {
  id: string;
  title: string;
  content: string;
  averageRating: number;
  totalRatings: number;
  isOwner: boolean;
  userRating: number | null;
  createdAt: string;
}

export interface SubmitIdeaRequest {
  title: string;
  content: string;
}

export interface RateIdeaRequest {
  score: number;
}
