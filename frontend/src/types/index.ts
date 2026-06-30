export enum AppointmentStatus {
  Scheduled = 0,
  Confirmed = 1,
  InProgress = 2,
  Completed = 3,
  Cancelled = 4,
  NoShow = 5,
}

export enum BillingStatus {
  Pending = 0,
  Paid = 1,
  PartiallyPaid = 2,
  Cancelled = 3,
  Refunded = 4,
}

export enum PaymentMethod {
  Cash = 0,
  Card = 1,
  Online = 2,
  Insurance = 3,
}

export enum UserRole {
  Admin = 0,
  Doctor = 1,
  Receptionist = 2,
  Pharmacist = 3,
}

export interface RegisterRequest {
  username: string;
  email: string;
  password: string;
  firstName: string;
  lastName: string;
  phoneNumber: string;
  role: string;
}

export interface UserInfo {
  id: string;
  firstName: string;
  lastName: string;
  email: string;
  phoneNumber?: string;
  role: string;
  isActive: boolean;
}

export interface LoginRequest {
  username: string;
  password: string;
}

export interface LoginResponse {
  accessToken: string;
  refreshToken: string;
  expiresAt: string;
  user: UserInfo;
}

export interface Patient {
  id: string;
  firstName: string;
  lastName: string;
  dateOfBirth: string;
  gender: string;
  phone: string;
  email?: string;
  address?: string;
  bloodType?: string;
  emergencyContact?: string;
  emergencyPhone?: string;
  medicalHistory: string;
  allergies?: string;
  createdAt: string;
}

export interface Doctor {
  id: string;
  userId: string;
  firstName: string;
  lastName: string;
  specialization: string;
  licenseNumber: string;
  phone?: string;
  email?: string;
  bio?: string;
  consultationFee: number;
  profileImageUrl?: string;
  licenseImageUrl?: string;
  isActive: boolean;
}

export interface Appointment {
  id: string;
  patientId: string;
  patientName: string;
  doctorId: string;
  doctorName: string;
  startTime: string;
  endTime: string;
  status: AppointmentStatus;
  token?: string;
  reason?: string;
  notes?: string;
  cancellationReason?: string;
  createdAt: string;
}

export interface CalendarEvent {
  id: string;
  title: string;
  patientName: string;
  doctorName: string;
  reason?: string;
  start: string;
  end: string;
  color?: string;
  status: AppointmentStatus;
  patientId: string;
  doctorId: string;
}

export interface MedicalRecord {
  id: string;
  patientId: string;
  patientName: string;
  doctorId: string;
  doctorName: string;
  appointmentId?: string;
  diagnosis: string;
  symptoms?: string;
  treatment?: string;
  notes?: string;
  isConfidential: boolean;
  createdAt: string;
  updatedAt: string;
  prescriptions: Prescription[];
}

export enum PrescriptionStatus {
  Pending = 0,
  Paid = 1,
  Completed = 2,
}

export interface Prescription {
  id: string;
  medicalRecordId: string;
  medicineId: string;
  medicineName: string;
  dosage: string;
  frequency: string;
  duration?: string;
  instructions?: string;
  quantity: number;
  status: string;
  dispensedBy?: string;
  dispensedAt?: string;
  inventoryBatchId?: string;
}

export interface Billing {
  id: string;
  patientId: string;
  patientName: string;
  appointmentId?: string;
  invoiceNumber: string;
  subTotal: number;
  discount: number;
  tax: number;
  total: number;
  amountPaid: number;
  balance: number;
  status: BillingStatus;
  createdAt: string;
  items: BillingItem[];
  payments: Payment[];
}

export interface BillingItem {
  id: string;
  description: string;
  quantity: number;
  unitPrice: number;
  total: number;
}

export interface Payment {
  id: string;
  amount: number;
  paymentMethod: PaymentMethod;
  transactionReference?: string;
  qrCodeImageUrl?: string;
  isVerified: boolean;
  receivedBy?: string;
  receivedAt: string;
}

export interface Medicine {
  id: string;
  name: string;
  genericName?: string;
  category?: string;
  manufacturer?: string;
  unit: string;
  price: number;
  reorderLevel: number;
  currentStock: number;
  description?: string;
  isActive: boolean;
}

export interface InventoryBatch {
  id: string;
  medicineId: string;
  medicineName: string;
  batchNumber: string;
  quantity: number;
  unitPrice: number;
  expiryDate?: string;
  supplier?: string;
  isLowStock: boolean;
}

export interface PaginatedResult<T> {
  items: T[];
  totalCount: number;
  page: number;
  pageSize: number;
}

export interface AvailableSlot {
  startTime: string;
  endTime: string;
  isAvailable: boolean;
}

export interface AuditLog {
  id: string;
  action: string;
  entityType: string;
  entityId?: string;
  oldValues?: string;
  newValues?: string;
  ipAddress?: string;
  userAgent?: string;
  createdAt: string;
  userId?: string;
  user?: { firstName: string; lastName: string; role: string };
}
