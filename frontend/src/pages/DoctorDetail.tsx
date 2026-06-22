import { useState, useEffect } from 'react';
import { useParams } from 'react-router-dom';
import { Card } from '../components/common/Card';
import { LoadingSpinner } from '../components/common/LoadingSpinner';
import { doctorService } from '../services/doctorService';
import { formatCurrency } from '../utils/formatters';

export function DoctorDetail() {
  const { id } = useParams<{ id: string }>();
  const [doctor, setDoctor] = useState<any>(null);

  useEffect(() => {
    doctorService.getById(id!).then((res) => setDoctor(res.data));
  }, [id]);

  if (!doctor) return <LoadingSpinner />;

  return (
    <div className="space-y-6">
      <Card title="Doctor Information">
        <div className="grid grid-cols-2 gap-4">
          <div><label className="text-sm text-[#6C757D]">Name</label>
            <p className="font-medium">Dr. {doctor.firstName} {doctor.lastName}</p></div>
          <div><label className="text-sm text-[#6C757D]">Specialization</label>
            <p>{doctor.specialization}</p></div>
          <div><label className="text-sm text-[#6C757D]">License Number</label>
            <p>{doctor.licenseNumber}</p></div>
          <div><label className="text-sm text-[#6C757D]">Phone</label>
            <p>{doctor.phone || '—'}</p></div>
          <div><label className="text-sm text-[#6C757D]">Email</label>
            <p>{doctor.email || '—'}</p></div>
          <div><label className="text-sm text-[#6C757D]">Consultation Fee</label>
            <p>{formatCurrency(doctor.consultationFee)}</p></div>
        </div>
        {doctor.bio && (
          <div className="mt-4">
            <label className="text-sm text-[#6C757D]">Bio</label>
            <p className="mt-1">{doctor.bio}</p>
          </div>
        )}
      </Card>
    </div>
  );
}
