import { useState, useEffect, useCallback } from 'react';
import { useParams, useNavigate } from 'react-router-dom';
import { Card } from '../components/common/Card';
import { Button } from '../components/common/Button';
import { DataTable } from '../components/common/DataTable';
import { SearchBar } from '../components/common/SearchBar';
import { Modal } from '../components/common/Modal';
import { LoadingSpinner } from '../components/common/LoadingSpinner';
import { doctorService } from '../services/doctorService';
import { patientService } from '../services/patientService';
import { reportService } from '../services/reportService';
import { useAuth } from '../contexts/auth-context';
import { useDebounce } from '../hooks/useDebounce';
import { formatCurrency, formatDate } from '../utils/formatters';
import { PAGE_SIZE } from '../utils/constants';

type Tab = 'profile' | 'performance' | 'patients' | 'services' | 'availability' | 'timeoff';

export function DoctorDetail() {
  const { id } = useParams<{ id: string }>();
  const navigate = useNavigate();
  const { user, hasRole } = useAuth();
  const [doctor, setDoctor] = useState<any>(null);
  const [activeTab, setActiveTab] = useState<Tab>('profile');
  const [editing, setEditing] = useState(false);
  const [editForm, setEditForm] = useState<any>({});
  const [perfData, setPerfData] = useState<any>(null);
  const [patients, setPatients] = useState<any[]>([]);
  const [patientPage, setPatientPage] = useState(1);
  const [patientTotal, setPatientTotal] = useState(0);
  const [patientSearch, setPatientSearch] = useState('');
  const debouncedPatientSearch = useDebounce(patientSearch);

  const [uploading, setUploading] = useState(false);
  const [previewImg, setPreviewImg] = useState('');

  const isOwner = user?.id === doctor?.userId;
  const canEdit = hasRole('Admin') || isOwner;

  const loadDoctor = useCallback(async () => {
    const res = await doctorService.getById(id!);
    setDoctor(res.data);
    setEditForm({
      firstName: res.data.firstName,
      lastName: res.data.lastName,
      specialization: res.data.specialization || '',
      phone: res.data.phone || '',
      email: res.data.email || '',
      bio: res.data.bio || '',
      consultationFee: res.data.consultationFee,
    });
  }, [id]);

  useEffect(() => { loadDoctor(); }, [loadDoctor]);

  useEffect(() => {
    if (activeTab === 'performance') {
      reportService.getDoctorPerformance().then((res) => {
        const d = (res.data as any[]).find((p: any) => p.doctorId === id);
        setPerfData(d || null);
      });
    }
    if (activeTab === 'patients') {
      patientService.getByDoctor(id!, patientPage, PAGE_SIZE, debouncedPatientSearch).then((res) => {
        setPatients(res.data.items);
        setPatientTotal(res.data.totalCount);
      });
    }
  }, [activeTab, patientPage, debouncedPatientSearch, id]);

  const handleSaveProfile = async () => {
    await doctorService.updateProfile(id!, editForm);
    await loadDoctor();
    setEditing(false);
  };

  const handleImageUpload = async (e: React.ChangeEvent<HTMLInputElement>, type: 'profile' | 'license') => {
    const file = e.target.files?.[0];
    if (!file) return;
    setUploading(true);
    try {
      await doctorService.uploadImage(id!, file, type);
      await loadDoctor();
    } finally { setUploading(false); }
  };

  if (!doctor) return <LoadingSpinner />;

  const tabs: { key: Tab; label: string }[] = [
    { key: 'profile', label: 'Profile' },
    { key: 'performance', label: 'Performance' },
    { key: 'patients', label: 'Patients' },
    { key: 'services', label: 'Services' },
  ];
  if (canEdit) {
    tabs.push({ key: 'availability', label: 'Availability' });
    tabs.push({ key: 'timeoff', label: 'Time Off' });
  }

  return (
    <div className="space-y-6">
      <Card title={`Dr. ${doctor.firstName} ${doctor.lastName}`} actions={
        <Button variant="secondary" size="sm" onClick={() => navigate('/doctors')}>
          <svg xmlns="http://www.w3.org/2000/svg" className="h-4 w-4" viewBox="0 0 24 24" fill="none" stroke="currentColor" strokeWidth="2" strokeLinecap="round" strokeLinejoin="round">
            <line x1="19" y1="12" x2="5" y2="12" /><polyline points="12 19 5 12 12 5" />
          </svg>
          Back
        </Button>
      }>
        <div className="flex items-center gap-4">
          <div className="relative w-16 h-16 rounded-full bg-[#2C7DA0] flex items-center justify-center text-white text-xl font-bold overflow-hidden shrink-0">
            {doctor.profileImageUrl ? (
              <img src={doctor.profileImageUrl} alt="" className="w-full h-full object-cover" />
            ) : (
              `${doctor.firstName[0]}${doctor.lastName[0]}`
            )}
          </div>
          <div>
            <p className="text-sm text-[#3B82F6] font-medium">{doctor.specialization}</p>
            <p className="text-xs text-[#6C757D]">License: {doctor.licenseNumber}</p>
            <p className="text-xs text-[#6C757D]">{formatCurrency(doctor.consultationFee)} / consult</p>
          </div>
        </div>
      </Card>

      <div className="flex gap-1 border-b border-[#E9ECEF]">
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

      {activeTab === 'profile' && (
        <Card title="Profile Information" actions={
          canEdit && <Button onClick={() => setEditing(!editing)} size="sm">{editing ? 'Cancel' : 'Edit Profile'}</Button>
        }>
          {editing ? (
            <div className="space-y-4">
              <div className="grid grid-cols-1 sm:grid-cols-2 gap-4">
                <div><label className="block text-sm font-medium mb-1">First Name</label>
                  <input value={editForm.firstName} onChange={(e) => setEditForm({...editForm, firstName: e.target.value})}
                         className="w-full px-3 py-2 border border-[#E9ECEF] rounded-md text-sm" /></div>
                <div><label className="block text-sm font-medium mb-1">Last Name</label>
                  <input value={editForm.lastName} onChange={(e) => setEditForm({...editForm, lastName: e.target.value})}
                         className="w-full px-3 py-2 border border-[#E9ECEF] rounded-md text-sm" /></div>
                <div><label className="block text-sm font-medium mb-1">Phone</label>
                  <input value={editForm.phone} onChange={(e) => setEditForm({...editForm, phone: e.target.value})}
                         className="w-full px-3 py-2 border border-[#E9ECEF] rounded-md text-sm" /></div>
                <div><label className="block text-sm font-medium mb-1">Email</label>
                  <input value={editForm.email} onChange={(e) => setEditForm({...editForm, email: e.target.value})}
                         className="w-full px-3 py-2 border border-[#E9ECEF] rounded-md text-sm" /></div>
                <div className="sm:col-span-2"><label className="block text-sm font-medium mb-1">Bio</label>
                  <textarea value={editForm.bio} onChange={(e) => setEditForm({...editForm, bio: e.target.value})}
                            className="w-full px-3 py-2 border border-[#E9ECEF] rounded-md text-sm" rows={3} /></div>
                <div><label className="block text-sm font-medium mb-1">Specialization</label>
                  <input value={editForm.specialization} onChange={(e) => setEditForm({...editForm, specialization: e.target.value})}
                         className="w-full px-3 py-2 border border-[#E9ECEF] rounded-md text-sm" /></div>
                <div><label className="block text-sm font-medium mb-1">Consultation Fee (₱)</label>
                  <input type="number" value={editForm.consultationFee} onChange={(e) => setEditForm({...editForm, consultationFee: Number(e.target.value)})}
                         className="w-full px-3 py-2 border border-[#E9ECEF] rounded-md text-sm" /></div>
              </div>
              <div className="grid grid-cols-1 sm:grid-cols-2 gap-4">
                <div>
                  <label className="block text-sm font-medium mb-1">Profile Image</label>
                  <input type="file" accept="image/*" onChange={(e) => handleImageUpload(e, 'profile')}
                         className="w-full text-sm" disabled={uploading} />
                  {doctor.profileImageUrl && (
                    <img src={doctor.profileImageUrl} alt="" className="mt-2 w-20 h-20 rounded-full object-cover" />
                  )}
                </div>
                <div>
                  <label className="block text-sm font-medium mb-1">License Image</label>
                  <input type="file" accept="image/*" onChange={(e) => handleImageUpload(e, 'license')}
                         className="w-full text-sm" disabled={uploading} />
                  {doctor.licenseImageUrl && (
                    <img src={doctor.licenseImageUrl} alt="" className="mt-2 w-32 h-20 rounded object-cover" />
                  )}
                </div>
              </div>
              <div className="flex justify-end gap-2">
                <Button variant="secondary" onClick={() => setEditing(false)} size="sm">Cancel</Button>
                <Button onClick={handleSaveProfile} size="sm" isLoading={uploading}>Save Changes</Button>
              </div>
            </div>
          ) : (
            <div className="grid grid-cols-1 sm:grid-cols-2 gap-4">
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
              {doctor.licenseImageUrl && (
                <div className="col-span-2">
                  <label className="text-sm text-[#6C757D]">License Image</label>
                  <img src={doctor.licenseImageUrl} alt="License"
                    className="mt-1 max-w-xs rounded border cursor-pointer hover:opacity-80"
                    onClick={() => setPreviewImg(doctor.licenseImageUrl)} />
                </div>
              )}
            </div>
          )}
        </Card>
      )}

      {activeTab === 'performance' && (
        <div className="grid grid-cols-1 md:grid-cols-3 gap-5">
          <div className="bg-white rounded-xl border border-[#E9ECEF] shadow-sm p-5">
            <p className="text-xs text-[#6C757D] uppercase tracking-wider font-semibold">Appointments Completed</p>
            <p className="text-3xl font-bold text-[#212529] mt-2">{perfData?.appointmentsCompleted || 0}</p>
          </div>
          <div className="bg-white rounded-xl border border-[#E9ECEF] shadow-sm p-5">
            <p className="text-xs text-[#6C757D] uppercase tracking-wider font-semibold">Patients Seen</p>
            <p className="text-3xl font-bold text-[#212529] mt-2">{perfData?.patientsSeen || 0}</p>
          </div>
          <div className="bg-white rounded-xl border border-[#E9ECEF] shadow-sm p-5">
            <p className="text-xs text-[#6C757D] uppercase tracking-wider font-semibold">Revenue Generated</p>
            <p className="text-3xl font-bold text-[#28A745] mt-2">{perfData ? formatCurrency(perfData.revenueGenerated) : '₱0'}</p>
          </div>
        </div>
      )}

      {activeTab === 'patients' && (
        <Card title="Patients">
          <div className="mb-4">
            <SearchBar value={patientSearch} onChange={(v) => { setPatientSearch(v); setPatientPage(1); }} placeholder="Search patients..." />
          </div>
          <DataTable
            columns={[
              { key: 'name', header: 'Name', render: (p: any) => `${p.firstName} ${p.lastName}` },
              { key: 'phone', header: 'Phone' },
              { key: 'gender', header: 'Gender' },
              { key: 'bloodType', header: 'Blood Type' },
            ]}
            data={patients}
            page={patientPage}
            totalPages={Math.ceil(patientTotal / PAGE_SIZE)}
            onPageChange={setPatientPage}
            onRowClick={(p) => navigate(`/patients/${p.id}`)}
          />
        </Card>
      )}

      {activeTab === 'services' && (
        canEdit ? <DoctorServices doctorId={id!} /> : <DoctorServicesReadOnly doctorId={id!} />
      )}

      {activeTab === 'availability' && canEdit && (
        <DoctorAvailability doctorId={id!} />
      )}

      {activeTab === 'timeoff' && canEdit && (
        <DoctorTimeOff doctorId={id!} />
      )}

      <Modal isOpen={!!previewImg} onClose={() => setPreviewImg('')} title="License Image" size="sm">
        {previewImg && <img src={previewImg} alt="License" className="w-full" />}
      </Modal>
    </div>
  );
}

