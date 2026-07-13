import { useState, useEffect } from 'react';
import { Card } from '../components/common/Card';
import { Button } from '../components/common/Button';
import { Modal } from '../components/common/Modal';
import { AppointmentCalendar } from '../components/appointments/AppointmentCalendar';
import { AppointmentForm } from '../components/appointments/AppointmentForm';
import { appointmentService } from '../services/appointmentService';
import { patientService } from '../services/patientService';
import { useAuth } from '../contexts/auth-context';
import { doctorService } from '../services/doctorService';
import { STATUS_COLORS } from '../utils/constants';
import type { CalendarEvent, Patient, Doctor } from '../types';

const STATUS_LABELS: Record<string, string> = {
  '0': 'Scheduled',
  '1': 'Confirmed',
  '2': 'In Progress',
  '3': 'Completed',
  '4': 'Cancelled',
  '5': 'No Show',
};

export function Appointments() {
  const { user, hasRole } = useAuth();
  const [events, setEvents] = useState<CalendarEvent[]>([]);
  const [patients, setPatients] = useState<Patient[]>([]);
  const [doctors, setDoctors] = useState<Doctor[]>([]);
  const [showModal, setShowModal] = useState(false);
  const [showDetailModal, setShowDetailModal] = useState(false);
  const [selectedEvent, setSelectedEvent] = useState<CalendarEvent | null>(null);
  const [selectedDate] = useState('');
  const [submitting, setSubmitting] = useState(false);
  const [submitError, setSubmitError] = useState<string | null>(null);

  useEffect(() => {
    patientService.getAll(1, 100).then((r) => setPatients(r.data.items));
    doctorService.getAll().then((r) => {
      const data = r.data;
      setDoctors(Array.isArray(data) ? data : data.items || []);
    });
  }, []);

  const loadEvents = async (start: string, end: string) => {
    const res = await appointmentService.getCalendarEvents(start, end);
    setEvents(res.data);
  };

  const formatDateTime = (start: string, end: string) => {
    const s = new Date(start);
    const e = new Date(end);
    const opts: Intl.DateTimeFormatOptions = { weekday: 'short', month: 'short', day: 'numeric', year: 'numeric' };
    const timeOpts: Intl.DateTimeFormatOptions = { hour: 'numeric', minute: '2-digit', hour12: true };
    return `${s.toLocaleDateString('en-US', opts)} · ${s.toLocaleTimeString('en-US', timeOpts)} - ${e.toLocaleTimeString('en-US', timeOpts)}`;
  };

  return (
    <div>
      <Card title="Appointments" actions={
        hasRole('Receptionist') && <Button onClick={() => setShowModal(true)}>+ New Appointment</Button>
      }>
        <AppointmentCalendar
          events={events}
          onDateRangeChange={loadEvents}
          onEventClick={(event) => { setSelectedEvent(event); setShowDetailModal(true); }}
        />
      </Card>

      <Modal isOpen={showModal} onClose={() => { setShowModal(false); setSubmitError(null); }} title="Schedule Appointment" size="lg">
        {submitError && (
          <div className="mb-4 p-3 bg-red-50 border border-red-200 text-red-700 rounded-md text-sm">
            {submitError}
          </div>
        )}
        <AppointmentForm
          patients={patients.map((p: any) => ({ id: p.id, name: `${p.firstName} ${p.lastName}`, phone: p.phone }))}
          doctors={doctors.map((d: any) => ({ id: d.id, name: `Dr. ${d.lastName} (${d.specialization})` }))}
          selectedDate={selectedDate}
          isLoading={submitting}
          onSubmit={async (data) => {
            setSubmitting(true);
            setSubmitError(null);
            try {
              const payload = { ...data };
              if (payload.serviceOfferingId === '') payload.serviceOfferingId = null;
              await appointmentService.create(payload);
              setShowModal(false);
            } catch (err: any) {
              setSubmitError(err?.response?.data?.message || err?.message || 'Failed to schedule appointment.');
            } finally {
              setSubmitting(false);
            }
          }}
        />
      </Modal>

      <Modal isOpen={showDetailModal} onClose={() => { setShowDetailModal(false); setSelectedEvent(null); }} title="Appointment Details" size="sm">
        {selectedEvent && (
          <div className="space-y-5">
            <div>
              <label className="block text-xs font-semibold text-[#6C757D] uppercase tracking-wider mb-1">Patient</label>
              <p className="text-sm font-medium text-[#212529]">{selectedEvent.patientName}</p>
            </div>
            {user?.role !== 'Doctor' && (
              <div>
                <label className="block text-xs font-semibold text-[#6C757D] uppercase tracking-wider mb-1">Doctor</label>
                <p className="text-sm font-medium text-[#212529]">{selectedEvent.doctorName}</p>
              </div>
            )}
            <div>
              <label className="block text-xs font-semibold text-[#6C757D] uppercase tracking-wider mb-1">Date &amp; Time</label>
              <p className="text-sm font-medium text-[#212529]">{formatDateTime(selectedEvent.start, selectedEvent.end)}</p>
            </div>
            {selectedEvent.reason && (
              <div>
                <label className="block text-xs font-semibold text-[#6C757D] uppercase tracking-wider mb-1">Reason</label>
                <p className="text-sm text-[#212529]">{selectedEvent.reason}</p>
              </div>
            )}
            <div>
              <label className="block text-xs font-semibold text-[#6C757D] uppercase tracking-wider mb-1">Status</label>
              <span className="text-xs font-semibold"
                style={{ color: STATUS_COLORS[selectedEvent.status.toString()] || '#17A2B8' }}
              >
                {STATUS_LABELS[selectedEvent.status.toString()] || 'Scheduled'}
              </span>
            </div>
          </div>
        )}
      </Modal>
    </div>
  );
}
