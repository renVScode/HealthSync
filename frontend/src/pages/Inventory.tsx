import { useState, useEffect } from 'react';
import { Card } from '../components/common/Card';
import { Button } from '../components/common/Button';
import { DataTable } from '../components/common/DataTable';
import { Modal } from '../components/common/Modal';
import { SearchBar } from '../components/common/SearchBar';
import { StockAlertBadge } from '../components/domain-components';
import { inventoryService } from '../services/inventoryService';
import { medicineService } from '../services/medicineService';
import { useDebounce } from '../hooks/useDebounce';
import { useAuth } from '../contexts/auth-context';
import { formatCurrency } from '../utils/formatters';
import { PAGE_SIZE } from '../utils/constants';

export function Inventory() {
  const { hasRole } = useAuth();
  const [batches, setBatches] = useState<any[]>([]);
  const [lowStock, setLowStock] = useState<any[]>([]);
  const [search, setSearch] = useState('');
  const [page, setPage] = useState(1);
  const [total, setTotal] = useState(0);
  const debouncedSearch = useDebounce(search);

  const [showAddModal, setShowAddModal] = useState(false);
  const [medicines, setMedicines] = useState<any[]>([]);
  const [submitting, setSubmitting] = useState(false);
  const [submitError, setSubmitError] = useState<string | null>(null);
  const [restockMedicineId, setRestockMedicineId] = useState('');
  const [restockMedicineName, setRestockMedicineName] = useState('');
  const [form, setForm] = useState({
    medicineId: '',
    batchNumber: '',
    quantity: 1,
    unitPrice: 0,
    expiryDate: '',
    supplier: '',
  });

  useEffect(() => {
    inventoryService.getLowStock().then((r) => setLowStock(r.data));
  }, []);

  useEffect(() => {
    const load = async () => {
      const res = await inventoryService.getAll(page, PAGE_SIZE, debouncedSearch || undefined);
      const data = res.data;
      if (Array.isArray(data)) {
        setBatches(data);
        setTotal(0);
      } else if (data?.items) {
        setBatches(data.items);
        setTotal(data.totalCount);
      }
    };
    load();
  }, [page, debouncedSearch]);

  useEffect(() => {
    medicineService.getAll().then((r) => {
      setMedicines(Array.isArray(r.data) ? r.data : []);
    });
  }, []);

  const handleRestock = (item: any) => {
    setRestockMedicineId(item.medicineId);
    setRestockMedicineName(item.medicineName);
    setForm({ medicineId: item.medicineId, batchNumber: '', quantity: 1, unitPrice: 0, expiryDate: '', supplier: '' });
    setShowAddModal(true);
  };

  const closeModal = () => {
    setShowAddModal(false);
    setSubmitError(null);
    setRestockMedicineId('');
    setRestockMedicineName('');
  };

  const handleAddStock = async () => {
    if (!form.medicineId || !form.batchNumber.trim() || form.quantity < 1 || form.unitPrice <= 0) return;
    setSubmitting(true);
    setSubmitError(null);
    try {
      await inventoryService.addBatch({
        medicineId: form.medicineId,
        batchNumber: form.batchNumber.trim(),
        quantity: form.quantity,
        unitPrice: form.unitPrice,
        expiryDate: form.expiryDate || undefined,
        supplier: form.supplier.trim() || undefined,
      });
      closeModal();
      setForm({ medicineId: '', batchNumber: '', quantity: 1, unitPrice: 0, expiryDate: '', supplier: '' });
      const [batchRes, lowStockRes] = await Promise.all([
        inventoryService.getAll(page, PAGE_SIZE, debouncedSearch || undefined),
        inventoryService.getLowStock(),
      ]);
      const data = batchRes.data;
      if (Array.isArray(data)) {
        setBatches(data);
        setTotal(0);
      } else if (data?.items) {
        setBatches(data.items);
        setTotal(data.totalCount);
      }
      setLowStock(lowStockRes.data);
    } catch (err: any) {
      setSubmitError(err?.response?.data?.message || err?.message || 'Failed to add stock.');
    } finally {
      setSubmitting(false);
    }
  };

  const columns = [
    { key: 'medicineName', header: 'Medicine' },
    { key: 'batchNumber', header: 'Batch' },
    { key: 'quantity', header: 'Qty' },
    { key: 'unitPrice', header: 'Unit Price', render: (b: any) => formatCurrency(b.unitPrice) },
    { key: 'expiryDate', header: 'Expiry', render: (b: any) => b.expiryDate ? new Date(b.expiryDate).toLocaleDateString() : '—' },
    { key: 'isLowStock', header: 'Status', render: (b: any) => <StockAlertBadge currentStock={b.quantity} reorderLevel={5} /> },
  ];

  return (
    <div className="space-y-6">
      {lowStock.length > 0 && (
        <Card title="Low Stock Alerts" className="border-yellow-200">
          <div className="space-y-2">
            {lowStock.map((item: any) => (
              <button key={item.medicineId} onClick={() => handleRestock(item)}
                className="w-full flex justify-between items-center p-2 bg-yellow-50 rounded hover:bg-yellow-100 text-left">
                <span className="font-medium">{item.medicineName}</span>
                <span className="text-sm">Stock: {item.currentStock} / Reorder at: {item.reorderLevel}</span>
              </button>
            ))}
          </div>
        </Card>
      )}
      <Card title="Inventory Batches" actions={
        hasRole('Pharmacist') && <Button onClick={() => setShowAddModal(true)}>+ Add Stock</Button>
      }>
        <div className="mb-4">
          <SearchBar value={search} onChange={(v) => { setSearch(v); setPage(1); }} placeholder="Search by medicine name, batch, or supplier..." />
        </div>
        <DataTable
          columns={columns}
          data={batches}
          page={page}
          totalPages={total > 0 ? Math.ceil(total / PAGE_SIZE) : undefined}
          onPageChange={setPage}
        />
      </Card>

      <Modal isOpen={showAddModal} onClose={closeModal} title={restockMedicineId ? `Restock: ${restockMedicineName}` : 'Add Stock'} size="md"
        footer={
          <div className="flex gap-2 justify-end">
            <Button variant="secondary" size="sm" onClick={closeModal}>Cancel</Button>
            <Button size="sm" onClick={handleAddStock} disabled={submitting || !form.medicineId || !form.batchNumber.trim() || form.quantity < 1 || form.unitPrice <= 0}>
              {submitting ? 'Adding...' : 'Add Stock'}
            </Button>
          </div>
        }
      >
        {submitError && (
          <div className="mb-4 p-3 bg-red-50 border border-red-200 text-red-700 rounded-md text-sm">{submitError}</div>
        )}
        <div className="space-y-4">
          <div>
            <label className="block text-xs font-semibold text-[#6C757D] uppercase tracking-wider mb-1">Medicine *</label>
            <select className="w-full border border-[#CED4DA] rounded-lg p-2 text-sm" value={form.medicineId} onChange={(e) => setForm({ ...form, medicineId: e.target.value })}>
              <option value="">Select medicine...</option>
              {medicines.map((m: any) => (
                <option key={m.id} value={m.id}>{m.name} ({m.genericName || m.unit})</option>
              ))}
            </select>
          </div>
          <div>
            <label className="block text-xs font-semibold text-[#6C757D] uppercase tracking-wider mb-1">Batch Number *</label>
            <input className="w-full border border-[#CED4DA] rounded-lg p-2 text-sm" value={form.batchNumber} onChange={(e) => setForm({ ...form, batchNumber: e.target.value })} placeholder="e.g. BATCH-001" />
          </div>
          <div className="grid grid-cols-2 gap-4">
            <div>
              <label className="block text-xs font-semibold text-[#6C757D] uppercase tracking-wider mb-1">Quantity *</label>
              <input type="number" min={1} className="w-full border border-[#CED4DA] rounded-lg p-2 text-sm" value={form.quantity} onChange={(e) => setForm({ ...form, quantity: parseInt(e.target.value) || 0 })} />
            </div>
            <div>
              <label className="block text-xs font-semibold text-[#6C757D] uppercase tracking-wider mb-1">Unit Price *</label>
              <input type="number" min={0} step="0.01" className="w-full border border-[#CED4DA] rounded-lg p-2 text-sm" value={form.unitPrice} onChange={(e) => setForm({ ...form, unitPrice: parseFloat(e.target.value) || 0 })} placeholder="0.00" />
            </div>
          </div>
          <div>
            <label className="block text-xs font-semibold text-[#6C757D] uppercase tracking-wider mb-1">Expiry Date</label>
            <input type="date" className="w-full border border-[#CED4DA] rounded-lg p-2 text-sm" value={form.expiryDate} onChange={(e) => setForm({ ...form, expiryDate: e.target.value })} />
          </div>
          <div>
            <label className="block text-xs font-semibold text-[#6C757D] uppercase tracking-wider mb-1">Supplier</label>
            <input className="w-full border border-[#CED4DA] rounded-lg p-2 text-sm" value={form.supplier} onChange={(e) => setForm({ ...form, supplier: e.target.value })} placeholder="Supplier name..." />
          </div>
        </div>
      </Modal>
    </div>
  );
}
