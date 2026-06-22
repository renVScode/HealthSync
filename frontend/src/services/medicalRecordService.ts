import api from './api';
import { MedicalRecord } from '../types';

export const medicalRecordService = {
  getByPatient: (patientId: string) => api.get<MedicalRecord[]>(`/medical-records/patient/${patientId}`),
  getById: (id: string) => api.get<MedicalRecord>(`/medical-records/${id}`),
  create: (data: any) => api.post<MedicalRecord>('/medical-records', data),
  update: (id: string, data: any) => api.put<MedicalRecord>(`/medical-records/${id}`, data),
  getPrescriptions: (id: string) => api.get(`/medical-records/${id}/prescriptions`),
  addPrescription: (id: string, data: any) => api.post(`/medical-records/${id}/prescriptions`, data),
};
