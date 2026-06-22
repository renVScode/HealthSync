import { useState } from 'react';
import { Card } from '../components/common/Card';
import { ReportFilters } from '../components/domain-components';
import { ReportChart } from '../components/domain-components';
import { reportService } from '../services/reportService';

export function Reports() {
  const [appointmentData, setAppointmentData] = useState<any>(null);

  const loadReport = async (from: string, to: string) => {
    const res = await reportService.getAppointmentSummary(from, to);
    setAppointmentData(res.data);
  };

  return (
    <div className="space-y-6">
      <Card title="Appointment Summary">
        <ReportFilters onFilter={loadReport} />
        {appointmentData && (
          <ReportChart data={[
            { label: 'Completed', value: appointmentData.completed },
            { label: 'Scheduled', value: appointmentData.scheduled },
            { label: 'Cancelled', value: appointmentData.cancelled },
            { label: 'No Show', value: appointmentData.noShow },
          ]} />
        )}
      </Card>
    </div>
  );
}
