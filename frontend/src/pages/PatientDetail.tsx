import { useState, useEffect } from 'react';
import { useParams } from 'react-router-dom';
import { Card } from '../components/common/Card';
import { Button } from '../components/common/Button';
import { LoadingSpinner } from '../components/common/LoadingSpinner';
import { PatientForm } from '../components/domain-components';
import { patientService } from '../services/patientService';
import { medicalRecordService } from '../services/medicalRecordService';
import { useAuth } from '../contexts/auth-context';
import { formatDate, getAge } from '../utils/formatters';

export function PatientDetail() {
  const { id } = useParams<{ id: string }>();
  const { hasRole } = useAuth();
  const [patient, setPatient] = useState<any>(null);
  const [editing, setEditing] = useState(false);
  const [records, setRecords] = useState<any[]>([]);
  const [expandedId, setExpandedId] = useState<string | null>(null);

  useEffect(() => {
    patientService.getById(id!).then((res) => setPatient(res.data));
  }, [id]);

  useEffect(() => {
    if (id && (hasRole('Admin') || hasRole('Doctor') || hasRole('Receptionist'))) {
      medicalRecordService.getByPatient(id).then((res) => {
        const data = res.data;
        setRecords(Array.isArray(data) ? data : []);
      });
    }
  }, [id, hasRole]);

  if (!patient) return <LoadingSpinner />;

  const canViewHistory = hasRole('Admin') || hasRole('Doctor') || hasRole('Receptionist');

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

      {canViewHistory && (
        <Card title="Medical History">
          {records.length === 0 ? (
            <p className="text-sm text-[#6C757D] py-4 text-center">No medical records found for this patient</p>
          ) : (
            <div className="relative pl-8 space-y-6">
              {records.map((record) => (
                <div key={record.id} className="relative">
                  <div className="absolute -left-8 top-1 w-4 h-4 rounded-full border-2 border-[#2C7DA0] bg-white" />
                  <div className="border border-[#E9ECEF] rounded-lg p-4 hover:shadow-sm transition-shadow cursor-pointer"
                    onClick={() => setExpandedId(expandedId === record.id ? null : record.id)}
                  >
                    <div className="flex items-center justify-between text-sm">
                      <div>
                        <span className="font-semibold text-[#212529]">{record.diagnosis}</span>
                        <span className="text-[#6C757D] mx-2">·</span>
                        <span className="text-[#6C757D]">{record.doctorName}</span>
                      </div>
                      <div className="flex items-center gap-2">
                        <span className="text-xs text-[#6C757D]">{formatDate(record.createdAt)}</span>
                        <svg className={`h-4 w-4 text-[#6C757D] transition-transform ${expandedId === record.id ? 'rotate-180' : ''}`} viewBox="0 0 24 24" fill="none" stroke="currentColor" strokeWidth="2"><polyline points="6 9 12 15 18 9" /></svg>
                      </div>
                    </div>

                    {expandedId === record.id && (
                      <div className="mt-4 pt-3 border-t border-[#E9ECEF] space-y-3 text-sm">
                        <div>
                          <span className="block text-xs font-semibold text-[#6C757D] uppercase tracking-wider mb-1">Diagnosis</span>
                          <p className="bg-[#F8F9FA] p-3 rounded">{record.diagnosis}</p>
                        </div>
                        {record.symptoms && (
                          <div>
                            <span className="block text-xs font-semibold text-[#6C757D] uppercase tracking-wider mb-1">Symptoms</span>
                            <p className="bg-[#F8F9FA] p-3 rounded">{record.symptoms}</p>
                          </div>
                        )}
                        {record.treatment && (
                          <div>
                            <span className="block text-xs font-semibold text-[#6C757D] uppercase tracking-wider mb-1">Treatment</span>
                            <p className="bg-[#F8F9FA] p-3 rounded">{record.treatment}</p>
                          </div>
                        )}
                        {record.notes && (
                          <div>
                            <span className="block text-xs font-semibold text-[#6C757D] uppercase tracking-wider mb-1">Notes</span>
                            <p className="bg-[#F8F9FA] p-3 rounded">{record.notes}</p>
                          </div>
                        )}
                        {record.prescriptions?.length > 0 && (
                          <div>
                            <span className="block text-xs font-semibold text-[#6C757D] uppercase tracking-wider mb-1">Prescriptions</span>
                            {record.prescriptions.map((px: any) => (
                              <div key={px.id} className="flex items-center justify-between p-2 bg-[#F8F9FA] rounded mb-1">
                                <span className="font-medium">{px.medicineName}</span>
                                <span className="text-[#6C757D]">{px.dosage} - {px.frequency}{px.duration ? ` for ${px.duration}` : ''} ({px.quantity} {px.quantity > 1 ? 'units' : 'unit'})</span>
                                <span className={`text-xs font-semibold px-2 py-0.5 rounded ${px.status === 'Completed' ? 'bg-[#D4EDDA] text-[#155724]' : px.status === 'Paid' ? 'bg-[#FFF3CD] text-[#856404]' : 'bg-[#E2E3E5] text-[#383D41]'}`}>
                                  {px.status}
                                </span>
                              </div>
                            ))}
                          </div>
                        )}
                      </div>
                    )}
                  </div>
                </div>
              ))}
            </div>
          )}
        </Card>
      )}
    </div>
  );
}
