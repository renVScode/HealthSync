import { useState } from 'react';
import { Button } from './common/Button';
import { StatusBadge } from './common/StatusBadge';
import { BillingStatus } from '../types';
import { formatDate, formatCurrency, getAge } from '../utils/formatters';

// Patient Components

interface PatientListProps { patients: any[]; onSelect: (id: string) => void; }
export function PatientList({ patients, onSelect }: PatientListProps) {
  return (
    <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-3 gap-4">
      {patients.map((p) => (
        <div key={p.id} className="bg-white border border-[#E9ECEF] rounded-lg p-4 cursor-pointer hover:shadow-md"
             onClick={() => onSelect(p.id)}>
          <p className="font-medium text-[#212529]">{p.firstName} {p.lastName}</p>
          <p className="text-sm text-[#6C757D]">{p.phone}</p>
          <p className="text-sm text-[#6C757D]">{p.gender}, {getAge(p.dateOfBirth)} yrs</p>
        </div>
      ))}
    </div>
  );
}

interface PatientFormProps { onSubmit: (data: any) => void; initial?: any; }
export function PatientForm({ onSubmit, initial }: PatientFormProps) {
  const [form, setForm] = useState({ firstName: initial?.firstName || '', lastName: initial?.lastName || '',
    dateOfBirth: initial?.dateOfBirth?.split('T')[0] || '', gender: initial?.gender || 'Male',
    phone: initial?.phone || '', email: initial?.email || '', address: initial?.address || '',
    bloodType: initial?.bloodType || '', emergencyContact: initial?.emergencyContact || '',
    emergencyPhone: initial?.emergencyPhone || '', medicalHistory: initial?.medicalHistory || '',
    allergies: initial?.allergies || '' });

  const handleSubmit = (e: React.FormEvent) => { e.preventDefault(); onSubmit(form); };

  return (
    <form onSubmit={handleSubmit} className="grid grid-cols-2 gap-4">
      <div><label className="block text-sm font-medium mb-1">First Name</label>
        <input value={form.firstName} onChange={(e) => setForm({...form, firstName: e.target.value})}
               className="w-full px-4 py-2 border border-[#E9ECEF] rounded-md" required /></div>
      <div><label className="block text-sm font-medium mb-1">Last Name</label>
        <input value={form.lastName} onChange={(e) => setForm({...form, lastName: e.target.value})}
               className="w-full px-4 py-2 border border-[#E9ECEF] rounded-md" required /></div>
      <div><label className="block text-sm font-medium mb-1">Date of Birth</label>
        <input type="date" value={form.dateOfBirth} onChange={(e) => setForm({...form, dateOfBirth: e.target.value})}
               className="w-full px-4 py-2 border border-[#E9ECEF] rounded-md" required /></div>
      <div><label className="block text-sm font-medium mb-1">Gender</label>
        <select value={form.gender} onChange={(e) => setForm({...form, gender: e.target.value})}
                className="w-full px-4 py-2 border border-[#E9ECEF] rounded-md">
          <option>Male</option><option>Female</option><option>Other</option>
        </select></div>
      <div><label className="block text-sm font-medium mb-1">Phone</label>
        <input value={form.phone} onChange={(e) => setForm({...form, phone: e.target.value})}
               className="w-full px-4 py-2 border border-[#E9ECEF] rounded-md" required /></div>
      <div><label className="block text-sm font-medium mb-1">Email</label>
        <input type="email" value={form.email} onChange={(e) => setForm({...form, email: e.target.value})}
               className="w-full px-4 py-2 border border-[#E9ECEF] rounded-md" /></div>
      <div className="col-span-2"><label className="block text-sm font-medium mb-1">Address</label>
        <textarea value={form.address} onChange={(e) => setForm({...form, address: e.target.value})}
                  className="w-full px-4 py-2 border border-[#E9ECEF] rounded-md" rows={2} /></div>
      <div><label className="block text-sm font-medium mb-1">Blood Type</label>
        <select value={form.bloodType} onChange={(e) => setForm({...form, bloodType: e.target.value})}
                className="w-full px-4 py-2 border border-[#E9ECEF] rounded-md">
          <option value="">Select</option><option>A+</option><option>A-</option><option>B+</option><option>B-</option>
          <option>AB+</option><option>AB-</option><option>O+</option><option>O-</option>
        </select></div>
      <div><label className="block text-sm font-medium mb-1">Emergency Contact</label>
        <input value={form.emergencyContact} onChange={(e) => setForm({...form, emergencyContact: e.target.value})}
               className="w-full px-4 py-2 border border-[#E9ECEF] rounded-md" /></div>
      <div><label className="block text-sm font-medium mb-1">Emergency Phone</label>
        <input value={form.emergencyPhone} onChange={(e) => setForm({...form, emergencyPhone: e.target.value})}
               className="w-full px-4 py-2 border border-[#E9ECEF] rounded-md" /></div>
      <div className="col-span-2"><label className="block text-sm font-medium mb-1">Medical History</label>
        <textarea value={form.medicalHistory} onChange={(e) => setForm({...form, medicalHistory: e.target.value})}
                  className="w-full px-4 py-2 border border-[#E9ECEF] rounded-md" rows={3} /></div>
      <div className="col-span-2"><label className="block text-sm font-medium mb-1">Allergies</label>
        <textarea value={form.allergies} onChange={(e) => setForm({...form, allergies: e.target.value})}
                  className="w-full px-4 py-2 border border-[#E9ECEF] rounded-md" rows={2} /></div>
      <div className="col-span-2 flex justify-end">
        <Button type="submit">{initial ? 'Update Patient' : 'Register Patient'}</Button>
      </div>
    </form>
  );
}

