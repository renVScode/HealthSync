import { Routes, Route, Navigate } from 'react-router-dom';
import { MainLayout } from './components/layout/MainLayout';
import { ProtectedRoute } from './components/auth/ProtectedRoute';
import { useAuth } from './contexts/auth-context';
import { LoadingSpinner } from './components/common/LoadingSpinner';
import { Login } from './pages/Login';
import { Landing } from './pages/Landing';
import { Dashboard } from './pages/Dashboard';
import { Patients } from './pages/Patients';
import { PatientDetail } from './pages/PatientDetail';
import { Doctors } from './pages/Doctors';
import { DoctorDetail } from './pages/DoctorDetail';
import { Appointments } from './pages/Appointments';
import { MedicalRecords } from './pages/MedicalRecords';
import { Billings } from './pages/Billings';
import { Inventory } from './pages/Inventory';
import { Reports } from './pages/Reports';
import { Users } from './pages/Users';
import { Settings } from './pages/Settings';
import { NotFound } from './pages/NotFound';

export function AppRoutes() {
  const { isAuthenticated, isLoading } = useAuth();

  if (isLoading) return <LoadingSpinner />;

  return (
    <Routes>
      <Route path="/" element={isAuthenticated ? <Navigate to="/dashboard" replace /> : <Landing />} />
      <Route path="/login" element={isAuthenticated ? <Navigate to="/dashboard" replace /> : <Login />} />
      <Route element={<ProtectedRoute><MainLayout /></ProtectedRoute>}>
        <Route path="dashboard" element={<Dashboard />} />
        <Route path="patients" element={<ProtectedRoute roles={['Admin', 'Doctor', 'Receptionist']}><Patients /></ProtectedRoute>} />
        <Route path="patients/:id" element={<ProtectedRoute roles={['Admin', 'Doctor', 'Receptionist']}><PatientDetail /></ProtectedRoute>} />
        <Route path="doctors" element={<ProtectedRoute roles={['Admin', 'Doctor', 'Receptionist']}><Doctors /></ProtectedRoute>} />
        <Route path="doctors/:id" element={<ProtectedRoute roles={['Admin', 'Doctor']}><DoctorDetail /></ProtectedRoute>} />
        <Route path="appointments" element={<Appointments />} />
        <Route path="medical-records" element={<MedicalRecords />} />
        <Route path="billings" element={<ProtectedRoute roles={['Admin', 'Receptionist']}><Billings /></ProtectedRoute>} />
        <Route path="inventory" element={<ProtectedRoute roles={['Admin', 'Pharmacist']}><Inventory /></ProtectedRoute>} />
        <Route path="reports" element={<Reports />} />
        <Route path="users" element={<ProtectedRoute roles={['Admin']}><Users /></ProtectedRoute>} />
        <Route path="settings" element={<ProtectedRoute roles={['Admin']}><Settings /></ProtectedRoute>} />
        <Route path="*" element={<NotFound />} />
      </Route>
    </Routes>
  );
}
