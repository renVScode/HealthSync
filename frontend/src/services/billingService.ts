import api from './api';
import { Billing, PaginatedResult } from '../types';

export const billingService = {
  getAll: (params?: any) => api.get<PaginatedResult<Billing>>('/billings', { params }),
  getById: (id: string) => api.get<Billing>(`/billings/${id}`),
  create: (data: any) => api.post<Billing>('/billings', data),
  generateInvoiceFromVisit: (appointmentId: string) => api.post<Billing>(`/billings/from-visit/${appointmentId}`),
  addPayment: (id: string, data: any) => api.post(`/billings/${id}/payments`, data),
  getPayments: (id: string) => api.get(`/billings/${id}/payments`),
  verifyPayment: (paymentId: string) => api.post(`/billings/payments/${paymentId}/verify`),
  uploadQr: (file: File) => {
    const fd = new FormData();
    fd.append('file', file);
    return api.post('/billings/upload-qr', fd);
  },
  uploadQrCode: (billingId: string, paymentId: string, file: File) => {
    const fd = new FormData();
    fd.append('file', file);
    return api.post(`/billings/${billingId}/payments/${paymentId}/upload-qr`, fd);
  },
};
