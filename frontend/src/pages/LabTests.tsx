import { useState, useEffect, useCallback } from 'react';
import { Card } from '../components/common/Card';
import { Button } from '../components/common/Button';
import { DataTable } from '../components/common/DataTable';
import { SearchBar } from '../components/common/SearchBar';
import { Modal } from '../components/common/Modal';

import { labTestService } from '../services/labTestService';
import { doctorService } from '../services/doctorService';
import { billingService } from '../services/billingService';
import { useAuth } from '../contexts/auth-context';
import { useDebounce } from '../hooks/useDebounce';
import { formatCurrency, formatDate } from '../utils/formatters';
import { PAGE_SIZE } from '../utils/constants';
import { LabOrderStatus, LabTest, PaymentMethod } from '../types';
import { patientService } from '../services/patientService';

type Tab = 'orders' | 'catalog';

const statusLabels: Record<LabOrderStatus, string> = {
  [LabOrderStatus.Ordered]: 'Ordered',
  [LabOrderStatus.Collected]: 'Collected',
  [LabOrderStatus.Processing]: 'Processing',
  [LabOrderStatus.Completed]: 'Completed',
  [LabOrderStatus.Cancelled]: 'Cancelled',
};

const statusColors: Record<LabOrderStatus, string> = {
  [LabOrderStatus.Ordered]: 'text-[#3B82F6]',
  [LabOrderStatus.Collected]: 'text-[#8B5CF6]',
  [LabOrderStatus.Processing]: 'text-[#FFC107]',
  [LabOrderStatus.Completed]: 'text-[#28A745]',
  [LabOrderStatus.Cancelled]: 'text-[#DC3545]',
};

export function LabTests() {
  const { hasRole } = useAuth();
  const [activeTab, setActiveTab] = useState<Tab>('orders');

  const tabs: { key: Tab; label: string }[] = [
    { key: 'orders', label: 'Orders' },
  ];
  if (hasRole('Admin')) tabs.push({ key: 'catalog', label: 'Catalog' });

  return (
    <div className="space-y-6">
      <h1 className="text-2xl font-bold text-[#212529]">Lab Tests</h1>

      <div className="flex gap-1 border-b border-[#E9ECEF]">
        {tabs.map((t) => (
          <button key={t.key} onClick={() => setActiveTab(t.key)}
            className={`px-4 py-2 text-sm font-medium border-b-2 transition-colors ${
              activeTab === t.key
                ? 'border-[#2C7DA0] text-[#2C7DA0]'
                : 'border-transparent text-[#6C757D] hover:text-[#212529]'
            }`}
          >{t.label}</button>
        ))}
      </div>

      {activeTab === 'orders' && <LabOrdersView />}
      {activeTab === 'catalog' && <LabCatalogView />}
    </div>
  );
}

