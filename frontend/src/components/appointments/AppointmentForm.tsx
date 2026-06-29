import { useState } from 'react';
import { Button } from '../common/Button';

interface PatientOption { id: string; name: string; phone?: string; }

interface AppointmentFormProps {
  onSubmit: (data: any) => void;
  patients: PatientOption[];
  doctors: { id: string; name: string }[];
  selectedDate?: string;
  isLoading?: boolean;
}

export function AppointmentForm({ onSubmit, patients, doctors, selectedDate, isLoading }: AppointmentFormProps) {
  const [formData, setFormData] = useState({
    patientId: '', doctorId: '', startTime: selectedDate || '',
    reason: '', notes: '',
  });
  const [errors, setErrors] = useState<Record<string, string>>({});

  const validate = () => {
    const newErrors: Record<string, string> = {};
    if (!formData.patientId) newErrors.patientId = 'Patient is required';
    if (!formData.doctorId) newErrors.doctorId = 'Doctor is required';
    if (!formData.startTime) newErrors.startTime = 'Date and time is required';
    setErrors(newErrors);
    return Object.keys(newErrors).length === 0;
  };

  const handleSubmit = (e: React.FormEvent) => {
    e.preventDefault();
    if (validate()) onSubmit(formData);
  };

  return (
    <form onSubmit={handleSubmit} className="space-y-4">
      <div>
        <label className="block text-sm font-medium text-[#212529] mb-1">Patient</label>
        <select
          value={formData.patientId}
          onChange={(e) => setFormData({ ...formData, patientId: e.target.value })}
          className="w-full px-4 py-2 border border-[#E9ECEF] rounded-md"
        >
          <option value="">Select patient</option>
          {patients.map((p) => <option key={p.id} value={p.id}>{p.name}{p.phone ? ` — ${p.phone}` : ''}</option>)}
        </select>
        {errors.patientId && <p className="text-sm text-[#DC3545] mt-1">{errors.patientId}</p>}
      </div>
      <div>
        <label className="block text-sm font-medium text-[#212529] mb-1">Doctor</label>
        <select
          value={formData.doctorId}
          onChange={(e) => setFormData({ ...formData, doctorId: e.target.value })}
          className="w-full px-4 py-2 border border-[#E9ECEF] rounded-md"
        >
          <option value="">Select doctor</option>
          {doctors.map((d) => <option key={d.id} value={d.id}>{d.name}</option>)}
        </select>
        {errors.doctorId && <p className="text-sm text-[#DC3545] mt-1">{errors.doctorId}</p>}
      </div>
      <div>
        <label className="block text-sm font-medium text-[#212529] mb-1">Date & Time</label>
        <input
          type="datetime-local"
          value={formData.startTime}
          onChange={(e) => setFormData({ ...formData, startTime: e.target.value })}
          className="w-full px-4 py-2 border border-[#E9ECEF] rounded-md"
          step="900"
        />
        {errors.startTime && <p className="text-sm text-[#DC3545] mt-1">{errors.startTime}</p>}
      </div>
      <div>
        <label className="block text-sm font-medium text-[#212529] mb-1">Reason</label>
        <input
          type="text"
          value={formData.reason}
          onChange={(e) => setFormData({ ...formData, reason: e.target.value })}
          className="w-full px-4 py-2 border border-[#E9ECEF] rounded-md"
          placeholder="Reason for visit"
        />
      </div>
      <div>
        <label className="block text-sm font-medium text-[#212529] mb-1">Notes</label>
        <textarea
          value={formData.notes}
          onChange={(e) => setFormData({ ...formData, notes: e.target.value })}
          className="w-full px-4 py-2 border border-[#E9ECEF] rounded-md"
          rows={3}
        />
      </div>
      <div className="flex justify-end gap-2">
        <Button type="submit" isLoading={isLoading}>Schedule Appointment</Button>
      </div>
    </form>
  );
}