function DoctorServices({ doctorId }: { doctorId: string }) {
  const [services, setServices] = useState<any[]>([]);
  const [showForm, setShowForm] = useState(false);
  const [editingId, setEditingId] = useState<string | null>(null);
  const [form, setForm] = useState({ serviceName: '', description: '', price: '' });
  const [loading, setLoading] = useState(false);

  const load = useCallback(async () => {
    const res = await doctorService.getServiceOfferings(doctorId);
    setServices(res.data);
  }, [doctorId]);

  useEffect(() => { load(); }, [load]);

  const resetForm = () => { setForm({ serviceName: '', description: '', price: '' }); setEditingId(null); };

  const handleSubmit = async () => {
    setLoading(true);
    try {
      const payload = { ...form, price: parseFloat(form.price) || 0 };
      if (editingId) {
        await doctorService.updateServiceOffering(doctorId, editingId, payload);
      } else {
        await doctorService.addServiceOffering(doctorId, payload);
      }
      resetForm();
      setShowForm(false);
      await load();
    } finally { setLoading(false); }
  };

  const handleEdit = (s: any) => {
    setForm({ serviceName: s.serviceName, description: s.description || '', price: s.price.toString() });
    setEditingId(s.id);
    setShowForm(true);
  };

  const handleToggle = async (s: any) => {
    await doctorService.updateServiceOffering(doctorId, s.id, { isActive: !s.isActive });
    await load();
  };

  const handleDelete = async (s: any) => {
    if (!confirm(`Delete "${s.serviceName}"?`)) return;
    await doctorService.deleteServiceOffering(doctorId, s.id);
    await load();
  };

  return (
    <Card title="Services" actions={
      <Button size="sm" onClick={() => { resetForm(); setShowForm(!showForm); }}>
        {showForm ? 'Cancel' : '+ Add Service'}
      </Button>
    }>
      {showForm && (
        <div className="mb-4 p-4 bg-[#F8F9FA] rounded-md space-y-3">
          <div className="grid grid-cols-2 gap-3">
            <div>
              <label className="block text-xs font-medium mb-1">Service Name</label>
              <input value={form.serviceName} onChange={(e) => setForm({...form, serviceName: e.target.value})}
                className="w-full px-3 py-1.5 border rounded text-sm" placeholder="e.g. ECG" />
            </div>
            <div>
              <label className="block text-xs font-medium mb-1">Price (₱)</label>
              <input type="number" step="0.01" value={form.price} onChange={(e) => setForm({...form, price: e.target.value})}
                className="w-full px-3 py-1.5 border rounded text-sm" />
            </div>
            <div className="col-span-2">
              <label className="block text-xs font-medium mb-1">Description</label>
              <input value={form.description} onChange={(e) => setForm({...form, description: e.target.value})}
                className="w-full px-3 py-1.5 border rounded text-sm" placeholder="Optional" />
            </div>
          </div>
          <div className="flex justify-end gap-2">
            <Button variant="secondary" size="sm" onClick={() => { resetForm(); setShowForm(false); }}>Cancel</Button>
            <Button size="sm" onClick={handleSubmit} isLoading={loading}>
              {editingId ? 'Update' : 'Add Service'}
            </Button>
          </div>
        </div>
      )}

      {services.length === 0 ? (
        <p className="text-sm text-[#6C757D]">No services added yet</p>
      ) : (
        <div className="space-y-2">
          {services.map((s: any) => (
            <div key={s.id} className="flex items-center justify-between p-3 bg-[#F8F9FA] rounded-md">
              <div className="flex-1">
                <div className="flex items-center gap-2">
                  <p className="text-sm font-medium">{s.serviceName}</p>
                  <span className={`text-xs px-2 py-0.5 rounded-full ${s.isActive ? 'bg-green-100 text-[#28A745]' : 'bg-gray-100 text-[#6C757D]'}`}>
                    {s.isActive ? 'Active' : 'Inactive'}
                  </span>
                </div>
                {s.description && <p className="text-xs text-[#6C757D]">{s.description}</p>}
                <p className="text-xs font-medium text-[#212529]">{formatCurrency(s.price)}</p>
              </div>
              <div className="flex items-center gap-2">
                <button onClick={() => handleToggle(s)} className="p-1.5 rounded hover:bg-gray-200 text-[#6C757D] hover:text-[#212529]" title={s.isActive ? 'Deactivate' : 'Activate'}>
                  <svg xmlns="http://www.w3.org/2000/svg" width="16" height="16" viewBox="0 0 24 24" fill="none" stroke="currentColor" strokeWidth="2" strokeLinecap="round" strokeLinejoin="round">
                    <rect x="1" y="5" width="22" height="14" rx="7" ry="7"/><circle cx={s.isActive ? '15' : '9'} cy="12" r="3"/>
                  </svg>
                </button>
                <button onClick={() => handleEdit(s)} className="p-1.5 rounded hover:bg-gray-200 text-[#3B82F6] hover:text-[#2563EB]" title="Edit">
                  <svg xmlns="http://www.w3.org/2000/svg" width="16" height="16" viewBox="0 0 24 24" fill="none" stroke="currentColor" strokeWidth="2" strokeLinecap="round" strokeLinejoin="round">
                    <path d="M17 3a2.85 2.85 0 1 1 4 4L7.5 20.5 2 22l1.5-5.5Z"/><path d="m15 5 4 4"/>
                  </svg>
                </button>
                <button onClick={() => handleDelete(s)} className="p-1.5 rounded hover:bg-gray-200 text-[#DC3545] hover:text-[#B91C1C]" title="Delete">
                  <svg xmlns="http://www.w3.org/2000/svg" width="16" height="16" viewBox="0 0 24 24" fill="none" stroke="currentColor" strokeWidth="2" strokeLinecap="round" strokeLinejoin="round">
                    <path d="M3 6h18"/><path d="M19 6v14c0 1-1 2-2 2H7c-1 0-2-1-2-2V6"/><path d="M8 6V4c0-1 1-2 2-2h4c1 0 2 1 2 2v2"/><line x1="10" y1="11" x2="10" y2="17"/><line x1="14" y1="11" x2="14" y2="17"/>
                  </svg>
                </button>
              </div>
            </div>
          ))}
        </div>
      )}
    </Card>
  );
}

