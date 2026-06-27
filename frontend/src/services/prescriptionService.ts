import api from './api';
import { Prescription } from '../types';

export const prescriptionService = {
  getPharmacyQueue: (page = 1, pageSize = 25) =>
    api.get<{ items: Prescription[]; totalCount: number }>('/prescriptions/pharmacy', { params: { page, pageSize } }),
  dispense: (id: string, data: { batchId: string; quantity: number }) =>
    api.post(`/prescriptions/${id}/dispense`, data),
};
