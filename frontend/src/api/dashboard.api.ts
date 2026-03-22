import api from './axios';
import type { DashboardResponse } from '../types/dashboard.types';

export const dashboardApi = {
  getDashboard: () =>
    api.get<DashboardResponse>('/Dashboard'),
};
