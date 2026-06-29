import api from './api';
import { MedicalRecord } from '../types';

export const medicalRecordService = {
  getAll: (page = 1, pageSize = 25, isArchived?: boolean) =>
    api.get<any>('/medical-records', { params: { page, pageSize, isArchived } }),
  getByPatient: (patientId: string) => api.get<MedicalRecord[]>(`/medical-records/patient/${patientId}`),
  getById: (id: string) => api.get<MedicalRecord>(`/medical-records/${id}`),
  create: (data: any) => api.post<MedicalRecord>('/medical-records', data),
  update: (id: string, data: any) => api.put<MedicalRecord>(`/medical-records/${id}`, data),
  getPrescriptions: (id: string) => api.get(`/medical-records/${id}/prescriptions`),
  addPrescription: (id: string, data: any) => api.post(`/medical-records/${id}/prescriptions`, data),
  addPrescriptions: (id: string, data: any[]) => api.post(`/medical-records/${id}/prescriptions/batch`, data),
  markPrescriptionsPaid: (medicalRecordId: string) => api.post(`/medical-records/${medicalRecordId}/prescriptions/mark-paid`),
  archive: (id: string) => api.patch(`/medical-records/${id}/archive`),
  restore: (id: string) => api.patch(`/medical-records/${id}/restore`),
};