// Doctor Components

export function DoctorList({ doctors, onSelect }: { doctors: any[]; onSelect: (id: string) => void }) {
  return (
    <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-3 gap-4">
      {doctors.map((d) => (
        <div key={d.id} className="bg-white border border-[#E9ECEF] rounded-lg p-4 cursor-pointer hover:shadow-md"
             onClick={() => onSelect(d.id)}>
          <p className="font-medium text-[#212529]">Dr. {d.lastName}</p>
          <p className="text-sm text-[#3B82F6]">{d.specialization}</p>
          <p className="text-sm text-[#6C757D]">{d.phone}</p>
        </div>
      ))}
    </div>
  );
}

export function AvailabilityEditor({ availabilities }: any) {
  const days = ['Sunday', 'Monday', 'Tuesday', 'Wednesday', 'Thursday', 'Friday', 'Saturday'];
  const [slots, setSlots] = useState(availabilities || []);

  const addSlot = (day: number) => {
    setSlots([...slots, { dayOfWeek: day, startTime: '09:00', endTime: '17:00', isAvailable: true }]);
  };

  return (
    <div className="space-y-4">
      {slots.map((slot: any, i: number) => (
        <div key={i} className="flex items-center gap-4 p-3 bg-[#F8F9FA] rounded-md">
          <span className="w-24 text-sm font-medium">{days[slot.dayOfWeek]}</span>
          <input type="time" value={slot.startTime} onChange={(e) => {
            const updated = [...slots]; updated[i].startTime = e.target.value; setSlots(updated);
          }} className="px-2 py-1 border rounded text-sm" />
          <span>to</span>
          <input type="time" value={slot.endTime} onChange={(e) => {
            const updated = [...slots]; updated[i].endTime = e.target.value; setSlots(updated);
          }} className="px-2 py-1 border rounded text-sm" />
          <label className="flex items-center gap-1 text-sm">
            <input type="checkbox" checked={slot.isAvailable}
              onChange={(e) => { const updated = [...slots]; updated[i].isAvailable = e.target.checked; setSlots(updated); }} />
            Available
          </label>
          <button onClick={() => setSlots(slots.filter((_: any, j: number) => j !== i))}
                  className="text-[#DC3545] text-sm">Remove</button>
        </div>
      ))}
      <div className="flex gap-2">
        {days.map((day, i) => (
          <button key={i} onClick={() => addSlot(i)} className="text-xs px-2 py-1 border rounded hover:bg-[#F8F9FA]">{day.slice(0, 3)}</button>
        ))}
      </div>
    </div>
  );
}

// Inventory

export function StockAlertBadge({ currentStock, reorderLevel }: { currentStock: number; reorderLevel: number }) {
  if (currentStock === 0) return <span className="px-2 py-1 text-xs font-medium rounded-full bg-red-100 text-[#DC3545]">Out of Stock</span>;
  if (currentStock <= reorderLevel) return <span className="px-2 py-1 text-xs font-medium rounded-full bg-yellow-100 text-[#FFC107]">Low Stock</span>;
  return <span className="px-2 py-1 text-xs font-medium rounded-full bg-green-100 text-[#28A745]">In Stock</span>;
}

// Billing

