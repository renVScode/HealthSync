import { useState, useEffect } from 'react';
import { Card } from '../components/common/Card';
import { Button } from '../components/common/Button';
import { DataTable } from '../components/common/DataTable';
import { Modal } from '../components/common/Modal';
import { LoadingSpinner } from '../components/common/LoadingSpinner';
import { prescriptionService } from '../services/prescriptionService';
import { inventoryService } from '../services/inventoryService';
import { formatDate } from '../utils/formatters';
import { printHtml } from '../utils/printUtils';
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
  const [dispensed, setDispensed] = useState<any>(null);

  const [lowStock, setLowStock] = useState<any[]>([]);
  const [showRestock, setShowRestock] = useState(false);
  const [restockItem, setRestockItem] = useState<any>(null);
  const [restockForm, setRestockForm] = useState({ batchNumber: '', quantity: 1, unitPrice: 0, expiryDate: '', supplier: '' });
  const [restockSubmitting, setRestockSubmitting] = useState(false);
  const [restockError, setRestockError] = useState<string | null>(null);

  const load = async () => {
    const res = await prescriptionService.getPharmacyQueue(page, PAGE_SIZE);
    setPrescriptions(res.data.items);
    setTotal(res.data.totalCount);
  };

  useEffect(() => { load(); }, [page]);

  useEffect(() => {
    inventoryService.getLowStock().then((r) => setLowStock(r.data));
  }, []);

  const openRestock = (item: any) => {
    setRestockItem(item);
    setRestockForm({ batchNumber: '', quantity: 1, unitPrice: 0, expiryDate: '', supplier: '' });
    setRestockError(null);
    setShowRestock(true);
  };

  const handleRestock = async () => {
    if (!restockForm.batchNumber.trim() || restockForm.quantity < 1 || restockForm.unitPrice <= 0) return;
    setRestockSubmitting(true);
    setRestockError(null);
    try {
      await inventoryService.addBatch({
        medicineId: restockItem.medicineId,
        batchNumber: restockForm.batchNumber.trim(),
        quantity: restockForm.quantity,
        unitPrice: restockForm.unitPrice,
        expiryDate: restockForm.expiryDate || undefined,
        supplier: restockForm.supplier.trim() || undefined,
      });
      setShowRestock(false);
      setRestockItem(null);
      const res = await inventoryService.getLowStock();
      setLowStock(res.data);
    } catch (err: any) {
      setRestockError(err?.response?.data?.message || err?.message || 'Failed to add stock.');
    } finally {
      setRestockSubmitting(false);
    }
  };

  const openDispense = async (px: any) => {
    setSelected(px);
    setDispensed(null);
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
      const batch = batches.find((b) => b.id === selectedBatch);
      setDispensed({ ...selected, batchNumber: batch?.batchNumber, dispensedAt: new Date().toISOString() });
      load();
    } finally {
      setDispensing(false);
    }
  };

  const handlePrintReceipt = (px: any) => {
    const receiptHtml = `
      <div style="max-width:400px;margin:0 auto;font-family:'Times New Roman',Times,serif;">
        <div style="text-align:center;margin-bottom:20px;">
          <h2 style="margin:0;font-size:18px;">HealthSync</h2>
          <p style="margin:4px 0;font-size:12px;color:#555;">Pharmacy Dispensation Receipt</p>
          <hr style="border-top:1px dashed #333;margin:12px 0;" />
        </div>
        <div style="margin-bottom:16px;font-size:14px;">
          <div style="margin-bottom:4px;"><strong>Patient:</strong> ${px.patientName || 'N/A'}</div>
          <div style="margin-bottom:4px;"><strong>Date:</strong> ${formatDate(px.dispensedAt)}</div>
          <div style="margin-bottom:4px;"><strong>Prescription #:</strong> ${px.id.slice(0, 8).toUpperCase()}</div>
        </div>
        <hr style="border-top:1px solid #333;margin:12px 0;" />
        <table style="width:100%;font-size:14px;border-collapse:collapse;">
          <thead>
            <tr style="border-bottom:1px solid #333;">
              <th style="text-align:left;padding:6px 4px;">Medicine</th>
              <th style="text-align:center;padding:6px 4px;">Dosage</th>
              <th style="text-align:center;padding:6px 4px;">Freq</th>
              <th style="text-align:right;padding:6px 4px;">Qty</th>
            </tr>
          </thead>
          <tbody>
            <tr>
              <td style="padding:6px 4px;">${px.medicineName}</td>
              <td style="text-align:center;padding:6px 4px;">${px.dosage}</td>
              <td style="text-align:center;padding:6px 4px;">${px.frequency}</td>
              <td style="text-align:right;padding:6px 4px;">${dispenseQty}</td>
            </tr>
          </tbody>
        </table>
        <hr style="border-top:1px solid #333;margin:12px 0;" />
        <div style="margin-bottom:8px;font-size:13px;">
          <div style="margin-bottom:2px;"><strong>Batch:</strong> ${px.batchNumber || 'N/A'}</div>
          <div style="margin-bottom:2px;"><strong>Dispensed by:</strong> Pharmacist</div>
        </div>
        <hr style="border-top:1px dashed #333;margin:12px 0;" />
        <div style="text-align:center;font-size:11px;color:#666;margin-top:16px;">
          <p style="margin:2px 0;">Thank you for choosing HealthSync</p>
          <p style="margin:2px 0;">Generated on ${new Date().toLocaleString('en-PH')}</p>
        </div>
      </div>
    `;
    printHtml(`Rx Receipt - ${px.patientName || 'Pharmacy'}`, receiptHtml);
  };

  return (
    <div className="space-y-6">
      {lowStock.length > 0 && (
        <Card title="Low Stock Alerts" className="border-yellow-200">
          <div className="space-y-2">
            {lowStock.map((item: any) => (
              <button key={item.medicineId} onClick={() => openRestock(item)}
                className="w-full flex justify-between items-center p-2 bg-yellow-50 rounded hover:bg-yellow-100 text-left">
                <span className="font-medium">{item.medicineName}</span>
                <span className="text-sm">Stock: {item.currentStock} / Reorder at: {item.reorderLevel}</span>
              </button>
            ))}
          </div>
        </Card>
      )}

      {showRestock && (
        <Modal isOpen={showRestock} onClose={() => { setShowRestock(false); setRestockItem(null); setRestockError(null); }}
          title={`Restock: ${restockItem?.medicineName || ''}`} size="sm"
          footer={
            <div className="flex gap-2 justify-end">
              <Button variant="secondary" size="sm" onClick={() => { setShowRestock(false); setRestockItem(null); setRestockError(null); }}>Cancel</Button>
              <Button size="sm" onClick={handleRestock} disabled={restockSubmitting || !restockForm.batchNumber.trim() || restockForm.quantity < 1 || restockForm.unitPrice <= 0}>
                {restockSubmitting ? 'Adding...' : 'Add Stock'}
              </Button>
            </div>
          }>
          {restockError && (
            <div className="mb-4 p-3 bg-red-50 border border-red-200 text-red-700 rounded-md text-sm">{restockError}</div>
          )}
          <div className="space-y-4">
            <div className="bg-[#F8F9FA] p-3 rounded text-sm">
              <span className="text-[#6C757D]">Medicine:</span>
              <span className="font-medium ml-1">{restockItem?.medicineName}</span>
            </div>
            <div>
              <label className="block text-xs font-semibold text-[#6C757D] uppercase tracking-wider mb-1">Batch Number *</label>
              <input className="w-full border border-[#CED4DA] rounded-lg p-2 text-sm" value={restockForm.batchNumber}
                onChange={(e) => setRestockForm({...restockForm, batchNumber: e.target.value})} placeholder="e.g. BATCH-001" />
            </div>
            <div className="grid grid-cols-2 gap-4">
              <div>
                <label className="block text-xs font-semibold text-[#6C757D] uppercase tracking-wider mb-1">Quantity *</label>
                <input type="number" min={1} className="w-full border border-[#CED4DA] rounded-lg p-2 text-sm" value={restockForm.quantity}
                  onChange={(e) => setRestockForm({...restockForm, quantity: parseInt(e.target.value) || 0})} />
              </div>
              <div>
                <label className="block text-xs font-semibold text-[#6C757D] uppercase tracking-wider mb-1">Unit Price *</label>
                <input type="number" min={0} step="0.01" className="w-full border border-[#CED4DA] rounded-lg p-2 text-sm" value={restockForm.unitPrice}
                  onChange={(e) => setRestockForm({...restockForm, unitPrice: parseFloat(e.target.value) || 0})} placeholder="0.00" />
              </div>
            </div>
            <div>
              <label className="block text-xs font-semibold text-[#6C757D] uppercase tracking-wider mb-1">Expiry Date</label>
              <input type="date" className="w-full border border-[#CED4DA] rounded-lg p-2 text-sm" value={restockForm.expiryDate}
                onChange={(e) => setRestockForm({...restockForm, expiryDate: e.target.value})} />
            </div>
            <div>
              <label className="block text-xs font-semibold text-[#6C757D] uppercase tracking-wider mb-1">Supplier</label>
              <input className="w-full border border-[#CED4DA] rounded-lg p-2 text-sm" value={restockForm.supplier}
                onChange={(e) => setRestockForm({...restockForm, supplier: e.target.value})} placeholder="Supplier name..." />
            </div>
          </div>
        </Modal>
      )}

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

      <Modal isOpen={!!selected} onClose={() => setSelected(null)} title={dispensed ? 'Dispensed Successfully' : 'Dispense Prescription'} size="sm"
        footer={
          dispensed ? (
            <div className="flex gap-2 justify-end w-full">
              <Button variant="secondary" size="sm" onClick={() => setSelected(null)}>Close</Button>
              <Button size="sm" onClick={() => handlePrintReceipt(dispensed)}>
                <svg xmlns="http://www.w3.org/2000/svg" className="h-4 w-4" viewBox="0 0 24 24" fill="none" stroke="currentColor" strokeWidth="2"><polyline points="6 9 6 2 18 2 18 9" /><path d="M6 18H4a2 2 0 0 1-2-2v-5a2 2 0 0 1 2-2h16a2 2 0 0 1 2 2v5a2 2 0 0 1-2 2h-2" /><rect x="6" y="14" width="12" height="8" /></svg>
                Print Receipt
              </Button>
            </div>
          ) : (
            <div className="flex gap-2 justify-end">
              <Button variant="secondary" size="sm" onClick={() => setSelected(null)}>Cancel</Button>
              <Button size="sm" onClick={handleDispense} disabled={dispensing || !selectedBatch}>
                {dispensing ? 'Dispensing...' : 'Dispense'}
              </Button>
            </div>
          )
        }
      >
        {selected && !dispensed && (
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
        {dispensed && (
          <div className="space-y-3 text-sm">
            <div className="bg-[#D4EDDA] text-[#155724] p-3 rounded-lg text-center font-medium">
              Prescription dispensed successfully
            </div>
            <div className="bg-[#F8F9FA] p-3 rounded space-y-1">
              <div><span className="text-[#6C757D]">Medicine:</span> <span className="font-medium">{dispensed.medicineName}</span></div>
              <div><span className="text-[#6C757D]">Dosage:</span> {dispensed.dosage}</div>
              <div><span className="text-[#6C757D]">Quantity Dispensed:</span> {dispenseQty}</div>
              <div><span className="text-[#6C757D]">Batch:</span> {dispensed.batchNumber}</div>
              <div><span className="text-[#6C757D]">Patient:</span> {dispensed.patientName || 'N/A'}</div>
              <div><span className="text-[#6C757D]">Date:</span> {formatDate(dispensed.dispensedAt)}</div>
            </div>
          </div>
        )}
      </Modal>
    </div>
  );
}