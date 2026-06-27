import api from './api';
import { LoginRequest, LoginResponse, UserInfo, RegisterRequest } from '../types';

export const authService = {
  login: (data: LoginRequest) => api.post<LoginResponse>('/auth/login', data),
  register: (data: RegisterRequest) => api.post('/auth/register', data),
  refresh: (refreshToken: string) => api.post('/auth/refresh', { refreshToken }),
  logout: () => api.post('/auth/logout'),
  getMe: () => api.get<UserInfo>('/auth/me'),
  getAllUsers: (page?: number, pageSize?: number, search?: string) =>
    api.get<any>('/users', { params: { page, pageSize, search } }),
  updateUser: (id: string, data: any) => api.put(`/users/${id}`, data),
  toggleActivation: (id: string, isActive: boolean) => api.patch(`/users/${id}/activate`, { isActive }),
};
