import api from './api';
import { Patient, PaginatedResult } from '../types';

export const patientService = {
  getAll: (page = 1, pageSize = 25, search?: string) =>
    api.get<PaginatedResult<Patient>>('/patients', { params: { page, pageSize, search } }),
  getById: (id: string) => api.get<Patient>(`/patients/${id}`),
  getByDoctor: (doctorId: string, page = 1, pageSize = 25, search?: string) =>
    api.get<PaginatedResult<Patient>>(`/patients/by-doctor/${doctorId}`, { params: { page, pageSize, search } }),
  create: (data: any) => api.post<Patient>('/patients', data),
  update: (id: string, data: any) => api.put<Patient>(`/patients/${id}`, data),
  delete: (id: string) => api.delete(`/patients/${id}`),
  search: (q: string) => api.get<Patient[]>('/patients/search', { params: { q } }),
};
