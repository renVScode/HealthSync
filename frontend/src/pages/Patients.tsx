import { useState, useEffect } from 'react';
import { useNavigate } from 'react-router-dom';
import { Card } from '../components/common/Card';
import { Button } from '../components/common/Button';
import { DataTable } from '../components/common/DataTable';
import { SearchBar } from '../components/common/SearchBar';
import { Modal } from '../components/common/Modal';
import { PatientForm } from '../components/domain-components';
import { patientService } from '../services/patientService';
import { useAuth } from '../contexts/auth-context';
import { useDebounce } from '../hooks/useDebounce';
import { formatDate, getAge } from '../utils/formatters';

export function Patients() {
  const navigate = useNavigate();
  const { hasRole } = useAuth();
  const [patients, setPatients] = useState<any[]>([]);
  const [search, setSearch] = useState('');
  const [page, setPage] = useState(1);
  const [total, setTotal] = useState(0);
  const [showModal, setShowModal] = useState(false);
  const debouncedSearch = useDebounce(search);

  const canEdit = hasRole('Admin', 'Receptionist');

  useEffect(() => {
    patientService.getAll(page, 20, debouncedSearch).then((res) => {
      setPatients(res.data.items);
      setTotal(res.data.totalCount);
    });
  }, [page, debouncedSearch]);

  const columns = [
    { key: 'name', header: 'Name', render: (p: any) => `${p.firstName} ${p.lastName}` },
    { key: 'phone', header: 'Phone' },
    { key: 'gender', header: 'Gender' },
    { key: 'age', header: 'Age', render: (p: any) => getAge(p.dateOfBirth) },
    { key: 'bloodType', header: 'Blood Type' },
    { key: 'createdAt', header: 'Registered', render: (p: any) => formatDate(p.createdAt) },
  ];

  return (
    <div>
      <Card title="Patients" actions={
        canEdit && <Button onClick={() => setShowModal(true)}>+ New Patient</Button>
      }>
        <div className="mb-4">
          <SearchBar value={search} onChange={setSearch} placeholder="Search by name or phone..." />
        </div>
        <DataTable columns={columns} data={patients} page={page}
          totalPages={Math.ceil(total / 20)} onPageChange={setPage}
          onRowClick={(p) => navigate(`/patients/${p.id}`)} />
      </Card>
      <Modal isOpen={showModal} onClose={() => setShowModal(false)} title="Register New Patient" size="lg">
        <PatientForm onSubmit={async (data) => {
          await patientService.create(data);
          setShowModal(false);
          const res = await patientService.getAll(1, 20, debouncedSearch);
          setPatients(res.data.items);
        }} />
      </Modal>
    </div>
  );
}
