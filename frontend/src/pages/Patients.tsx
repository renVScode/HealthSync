import { useState, useEffect } from 'react';
import { useNavigate } from 'react-router-dom';
import { Card } from '../components/common/Card';
import { Button } from '../components/common/Button';
import { DataTable } from '../components/common/DataTable';
import { SearchBar } from '../components/common/SearchBar';
import { Modal } from '../components/common/Modal';
import { ConfirmDialog } from '../components/common/ConfirmDialog';
import { PatientForm } from '../components/domain-components';
import { patientService } from '../services/patientService';
import { doctorService } from '../services/doctorService';
import { useAuth } from '../contexts/auth-context';
import { useDebounce } from '../hooks/useDebounce';
import { formatDate, getAge } from '../utils/formatters';
import { PAGE_SIZE } from '../utils/constants';

export function Patients() {
  const navigate = useNavigate();
  const { user, hasRole } = useAuth();
  const [patients, setPatients] = useState<any[]>([]);
  const [search, setSearch] = useState('');
  const [page, setPage] = useState(1);
  const [total, setTotal] = useState(0);
  const [showModal, setShowModal] = useState(false);
  const [doctorId, setDoctorId] = useState<string | null>(null);
  const [confirmAction, setConfirmAction] = useState<(() => void) | null>(null);
  const debouncedSearch = useDebounce(search);

  const canEdit = hasRole('Receptionist');
  const canArchive = hasRole('Admin') || hasRole('Receptionist');
  const isDoctor = user?.role === 'Doctor';

  useEffect(() => {
    if (isDoctor && !doctorId) {
      doctorService.getAll().then((res) => {
        const data = res.data;
        const doctors = Array.isArray(data) ? data : data.items || [];
        const myDoctor = doctors.find((d: any) => d.userId === user?.id);
        if (myDoctor) setDoctorId(myDoctor.id);
      });
    }
  }, [isDoctor, doctorId, user?.id]);

  useEffect(() => {
    if (isDoctor && doctorId) {
      patientService.getByDoctor(doctorId, page, PAGE_SIZE, debouncedSearch).then((res) => {
        setPatients(res.data.items);
        setTotal(res.data.totalCount);
      });
    } else if (!isDoctor) {
      patientService.getAll(page, PAGE_SIZE, debouncedSearch, false).then((res) => {
        setPatients(res.data.items);
        setTotal(res.data.totalCount);
      });
    }
  }, [page, debouncedSearch, isDoctor, doctorId]);

  const columns = [
    { key: 'name', header: 'Name', render: (p: any) => `${p.firstName} ${p.lastName}` },
    { key: 'phone', header: 'Phone' },
    { key: 'gender', header: 'Gender' },
    { key: 'age', header: 'Age', render: (p: any) => getAge(p.dateOfBirth) },
    { key: 'bloodType', header: 'Blood Type' },
    { key: 'createdAt', header: 'Registered', render: (p: any) => formatDate(p.createdAt) },
    ...(canArchive ? [{ key: 'actions', header: 'Actions', render: (p: any) => (
      <button
        onClick={(e: React.MouseEvent) => {
          e.stopPropagation();
          setConfirmAction(() => async () => {
            await patientService.archive(p.id);
            const res = await patientService.getAll(page, PAGE_SIZE, debouncedSearch, false);
            setPatients(res.data.items);
            setTotal(res.data.totalCount);
          });
        }}
        className="p-1.5 rounded bg-[#FFC107] text-black hover:bg-[#E0A800]" title="Archive"
      >
        <svg xmlns="http://www.w3.org/2000/svg" className="h-4 w-4" viewBox="0 0 24 24" fill="none" stroke="currentColor" strokeWidth="2"><polyline points="21 8 21 21 3 21 3 8" /><rect x="1" y="3" width="22" height="5" /><line x1="10" y1="12" x2="14" y2="12" /></svg>
      </button>
    ) }] : []),
  ];

  return (
    <div>
      <Card title="Patients" actions={
        canEdit && <Button onClick={() => setShowModal(true)}>+ New Patient</Button>
      }>
        <div className="mb-4">
          <SearchBar value={search} onChange={(v) => { setSearch(v); setPage(1); }} placeholder="Search by name or phone..." />
        </div>
        <DataTable columns={columns} data={patients} page={page}
          totalPages={Math.ceil(total / PAGE_SIZE)} onPageChange={setPage}
          onRowClick={(p) => navigate(`/patients/${p.id}`)} />
      </Card>
      <Modal isOpen={showModal} onClose={() => setShowModal(false)} title="Register New Patient" size="lg">
        <PatientForm onSubmit={async (data) => {
          await patientService.create(data);
          setShowModal(false);
          const res = await patientService.getAll(1, PAGE_SIZE, debouncedSearch, false);
          setPatients(res.data.items);
        }} />
      </Modal>

      <ConfirmDialog
        isOpen={!!confirmAction}
        title="Confirm Archive"
        message="Are you sure you want to archive this patient?"
        confirmLabel="Archive"
        onConfirm={() => { confirmAction?.(); setConfirmAction(null); }}
        onCancel={() => setConfirmAction(null)}
      />
    </div>
  );
}