import api from './api';
import { Doctor, AvailableSlot } from '../types';

export const doctorService = {
  getAll: () => api.get<Doctor[]>('/doctors'),
  getById: (id: string) => api.get<Doctor>(`/doctors/${id}`),
  create: (data: any) => api.post<Doctor>('/doctors', data),
  update: (id: string, data: any) => api.put<Doctor>(`/doctors/${id}`, data),
  getAvailability: (id: string) => api.get(`/doctors/${id}/availability`),
  updateAvailability: (id: string, data: any) => api.put(`/doctors/${id}/availability`, data),
  getTimeOffs: (id: string) => api.get(`/doctors/${id}/time-offs`),
  requestTimeOff: (id: string, data: any) => api.post(`/doctors/${id}/time-offs`, data),
  getSlots: (id: string, date: string) => api.get<AvailableSlot[]>(`/doctors/${id}/slots`, { params: { date } }),
};
