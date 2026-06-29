import api from './api';
import { InventoryBatch } from '../types';

export const inventoryService = {
  getAll: (page?: number, pageSize?: number, search?: string, medicineId?: string, expiringSoon?: boolean) =>
    api.get<any>('/inventory', { params: { page, pageSize, search, medicineId, expiringSoon } }),
  getById: (id: string) => api.get<InventoryBatch>(`/inventory/${id}`),
  addBatch: (data: any) => api.post<InventoryBatch>('/inventory', data),
  dispense: (id: string, data: any) => api.post(`/inventory/${id}/dispense`, data),
  getLowStock: () => api.get('/inventory/low-stock'),
  getTransactions: (from?: string, to?: string) => api.get('/inventory/transactions', { params: { from, to } }),
  archive: (id: string) => api.patch(`/inventory/${id}/archive`),
  restore: (id: string) => api.patch(`/inventory/${id}/restore`),
};
