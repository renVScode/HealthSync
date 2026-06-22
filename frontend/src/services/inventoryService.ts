import api from './api';
import { InventoryBatch } from '../types';

export const inventoryService = {
  getAll: (medicineId?: string, expiringSoon?: boolean) =>
    api.get<InventoryBatch[]>('/inventory', { params: { medicineId, expiringSoon } }),
  getById: (id: string) => api.get<InventoryBatch>(`/inventory/${id}`),
  addBatch: (data: any) => api.post<InventoryBatch>('/inventory', data),
  dispense: (id: string, data: any) => api.post(`/inventory/${id}/dispense`, data),
  getLowStock: () => api.get('/inventory/low-stock'),
  getTransactions: (from?: string, to?: string) => api.get('/inventory/transactions', { params: { from, to } }),
};
