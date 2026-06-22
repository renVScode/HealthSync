import { useState, useEffect } from 'react';
import { useNavigate } from 'react-router-dom';
import { Card } from '../components/common/Card';
import { DataTable } from '../components/common/DataTable';
import { doctorService } from '../services/doctorService';

export function Doctors() {
  const navigate = useNavigate();
  const [doctors, setDoctors] = useState<any[]>([]);

  useEffect(() => {
    doctorService.getAll().then((res) => setDoctors(res.data));
  }, []);

  const columns = [
    { key: 'name', header: 'Name', render: (d: any) => `Dr. ${d.firstName} ${d.lastName}` },
    { key: 'specialization', header: 'Specialization' },
    { key: 'phone', header: 'Phone' },
    { key: 'consultationFee', header: 'Fee', render: (d: any) => `₱${d.consultationFee}` },
  ];

  return (
    <div>
      <Card title="Doctors">
        <DataTable columns={columns} data={doctors}
          onRowClick={(d) => navigate(`/doctors/${d.id}`)} />
      </Card>
    </div>
  );
}
