import { useRef } from 'react';
import FullCalendar from '@fullcalendar/react';
import dayGridPlugin from '@fullcalendar/daygrid';
import timeGridPlugin from '@fullcalendar/timegrid';
import interactionPlugin from '@fullcalendar/interaction';
import { CalendarEvent } from '../../types';
import { STATUS_COLORS } from '../../utils/constants';

interface AppointmentCalendarProps {
  events: CalendarEvent[];
  onEventClick?: (event: CalendarEvent) => void;
  onDateSelect?: (start: Date, end: Date) => void;
  onDateRangeChange?: (start: string, end: string) => void;
}

export function AppointmentCalendar({ events, onEventClick, onDateSelect, onDateRangeChange }: AppointmentCalendarProps) {
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
    <div className="bg-white rounded-lg border border-[#E9ECEF] p-4">
      <FullCalendar
        ref={calendarRef}
        plugins={[dayGridPlugin, timeGridPlugin, interactionPlugin]}
        initialView="timeGridDay"
        headerToolbar={{
          left: 'prev,next today',
          center: 'title',
          right: 'dayGridMonth,timeGridWeek,timeGridDay',
        }}
        slotMinTime="07:00:00"
        slotMaxTime="19:00:00"
        slotDuration="00:15:00"
        allDaySlot={false}
        height="auto"
        events={calendarEvents}
        eventClick={(info) => onEventClick?.(info.event.extendedProps as CalendarEvent)}
        selectable={true}
        select={(info) => onDateSelect?.(info.start, info.end)}
        datesSet={(info) => onDateRangeChange?.(info.start.toISOString(), info.end.toISOString())}
        selectConstraint={{
          start: '07:00',
          end: '19:00',
          dow: [1, 2, 3, 4, 5, 6],
        }}
      />
    </div>
  );
}
