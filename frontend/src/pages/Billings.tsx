import { useState, useEffect } from 'react';
import { Card } from '../components/common/Card';
import { DataTable } from '../components/common/DataTable';
import { Modal } from '../components/common/Modal';
import { SearchBar } from '../components/common/SearchBar';
import { StatusBadge } from '../components/common/StatusBadge';
import { Button } from '../components/common/Button';
import { InvoiceView } from '../components/domain-components';
import { billingService } from '../services/billingService';
import { patientService } from '../services/patientService';
import { doctorService } from '../services/doctorService';
import { useDebounce } from '../hooks/useDebounce';
import { formatCurrency, formatDate } from '../utils/formatters';
import { BillingStatus, Billing } from '../types';
import { PAGE_SIZE } from '../utils/constants';

export function Billings() {
  const [billings, setBillings] = useState<Billing[]>([]);
  const [page, setPage] = useState(1);
  const [total, setTotal] = useState(0);
  const [selected, setSelected] = useState<any>(null);
  const [search, setSearch] = useState('');
  const [showCreate, setShowCreate] = useState(false);
  const [patients, setPatients] = useState<any[]>([]);
  const [doctors, setDoctors] = useState<any[]>([]);
  const [createPatientId, setCreatePatientId] = useState('');
  const [createDoctorId, setCreateDoctorId] = useState('');
  const [serviceOfferings, setServiceOfferings] = useState<any[]>([]);
  const [items, setItems] = useState<{ description: string; quantity: number; unitPrice: number }[]>([]);
  const [loading, setLoading] = useState(false);
  const debouncedSearch = useDebounce(search);

  const load = () => billingService.getAll({ page, pageSize: PAGE_SIZE, search: debouncedSearch || undefined }).then((res) => {
    setBillings(res.data.items);
    setTotal(res.data.totalCount);
  });
  useEffect(() => { load(); }, [page, debouncedSearch]);
  useEffect(() => {
    patientService.getAll(1, 200).then((r) => setPatients(r.data.items));
    doctorService.getAll().then((r) => {
      const data = r.data;
      setDoctors(Array.isArray(data) ? data : data.items || []);
    });
  }, []);

  useEffect(() => {
    if (createDoctorId) {
      doctorService.getServiceOfferings(createDoctorId).then((res) => {
        const data = Array.isArray(res.data) ? res.data : res.data.items || [];
        setServiceOfferings(data.filter((s: any) => s.isActive));
      });
    } else {
      setServiceOfferings([]);
    }
  }, [createDoctorId]);

  const addServiceItem = (s: any) => {
    setItems([...items, { description: s.serviceName, quantity: 1, unitPrice: s.price }]);
  };

  const removeItem = (idx: number) => {
    setItems(items.filter((_, i) => i !== idx));
  };

  const resetCreate = () => {
    setCreatePatientId('');
    setCreateDoctorId('');
    setServiceOfferings([]);
    setItems([]);
    setShowCreate(false);
  };

  const handleCreate = async () => {
    if (!createPatientId || items.length === 0) return;
    setLoading(true);
    try {
      await billingService.create({ patientId: createPatientId, items });
      resetCreate();
      setPage(1);
      await load();
    } finally {
      setLoading(false);
    }
  };

  const columns = [
    { key: 'invoiceNumber', header: 'Invoice' },
    { key: 'patientName', header: 'Patient' },
    { key: 'total', header: 'Total', render: (b: any) => formatCurrency(b.total) },
    { key: 'amountPaid', header: 'Paid', render: (b: any) => formatCurrency(b.amountPaid) },
    { key: 'balance', header: 'Balance', render: (b: any) => formatCurrency(b.balance) },
    { key: 'status', header: 'Status', render: (b: any) => <StatusBadge status={BillingStatus[b.status]} /> },
    { key: 'createdAt', header: 'Date', render: (b: any) => formatDate(b.createdAt) },
  ];

  return (
    <div>
      <Card title="Billings" actions={
        <Button onClick={() => setShowCreate(true)}>+ New Invoice</Button>
      }>
        <div className="mb-4">
          <SearchBar value={search} onChange={(v) => { setSearch(v); setPage(1); }} placeholder="Search by invoice or patient name..." />
        </div>
        <DataTable columns={columns} data={billings} page={page}
          totalPages={Math.ceil(total / PAGE_SIZE)} onPageChange={setPage}
          onRowClick={(b) => setSelected(b)} />
      </Card>
      <Modal isOpen={!!selected} onClose={() => setSelected(null)} title={`Invoice ${selected?.invoiceNumber}`} size="lg">
        {selected && <InvoiceView billing={selected} onRefresh={load} />}
      </Modal>

      <Modal isOpen={showCreate} onClose={resetCreate} title="New Invoice" size="lg">
        <div className="space-y-4">
          <div>
            <label className="block text-sm font-medium mb-1">Patient</label>
            <select value={createPatientId} onChange={(e) => setCreatePatientId(e.target.value)}
              className="w-full px-4 py-2 border border-[#E9ECEF] rounded-md">
              <option value="">Select patient</option>
              {patients.map((p: any) => <option key={p.id} value={p.id}>{p.firstName} {p.lastName}</option>)}
            </select>
          </div>
          <div>
            <label className="block text-sm font-medium mb-1">Quick-add from Doctor Service</label>
            <div className="flex gap-2">
              <select value={createDoctorId} onChange={(e) => setCreateDoctorId(e.target.value)}
                className="flex-1 px-4 py-2 border border-[#E9ECEF] rounded-md">
                <option value="">Select doctor</option>
                {doctors.map((d: any) => <option key={d.id} value={d.id}>Dr. {d.lastName} ({d.specialization})</option>)}
              </select>
              <select
                className="flex-1 px-4 py-2 border border-[#E9ECEF] rounded-md"
                value=""
                onChange={(e) => {
                  const s = serviceOfferings.find((x: any) => x.id === e.target.value);
                  if (s) addServiceItem(s);
                }}
              >
                <option value="">Select service...</option>
                {serviceOfferings.map((s: any) => <option key={s.id} value={s.id}>{s.serviceName} — ₱{Number(s.price).toLocaleString()}</option>)}
              </select>
            </div>
          </div>
          <div>
            <label className="block text-sm font-medium mb-1">Line Items</label>
            {items.length === 0 ? (
              <p className="text-sm text-[#6C757D]">No items added yet</p>
            ) : (
              <div className="space-y-2">
                {items.map((item, i) => (
                  <div key={i} className="flex items-center gap-2 p-2 bg-[#F8F9FA] rounded-md">
                    <span className="flex-1 text-sm">{item.description}</span>
                    <span className="text-sm text-[#6C757D]">x{item.quantity}</span>
                    <span className="text-sm font-medium">{formatCurrency(item.unitPrice)}</span>
                    <button onClick={() => removeItem(i)} className="p-1 text-[#DC3545] hover:text-[#B91C1C]">
                      <svg xmlns="http://www.w3.org/2000/svg" width="14" height="14" viewBox="0 0 24 24" fill="none" stroke="currentColor" strokeWidth="2" strokeLinecap="round" strokeLinejoin="round">
                        <path d="M3 6h18"/><path d="M19 6v14c0 1-1 2-2 2H7c-1 0-2-1-2-2V6"/><line x1="10" y1="11" x2="10" y2="17"/><line x1="14" y1="11" x2="14" y2="17"/>
                      </svg>
                    </button>
                  </div>
                ))}
                <div className="text-right text-sm font-medium pt-1 border-t border-[#E9ECEF]">
                  Total: {formatCurrency(items.reduce((sum, i) => sum + i.quantity * i.unitPrice, 0))}
                </div>
              </div>
            )}
          </div>
          <div className="flex justify-end gap-2 pt-2">
            <Button variant="secondary" onClick={resetCreate}>Cancel</Button>
            <Button onClick={handleCreate} isLoading={loading} disabled={!createPatientId || items.length === 0}>
              Create Invoice
            </Button>
          </div>
        </div>
      </Modal>
    </div>
  );
}
