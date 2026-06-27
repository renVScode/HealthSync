import { useState, useEffect } from 'react';
import { Card } from '../components/common/Card';
import { Button } from '../components/common/Button';
import { DataTable } from '../components/common/DataTable';
import { SearchBar } from '../components/common/SearchBar';
import { Modal } from '../components/common/Modal';
import { authService } from '../services/authService';
import { useDebounce } from '../hooks/useDebounce';
import { PAGE_SIZE } from '../utils/constants';

export function Users() {
  const [users, setUsers] = useState<any[]>([]);
  const [search, setSearch] = useState('');
  const [page, setPage] = useState(1);
  const [total, setTotal] = useState(0);
  const [showModal, setShowModal] = useState(false);
  const [submitting, setSubmitting] = useState(false);
  const [form, setForm] = useState({ firstName: '', lastName: '', email: '', password: '', role: 'Doctor' });
  const debouncedSearch = useDebounce(search);

  const loadUsers = async () => {
    const res = await authService.getAllUsers(page, PAGE_SIZE, debouncedSearch || undefined);
    const data = res.data;
    if (Array.isArray(data)) {
      setUsers(data);
      setTotal(0);
    } else if (data?.items) {
      setUsers(data.items);
      setTotal(data.totalCount);
    }
  };

  useEffect(() => { loadUsers(); }, [page, debouncedSearch]);

  const openCreateModal = () => {
    setForm({ firstName: '', lastName: '', email: '', password: '', role: 'Doctor' });
    setShowModal(true);
  };

  const handleCreate = async () => {
    if (!form.firstName.trim() || !form.lastName.trim() || !form.email.trim() || !form.password.trim()) return;
    setSubmitting(true);
    try {
      const username = form.email.split('@')[0];
      await authService.register({ ...form, username, phoneNumber: '' });
      setShowModal(false);
      await loadUsers();
    } finally {
      setSubmitting(false);
    }
  };

  const columns = [
    { key: 'firstName', header: 'First Name' },
    { key: 'lastName', header: 'Last Name' },
    { key: 'email', header: 'Email' },
    { key: 'role', header: 'Role' },
    { key: 'isActive', header: 'Active', render: (u: any) => u.isActive ? 'Yes' : 'No' },
  ];

  return (
    <div>
      <Card title="Users" actions={<Button onClick={openCreateModal}>+ New User</Button>}>
        <div className="mb-4">
          <SearchBar value={search} onChange={(v) => { setSearch(v); setPage(1); }} placeholder="Search by name, username, or email..." />
        </div>
        <DataTable
          columns={columns}
          data={users}
          page={page}
          totalPages={total > 0 ? Math.ceil(total / PAGE_SIZE) : undefined}
          onPageChange={setPage}
        />
      </Card>

      <Modal isOpen={showModal} onClose={() => setShowModal(false)} title="Create User" size="sm"
        footer={
          <div className="flex gap-2 justify-end">
            <Button variant="secondary" size="sm" onClick={() => setShowModal(false)}>Cancel</Button>
            <Button size="sm" onClick={handleCreate} disabled={submitting || !form.firstName.trim() || !form.lastName.trim() || !form.email.trim() || !form.password.trim()}>
              {submitting ? 'Creating...' : 'Create'}
            </Button>
          </div>
        }
      >
        <div className="space-y-4">
          <div>
            <label className="block text-xs font-semibold text-[#6C757D] uppercase tracking-wider mb-1">First Name *</label>
            <input className="w-full border border-[#CED4DA] rounded-lg p-2 text-sm" value={form.firstName} onChange={(e) => setForm({ ...form, firstName: e.target.value })} />
          </div>
          <div>
            <label className="block text-xs font-semibold text-[#6C757D] uppercase tracking-wider mb-1">Last Name *</label>
            <input className="w-full border border-[#CED4DA] rounded-lg p-2 text-sm" value={form.lastName} onChange={(e) => setForm({ ...form, lastName: e.target.value })} />
          </div>
          <div>
            <label className="block text-xs font-semibold text-[#6C757D] uppercase tracking-wider mb-1">Email *</label>
            <input type="email" className="w-full border border-[#CED4DA] rounded-lg p-2 text-sm" value={form.email} onChange={(e) => setForm({ ...form, email: e.target.value })} />
          </div>
          <div>
            <label className="block text-xs font-semibold text-[#6C757D] uppercase tracking-wider mb-1">Password *</label>
            <input type="password" className="w-full border border-[#CED4DA] rounded-lg p-2 text-sm" value={form.password} onChange={(e) => setForm({ ...form, password: e.target.value })} />
          </div>
          <div>
            <label className="block text-xs font-semibold text-[#6C757D] uppercase tracking-wider mb-1">Role *</label>
            <select className="w-full border border-[#CED4DA] rounded-lg p-2 text-sm" value={form.role} onChange={(e) => setForm({ ...form, role: e.target.value })}>
              <option value="Doctor">Doctor</option>
              <option value="Receptionist">Receptionist</option>
              <option value="Pharmacist">Pharmacist</option>
              <option value="Admin">Admin</option>
            </select>
          </div>
        </div>
      </Modal>
    </div>
  );
}