export function InvoiceView({ billing }: { billing: any }) {
  return (
    <div className="bg-white border border-[#E9ECEF] rounded-lg p-6">
      <div className="flex justify-between items-start mb-6">
        <div>
          <h2 className="text-xl font-bold text-[#212529]">{billing.invoiceNumber}</h2>
          <p className="text-sm text-[#6C757D]">{formatDate(billing.createdAt)}</p>
        </div>
        <StatusBadge status={BillingStatus[billing.status]} />
      </div>
      <div className="mb-4">
        <p className="font-medium">{billing.patientName}</p>
      </div>
      <table className="w-full mb-4">
        <thead><tr className="border-b border-[#E9ECEF]">
          <th className="text-left py-2 text-sm">Description</th>
          <th className="text-right py-2 text-sm">Qty</th>
          <th className="text-right py-2 text-sm">Price</th>
          <th className="text-right py-2 text-sm">Total</th>
        </tr></thead>
        <tbody>
          {billing.items.map((item: any) => (
            <tr key={item.id} className="border-b border-[#E9ECEF]">
              <td className="py-2 text-sm">{item.description}</td>
              <td className="py-2 text-sm text-right">{item.quantity}</td>
              <td className="py-2 text-sm text-right">{formatCurrency(item.unitPrice)}</td>
              <td className="py-2 text-sm text-right">{formatCurrency(item.total)}</td>
            </tr>
          ))}
        </tbody>
        <tfoot>
          <tr><td colSpan={3} className="text-right py-1 text-sm">Subtotal:</td>
            <td className="text-right py-1 text-sm">{formatCurrency(billing.subTotal)}</td></tr>
          {billing.discount > 0 && <tr><td colSpan={3} className="text-right py-1 text-sm">Discount:</td>
            <td className="text-right py-1 text-sm text-[#DC3545]">-{formatCurrency(billing.discount)}</td></tr>}
          <tr><td colSpan={3} className="text-right py-1 text-sm">Tax:</td>
            <td className="text-right py-1 text-sm">{formatCurrency(billing.tax)}</td></tr>
          <tr className="font-bold"><td colSpan={3} className="text-right py-2">Total:</td>
            <td className="text-right py-2">{formatCurrency(billing.total)}</td></tr>
        </tfoot>
      </table>
      <div className="flex justify-between items-center pt-4 border-t border-[#E9ECEF]">
        <div>
          <p className="text-sm text-[#6C757D]">Paid: {formatCurrency(billing.amountPaid)}</p>
          <p className="text-sm text-[#6C757D]">Balance: <span className="font-medium text-[#212529]">{formatCurrency(billing.balance)}</span></p>
        </div>
      </div>
    </div>
  );
}

// Reports

interface ReportFiltersProps { onFilter: (from: string, to: string) => void; }
export function ReportFilters({ onFilter }: ReportFiltersProps) {
  const [from, setFrom] = useState(() => new Date(Date.now() - 30 * 86400000).toISOString().split('T')[0]);
  const [to, setTo] = useState(() => new Date().toISOString().split('T')[0]);

  return (
    <div className="flex items-center gap-4 mb-4">
      <div><label className="block text-xs text-[#6C757D]">From</label>
        <input type="date" value={from} onChange={(e) => setFrom(e.target.value)}
               className="px-3 py-1.5 border border-[#E9ECEF] rounded text-sm" /></div>
      <div><label className="block text-xs text-[#6C757D]">To</label>
        <input type="date" value={to} onChange={(e) => setTo(e.target.value)}
               className="px-3 py-1.5 border border-[#E9ECEF] rounded text-sm" /></div>
      <Button size="sm" onClick={() => onFilter(from, to)} className="mt-5">Apply</Button>
    </div>
  );
}

export function ReportChart({ data }: { data: { label: string; value: number }[] }) {
  const max = Math.max(...data.map((d) => d.value), 1);
  return (
    <div className="space-y-2">
      {data.map((d, i) => (
        <div key={i} className="flex items-center gap-3">
          <span className="w-24 text-sm text-[#6C757D] truncate">{d.label}</span>
          <div className="flex-1 bg-[#E9ECEF] rounded-full h-5 overflow-hidden">
            <div className="bg-[#2C7DA0] h-full rounded-full transition-all"
                 style={{ width: `${(d.value / max) * 100}%` }} />
          </div>
          <span className="w-16 text-sm text-right text-[#212529]">{d.value}</span>
        </div>
      ))}
    </div>
  );
}
