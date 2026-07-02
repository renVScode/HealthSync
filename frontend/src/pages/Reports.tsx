import { useState } from 'react';
import { Card } from '../components/common/Card';
import { Button } from '../components/common/Button';
import { ReportFilters } from '../components/domain-components';
import { ReportChart } from '../components/domain-components';
import { reportService } from '../services/reportService';
import { printHtml } from '../utils/printUtils';
import { formatCurrency } from '../utils/formatters';

export function Reports() {
  const [appointmentData, setAppointmentData] = useState<any>(null);
  const [revenueData, setRevenueData] = useState<any>(null);
  const [dateRange, setDateRange] = useState({ from: '', to: '' });
  const [error, setError] = useState('');

  const loadReport = async (from: string, to: string) => {
    setDateRange({ from, to });
    setError('');
    try {
      const [apptRes, revRes] = await Promise.all([
        reportService.getAppointmentSummary(from, to),
        reportService.getRevenue(from, to),
      ]);
      setAppointmentData(apptRes.data);
      setRevenueData(revRes.data);
    } catch (err: any) {
      setError(err?.response?.data?.message || err?.message || 'Failed to load report');
      setAppointmentData(null);
      setRevenueData(null);
    }
  };

  const chartData = appointmentData ? [
    { label: 'Completed', value: appointmentData.completed },
    { label: 'Scheduled', value: appointmentData.scheduled },
    { label: 'Cancelled', value: appointmentData.cancelled },
    { label: 'No Show', value: appointmentData.noShow },
  ] : [];

  const maxValue = Math.max(...chartData.map(d => d.value), 1);
  const barHtml = chartData.map(d =>
    `<div style="display:flex;align-items:center;gap:12px;margin:8px 0;">
      <span style="width:100px;font-size:13px;color:#6C757D;">${d.label}</span>
      <div style="flex:1;background:#E9ECEF;border-radius:8px;height:22px;overflow:hidden;">
        <div style="background:#2C7DA0;height:100%;width:${(d.value / maxValue) * 100}%;border-radius:8px;"></div>
      </div>
      <span style="width:50px;text-align:right;font-size:13px;">${d.value}</span>
    </div>`
  ).join('');

  const revenueCards = revenueData ? [
    { label: 'Total Revenue', value: formatCurrency(revenueData.totalRevenue) },
    { label: 'Total Tax', value: formatCurrency(revenueData.totalTax) },
    { label: 'Total Discount', value: formatCurrency(revenueData.totalDiscount) },
    { label: 'Invoices', value: revenueData.invoiceCount },
    { label: 'Paid', value: revenueData.paidCount },
    { label: 'Pending', value: revenueData.pendingCount },
  ] : [];

  const revenueHtml = revenueData ? `
    <h2 style="margin-top:24px;">Revenue Summary</h2>
    <hr />
    <div style="display:grid;grid-template-columns:1fr 1fr 1fr;gap:12px;margin-top:16px;">
      <div style="text-align:center;padding:16px;background:#F8F9FA;border-radius:8px;">
        <div style="font-size:11px;color:#6C757D;text-transform:uppercase;">Total Revenue</div>
        <div style="font-size:24px;font-weight:bold;">${formatCurrency(revenueData.totalRevenue)}</div>
      </div>
      <div style="text-align:center;padding:16px;background:#F8F9FA;border-radius:8px;">
        <div style="font-size:11px;color:#6C757D;text-transform:uppercase;">Paid Invoices</div>
        <div style="font-size:24px;font-weight:bold;">${revenueData.paidCount}</div>
      </div>
      <div style="text-align:center;padding:16px;background:#F8F9FA;border-radius:8px;">
        <div style="font-size:11px;color:#6C757D;text-transform:uppercase;">Pending</div>
        <div style="font-size:24px;font-weight:bold;">${revenueData.pendingCount}</div>
      </div>
    </div>
  ` : '';

  return (
    <div className="space-y-6">
      <Card title="Appointment Summary" actions={appointmentData ? <Button size="sm" onClick={() => {
        printHtml('Appointment Summary Report', `
          <h1>Appointment Summary Report</h1>
          <div class="sub">${dateRange.from} — ${dateRange.to}</div>
          <hr />
          <div style="margin-top:16px;">${barHtml}</div>
          <hr />
          <div style="display:grid;grid-template-columns:1fr 1fr 1fr 1fr;gap:12px;margin-top:16px;">
            <div style="text-align:center;padding:16px;background:#F8F9FA;border-radius:8px;">
              <div style="font-size:11px;color:#6C757D;text-transform:uppercase;">Completed</div>
              <div style="font-size:24px;font-weight:bold;">${appointmentData.completed}</div>
            </div>
            <div style="text-align:center;padding:16px;background:#F8F9FA;border-radius:8px;">
              <div style="font-size:11px;color:#6C757D;text-transform:uppercase;">Scheduled</div>
              <div style="font-size:24px;font-weight:bold;">${appointmentData.scheduled}</div>
            </div>
            <div style="text-align:center;padding:16px;background:#F8F9FA;border-radius:8px;">
              <div style="font-size:11px;color:#6C757D;text-transform:uppercase;">Cancelled</div>
              <div style="font-size:24px;font-weight:bold;">${appointmentData.cancelled}</div>
            </div>
            <div style="text-align:center;padding:16px;background:#F8F9FA;border-radius:8px;">
              <div style="font-size:11px;color:#6C757D;text-transform:uppercase;">No Show</div>
              <div style="font-size:24px;font-weight:bold;">${appointmentData.noShow}</div>
            </div>
          </div>
          ${revenueHtml}
        `);
        }}>
            <svg xmlns="http://www.w3.org/2000/svg" className="h-4 w-4" viewBox="0 0 24 24" fill="none" stroke="currentColor" strokeWidth="2" strokeLinecap="round" strokeLinejoin="round">
              <polyline points="6 9 6 2 18 2 18 9" /><path d="M6 18H4a2 2 0 0 1-2-2v-5a2 2 0 0 1 2-2h16a2 2 0 0 1 2 2v5a2 2 0 0 1-2 2h-2" /><rect x="6" y="14" width="12" height="8" />
            </svg>
            Print
          </Button> : undefined}>
        <ReportFilters onFilter={loadReport} />
        {error && <div className="text-sm text-[#DC3545] bg-[#F8D7DA] p-3 rounded mb-4">{error}</div>}
        {appointmentData && <ReportChart data={chartData} />}
        {!appointmentData && !error && <p className="text-sm text-[#6C757D] py-8 text-center">Click "Apply" to load the report for the selected date range</p>}
      </Card>

      {revenueData && (
        <Card title="Revenue Summary">
          <div className="grid grid-cols-1 sm:grid-cols-3 gap-4">
            {revenueCards.map((item) => (
              <div key={item.label} className="text-center p-4 bg-[#F8F9FA] rounded-lg">
                <div className="text-xs text-[#6C757D] uppercase tracking-wider">{item.label}</div>
                <div className="text-xl font-bold text-[#212529] mt-1">{item.value}</div>
              </div>
            ))}
          </div>
        </Card>
      )}
    </div>
  );
}
