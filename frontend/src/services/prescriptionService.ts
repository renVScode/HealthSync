import api from './api';
import { Prescription } from '../types';

export const prescriptionService = {
  getPharmacyQueue: (page = 1, pageSize = 25, search?: string) =>
    api.get<{ items: Prescription[]; totalCount: number }>('/prescriptions/pharmacy', { params: { page, pageSize, search } }),
  dispense: (id: string, data: { batchId: string; quantity: number }) =>
    api.post(`/prescriptions/${id}/dispense`, data),
};
