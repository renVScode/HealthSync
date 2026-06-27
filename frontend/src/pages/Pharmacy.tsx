import { useState, useEffect } from 'react';
import { Card } from '../components/common/Card';
import { Button } from '../components/common/Button';
import { DataTable } from '../components/common/DataTable';
import { Modal } from '../components/common/Modal';
import { LoadingSpinner } from '../components/common/LoadingSpinner';
import { prescriptionService } from '../services/prescriptionService';
import { inventoryService } from '../services/inventoryService';
import { PAGE_SIZE } from '../utils/constants';

export function Pharmacy() {
  const [prescriptions, setPrescriptions] = useState<any[]>([]);
  const [total, setTotal] = useState(0);
  const [page, setPage] = useState(1);
  const [selected, setSelected] = useState<any>(null);
  const [batches, setBatches] = useState<any[]>([]);
  const [selectedBatch, setSelectedBatch] = useState('');
  const [loadingBatches, setLoadingBatches] = useState(false);
  const [dispensing, setDispensing] = useState(false);

  const load = async () => {
    const res = await prescriptionService.getPharmacyQueue(page, PAGE_SIZE);
    setPrescriptions(res.data.items);
    setTotal(res.data.totalCount);
  };

  useEffect(() => { load(); }, [page]);

  const openDispense = async (px: any) => {
    setSelected(px);
    setDispenseQty(px.quantity);
    setSelectedBatch('');
    setLoadingBatches(true);
    try {
      const res = await inventoryService.getAll(1, 100, undefined, px.medicineId);
      const data = res.data;
      const items = data?.items ?? (Array.isArray(data) ? data : []);
      setBatches(items.filter((b: any) => b.quantity > 0));
    } finally {
      setLoadingBatches(false);
    }
  };

  const [dispenseQty, setDispenseQty] = useState(1);

  const handleDispense = async () => {
    if (!selectedBatch) return;
    setDispensing(true);
    try {
      await prescriptionService.dispense(selected.id, { batchId: selectedBatch, quantity: dispenseQty });
      setSelected(null);
      load();
    } finally {
      setDispensing(false);
    }
  };

  return (
    <div className="space-y-6">
      <Card title="Pharmacy - Dispensation Queue">
        <p className="text-sm text-[#6C757D] mb-4">Paid prescriptions awaiting dispensation</p>
        <DataTable
          columns={[
            { key: 'medicineName', header: 'Medicine' },
            { key: 'dosage', header: 'Dosage' },
            { key: 'frequency', header: 'Frequency' },
            { key: 'quantity', header: 'Qty' },
            { key: 'status', header: 'Status', render: (px: any) => (
              <span className="text-xs font-semibold px-2 py-0.5 rounded bg-[#FFF3CD] text-[#856404]">{px.status}</span>
            )},
          ]}
          data={prescriptions}
          page={page}
          totalPages={Math.ceil(total / PAGE_SIZE)}
          onPageChange={setPage}
          onRowClick={openDispense}
        />
        {prescriptions.length === 0 && (
          <p className="text-sm text-[#6C757D] py-4 text-center">No prescriptions waiting for dispensation</p>
        )}
      </Card>

      <Modal isOpen={!!selected} onClose={() => setSelected(null)} title="Dispense Prescription" size="sm"
        footer={
          <div className="flex gap-2 justify-end">
            <Button variant="secondary" size="sm" onClick={() => setSelected(null)}>Cancel</Button>
            <Button size="sm" onClick={handleDispense} disabled={dispensing || !selectedBatch}>
              {dispensing ? 'Dispensing...' : 'Dispense'}
            </Button>
          </div>
        }
      >
        {selected && (
          <div className="space-y-4 text-sm">
            <div className="bg-[#F8F9FA] p-3 rounded space-y-2">
              <div><span className="text-[#6C757D]">Medicine:</span> <span className="font-medium">{selected.medicineName}</span></div>
              <div><span className="text-[#6C757D]">Dosage:</span> {selected.dosage}</div>
              <div><span className="text-[#6C757D]">Frequency:</span> {selected.frequency}</div>
              <div><span className="text-[#6C757D]">Duration:</span> {selected.duration || '—'}</div>
              <div><span className="text-[#6C757D]">Prescribed Qty:</span> {selected.quantity}</div>
            </div>
            <div>
              <label className="block text-xs font-semibold text-[#6C757D] uppercase tracking-wider mb-1">Dispense Quantity</label>
              <input type="number" min={1} max={selected.quantity} className="w-full border border-[#CED4DA] rounded-lg p-2 text-sm" value={dispenseQty} onChange={(e) => setDispenseQty(Math.min(parseInt(e.target.value) || 1, selected.quantity))} />
            </div>
            <div>
              <label className="block text-xs font-semibold text-[#6C757D] uppercase tracking-wider mb-1">Inventory Batch</label>
              {loadingBatches ? (
                <LoadingSpinner />
              ) : (
                <select className="w-full border border-[#CED4DA] rounded-lg p-2 text-sm" value={selectedBatch} onChange={(e) => setSelectedBatch(e.target.value)}>
                  <option value="">Select a batch...</option>
                  {batches.map((b: any) => (
                    <option key={b.id} value={b.id}>{b.batchNumber} — {b.quantity} available</option>
                  ))}
                  {batches.length === 0 && <option value="" disabled>No batches available</option>}
                </select>
              )}
            </div>
          </div>
        )}
      </Modal>
    </div>
  );
}
