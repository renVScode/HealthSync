import api from './api';
import { AuditLog } from '../types';

export const auditService = {
  getAll: (params?: any) => api.get<{ items: AuditLog[]; totalCount: number }>('/audit-logs', { params }),
};
