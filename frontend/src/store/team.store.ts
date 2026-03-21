import { create } from 'zustand';
import type { Team } from '../types/team.types';

interface TeamState {
  team: Team | null;
  setTeam: (team: Team | null) => void;
  clearTeam: () => void;
}

export const useTeamStore = create<TeamState>((set) => ({
  team: null,
  setTeam: (team) => set({ team }),
  clearTeam: () => set({ team: null }),
}));
