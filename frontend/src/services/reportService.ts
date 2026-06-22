import api from './api';

export const reportService = {
  getAppointmentSummary: (from?: string, to?: string) =>
    api.get('/reports/appointment-summary', { params: { from, to } }),
  getRevenue: (from?: string, to?: string) => api.get('/reports/revenue', { params: { from, to } }),
  getDoctorPerformance: (from?: string, to?: string) =>
    api.get('/reports/doctor-performance', { params: { from, to } }),
  getInventorySummary: () => api.get('/reports/inventory-summary'),
  getPatientVisits: (from?: string, to?: string) =>
    api.get('/reports/patient-visits', { params: { from, to } }),
};