function DoctorServicesReadOnly({ doctorId }: { doctorId: string }) {
  const [services, setServices] = useState<any[]>([]);
  const [loading, setLoading] = useState(true);

  useEffect(() => {
    doctorService.getServiceOfferings(doctorId).then((res) => {
      setServices(res.data);
      setLoading(false);
    });
  }, [doctorId]);

  return (
    <Card title="Services">
      <DataTable
        columns={[
          { key: 'serviceName', header: 'Service Name' },
          { key: 'price', header: 'Price', render: (s: any) => formatCurrency(s.price) },
        ]}
        data={services.filter((s: any) => s.isActive)}
        isLoading={loading}
      />
    </Card>
  );
}

function DoctorAvailability({ doctorId }: { doctorId: string }) {
  const [availabilities, setAvailabilities] = useState<any[]>([]);
  const days = ['Sunday', 'Monday', 'Tuesday', 'Wednesday', 'Thursday', 'Friday', 'Saturday'];

  useEffect(() => {
    doctorService.getAvailability(doctorId).then((r) => {
      const items = (r.data as any[]).map((a: any) => ({
        ...a,
        startTime: a.startTime.slice(0, 5),
        endTime: a.endTime.slice(0, 5),
      }));
      setAvailabilities(items.length ? items : days.map((_, i) => ({
        dayOfWeek: i, startTime: '09:00', endTime: '17:00', isAvailable: i === 0 || i === 6 ? false : true,
      })));
    });
  }, [doctorId]);

  const toggleDay = (day: number) => {
    setAvailabilities(availabilities.map((a: any) =>
      a.dayOfWeek === day ? { ...a, isAvailable: !a.isAvailable } : a
    ));
  };

  const updateTime = (day: number, field: string, value: string) => {
    setAvailabilities(availabilities.map((a: any) =>
      a.dayOfWeek === day ? { ...a, [field]: value } : a
    ));
  };

  const save = async () => {
    await doctorService.updateAvailability(doctorId, availabilities);
    alert('Availability updated');
  };

  return (
    <Card title="Availability">
      <div className="space-y-3">
        {availabilities.map((slot: any) => (
          <div key={slot.dayOfWeek} className="flex items-center gap-4 p-3 bg-[#F8F9FA] rounded-md">
            <label className="flex items-center gap-2 w-24">
              <input type="checkbox" checked={slot.isAvailable} onChange={() => toggleDay(slot.dayOfWeek)} />
              <span className="text-sm font-medium">{days[slot.dayOfWeek]}</span>
            </label>
            {slot.isAvailable && (
              <>
                <input type="time" value={slot.startTime} onChange={(e) => updateTime(slot.dayOfWeek, 'startTime', e.target.value)}
                       className="px-2 py-1 border rounded text-sm" />
                <span className="text-sm text-[#6C757D]">to</span>
                <input type="time" value={slot.endTime} onChange={(e) => updateTime(slot.dayOfWeek, 'endTime', e.target.value)}
                       className="px-2 py-1 border rounded text-sm" />
              </>
            )}
          </div>
        ))}
      </div>
      <div className="mt-4 flex justify-end">
        <Button size="sm" onClick={save}>Save Availability</Button>
      </div>
    </Card>
  );
}

