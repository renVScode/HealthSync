import api from './api';

export const labTestService = {
  // Catalog
  getAll: (page?: number, pageSize?: number, search?: string) =>
    api.get('/labtests', { params: { page, pageSize, search } }),
  getById: (id: string) => api.get(`/labtests/${id}`),
  create: (data: any) => api.post('/labtests', data),
  update: (id: string, data: any) => api.put(`/labtests/${id}`, data),
  delete: (id: string) => api.delete(`/labtests/${id}`),

  // Orders
  getOrders: (params?: any) => api.get('/labtests/orders', { params }),
  getOrderById: (id: string) => api.get(`/labtests/orders/${id}`),
  createOrder: (data: any) => api.post('/labtests/orders', data),
  updateOrder: (id: string, data: any) => api.put(`/labtests/orders/${id}`, data),
  deleteOrder: (id: string) => api.delete(`/labtests/orders/${id}`),
};
