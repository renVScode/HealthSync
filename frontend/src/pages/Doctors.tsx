import { useState, useEffect, useCallback } from 'react';
import { useNavigate } from 'react-router-dom';
import { Card } from '../components/common/Card';
import { DataTable } from '../components/common/DataTable';
import { SearchBar } from '../components/common/SearchBar';
import { Modal } from '../components/common/Modal';
import { LoadingSpinner } from '../components/common/LoadingSpinner';
import { doctorService } from '../services/doctorService';
import { useDebounce } from '../hooks/useDebounce';
import { useAuth } from '../contexts/auth-context';
import { formatCurrency } from '../utils/formatters';
import { PAGE_SIZE } from '../utils/constants';

export function Doctors() {
  const navigate = useNavigate();
  const { user, hasRole } = useAuth();
  const [doctors, setDoctors] = useState<any[]>([]);
  const [search, setSearch] = useState('');
  const [page, setPage] = useState(1);
  const [total, setTotal] = useState(0);
  const [previewImg, setPreviewImg] = useState('');
  const [redirecting, setRedirecting] = useState(false);
  const debouncedSearch = useDebounce(search);

  const loadDoctors = useCallback(async () => {
    const res = await doctorService.getAll(page, PAGE_SIZE, debouncedSearch || undefined);
    const data = res.data;
    if (data && Array.isArray(data)) {
      setDoctors(data);
      setTotal(0);
    } else if (data?.items) {
      setDoctors(data.items);
      setTotal(data.totalCount);
    }
  }, [page, debouncedSearch]);

  useEffect(() => {
    if (user?.role === 'Doctor') {
      setRedirecting(true);
      doctorService.getMyProfile().then((res) => {
        navigate(`/doctors/${res.data.id}`, { replace: true });
      }).catch(() => {
        setRedirecting(false);
        loadDoctors();
      });
      return;
    }
    loadDoctors();
  }, [loadDoctors, user, navigate]);

  if (redirecting) return <LoadingSpinner />;

  const handleToggleActive = async (d: any, e: React.MouseEvent) => {
    e.stopPropagation();
    await doctorService.update(d.id, { isActive: !d.isActive });
    await loadDoctors();
  };

  const columns = [
    { key: 'name', header: 'Name', render: (d: any) => (
      <div className="flex items-center gap-2">
        <div className="w-8 h-8 rounded-full bg-[#2C7DA0] flex items-center justify-center text-white text-xs font-bold overflow-hidden shrink-0">
          {d.profileImageUrl ? (
            <img src={d.profileImageUrl} alt="" className="w-full h-full object-cover" />
          ) : (
            `${d.firstName[0]}${d.lastName[0]}`
          )}
        </div>
        <span>Dr. {d.firstName} {d.lastName}</span>
      </div>
    )},
    { key: 'specialization', header: 'Specialization' },
    { key: 'licenseNumber', header: 'License', render: (d: any) => (
      <div className="flex items-center gap-2">
        <span>{d.licenseNumber}</span>
        {d.licenseImageUrl && (
          <img src={d.licenseImageUrl} alt="License"
            className="w-7 h-7 object-cover border rounded cursor-pointer hover:opacity-75"
            onClick={(e) => { e.stopPropagation(); setPreviewImg(d.licenseImageUrl); }} />
        )}
      </div>
    )},
    { key: 'phone', header: 'Phone' },
    { key: 'consultationFee', header: 'Fee', render: (d: any) => formatCurrency(d.consultationFee) },
    ...(hasRole('Admin') ? [{
      key: 'isActive' as const, header: 'Active', render: (d: any) => (
        <button onClick={(e) => handleToggleActive(d, e)}
          className={`text-xs font-semibold ${d.isActive ? 'text-[#155724]' : 'text-[#721C24]'}`}
        >
          {d.isActive ? 'Active' : 'Inactive'}
        </button>
      )
    }] : []),
  ];

  const handleRowClick = (d: any) => navigate(`/doctors/${d.id}`);

  return (
    <div>
      <Card title="Doctors">
        <div className="mb-4">
          <SearchBar value={search} onChange={(v) => { setSearch(v); setPage(1); }} placeholder="Search by name, specialization, or license..." />
        </div>
        <DataTable
          columns={columns}
          data={doctors}
          page={page}
          totalPages={total > 0 ? Math.ceil(total / PAGE_SIZE) : undefined}
          onPageChange={setPage}
          onRowClick={handleRowClick}
        />
      </Card>
      <Modal isOpen={!!previewImg} onClose={() => setPreviewImg('')} title="License Image" size="sm">
        {previewImg && <img src={previewImg} alt="License" className="w-full" />}
      </Modal>
    </div>
  );
}