function DoctorTimeOff({ doctorId }: { doctorId: string }) {
  const [timeOffs, setTimeOffs] = useState<any[]>([]);
  const [showForm, setShowForm] = useState(false);
  const [form, setForm] = useState({ startDate: '', endDate: '', reason: '' });

  useEffect(() => {
    doctorService.getTimeOffs(doctorId).then((r) => setTimeOffs(r.data));
  }, [doctorId]);

  const submit = async () => {
    await doctorService.requestTimeOff(doctorId, {
      startDate: form.startDate,
      endDate: form.endDate,
      reason: form.reason,
    });
    setShowForm(false);
    setForm({ startDate: '', endDate: '', reason: '' });
    const res = await doctorService.getTimeOffs(doctorId);
    setTimeOffs(res.data);
  };

  return (
    <Card title="Time Off" actions={<Button size="sm" onClick={() => setShowForm(true)}>+ Request Time Off</Button>}>
      {showForm && (
        <div className="mb-4 p-4 bg-[#F8F9FA] rounded-md space-y-3">
          <div className="grid grid-cols-2 gap-3">
            <div><label className="block text-xs font-medium mb-1">Start Date</label>
              <input type="date" value={form.startDate} onChange={(e) => setForm({...form, startDate: e.target.value})}
                     className="w-full px-3 py-1.5 border rounded text-sm" /></div>
            <div><label className="block text-xs font-medium mb-1">End Date</label>
              <input type="date" value={form.endDate} onChange={(e) => setForm({...form, endDate: e.target.value})}
                     className="w-full px-3 py-1.5 border rounded text-sm" /></div>
          </div>
          <div><label className="block text-xs font-medium mb-1">Reason</label>
            <input value={form.reason} onChange={(e) => setForm({...form, reason: e.target.value})}
                   className="w-full px-3 py-1.5 border rounded text-sm" /></div>
          <div className="flex justify-end gap-2">
            <Button variant="secondary" size="sm" onClick={() => setShowForm(false)}>Cancel</Button>
            <Button size="sm" onClick={submit}>Submit</Button>
          </div>
        </div>
      )}
      {timeOffs.length === 0 ? (
        <p className="text-sm text-[#6C757D]">No time off requests</p>
      ) : (
        <div className="space-y-2">
          {timeOffs.map((t: any) => (
            <div key={t.id} className="flex items-center justify-between p-3 bg-[#F8F9FA] rounded-md">
              <div>
                <p className="text-sm font-medium">{formatDate(t.startDate)} - {formatDate(t.endDate)}</p>
                {t.reason && <p className="text-xs text-[#6C757D]">{t.reason}</p>}
              </div>
              <span className={`text-xs px-2 py-1 rounded-full ${t.isApproved ? 'bg-green-100 text-[#28A745]' : 'bg-yellow-100 text-[#FFC107]'}`}>
                {t.isApproved ? 'Approved' : 'Pending'}
              </span>
            </div>
          ))}
        </div>
      )}
    </Card>
  );
}