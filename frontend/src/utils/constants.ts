export const APP_NAME = 'HealthSync';
export const API_URL = import.meta.env.VITE_API_URL || 'http://localhost:5000/api';
export const PAGE_SIZE = 25;

export const STATUS_COLORS: Record<string, string> = {
  Scheduled: '#17A2B8',
  Confirmed: '#28A745',
  InProgress: '#FFC107',
  Completed: '#6C757D',
  Cancelled: '#DC3545',
  NoShow: '#DC3545',
  Pending: '#FFC107',
  Paid: '#28A745',
  PartiallyPaid: '#17A2B8',
  Refunded: '#6C757D',
}

export const ROLE_COLORS: Record<string, string> = {
  Admin: '#4A4E69',
  Doctor: '#3B82F6',
  Receptionist: '#F97316',
  Pharmacist: '#10B981',
};
