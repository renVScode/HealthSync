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
import type { CalendarEvent, Patient, Doctor } from '../types';

export function Appointments() {
  const { hasRole } = useAuth();
  const [events, setEvents] = useState<CalendarEvent[]>([]);
  const [patients, setPatients] = useState<Patient[]>([]);
  const [doctors, setDoctors] = useState<Doctor[]>([]);
  const [showModal, setShowModal] = useState(false);
  const [selectedDate, setSelectedDate] = useState('');
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

  return (
    <div>
      <Card title="Appointments" actions={
        hasRole('Receptionist') && <Button onClick={() => setShowModal(true)}>+ New Appointment</Button>
      }>
        <AppointmentCalendar
          events={events}
          onDateRangeChange={loadEvents}
          onDateSelect={(start) => { setSelectedDate(start.toISOString()); setShowModal(true); }}
          onEventClick={(event) => console.log('Event:', event)}
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
              await appointmentService.create(data);
              setShowModal(false);
            } catch (err: any) {
              setSubmitError(err?.response?.data?.message || err?.message || 'Failed to schedule appointment.');
            } finally {
              setSubmitting(false);
            }
          }}
        />
      </Modal>
    </div>
  );
}
