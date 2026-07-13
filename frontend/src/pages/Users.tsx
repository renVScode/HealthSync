import { useState, useEffect } from 'react';
import { Card } from '../components/common/Card';
import { Button } from '../components/common/Button';
import { DataTable } from '../components/common/DataTable';
import { SearchBar } from '../components/common/SearchBar';
import { Modal } from '../components/common/Modal';
import { ConfirmDialog } from '../components/common/ConfirmDialog';
import { authService } from '../services/authService';
import { useDebounce } from '../hooks/useDebounce';
import { PAGE_SIZE } from '../utils/constants';

export function Users() {
  const [users, setUsers] = useState<any[]>([]);
  const [search, setSearch] = useState('');
  const [page, setPage] = useState(1);
  const [total, setTotal] = useState(0);
  const [showModal, setShowModal] = useState(false);
  const [showEditModal, setShowEditModal] = useState(false);
  const [editUser, setEditUser] = useState<any>(null);
  const [submitting, setSubmitting] = useState(false);
  const [showPassword, setShowPassword] = useState(false);
  const [showConfirm, setShowConfirm] = useState(false);
  const [form, setForm] = useState({ firstName: '', lastName: '', username: '', email: '', password: '', confirmPassword: '', role: 'Doctor' });
  const [editForm, setEditForm] = useState({ firstName: '', lastName: '', username: '', email: '', role: '', password: '', confirmPassword: '' });
  const [changePassword, setChangePassword] = useState(false);
  const [editShowPassword, setEditShowPassword] = useState(false);
  const [editShowConfirm, setEditShowConfirm] = useState(false);
  const [confirmAction, setConfirmAction] = useState<(() => void) | null>(null);
  const debouncedSearch = useDebounce(search);

  const loadUsers = async () => {
    const res = await authService.getAllUsers(page, PAGE_SIZE, debouncedSearch || undefined, false);
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
    setForm({ firstName: '', lastName: '', username: '', email: '', password: '', confirmPassword: '', role: 'Doctor' });
    setShowPassword(false);
    setShowConfirm(false);
    setShowModal(true);
  };

  const openEditModal = (user: any) => {
    setEditUser({ ...user, _originalActive: user.isActive });
    setEditForm({ firstName: user.firstName, lastName: user.lastName, username: user.userName || '', email: user.email, role: user.role, password: '', confirmPassword: '' });
    setChangePassword(false);
    setEditShowPassword(false);
    setEditShowConfirm(false);
    setShowEditModal(true);
  };

  const handleEdit = async () => {
    if (!editForm.firstName.trim() || !editForm.lastName.trim() || !editForm.email.trim()) return;
    if (changePassword && (!editForm.password.trim() || editForm.password !== editForm.confirmPassword)) return;
    setSubmitting(true);
    try {
      await authService.updateUser(editUser.id, {
        firstName: editForm.firstName,
        lastName: editForm.lastName,
        email: editForm.email,
        username: editForm.username || undefined,
        role: editForm.role,
        password: changePassword ? editForm.password : undefined,
      });
      if (editUser._originalActive !== undefined && editUser._originalActive !== editUser.isActive) {
        await authService.toggleActivation(editUser.id, editUser.isActive);
      }
      setShowEditModal(false);
      await loadUsers();
    } finally {
      setSubmitting(false);
    }
  };

  const editPasswordsMatch = editForm.password === editForm.confirmPassword;
  const editCanSubmit = editForm.firstName.trim() && editForm.lastName.trim() && editForm.email.trim() && (!changePassword || (editForm.password.trim() && editForm.confirmPassword.trim() && editPasswordsMatch));

  const handleCreate = async () => {
    if (!form.firstName.trim() || !form.lastName.trim() || !form.email.trim() || !form.password.trim()) return;
    if (form.password !== form.confirmPassword) return;
    setSubmitting(true);
    try {
      const username = form.username.trim() || form.email.split('@')[0];
      await authService.register({ ...form, username, phoneNumber: '', password: form.password });
      setShowModal(false);
      await loadUsers();
    } finally {
      setSubmitting(false);
    }
  };

  const passwordsMatch = form.password === form.confirmPassword;
  const canSubmit = form.firstName.trim() && form.lastName.trim() && form.email.trim() && form.password.trim() && form.confirmPassword.trim() && passwordsMatch;

  const columns = [
    { key: 'firstName', header: 'First Name' },
    { key: 'lastName', header: 'Last Name' },
    { key: 'email', header: 'Email' },
    { key: 'role', header: 'Role' },
    { key: 'isActive', header: 'Active', render: (u: any) => (
      <button
        onClick={(e) => { e.stopPropagation(); authService.toggleActivation(u.id, !u.isActive).then(loadUsers); }}
        className={`text-xs font-semibold ${u.isActive ? 'text-[#155724]' : 'text-[#721C24]'}`}
      >
        {u.isActive ? 'Active' : 'Inactive'}
      </button>
    )},
    { key: 'actions', header: 'Actions', render: (u: any) => (
      <button
        onClick={(e: React.MouseEvent) => {
          e.stopPropagation();
          setConfirmAction(() => async () => {
            await authService.archive(u.id);
            await loadUsers();
          });
        }}
        className="p-1.5 rounded bg-[#FFC107] text-black hover:bg-[#E0A800]" title="Archive"
      >
        <svg xmlns="http://www.w3.org/2000/svg" className="h-4 w-4" viewBox="0 0 24 24" fill="none" stroke="currentColor" strokeWidth="2"><polyline points="21 8 21 21 3 21 3 8" /><rect x="1" y="3" width="22" height="5" /><line x1="10" y1="12" x2="14" y2="12" /></svg>
      </button>
    )},
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
          onRowClick={openEditModal}
        />
      </Card>

      <Modal isOpen={showModal} onClose={() => setShowModal(false)} title="Create User" size="sm"
        footer={
          <div className="flex gap-2 justify-end">
            <Button variant="secondary" size="sm" onClick={() => setShowModal(false)}>Cancel</Button>
            <Button size="sm" onClick={handleCreate} disabled={submitting || !canSubmit}>
              {submitting ? 'Creating...' : 'Create'}
            </Button>
          </div>
        }
      >
        <div className="space-y-4">
          <div className="grid grid-cols-2 gap-4">
            <div>
              <label className="block text-xs font-semibold text-[#6C757D] uppercase tracking-wider mb-1">First Name *</label>
              <input className="w-full border border-[#CED4DA] rounded-lg p-2 text-sm" value={form.firstName} onChange={(e) => setForm({ ...form, firstName: e.target.value })} />
            </div>
            <div>
              <label className="block text-xs font-semibold text-[#6C757D] uppercase tracking-wider mb-1">Last Name *</label>
              <input className="w-full border border-[#CED4DA] rounded-lg p-2 text-sm" value={form.lastName} onChange={(e) => setForm({ ...form, lastName: e.target.value })} />
            </div>
          </div>
          <div>
            <label className="block text-xs font-semibold text-[#6C757D] uppercase tracking-wider mb-1">Email *</label>
            <input type="email" className="w-full border border-[#CED4DA] rounded-lg p-2 text-sm" value={form.email} onChange={(e) => {
              const email = e.target.value;
              setForm({ ...form, email, username: form.username || email.split('@')[0] });
            }} />
          </div>
          <div>
            <label className="block text-xs font-semibold text-[#6C757D] uppercase tracking-wider mb-1">Username</label>
            <input className="w-full border border-[#CED4DA] rounded-lg p-2 text-sm" value={form.username} onChange={(e) => setForm({ ...form, username: e.target.value })} placeholder="Auto-filled from email" />
          </div>
          <div>
            <label className="block text-xs font-semibold text-[#6C757D] uppercase tracking-wider mb-1">Password *</label>
            <div className="relative">
              <input type={showPassword ? 'text' : 'password'} className="w-full border border-[#CED4DA] rounded-lg p-2 text-sm pr-8" value={form.password} onChange={(e) => setForm({ ...form, password: e.target.value })} />
              <button type="button" className="absolute right-2 top-1/2 -translate-y-1/2 text-[#6C757D] hover:text-[#495057]" onClick={() => setShowPassword(!showPassword)}>
                {showPassword ? (
                  <svg xmlns="http://www.w3.org/2000/svg" className="h-4 w-4" viewBox="0 0 24 24" fill="none" stroke="currentColor" strokeWidth="2"><path d="M17.94 17.94A10.07 10.07 0 0 1 12 20c-7 0-11-8-11-8a18.45 18.45 0 0 1 5.06-5.94M9.9 4.24A9.12 9.12 0 0 1 12 4c7 0 11 8 11 8a18.5 18.5 0 0 1-2.16 3.19m-6.72-1.07a3 3 0 1 1-4.24-4.24" /><line x1="1" y1="1" x2="23" y2="23" /></svg>
                ) : (
                  <svg xmlns="http://www.w3.org/2000/svg" className="h-4 w-4" viewBox="0 0 24 24" fill="none" stroke="currentColor" strokeWidth="2"><path d="M1 12s4-8 11-8 11 8 11 8-4 8-11 8-11-8-11-8z" /><circle cx="12" cy="12" r="3" /></svg>
                )}
              </button>
            </div>
          </div>
          <div>
            <label className="block text-xs font-semibold text-[#6C757D] uppercase tracking-wider mb-1">Confirm Password *</label>
            <div className="relative">
              <input type={showConfirm ? 'text' : 'password'} className={`w-full border rounded-lg p-2 text-sm pr-8 ${form.confirmPassword && !passwordsMatch ? 'border-[#DC3545]' : 'border-[#CED4DA]'}`} value={form.confirmPassword} onChange={(e) => setForm({ ...form, confirmPassword: e.target.value })} />
              <button type="button" className="absolute right-2 top-1/2 -translate-y-1/2 text-[#6C757D] hover:text-[#495057]" onClick={() => setShowConfirm(!showConfirm)}>
                {showConfirm ? (
                  <svg xmlns="http://www.w3.org/2000/svg" className="h-4 w-4" viewBox="0 0 24 24" fill="none" stroke="currentColor" strokeWidth="2"><path d="M17.94 17.94A10.07 10.07 0 0 1 12 20c-7 0-11-8-11-8a18.45 18.45 0 0 1 5.06-5.94M9.9 4.24A9.12 9.12 0 0 1 12 4c7 0 11 8 11 8a18.5 18.5 0 0 1-2.16 3.19m-6.72-1.07a3 3 0 1 1-4.24-4.24" /><line x1="1" y1="1" x2="23" y2="23" /></svg>
                ) : (
                  <svg xmlns="http://www.w3.org/2000/svg" className="h-4 w-4" viewBox="0 0 24 24" fill="none" stroke="currentColor" strokeWidth="2"><path d="M1 12s4-8 11-8 11 8 11 8-4 8-11 8-11-8-11-8z" /><circle cx="12" cy="12" r="3" /></svg>
                )}
              </button>
            </div>
            {form.confirmPassword && !passwordsMatch && (
              <p className="text-xs text-[#DC3545] mt-1">Passwords do not match</p>
            )}
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

      <Modal isOpen={showEditModal} onClose={() => setShowEditModal(false)} title={`Edit User - ${editUser?.firstName} ${editUser?.lastName}`} size="sm"
        footer={
          <div className="flex gap-2 justify-end">
            <Button variant="secondary" size="sm" onClick={() => setShowEditModal(false)}>Cancel</Button>
            <Button size="sm" onClick={handleEdit} disabled={submitting || !editCanSubmit}>
              {submitting ? 'Saving...' : 'Save'}
            </Button>
          </div>
        }
      >
        <div className="space-y-4">
          <div className="grid grid-cols-2 gap-4">
            <div>
              <label className="block text-xs font-semibold text-[#6C757D] uppercase tracking-wider mb-1">First Name *</label>
              <input className="w-full border border-[#CED4DA] rounded-lg p-2 text-sm" value={editForm.firstName} onChange={(e) => setEditForm({ ...editForm, firstName: e.target.value })} />
            </div>
            <div>
              <label className="block text-xs font-semibold text-[#6C757D] uppercase tracking-wider mb-1">Last Name *</label>
              <input className="w-full border border-[#CED4DA] rounded-lg p-2 text-sm" value={editForm.lastName} onChange={(e) => setEditForm({ ...editForm, lastName: e.target.value })} />
            </div>
          </div>
          <div>
            <label className="block text-xs font-semibold text-[#6C757D] uppercase tracking-wider mb-1">Email *</label>
            <input type="email" className="w-full border border-[#CED4DA] rounded-lg p-2 text-sm" value={editForm.email} onChange={(e) => setEditForm({ ...editForm, email: e.target.value })} />
          </div>
          <div>
            <label className="block text-xs font-semibold text-[#6C757D] uppercase tracking-wider mb-1">Username</label>
            <input className="w-full border border-[#CED4DA] rounded-lg p-2 text-sm" value={editForm.username} onChange={(e) => setEditForm({ ...editForm, username: e.target.value })} />
          </div>
          <div>
            <label className="block text-xs font-semibold text-[#6C757D] uppercase tracking-wider mb-1">Role *</label>
            <select className="w-full border border-[#CED4DA] rounded-lg p-2 text-sm" value={editForm.role} onChange={(e) => setEditForm({ ...editForm, role: e.target.value })}>
              <option value="Doctor">Doctor</option>
              <option value="Receptionist">Receptionist</option>
              <option value="Pharmacist">Pharmacist</option>
              <option value="Admin">Admin</option>
            </select>
          </div>
          <div className="flex items-center gap-3 pt-2 border-t border-[#E9ECEF]">
            <label className="block text-xs font-semibold text-[#6C757D] uppercase tracking-wider">Active</label>
            <button
              onClick={() => { const newVal = !editUser?.isActive; setEditUser({ ...editUser, isActive: newVal }); }}
              className={`relative inline-flex h-5 w-9 items-center rounded-full transition-colors ${editUser?.isActive ? 'bg-[#28A745]' : 'bg-[#CED4DA]'}`}
            >
              <span className={`inline-block h-3.5 w-3.5 transform rounded-full bg-white transition-transform ${editUser?.isActive ? 'translate-x-4.5' : 'translate-x-1'}`} />
            </button>
          </div>
          <div className="flex items-center gap-2">
            <input type="checkbox" id="changePassword" checked={changePassword} onChange={(e) => setChangePassword(e.target.checked)} className="rounded border-[#CED4DA]" />
            <label htmlFor="changePassword" className="text-xs font-semibold text-[#6C757D] uppercase tracking-wider cursor-pointer">Change Password</label>
          </div>
          {changePassword && (
            <>
              <div>
                <label className="block text-xs font-semibold text-[#6C757D] uppercase tracking-wider mb-1">Password *</label>
                <div className="relative">
                  <input type={editShowPassword ? 'text' : 'password'} className="w-full border border-[#CED4DA] rounded-lg p-2 text-sm pr-8" value={editForm.password} onChange={(e) => setEditForm({ ...editForm, password: e.target.value })} />
                  <button type="button" className="absolute right-2 top-1/2 -translate-y-1/2 text-[#6C757D] hover:text-[#495057]" onClick={() => setEditShowPassword(!editShowPassword)}>
                    {editShowPassword ? (
                      <svg xmlns="http://www.w3.org/2000/svg" className="h-4 w-4" viewBox="0 0 24 24" fill="none" stroke="currentColor" strokeWidth="2"><path d="M17.94 17.94A10.07 10.07 0 0 1 12 20c-7 0-11-8-11-8a18.45 18.45 0 0 1 5.06-5.94M9.9 4.24A9.12 9.12 0 0 1 12 4c7 0 11 8 11 8a18.5 18.5 0 0 1-2.16 3.19m-6.72-1.07a3 3 0 1 1-4.24-4.24" /><line x1="1" y1="1" x2="23" y2="23" /></svg>
                    ) : (
                      <svg xmlns="http://www.w3.org/2000/svg" className="h-4 w-4" viewBox="0 0 24 24" fill="none" stroke="currentColor" strokeWidth="2"><path d="M1 12s4-8 11-8 11 8 11 8-4 8-11 8-11-8-11-8z" /><circle cx="12" cy="12" r="3" /></svg>
                    )}
                  </button>
                </div>
              </div>
              <div>
                <label className="block text-xs font-semibold text-[#6C757D] uppercase tracking-wider mb-1">Confirm Password *</label>
                <div className="relative">
                  <input type={editShowConfirm ? 'text' : 'password'} className={`w-full border rounded-lg p-2 text-sm pr-8 ${editForm.confirmPassword && !editPasswordsMatch ? 'border-[#DC3545]' : 'border-[#CED4DA]'}`} value={editForm.confirmPassword} onChange={(e) => setEditForm({ ...editForm, confirmPassword: e.target.value })} />
                  <button type="button" className="absolute right-2 top-1/2 -translate-y-1/2 text-[#6C757D] hover:text-[#495057]" onClick={() => setEditShowConfirm(!editShowConfirm)}>
                    {editShowConfirm ? (
                      <svg xmlns="http://www.w3.org/2000/svg" className="h-4 w-4" viewBox="0 0 24 24" fill="none" stroke="currentColor" strokeWidth="2"><path d="M17.94 17.94A10.07 10.07 0 0 1 12 20c-7 0-11-8-11-8a18.45 18.45 0 0 1 5.06-5.94M9.9 4.24A9.12 9.12 0 0 1 12 4c7 0 11 8 11 8a18.5 18.5 0 0 1-2.16 3.19m-6.72-1.07a3 3 0 1 1-4.24-4.24" /><line x1="1" y1="1" x2="23" y2="23" /></svg>
                    ) : (
                      <svg xmlns="http://www.w3.org/2000/svg" className="h-4 w-4" viewBox="0 0 24 24" fill="none" stroke="currentColor" strokeWidth="2"><path d="M1 12s4-8 11-8 11 8 11 8-4 8-11 8-11-8-11-8z" /><circle cx="12" cy="12" r="3" /></svg>
                    )}
                  </button>
                </div>
                {editForm.confirmPassword && !editPasswordsMatch && (
                  <p className="text-xs text-[#DC3545] mt-1">Passwords do not match</p>
                )}
              </div>
            </>
          )}
        </div>
      </Modal>

      <ConfirmDialog
        isOpen={!!confirmAction}
        title="Confirm Archive"
        message="Are you sure you want to archive this user?"
        confirmLabel="Archive"
        onConfirm={() => { confirmAction?.(); setConfirmAction(null); }}
        onCancel={() => setConfirmAction(null)}
      />
    </div>
  );
}