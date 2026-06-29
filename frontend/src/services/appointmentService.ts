import api from './api';
import { Appointment, CalendarEvent, PaginatedResult } from '../types';

export const appointmentService = {
  getAll: (params?: any) => api.get<PaginatedResult<Appointment>>('/appointments', { params }),
  getById: (id: string) => api.get<Appointment>(`/appointments/${id}`),
  create: (data: any) => api.post<Appointment>('/appointments', data),
  update: (id: string, data: any) => api.put<Appointment>(`/appointments/${id}`, data),
  updateStatus: (id: string, status: number, cancellationReason?: string) =>
    api.patch(`/appointments/${id}/status`, { status, cancellationReason }),
  delete: (id: string) => api.delete(`/appointments/${id}`),
  getCalendarEvents: (start: string, end: string, doctorId?: string) =>
    api.get<CalendarEvent[]>('/appointments/calendar', { params: { start, end, doctorId } }),
  archive: (id: string) => api.patch(`/appointments/${id}/archive`),
  restore: (id: string) => api.patch(`/appointments/${id}/restore`),
};
