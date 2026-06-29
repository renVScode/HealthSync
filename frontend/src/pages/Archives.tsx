import { useState, useEffect } from 'react';
import { Card } from '../components/common/Card';
import { DataTable } from '../components/common/DataTable';
import { ConfirmDialog } from '../components/common/ConfirmDialog';
import { authService } from '../services/authService';
import { patientService } from '../services/patientService';
import { PAGE_SIZE } from '../utils/constants';

type Tab = 'users' | 'patients';

export function Archives() {
  const [activeTab, setActiveTab] = useState<Tab>('users');

  const tabs: { key: Tab; label: string }[] = [
    { key: 'users', label: 'Users' },
    { key: 'patients', label: 'Patients' },
  ];

  return (
    <div>
      <Card title="Archives">
        <div className="flex gap-1 border-b border-[#E9ECEF] mb-4">
          {tabs.map((t) => (
            <button
              key={t.key}
              onClick={() => setActiveTab(t.key)}
              className={`px-4 py-2 text-sm font-medium border-b-2 transition-colors ${
                activeTab === t.key
                  ? 'border-[#2C7DA0] text-[#2C7DA0]'
                  : 'border-transparent text-[#6C757D] hover:text-[#212529]'
              }`}
            >
              {t.label}
            </button>
          ))}
        </div>

        {activeTab === 'users' && <ArchivedUsers />}
        {activeTab === 'patients' && <ArchivedPatients />}
      </Card>
    </div>
  );
}

function ArchivedUsers() {
  const [items, setItems] = useState<any[]>([]);
  const [loading, setLoading] = useState(true);
  const [confirmAction, setConfirmAction] = useState<(() => void) | null>(null);

  const load = async () => {
    setLoading(true);
    const res = await authService.getAllUsers(1, 100, undefined, true);
    const data = res.data;
    setItems(data?.items || data || []);
    setLoading(false);
  };

  useEffect(() => { load(); }, []);

  const columns = [
    { key: 'firstName', header: 'First Name' },
    { key: 'lastName', header: 'Last Name' },
    { key: 'email', header: 'Email' },
    { key: 'role', header: 'Role' },
    { key: 'actions', header: 'Actions', render: (u: any) => (
      <button
        onClick={() => setConfirmAction(() => async () => { await authService.restore(u.id); await load(); })}
        className="p-1.5 rounded bg-[#28A745] text-white hover:bg-[#1E7E34]" title="Restore"
      >
        <svg xmlns="http://www.w3.org/2000/svg" className="h-4 w-4" viewBox="0 0 24 24" fill="none" stroke="currentColor" strokeWidth="2"><polyline points="1 4 1 10 7 10" /><path d="M3.51 15a9 9 0 1 0 2.13-9.36L1 10" /></svg>
      </button>
    )},
  ];

  return (
    <>
      <DataTable columns={columns} data={items} isLoading={loading} />
      <ConfirmDialog
        isOpen={!!confirmAction}
        title="Confirm Restore"
        message="Are you sure you want to restore this user?"
        confirmLabel="Restore"
        onConfirm={() => { confirmAction?.(); setConfirmAction(null); }}
        onCancel={() => setConfirmAction(null)}
      />
    </>
  );
}

function ArchivedPatients() {
  const [items, setItems] = useState<any[]>([]);
  const [page, setPage] = useState(1);
  const [total, setTotal] = useState(0);
  const [loading, setLoading] = useState(true);
  const [confirmAction, setConfirmAction] = useState<(() => void) | null>(null);

  const load = async () => {
    setLoading(true);
    const res = await patientService.getAll(page, PAGE_SIZE, undefined, true);
    const data = res.data;
    if (Array.isArray(data)) { setItems(data); setTotal(0); }
    else if (data?.items) { setItems(data.items); setTotal(data.totalCount); }
    setLoading(false);
  };

  useEffect(() => { load(); }, [page]);

  const columns = [
    { key: 'name', header: 'Name', render: (p: any) => `${p.firstName} ${p.lastName}` },
    { key: 'phone', header: 'Phone' },
    { key: 'gender', header: 'Gender' },
    { key: 'actions', header: 'Actions', render: (p: any) => (
      <button
        onClick={() => setConfirmAction(() => async () => { await patientService.restore(p.id); await load(); })}
        className="p-1.5 rounded bg-[#28A745] text-white hover:bg-[#1E7E34]" title="Restore"
      >
        <svg xmlns="http://www.w3.org/2000/svg" className="h-4 w-4" viewBox="0 0 24 24" fill="none" stroke="currentColor" strokeWidth="2"><polyline points="1 4 1 10 7 10" /><path d="M3.51 15a9 9 0 1 0 2.13-9.36L1 10" /></svg>
      </button>
    )},
  ];

  return (
    <>
      <DataTable
        columns={columns}
        data={items}
        page={page}
        totalPages={total > 0 ? Math.ceil(total / PAGE_SIZE) : undefined}
        onPageChange={setPage}
        isLoading={loading}
      />
      <ConfirmDialog
        isOpen={!!confirmAction}
        title="Confirm Restore"
        message="Are you sure you want to restore this patient?"
        confirmLabel="Restore"
        onConfirm={() => { confirmAction?.(); setConfirmAction(null); }}
        onCancel={() => setConfirmAction(null)}
      />
    </>
  );
}


