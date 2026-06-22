import { Appointment, AppointmentStatus } from '../../types';
import { StatusBadge } from '../common/StatusBadge';
import { formatTime, formatDate } from '../../utils/formatters';

interface AppointmentCardProps {
  appointment: Appointment;
  onClick?: () => void;
}

export function AppointmentCard({ appointment, onClick }: AppointmentCardProps) {
  return (
    <div
      className="bg-white border border-[#E9ECEF] rounded-lg p-4 cursor-pointer hover:shadow-md transition-shadow"
      onClick={onClick}
    >
      <div className="flex justify-between items-start mb-2">
        <div>
          <p className="font-medium text-[#212529]">{appointment.patientName}</p>
          <p className="text-sm text-[#6C757D]">{appointment.doctorName}</p>
        </div>
        <StatusBadge status={AppointmentStatus[appointment.status]} />
      </div>
      <div className="flex items-center gap-2 text-sm text-[#6C757D]">
        <span>{formatDate(appointment.startTime)}</span>
        <span>•</span>
        <span>{formatTime(appointment.startTime)} - {formatTime(appointment.endTime)}</span>
      </div>
      {appointment.reason && (
        <p className="text-sm text-[#6C757D] mt-2 truncate">{appointment.reason}</p>
      )}
    </div>
  );
}
