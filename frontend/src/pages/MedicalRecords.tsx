import { useState, useEffect } from 'react';
import { Card } from '../components/common/Card';
import { Button } from '../components/common/Button';
import { DataTable } from '../components/common/DataTable';
import { SearchBar } from '../components/common/SearchBar';
import { Modal } from '../components/common/Modal';
import { patientService } from '../services/patientService';
import { medicalRecordService } from '../services/medicalRecordService';
import { medicineService } from '../services/medicineService';
import { useAuth } from '../contexts/auth-context';
import { useDebounce } from '../hooks/useDebounce';
import { formatDate } from '../utils/formatters';
import { printHtml } from '../utils/printUtils';
import { PAGE_SIZE } from '../utils/constants';

export function MedicalRecords() {
  const { hasRole } = useAuth();
  const [patients, setPatients] = useState<any[]>([]);
  const [patientPage, setPatientPage] = useState(1);
  const [patientTotal, setPatientTotal] = useState(0);
  const [patientSearch, setPatientSearch] = useState('');
  const [selectedPatient, setSelectedPatient] = useState<any>(null);
  const [records, setRecords] = useState<any[]>([]);
  const [selectedRecord, setSelectedRecord] = useState<any>(null);
  const [showCreateModal, setShowCreateModal] = useState(false);
  const [medicines, setMedicines] = useState<any[]>([]);
  const [submitting, setSubmitting] = useState(false);
  const debouncedSearch = useDebounce(patientSearch);

  const [form, setForm] = useState({
    diagnosis: '',
    symptoms: '',
    treatment: '',
    notes: '',
    isConfidential: false,
  });
  const [prescriptionRows, setPrescriptionRows] = useState<any[]>([
    { medicineId: '', dosage: '', frequency: '', duration: '', instructions: '', quantity: 1 },
  ]);

  useEffect(() => {
    patientService.getAll(patientPage, PAGE_SIZE, debouncedSearch || undefined).then((res) => {
      setPatients(res.data.items);
      setPatientTotal(res.data.totalCount);
    });
  }, [patientPage, debouncedSearch]);

  useEffect(() => {
    if (selectedPatient) {
      medicalRecordService.getByPatient(selectedPatient.id).then((res) => {
        setRecords(Array.isArray(res.data) ? res.data : []);
      });
    }
  }, [selectedPatient]);

  const canCreate = hasRole('Admin', 'Doctor');

  const openCreateModal = async (patient: any) => {
    setSelectedPatient(patient);
    setForm({ diagnosis: '', symptoms: '', treatment: '', notes: '', isConfidential: false });
    setPrescriptionRows([{ medicineId: '', dosage: '', frequency: '', duration: '', instructions: '', quantity: 1 }]);
    const res = await medicineService.getAll();
    setMedicines(Array.isArray(res.data) ? res.data : []);
    setShowCreateModal(true);
  };

  const addPrescriptionRow = () => {
    setPrescriptionRows([...prescriptionRows, { medicineId: '', dosage: '', frequency: '', duration: '', instructions: '', quantity: 1 }]);
  };

  const removePrescriptionRow = (index: number) => {
    if (prescriptionRows.length > 1) {
      setPrescriptionRows(prescriptionRows.filter((_, i) => i !== index));
    }
  };

  const updatePrescriptionRow = (index: number, field: string, value: any) => {
    const rows = [...prescriptionRows];
    rows[index] = { ...rows[index], [field]: value };
    setPrescriptionRows(rows);
  };

  const handleSubmit = async () => {
    if (!form.diagnosis.trim()) return;
    setSubmitting(true);
    try {
      const record = await medicalRecordService.create({
        patientId: selectedPatient.id,
        diagnosis: form.diagnosis,
        symptoms: form.symptoms || undefined,
        treatment: form.treatment || undefined,
        notes: form.notes || undefined,
        isConfidential: form.isConfidential,
      });

      const validPrescriptions = prescriptionRows.filter((r) => r.medicineId && r.dosage && r.frequency);
      if (validPrescriptions.length > 0) {
        await medicalRecordService.addPrescriptions(record.data.id, validPrescriptions);
      }

      const res = await medicalRecordService.getByPatient(selectedPatient.id);
      setRecords(Array.isArray(res.data) ? res.data : []);
      setShowCreateModal(false);
    } finally {
      setSubmitting(false);
    }
  };

  return (
    <div className="space-y-6">
      <Card title="Medical Records">
        <p className="text-sm text-[#6C757D] mb-4">Search for a patient to view their medical records</p>
        <SearchBar value={patientSearch} onChange={(v) => { setPatientSearch(v); setPatientPage(1); }} placeholder="Search patients..." />
        <div className="mt-4">
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
            onRowClick={(p) => openCreateModal(p)}
          />
        </div>
      </Card>

      {selectedPatient && (
        <Card title={`Medical Records - ${selectedPatient.firstName} ${selectedPatient.lastName}`}
          actions={canCreate && <Button size="sm" onClick={() => openCreateModal(selectedPatient)}>+ New Record</Button>}
        >
          {records.length === 0 ? (
            <p className="text-sm text-[#6C757D] py-4 text-center">No medical records for this patient</p>
          ) : (
            <DataTable
              columns={[
                { key: 'diagnosis', header: 'Diagnosis' },
                { key: 'doctorName', header: 'Doctor' },
                { key: 'createdAt', header: 'Date', render: (r: any) => formatDate(r.createdAt) },
                { key: 'isConfidential', header: 'Confidential', render: (r: any) => r.isConfidential ? 'Yes' : 'No' },
              ]}
              data={records}
              onRowClick={(r) => setSelectedRecord(r)}
            />
          )}
        </Card>
      )}

      <Modal isOpen={showCreateModal} onClose={() => setShowCreateModal(false)} title="New Medical Record" size="lg"
        footer={
          <div className="flex gap-2 justify-end">
            <Button variant="secondary" size="sm" onClick={() => setShowCreateModal(false)}>Cancel</Button>
            <Button size="sm" onClick={handleSubmit} disabled={submitting || !form.diagnosis.trim()}>
              {submitting ? 'Submitting...' : 'Submit'}
            </Button>
          </div>
        }
      >
        <div className="space-y-4">
          <div>
            <label className="block text-xs font-semibold text-[#6C757D] uppercase tracking-wider mb-1">Diagnosis *</label>
            <textarea className="w-full border border-[#CED4DA] rounded-lg p-2 text-sm focus:outline-none focus:ring-2 focus:ring-[#2C7DA0] focus:border-transparent" rows={3} value={form.diagnosis} onChange={(e) => setForm({ ...form, diagnosis: e.target.value })} placeholder="Enter diagnosis..." />
          </div>
          <div>
            <label className="block text-xs font-semibold text-[#6C757D] uppercase tracking-wider mb-1">Symptoms</label>
            <textarea className="w-full border border-[#CED4DA] rounded-lg p-2 text-sm focus:outline-none focus:ring-2 focus:ring-[#2C7DA0] focus:border-transparent" rows={2} value={form.symptoms} onChange={(e) => setForm({ ...form, symptoms: e.target.value })} placeholder="Enter symptoms..." />
          </div>
          <div>
            <label className="block text-xs font-semibold text-[#6C757D] uppercase tracking-wider mb-1">Treatment</label>
            <textarea className="w-full border border-[#CED4DA] rounded-lg p-2 text-sm focus:outline-none focus:ring-2 focus:ring-[#2C7DA0] focus:border-transparent" rows={2} value={form.treatment} onChange={(e) => setForm({ ...form, treatment: e.target.value })} placeholder="Enter treatment plan..." />
          </div>
          <div>
            <label className="block text-xs font-semibold text-[#6C757D] uppercase tracking-wider mb-1">Notes</label>
            <textarea className="w-full border border-[#CED4DA] rounded-lg p-2 text-sm focus:outline-none focus:ring-2 focus:ring-[#2C7DA0] focus:border-transparent" rows={2} value={form.notes} onChange={(e) => setForm({ ...form, notes: e.target.value })} placeholder="Additional notes..." />
          </div>
          <label className="flex items-center gap-2 text-sm cursor-pointer">
            <input type="checkbox" checked={form.isConfidential} onChange={(e) => setForm({ ...form, isConfidential: e.target.checked })} className="rounded" />
            Confidential Record
          </label>

          <div className="border-t pt-4">
            <div className="flex items-center justify-between mb-2">
              <span className="text-xs font-semibold text-[#6C757D] uppercase tracking-wider">Prescriptions</span>
              <Button size="sm" variant="secondary" onClick={addPrescriptionRow}>+ Add Medicine</Button>
            </div>
            {prescriptionRows.map((row, i) => (
              <div key={i} className="relative flex flex-wrap gap-2 p-3 pt-6 bg-[#F8F9FA] rounded mb-2 items-end">
                <button className="absolute top-1 right-1 text-[#DC3545] hover:text-[#A71D2A] p-1" onClick={() => removePrescriptionRow(i)} title="Remove">
                  <svg xmlns="http://www.w3.org/2000/svg" className="h-4 w-4" viewBox="0 0 24 24" fill="none" stroke="currentColor" strokeWidth="2"><line x1="18" y1="6" x2="6" y2="18" /><line x1="6" y1="6" x2="18" y2="18" /></svg>
                </button>
                <div className="flex-1 min-w-[150px]">
                  <label className="block text-xs text-[#6C757D] mb-0.5">Medicine</label>
                  <select className="w-full border border-[#CED4DA] rounded p-1.5 text-sm" value={row.medicineId} onChange={(e) => updatePrescriptionRow(i, 'medicineId', e.target.value)}>
                    <option value="">Select...</option>
                    {medicines.map((m: any) => (
                      <option key={m.id} value={m.id}>{m.name} ({m.genericName || m.unit})</option>
                    ))}
                  </select>
                </div>
                <div className="w-[100px]">
                  <label className="block text-xs text-[#6C757D] mb-0.5">Dosage</label>
                  <input className="w-full border border-[#CED4DA] rounded p-1.5 text-sm" value={row.dosage} onChange={(e) => updatePrescriptionRow(i, 'dosage', e.target.value)} placeholder="e.g. 500mg" />
                </div>
                <div className="w-[120px]">
                  <label className="block text-xs text-[#6C757D] mb-0.5">Frequency</label>
                  <input className="w-full border border-[#CED4DA] rounded p-1.5 text-sm" value={row.frequency} onChange={(e) => updatePrescriptionRow(i, 'frequency', e.target.value)} placeholder="e.g. Twice daily" />
                </div>
                <div className="w-[100px]">
                  <label className="block text-xs text-[#6C757D] mb-0.5">Duration</label>
                  <input className="w-full border border-[#CED4DA] rounded p-1.5 text-sm" value={row.duration} onChange={(e) => updatePrescriptionRow(i, 'duration', e.target.value)} placeholder="e.g. 5 days" />
                </div>
                <div className="w-[80px]">
                  <label className="block text-xs text-[#6C757D] mb-0.5">Qty</label>
                  <input type="number" min={1} className="w-full border border-[#CED4DA] rounded p-1.5 text-sm" value={row.quantity} onChange={(e) => updatePrescriptionRow(i, 'quantity', parseInt(e.target.value) || 1)} />
                </div>
              </div>
            ))}
          </div>
        </div>
      </Modal>

      <Modal isOpen={!!selectedRecord} onClose={() => setSelectedRecord(null)} title="Medical Record Details" size="lg"
        footer={selectedRecord ? <Button size="sm" onClick={() => {
          const pxHtml = selectedRecord.prescriptions?.length > 0
            ? `<h3 style="margin-top:20px;font-size:14px;color:#6C757D;text-transform:uppercase;letter-spacing:0.5px;">Prescriptions</h3>
                <table><thead><tr><th>Medicine</th><th>Dosage</th><th>Frequency</th><th>Duration</th><th>Status</th></tr></thead>
                <tbody>${selectedRecord.prescriptions.map((px: any) =>
                  `<tr><td>${px.medicineName}</td><td>${px.dosage}</td><td>${px.frequency}</td><td>${px.duration || '—'}</td><td>${px.status}</td></tr>`
                ).join('')}</tbody></table>`
            : '';
          printHtml(`Medical Record - ${selectedRecord.patientName}`, `
            <h1>Medical Record</h1>
            <div class="sub">${selectedRecord.invoiceNumber || ''}</div>
            <div class="grid">
              <div><div class="label">Patient</div><div class="value">${selectedRecord.patientName}</div></div>
              <div><div class="label">Doctor</div><div class="value">${selectedRecord.doctorName}</div></div>
              <div><div class="label">Date</div><div class="value">${formatDate(selectedRecord.createdAt)}</div></div>
              <div><div class="label">Confidential</div><div class="value">${selectedRecord.isConfidential ? 'Yes' : 'No'}</div></div>
            </div>
            <hr />
            <div class="label">Diagnosis</div>
            <div class="value">${selectedRecord.diagnosis}</div>
            ${selectedRecord.symptoms ? `<div class="label">Symptoms</div><div class="value">${selectedRecord.symptoms}</div>` : ''}
            ${selectedRecord.treatment ? `<div class="label">Treatment</div><div class="value">${selectedRecord.treatment}</div>` : ''}
            ${selectedRecord.notes ? `<div class="label">Notes</div><div class="value">${selectedRecord.notes}</div>` : ''}
            ${pxHtml}
          `);
        }}>
            <svg xmlns="http://www.w3.org/2000/svg" className="h-4 w-4" viewBox="0 0 24 24" fill="none" stroke="currentColor" strokeWidth="2" strokeLinecap="round" strokeLinejoin="round">
              <polyline points="6 9 6 2 18 2 18 9" /><path d="M6 18H4a2 2 0 0 1-2-2v-5a2 2 0 0 1 2-2h16a2 2 0 0 1 2 2v5a2 2 0 0 1-2 2h-2" /><rect x="6" y="14" width="12" height="8" />
            </svg>
            Print
          </Button> : undefined}
      >
        {selectedRecord && (
          <div className="space-y-4">
            <div className="grid grid-cols-2 gap-4 text-sm">
              <div><span className="text-[#6C757D]">Patient:</span> <span className="font-medium">{selectedRecord.patientName}</span></div>
              <div><span className="text-[#6C757D]">Doctor:</span> <span className="font-medium">{selectedRecord.doctorName}</span></div>
              <div><span className="text-[#6C757D]">Date:</span> <span className="font-medium">{formatDate(selectedRecord.createdAt)}</span></div>
              <div><span className="text-[#6C757D]">Confidential:</span> <span className="font-medium">{selectedRecord.isConfidential ? 'Yes' : 'No'}</span></div>
            </div>
            <div>
              <span className="block text-xs font-semibold text-[#6C757D] uppercase tracking-wider mb-1">Diagnosis</span>
              <p className="text-sm bg-[#F8F9FA] p-3 rounded">{selectedRecord.diagnosis}</p>
            </div>
            {selectedRecord.symptoms && (
              <div>
                <span className="block text-xs font-semibold text-[#6C757D] uppercase tracking-wider mb-1">Symptoms</span>
                <p className="text-sm bg-[#F8F9FA] p-3 rounded">{selectedRecord.symptoms}</p>
              </div>
            )}
            {selectedRecord.treatment && (
              <div>
                <span className="block text-xs font-semibold text-[#6C757D] uppercase tracking-wider mb-1">Treatment</span>
                <p className="text-sm bg-[#F8F9FA] p-3 rounded">{selectedRecord.treatment}</p>
              </div>
            )}
            {selectedRecord.notes && (
              <div>
                <span className="block text-xs font-semibold text-[#6C757D] uppercase tracking-wider mb-1">Notes</span>
                <p className="text-sm bg-[#F8F9FA] p-3 rounded">{selectedRecord.notes}</p>
              </div>
            )}
            {selectedRecord.prescriptions?.length > 0 && (
              <div>
                <span className="block text-xs font-semibold text-[#6C757D] uppercase tracking-wider mb-1">Prescriptions</span>
                {selectedRecord.prescriptions.map((px: any) => (
                  <div key={px.id} className="flex items-center justify-between p-2 bg-[#F8F9FA] rounded mb-1 text-sm">
                    <span className="font-medium">{px.medicineName}</span>
                    <span className="text-[#6C757D]">{px.dosage} - {px.frequency}{px.duration ? ` for ${px.duration}` : ''} ({px.quantity} {px.quantity > 1 ? 'units' : 'unit'})</span>
                    <span className={`text-xs font-semibold px-2 py-0.5 rounded ${
                      px.status === 'Completed' ? 'bg-[#D4EDDA] text-[#155724]' :
                      px.status === 'Paid' ? 'bg-[#FFF3CD] text-[#856404]' :
                      'bg-[#E2E3E5] text-[#383D41]'
                    }`}>{px.status}</span>
                  </div>
                ))}
              </div>
            )}
          </div>
        )}
      </Modal>
    </div>
  );
}