function LabCatalogView() {
  const [tests, setTests] = useState<LabTest[]>([]);
  const [total, setTotal] = useState(0);
  const [page, setPage] = useState(1);
  const [search, setSearch] = useState('');
  const [showModal, setShowModal] = useState(false);
  const [editing, setEditing] = useState<LabTest | null>(null);
  const [form, setForm] = useState({ testName: '', category: '', description: '', price: '' });
  const [loading, setLoading] = useState(false);

  const load = useCallback(async () => {
    const res = await labTestService.getAll(page, PAGE_SIZE, search);
    const data = res.data;
    setTests(Array.isArray(data) ? data : data.items || []);
    setTotal(Array.isArray(data) ? 0 : data.totalCount || 0);
  }, [page, search]);

  useEffect(() => { load(); }, [load]);

  const resetForm = () => setForm({ testName: '', category: '', description: '', price: '' });

  const openCreate = () => { resetForm(); setEditing(null); setShowModal(true); };

  const openEdit = (t: LabTest) => {
    setForm({ testName: t.testName, category: t.category || '', description: t.description || '', price: t.price.toString() });
    setEditing(t);
    setShowModal(true);
  };

  const handleSubmit = async () => {
    setLoading(true);
    try {
      const payload = { ...form, price: parseFloat(form.price) || 0 };
      if (editing) {
        await labTestService.update(editing.id, payload);
      } else {
        await labTestService.create(payload);
      }
      setShowModal(false);
      await load();
    } finally { setLoading(false); }
  };

  const handleToggle = async (t: LabTest) => {
    await labTestService.update(t.id, { isActive: !t.isActive });
    await load();
  };

  const handleDelete = async (t: LabTest) => {
    if (!confirm(`Delete "${t.testName}"?`)) return;
    await labTestService.delete(t.id);
    await load();
  };

  return (
    <Card title="Lab Test Catalog" actions={
      <Button size="sm" onClick={openCreate}>+ New Test</Button>
    }>
      <div className="mb-4">
        <SearchBar value={search} onChange={(v) => { setSearch(v); setPage(1); }} placeholder="Search tests..." />
      </div>
      <DataTable
        columns={[
          { key: 'testName', header: 'Test Name' },
          { key: 'category', header: 'Category' },
          { key: 'price', header: 'Price', render: (t: LabTest) => formatCurrency(t.price) },
          { key: 'status', header: 'Status', render: (t: LabTest) => (
            <span className={`text-xs font-semibold ${t.isActive ? 'text-[#28A745]' : 'text-[#6C757D]'}`}>
              {t.isActive ? 'Active' : 'Inactive'}
            </span>
          )},
          { key: 'actions', header: 'Actions', render: (t: LabTest) => (
            <div className="flex gap-2" onClick={(e) => e.stopPropagation()}>
              <button onClick={() => openEdit(t)} className="p-1.5 rounded hover:bg-[#E9ECEF]" title="Edit">
                <svg xmlns="http://www.w3.org/2000/svg" className="h-4 w-4 text-[#3B82F6]" viewBox="0 0 24 24" fill="none" stroke="currentColor" strokeWidth="2"><path d="M11 4H4a2 2 0 0 0-2 2v14a2 2 0 0 0 2 2h14a2 2 0 0 0 2-2v-7"/><path d="M18.5 2.5a2.121 2.121 0 0 1 3 3L12 15l-4 1 1-4 9.5-9.5z"/></svg>
              </button>
              <button onClick={() => handleToggle(t)} className="p-1.5 rounded hover:bg-[#E9ECEF]" title={t.isActive ? 'Deactivate' : 'Activate'}>
                <svg xmlns="http://www.w3.org/2000/svg" className="h-4 w-4 text-[#6C757D]" viewBox="0 0 24 24" fill="none" stroke="currentColor" strokeWidth="2"><rect x="1" y="3" width="15" height="13"/><polyline points="16 8 20 8 23 11 23 16 16 16"/><circle cx="5.5" cy="18.5" r="2.5"/><circle cx="18.5" cy="18.5" r="2.5"/></svg>
              </button>
              <button onClick={() => handleDelete(t)} className="p-1.5 rounded hover:bg-[#E9ECEF]" title="Delete">
                <svg xmlns="http://www.w3.org/2000/svg" className="h-4 w-4 text-[#DC3545]" viewBox="0 0 24 24" fill="none" stroke="currentColor" strokeWidth="2"><polyline points="3 6 5 6 21 6"/><path d="M19 6v14a2 2 0 0 1-2 2H7a2 2 0 0 1-2-2V6m3 0V4a2 2 0 0 1 2-2h4a2 2 0 0 1 2 2v2"/></svg>
              </button>
            </div>
          )},
        ]}
        data={tests}
        page={page}
        totalPages={Math.ceil(total / PAGE_SIZE)}
        onPageChange={setPage}
      />

      <Modal isOpen={showModal} onClose={() => setShowModal(false)}
             title={editing ? 'Edit Lab Test' : 'New Lab Test'} size="md">
        <div className="space-y-4">
          <div><label className="block text-sm font-medium mb-1">Test Name</label>
            <input value={form.testName} onChange={(e) => setForm({...form, testName: e.target.value})}
              className="w-full px-3 py-2 border border-[#E9ECEF] rounded-md text-sm" /></div>
          <div><label className="block text-sm font-medium mb-1">Category</label>
            <input value={form.category} onChange={(e) => setForm({...form, category: e.target.value})}
              className="w-full px-3 py-2 border border-[#E9ECEF] rounded-md text-sm" placeholder="e.g. Hematology" /></div>
          <div><label className="block text-sm font-medium mb-1">Description</label>
            <textarea value={form.description} onChange={(e) => setForm({...form, description: e.target.value})}
              className="w-full px-3 py-2 border border-[#E9ECEF] rounded-md text-sm" rows={2} /></div>
          <div><label className="block text-sm font-medium mb-1">Price (₱)</label>
            <input type="number" step="0.01" value={form.price}
              onChange={(e) => setForm({...form, price: e.target.value})}
              className="w-full px-3 py-2 border border-[#E9ECEF] rounded-md text-sm" /></div>
          <div className="flex justify-end gap-2">
            <Button variant="secondary" size="sm" onClick={() => setShowModal(false)}>Cancel</Button>
            <Button size="sm" onClick={handleSubmit} isLoading={loading}>{editing ? 'Update' : 'Create'}</Button>
          </div>
        </div>
      </Modal>
    </Card>
  );
}

