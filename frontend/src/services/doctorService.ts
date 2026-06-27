import api from './api';
import { Doctor, AvailableSlot } from '../types';

export const doctorService = {
  getAll: (page?: number, pageSize?: number, search?: string) =>
    api.get<any>('/doctors', { params: { page, pageSize, search } }),
  getById: (id: string) => api.get<Doctor>(`/doctors/${id}`),
  getMyProfile: () => api.get<Doctor>('/doctors/me'),
  create: (data: any) => api.post<Doctor>('/doctors', data),
  update: (id: string, data: any) => api.put<Doctor>(`/doctors/${id}`, data),
  updateProfile: (id: string, data: any) => api.put<Doctor>(`/doctors/${id}/profile`, data),
  uploadImage: (id: string, file: File, type: 'profile' | 'license') => {
    const formData = new FormData();
    formData.append('file', file);
    return api.post(`/doctors/${id}/upload-image?type=${type}`, formData, {
      headers: { 'Content-Type': 'multipart/form-data' },
    });
  },
  getAvailability: (id: string) => api.get(`/doctors/${id}/availability`),
  updateAvailability: (id: string, data: any) => api.put(`/doctors/${id}/availability`, data),
  getTimeOffs: (id: string) => api.get(`/doctors/${id}/time-offs`),
  requestTimeOff: (id: string, data: any) => api.post(`/doctors/${id}/time-offs`, data),
  getSlots: (id: string, date: string) => api.get<AvailableSlot[]>(`/doctors/${id}/slots`, { params: { date } }),
};
