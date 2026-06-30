import { useRef } from 'react';
import FullCalendar from '@fullcalendar/react';
import dayGridPlugin from '@fullcalendar/daygrid';
import { CalendarEvent } from '../../types';
import { STATUS_COLORS } from '../../utils/constants';

interface AppointmentCalendarProps {
  events: CalendarEvent[];
  onEventClick?: (event: CalendarEvent) => void;
  onDateRangeChange?: (start: string, end: string) => void;
}

export function AppointmentCalendar({ events, onEventClick, onDateRangeChange }: AppointmentCalendarProps) {
  const calendarRef = useRef<FullCalendar>(null);

  const calendarEvents = events.map((e) => ({
    id: e.id,
    title: e.title,
    start: e.start,
    end: e.end,
    backgroundColor: STATUS_COLORS[e.status.toString()] || '#17A2B8',
    borderColor: 'transparent',
    textColor: '#fff',
    extendedProps: e,
  }));

  return (
    <div className="bg-white rounded-xl border border-[#E9ECEF] shadow-sm">
      <FullCalendar
        ref={calendarRef}
        plugins={[dayGridPlugin]}
        initialView="dayGridMonth"
        headerToolbar={{
          left: 'title',
          center: '',
          right: 'prev,next today',
        }}
        buttonText={{ today: 'Today' }}
        height="auto"
        events={calendarEvents}
        eventClick={(info) => onEventClick?.(info.event.extendedProps as CalendarEvent)}
        datesSet={(info) => onDateRangeChange?.(info.start.toISOString(), info.end.toISOString())}
        eventContent={(arg) => ({
          html: `<div class="custom-event">${arg.event.title}</div>`
        })}
      />
    </div>
  );
}
