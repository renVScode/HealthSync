import { useState, useEffect } from 'react';
import { useParams } from 'react-router-dom';
import { Card } from '../components/common/Card';
import { Button } from '../components/common/Button';
import { LoadingSpinner } from '../components/common/LoadingSpinner';
import { PatientForm } from '../components/domain-components';
import { patientService } from '../services/patientService';
import { useAuth } from '../contexts/auth-context';
import { formatDate, getAge } from '../utils/formatters';

export function PatientDetail() {
  const { id } = useParams<{ id: string }>();
  const { hasRole } = useAuth();
  const [patient, setPatient] = useState<any>(null);
  const [editing, setEditing] = useState(false);

  useEffect(() => {
    patientService.getById(id!).then((res) => setPatient(res.data));
  }, [id]);

  if (!patient) return <LoadingSpinner />;

  return (
    <div className="space-y-6">
      <Card title="Patient Information" actions={
        hasRole('Receptionist') && (
        <Button onClick={() => setEditing(!editing)}>
          {editing ? 'Cancel' : 'Edit'}
        </Button>)
      }>
        {editing ? (
          <PatientForm initial={patient} onSubmit={async (data) => {
            await patientService.update(id!, data);
            const res = await patientService.getById(id!);
            setPatient(res.data);
            setEditing(false);
          }} />
        ) : (
          <div className="grid grid-cols-2 gap-4">
            <div><label className="text-sm text-[#6C757D]">Name</label>
              <p className="font-medium">{patient.firstName} {patient.lastName}</p></div>
            <div><label className="text-sm text-[#6C757D]">Date of Birth</label>
              <p>{formatDate(patient.dateOfBirth)} ({getAge(patient.dateOfBirth)} yrs)</p></div>
            <div><label className="text-sm text-[#6C757D]">Gender</label><p>{patient.gender}</p></div>
            <div><label className="text-sm text-[#6C757D]">Phone</label><p>{patient.phone}</p></div>
            <div><label className="text-sm text-[#6C757D]">Email</label><p>{patient.email || '—'}</p></div>
            <div><label className="text-sm text-[#6C757D]">Blood Type</label><p>{patient.bloodType || '—'}</p></div>
            <div><label className="text-sm text-[#6C757D]">Emergency Contact</label>
              <p>{patient.emergencyContact || '—'} {patient.emergencyPhone ? `(${patient.emergencyPhone})` : ''}</p></div>
          </div>
        )}
      </Card>
    </div>
  );
}
