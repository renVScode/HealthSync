import api from './api';
import { Billing, PaginatedResult } from '../types';

export const billingService = {
  getAll: (params?: any) => api.get<PaginatedResult<Billing>>('/billings', { params }),
  getById: (id: string) => api.get<Billing>(`/billings/${id}`),
  create: (data: any) => api.post<Billing>('/billings', data),
  addPayment: (id: string, data: any) => api.post(`/billings/${id}/payments`, data),
  getPayments: (id: string) => api.get(`/billings/${id}/payments`),
};
