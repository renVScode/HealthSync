import { useState, useEffect } from 'react';
import { Card } from '../components/common/Card';
import { Button } from '../components/common/Button';
import { Modal } from '../components/common/Modal';
import { AppointmentCalendar } from '../components/appointments/AppointmentCalendar';
import { AppointmentForm } from '../components/appointments/AppointmentForm';
import { appointmentService } from '../services/appointmentService';
import { patientService } from '../services/patientService';
import { doctorService } from '../services/doctorService';
import type { CalendarEvent, Patient, Doctor } from '../types';

export function Appointments() {
  const [events, setEvents] = useState<CalendarEvent[]>([]);
  const [patients, setPatients] = useState<Patient[]>([]);
  const [doctors, setDoctors] = useState<Doctor[]>([]);
  const [showModal, setShowModal] = useState(false);
  const [selectedDate, setSelectedDate] = useState('');

  useEffect(() => {
    patientService.getAll(1, 100).then((r) => setPatients(r.data.items));
    doctorService.getAll().then((r) => setDoctors(r.data));
  }, []);

  const loadEvents = async (start: string, end: string) => {
    const res = await appointmentService.getCalendarEvents(start, end);
    setEvents(res.data);
  };

  return (
    <div>
      <Card title="Appointments" actions={
        <Button onClick={() => setShowModal(true)}>+ New Appointment</Button>
      }>
        <AppointmentCalendar
          events={events}
          onDateRangeChange={loadEvents}
          onDateSelect={(start) => { setSelectedDate(start.toISOString()); setShowModal(true); }}
          onEventClick={(event) => console.log('Event:', event)}
        />
      </Card>
      <Modal isOpen={showModal} onClose={() => setShowModal(false)} title="Schedule Appointment">
        <AppointmentForm
          patients={patients.map((p: any) => ({ id: p.id, name: `${p.firstName} ${p.lastName}` }))}
          doctors={doctors.map((d: any) => ({ id: d.id, name: `Dr. ${d.lastName} (${d.specialization})` }))}
          selectedDate={selectedDate}
          onSubmit={async (data) => {
            await appointmentService.create(data);
            setShowModal(false);
          }}
        />
      </Modal>
    </div>
  );
}