function LabOrdersView() {
  const { hasRole } = useAuth();
  const [orders, setOrders] = useState<any[]>([]);
  const [total, setTotal] = useState(0);
  const [page, setPage] = useState(1);
  const [search, setSearch] = useState('');
  const [statusFilter, setStatusFilter] = useState<string>('');
  const [showCreate, setShowCreate] = useState(false);
  const [selectedOrder, setSelectedOrder] = useState<any>(null);

  const [tests, setTests] = useState<any[]>([]);
  const [doctors, setDoctors] = useState<any[]>([]);
  const [createForm, setCreateForm] = useState({ patientId: '', labTestId: '', notes: '', doctorId: '' });
  const [resultForm, setResultForm] = useState({ result: '', resultSummary: '', referenceRange: '', status: '' });
  const [loading, setLoading] = useState(false);

  const [billingOrder, setBillingOrder] = useState<any>(null);
  const [showBillingModal, setShowBillingModal] = useState(false);
  const [billingPaymentMethod, setBillingPaymentMethod] = useState<PaymentMethod>(PaymentMethod.Cash);
  const [billingSubmitting, setBillingSubmitting] = useState(false);
  const [billingError, setBillingError] = useState('');

  const loadOrders = useCallback(async () => {
    const params: any = { page, pageSize: PAGE_SIZE, search };
    if (statusFilter) params.status = parseInt(statusFilter);
    const res = await labTestService.getOrders(params);
    const data = res.data;
    setOrders(data.items || []);
    setTotal(data.totalCount || 0);
  }, [page, search, statusFilter]);

  useEffect(() => { loadOrders(); }, [loadOrders]);

  useEffect(() => {
    if (showCreate) {
      labTestService.getAll().then((r) => setTests(r.data));
      doctorService.getAll().then((r) => setDoctors(r.data?.items || r.data || []));
    }
  }, [showCreate]);

  const handleCreateOrder = async () => {
    setLoading(true);
    try {
      await labTestService.createOrder(createForm);
      setShowCreate(false);
      setCreateForm({ patientId: '', labTestId: '', notes: '', doctorId: '' });
      await loadOrders();
    } finally { setLoading(false); }
  };

  const handleUpdateOrder = async () => {
    if (!selectedOrder) return;
    setLoading(true);
    try {
      const payload: any = {};
      if (resultForm.status) payload.status = parseInt(resultForm.status);
      if (resultForm.result) payload.result = resultForm.result;
      if (resultForm.resultSummary) payload.resultSummary = resultForm.resultSummary;
      if (resultForm.referenceRange) payload.referenceRange = resultForm.referenceRange;

      await labTestService.updateOrder(selectedOrder.id, payload);

      const newStatus = parseInt(resultForm.status);
      if (!isNaN(newStatus) && newStatus === LabOrderStatus.Completed) {
        setBillingOrder(selectedOrder);
        setShowBillingModal(true);
      } else {
        setSelectedOrder(null);
      }

      setResultForm({ result: '', resultSummary: '', referenceRange: '', status: '' });
      await loadOrders();
    } finally { setLoading(false); }
  };

  const openResult = (order: any) => {
    setSelectedOrder(order);
    setResultForm({
      result: order.result || '',
      resultSummary: order.resultSummary || '',
      referenceRange: order.referenceRange || '',
      status: order.status.toString(),
    });
  };

  const nextStatus = (current: LabOrderStatus): LabOrderStatus | null => {
    switch (current) {
      case LabOrderStatus.Ordered: return LabOrderStatus.Collected;
      case LabOrderStatus.Collected: return LabOrderStatus.Processing;
      case LabOrderStatus.Processing: return LabOrderStatus.Completed;
      default: return null;
    }
  };

  const quickAdvance = async (order: any) => {
    const next = nextStatus(order.status);
    if (next === null) return;
    if (next === LabOrderStatus.Completed) {
      setBillingOrder(order);
      setShowBillingModal(true);
      return;
    }
    await labTestService.updateOrder(order.id, { status: next });
    await loadOrders();
  };

  const handleBillOrder = async () => {
    if (!billingOrder) return;
    setBillingSubmitting(true);
    setBillingError('');
    try {
      await labTestService.updateOrder(billingOrder.id, { status: LabOrderStatus.Completed });
      const res = await billingService.create({
        patientId: billingOrder.patientId,
        items: [{ description: billingOrder.testName, quantity: 1, unitPrice: billingOrder.price }],
      });
      await billingService.addPayment(res.data.id, { amount: res.data.total, paymentMethod: billingPaymentMethod });
      setShowBillingModal(false);
      setBillingOrder(null);
      setSelectedOrder(null);
      await loadOrders();
    } catch (err: any) {
      setBillingError(err?.response?.data?.message || 'Billing failed');
    } finally {
      setBillingSubmitting(false);
    }
  };

  return (
    <Card title="Lab Orders" actions={
      hasRole('Admin') || hasRole('Doctor') ? (
        <Button size="sm" onClick={() => setShowCreate(!showCreate)}>
          {showCreate ? 'Cancel' : '+ New Order'}
        </Button>
      ) : null
    }>
      {/* Create Order Form */}
      {showCreate && (
        <div className="mb-4 p-4 bg-[#F8F9FA] rounded-md space-y-3">
          <div className="grid grid-cols-2 gap-3">
            <div>
              <label className="block text-xs font-medium mb-1">Patient</label>
              <PatientSearch onSelect={(id: string) => setCreateForm({...createForm, patientId: id})} />
            </div>
            <div>
              <label className="block text-xs font-medium mb-1">Lab Test</label>
              <select value={createForm.labTestId} onChange={(e) => setCreateForm({...createForm, labTestId: e.target.value})}
                className="w-full px-3 py-1.5 border rounded text-sm">
                <option value="">Select test...</option>
                {tests.filter((t: any) => t.isActive).map((t: any) => (
                  <option key={t.id} value={t.id}>{t.testName} ({formatCurrency(t.price)})</option>
                ))}
              </select>
            </div>
            {hasRole('Admin') && (
              <div>
                <label className="block text-xs font-medium mb-1">Doctor</label>
                <select value={createForm.doctorId} onChange={(e) => setCreateForm({...createForm, doctorId: e.target.value})}
                  className="w-full px-3 py-1.5 border rounded text-sm">
                  <option value="">Select doctor...</option>
                  {doctors.map((d: any) => (
                    <option key={d.id} value={d.id}>{d.firstName} {d.lastName}</option>
                  ))}
                </select>
              </div>
            )}
            <div className="col-span-2">
              <label className="block text-xs font-medium mb-1">Notes</label>
              <input value={createForm.notes} onChange={(e) => setCreateForm({...createForm, notes: e.target.value})}
                className="w-full px-3 py-1.5 border rounded text-sm" placeholder="Optional" />
            </div>
          </div>
          <div className="flex justify-end">
            <Button size="sm" onClick={handleCreateOrder} disabled={!createForm.patientId || !createForm.labTestId || (hasRole('Admin') && !createForm.doctorId)} isLoading={loading}>
              Create Order
            </Button>
          </div>
        </div>
      )}

      {/* Filters */}
      <div className="flex items-center gap-4 mb-4">
        <SearchBar value={search} onChange={(v) => { setSearch(v); setPage(1); }} placeholder="Search patient or test..." />
        <select value={statusFilter} onChange={(e) => { setStatusFilter(e.target.value); setPage(1); }}
          className="px-3 py-1.5 border border-[#E9ECEF] rounded text-sm">
          <option value="">All Status</option>
          {Object.entries(statusLabels).map(([k, v]) => (
            <option key={k} value={k}>{v}</option>
          ))}
        </select>
      </div>

      {/* Orders Table */}
      <DataTable
        columns={[
          { key: 'orderedAt', header: 'Date', render: (o: any) => formatDate(o.orderedAt) },
          { key: 'patientName', header: 'Patient' },
          { key: 'testName', header: 'Test' },
          { key: 'doctorName', header: 'Ordered By' },
          { key: 'price', header: 'Price', render: (o: any) => formatCurrency(o.price) },
          { key: 'status', header: 'Status', render: (o: any) => (
            <span className={`text-xs font-semibold ${statusColors[o.status as LabOrderStatus] || ''}`}>
              {statusLabels[o.status as LabOrderStatus] || o.status}
            </span>
          )},

        ]}
        data={orders}
        page={page}
        totalPages={Math.ceil(total / PAGE_SIZE)}
        onPageChange={setPage}
        onRowClick={openResult}
      />

      {/* Result Modal */}
      <Modal isOpen={!!selectedOrder} onClose={() => setSelectedOrder(null)}
             title={`Results: ${selectedOrder?.testName || ''}`} size="md">
        {selectedOrder && (
          <div className="space-y-4">
            <div className="grid grid-cols-2 gap-3 text-sm">
              <div><span className="text-[#6C757D]">Patient:</span> {selectedOrder.patientName}</div>
              <div><span className="text-[#6C757D]">Test:</span> {selectedOrder.testName}</div>
              <div><span className="text-[#6C757D]">Status:</span>
                <span className={`ml-1 text-xs font-semibold ${statusColors[selectedOrder.status as LabOrderStatus] || ''}`}>
                  {statusLabels[selectedOrder.status as LabOrderStatus]}
                </span>
              </div>
              <div><span className="text-[#6C757D]">Ordered:</span> {formatDate(selectedOrder.orderedAt)}</div>
            </div>

            {(selectedOrder.status === LabOrderStatus.Ordered || selectedOrder.status === LabOrderStatus.Collected || selectedOrder.status === LabOrderStatus.Processing) && (
              <div className="flex gap-2">
                {selectedOrder.status === LabOrderStatus.Ordered && (
                  <Button size="sm" onClick={async () => {
                    await labTestService.updateOrder(selectedOrder.id, { status: LabOrderStatus.Collected });
                    await loadOrders();
                    setSelectedOrder(null);
                  }}>Mark Collected</Button>
                )}
                {selectedOrder.status === LabOrderStatus.Collected && (
                  <Button size="sm" onClick={async () => {
                    await labTestService.updateOrder(selectedOrder.id, { status: LabOrderStatus.Processing });
                    await loadOrders();
                    setSelectedOrder(null);
                  }}>Mark Processing</Button>
                )}
                {selectedOrder.status === LabOrderStatus.Processing && (
                  <Button size="sm" onClick={() => { quickAdvance(selectedOrder); setSelectedOrder(null); }}>
                    Mark Completed
                  </Button>
                )}
              </div>
            )}
            <div className="border-t pt-4 space-y-3">
              <select value={resultForm.status} onChange={(e) => setResultForm({...resultForm, status: e.target.value})}
                className="w-full px-3 py-1.5 border rounded text-sm">
                <option value="">Keep current status</option>
                {Object.entries(statusLabels).map(([k, v]) => (
                  <option key={k} value={k}>{v}</option>
                ))}
              </select>
              <div>
                <label className="block text-xs font-medium mb-1">Result</label>
                <textarea value={resultForm.result} onChange={(e) => setResultForm({...resultForm, result: e.target.value})}
                  className="w-full px-3 py-2 border border-[#E9ECEF] rounded text-sm font-mono" rows={4}
                  placeholder="Enter result values..." />
              </div>
              <div>
                <label className="block text-xs font-medium mb-1">Reference Range</label>
                <input value={resultForm.referenceRange} onChange={(e) => setResultForm({...resultForm, referenceRange: e.target.value})}
                  className="w-full px-3 py-1.5 border border-[#E9ECEF] rounded text-sm" placeholder="e.g. 3.5-5.5" />
              </div>
              <div>
                <label className="block text-xs font-medium mb-1">Summary / Interpretation</label>
                <input value={resultForm.resultSummary} onChange={(e) => setResultForm({...resultForm, resultSummary: e.target.value})}
                  className="w-full px-3 py-1.5 border border-[#E9ECEF] rounded text-sm" placeholder="Normal / Abnormal / Critical" />
              </div>
              <div className="flex justify-end gap-2">
                <Button variant="secondary" size="sm" onClick={() => setSelectedOrder(null)}>Close</Button>
                <Button size="sm" onClick={handleUpdateOrder} isLoading={loading}>Save Results</Button>
              </div>
            </div>
          </div>
        )}
      </Modal>

      {/* Billing Modal */}
      <Modal isOpen={showBillingModal} onClose={() => { setShowBillingModal(false); setBillingOrder(null); setSelectedOrder(null); }}
             title="Lab Test Billing" size="sm"
             footer={
               <div className="flex gap-2 justify-end w-full">
                 <Button variant="secondary" size="sm" onClick={() => { setShowBillingModal(false); setBillingOrder(null); setSelectedOrder(null); }}>
                   Cancel
                 </Button>
                 <Button size="sm" onClick={handleBillOrder} isLoading={billingSubmitting}>
                   Confirm Payment
                 </Button>
               </div>
             }>
        {billingOrder && (
          <div className="space-y-4">
            <div className="text-sm space-y-1">
              <p><span className="text-[#6C757D]">Patient:</span> {billingOrder.patientName}</p>
              <p><span className="text-[#6C757D]">Test:</span> {billingOrder.testName}</p>
              <p><span className="text-[#6C757D]">Amount:</span> {formatCurrency(billingOrder.price)}</p>
            </div>
            <div>
              <label className="block text-xs font-semibold text-[#6C757D] uppercase tracking-wider mb-1">Mode of Payment</label>
              <select value={billingPaymentMethod} onChange={(e) => setBillingPaymentMethod(parseInt(e.target.value) as PaymentMethod)}
                className="w-full px-3 py-2 border border-[#E9ECEF] rounded-md text-sm">
                <option value={PaymentMethod.Cash}>Cash</option>
                <option value={PaymentMethod.Card}>Card</option>
                <option value={PaymentMethod.Online}>Online / GCash</option>
                <option value={PaymentMethod.Insurance}>Insurance</option>
              </select>
            </div>
            {billingError && <p className="text-xs text-[#DC3545]">{billingError}</p>}
          </div>
        )}
      </Modal>
    </Card>
  );
}

