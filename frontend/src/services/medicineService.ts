import api from './api';
import { Medicine } from '../types';

export const medicineService = {
  getAll: (search?: string, category?: string) => api.get<Medicine[]>('/medicines', { params: { search, category } }),
  getById: (id: string) => api.get<Medicine>(`/medicines/${id}`),
  create: (data: any) => api.post<Medicine>('/medicines', data),
  update: (id: string, data: any) => api.put<Medicine>(`/medicines/${id}`, data),
  delete: (id: string) => api.delete(`/medicines/${id}`),
};
