import api from './axios';
import type { DashboardStats } from '../types/dashboard.types';

export const dashboardApi = {
  getStats: () =>
    api.get<DashboardStats>('/Dashboard'),
};