function PatientSearch({ onSelect }: { onSelect: (id: string) => void }) {
  const [query, setQuery] = useState('');
  const [results, setResults] = useState<any[]>([]);
  const [selected, setSelected] = useState<any>(null);
  const debouncedQuery = useDebounce(query, 300);

  useEffect(() => {
    if (debouncedQuery.length < 2) { setResults([]); return; }
    patientService.search(debouncedQuery).then((r: any) => setResults(r.data));
  }, [debouncedQuery]);

  const selectPatient = (p: any) => {
    setSelected(p);
    onSelect(p.id);
    setResults([]);
    setQuery('');
  };

  return selected ? (
    <div className="flex items-center justify-between px-3 py-1.5 border rounded text-sm bg-white">
      <span>{selected.firstName} {selected.lastName}</span>
      <button onClick={() => { setSelected(null); onSelect(''); }} className="text-[#DC3545] text-xs">&times;</button>
    </div>
  ) : (
    <div className="relative">
      <input value={query} onChange={(e) => setQuery(e.target.value)} placeholder="Search patient..."
        className="w-full px-3 py-1.5 border rounded text-sm" />
      {results.length > 0 && (
        <div className="absolute z-10 top-full mt-1 w-full bg-white border rounded shadow-lg max-h-40 overflow-y-auto">
          {results.map((p: any) => (
            <button key={p.id} onClick={() => selectPatient(p)}
              className="w-full text-left px-3 py-2 text-sm hover:bg-[#F8F9FA]">
              {p.firstName} {p.lastName}
            </button>
          ))}
        </div>
      )}
    </div>
  );
}
