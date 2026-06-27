import { useState, useEffect } from 'react';
import { reportService } from '../services/reportService';
import { useAuth } from '../contexts/auth-context';
import { formatCurrency } from '../utils/formatters';

interface StatCardProps {
  title: string;
  value: string | number;
  accentColor: string;
  icon: React.ReactNode;
  trend?: string;
  trendUp?: boolean;
}

function StatCard({ title, value, accentColor, icon, trend, trendUp }: StatCardProps) {
  return (
    <div className="bg-white rounded-xl border border-[#E9ECEF] shadow-sm overflow-hidden">
      <div className="h-1" style={{ backgroundColor: accentColor }} />
      <div className="p-5 flex items-center gap-4">
        <div className="w-10 h-10 rounded-full flex items-center justify-center shrink-0" style={{ backgroundColor: `${accentColor}15` }}>
          <span style={{ color: accentColor }}>{icon}</span>
        </div>
        <div className="flex-1 text-right">
          <p className="text-sm text-[#6C757D]">{title}</p>
          <p className="text-3xl font-bold text-[#212529]">{value}</p>
          {trend && (
            <div className="flex items-center justify-end gap-1 text-xs mt-1">
              <svg xmlns="http://www.w3.org/2000/svg" className={`h-3 w-3 ${trendUp ? 'text-[#28A745]' : 'text-[#DC3545]'}`} viewBox="0 0 24 24" fill="none" stroke="currentColor" strokeWidth="2">
                {trendUp ? (
                  <polyline points="18 15 12 9 6 15" />
                ) : (
                  <polyline points="6 9 12 15 18 9" />
                )}
              </svg>
              <span className={trendUp ? 'text-[#28A745]' : 'text-[#DC3545]'}>{trend}</span>
            </div>
          )}
        </div>
      </div>
    </div>
  );
}

function BarChart({ data }: { data: { label: string; value: number; color: string }[] }) {
  const max = Math.max(...data.map((d) => d.value), 1);
  return (
    <div className="space-y-3">
      {data.map((d, i) => (
        <div key={i} className="flex items-center gap-3">
          <span className="w-20 text-sm text-[#6C757D] truncate">{d.label}</span>
          <div className="flex-1 bg-[#E9ECEF] rounded-full h-6 overflow-hidden">
            <div
              className="h-full rounded-full transition-all duration-500"
              style={{ width: `${(d.value / max) * 100}%`, backgroundColor: d.color }}
            />
          </div>
          <span className="w-12 text-sm text-right font-medium text-[#212529]">{d.value}</span>
        </div>
      ))}
    </div>
  );
}

function RevenueChart({ data }: { data: { date: string; revenue: number }[] }) {
  if (!data.length) return <p className="text-sm text-[#6C757D] py-4 text-center">No revenue data for this period</p>;
  const max = Math.max(...data.map((d) => d.revenue), 1);
  return (
    <div className="flex items-end gap-1 h-32 pt-2">
      {data.map((d, i) => (
        <div key={i} className="flex-1 flex flex-col items-center gap-1">
          <span className="text-[10px] text-[#6C757D]">{d.revenue > 0 ? `₱${Math.round(d.revenue / 1000)}k` : ''}</span>
          <div
            className="w-full rounded-t"
            style={{
              height: `${Math.max((d.revenue / max) * 100, 2)}%`,
              backgroundColor: '#2C7DA0',
              opacity: 0.6 + (d.revenue / max) * 0.4
            }}
          />
        </div>
      ))}
    </div>
  );
}

function DoctorCard({ title, doctor, metric }: { title: string; doctor: any; metric: string }) {
  if (!doctor) return null;
  return (
    <div className="bg-white rounded-xl border border-[#E9ECEF] p-4 shadow-sm">
      <p className="text-xs text-[#6C757D] uppercase tracking-wider font-semibold mb-2">{title}</p>
      <div className="flex items-center gap-3">
        <div className="w-10 h-10 rounded-full bg-[#2C7DA0] flex items-center justify-center text-white font-bold text-sm">
          {doctor.doctorName.split(' ').map((n: string) => n[0]).join('').slice(0, 2)}
        </div>
        <div className="flex-1">
          <p className="font-medium text-[#212529] text-sm">{doctor.doctorName}</p>
          <p className="text-xs text-[#6C757D]">{doctor.specialization}</p>
          <p className="text-xs text-[#2C7DA0] font-medium mt-1">{metric}</p>
        </div>
      </div>
    </div>
  );
}

export function Dashboard() {
  const { user, hasRole } = useAuth();
  const [stats, setStats] = useState<any>(null);
  const [doctorPerf, setDoctorPerf] = useState<any[]>([]);

  useEffect(() => {
    const loadStats = async () => {
      try {
        const summary = await reportService.getAppointmentSummary();
        const revenue = hasRole('Admin') || hasRole('Receptionist') ? await reportService.getRevenue() : null;
        const inventory = hasRole('Pharmacist') ? await reportService.getInventorySummary() : null;
        const perf = hasRole('Admin') ? await reportService.getDoctorPerformance() : null;
        setStats({ summary: summary.data, revenue: revenue?.data, inventory: inventory?.data, perf: perf?.data });
        if (perf?.data) setDoctorPerf(perf.data);
      } catch { /* ignore */ }
    };
    loadStats();
  }, []);

  const topAppointed = [...doctorPerf].sort((a, b) => b.appointmentsCompleted - a.appointmentsCompleted)[0];
  const topRevenue = [...doctorPerf].sort((a, b) => b.revenueGenerated - a.revenueGenerated)[0];

  const barData = stats?.summary ? [
    { label: 'Scheduled', value: stats.summary.scheduled, color: '#17A2B8' },
    { label: 'Confirmed', value: stats.summary.confirmed, color: '#28A745' },
    { label: 'Completed', value: stats.summary.completed, color: '#6C757D' },
    { label: 'Cancelled', value: stats.summary.cancelled, color: '#DC3545' },
    { label: 'No Show', value: stats.summary.noShow, color: '#FFC107' },
  ] : [];

  const revenueData = stats?.revenue?.dailyBreakdown || [];

  const showDoctorCards = hasRole('Admin') && doctorPerf.length > 0;

  return (
    <div>
      <div className="mb-8">
        <h1 className="text-2xl font-bold text-[#212529]">
          Welcome, {user?.firstName}!
        </h1>
        <p className="text-sm text-[#6C757D] mt-1">Here's what's happening at your clinic today.</p>
      </div>

      <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-4 gap-5 mb-6">
        <StatCard
          title="Total Appointments"
          value={stats?.summary?.total || 0}
          accentColor="#2C7DA0"
          trend={`Period: ${stats?.summary?.period?.split(' to ')[0] || ''}`}
          icon={
            <svg xmlns="http://www.w3.org/2000/svg" className="h-4 w-4" viewBox="0 0 24 24" fill="none" stroke="currentColor" strokeWidth="2">
              <rect x="3" y="4" width="18" height="18" rx="2" ry="2" />
              <line x1="16" y1="2" x2="16" y2="6" />
              <line x1="8" y1="2" x2="8" y2="6" />
              <line x1="3" y1="10" x2="21" y2="10" />
            </svg>
          }
        />
        <StatCard
          title="Completed"
          value={stats?.summary?.completed || 0}
          accentColor="#28A745"
          icon={
            <svg xmlns="http://www.w3.org/2000/svg" className="h-4 w-4" viewBox="0 0 24 24" fill="none" stroke="currentColor" strokeWidth="2">
              <polyline points="20 6 9 17 4 12" />
            </svg>
          }
        />
        <StatCard
          title="Cancelled"
          value={stats?.summary?.cancelled || 0}
          accentColor="#DC3545"
          icon={
            <svg xmlns="http://www.w3.org/2000/svg" className="h-4 w-4" viewBox="0 0 24 24" fill="none" stroke="currentColor" strokeWidth="2">
              <circle cx="12" cy="12" r="10" />
              <line x1="15" y1="9" x2="9" y2="15" />
              <line x1="9" y1="9" x2="15" y2="15" />
            </svg>
          }
        />
        <StatCard
          title="Revenue"
          value={stats?.revenue ? formatCurrency(stats.revenue.totalRevenue) : '—'}
          accentColor="#95D5B2"
          icon={
            <svg xmlns="http://www.w3.org/2000/svg" className="h-4 w-4" viewBox="0 0 24 24" fill="none" stroke="currentColor" strokeWidth="2">
              <line x1="12" y1="1" x2="12" y2="23" />
              <path d="M17 5H9.5a3.5 3.5 0 0 0 0 7h5a3.5 3.5 0 0 1 0 7H6" />
            </svg>
          }
        />
      </div>

      {showDoctorCards && (
        <div className="grid grid-cols-1 md:grid-cols-2 gap-5 mb-6">
          <DoctorCard
            title="Most Appointed Doctor"
            doctor={topAppointed}
            metric={`${topAppointed?.appointmentsCompleted || 0} appointments completed`}
          />
          <DoctorCard
            title="Highest Revenue Doctor"
            doctor={topRevenue}
            metric={`${topRevenue?.revenueGenerated ? formatCurrency(topRevenue.revenueGenerated) : '₱0'} generated`}
          />
        </div>
      )}

      <div className="grid grid-cols-1 lg:grid-cols-2 gap-6 mb-6">
        <div className="bg-white rounded-xl border border-[#E9ECEF] shadow-sm p-5">
          <h3 className="text-sm font-semibold text-[#212529] mb-4">Appointment Summary</h3>
          <BarChart data={barData} />
        </div>
        <div className="bg-white rounded-xl border border-[#E9ECEF] shadow-sm p-5">
          <h3 className="text-sm font-semibold text-[#212529] mb-4">Daily Revenue Trend</h3>
          {stats?.revenue ? (
            <>
              <div className="flex justify-between text-sm text-[#6C757D] mb-2">
                <span>{revenueData.length} days</span>
                <span>Total: {formatCurrency(stats.revenue.totalRevenue)}</span>
              </div>
              <RevenueChart data={revenueData} />
            </>
          ) : (
            <p className="text-sm text-[#6C757D] py-8 text-center">Revenue data available for Admin & Receptionist</p>
          )}
        </div>
      </div>

      {stats?.inventory && (
        <div className="grid grid-cols-1 md:grid-cols-3 gap-5 mb-6">
          <div className="bg-white rounded-xl border border-[#E9ECEF] shadow-sm p-4">
            <p className="text-xs text-[#6C757D] uppercase tracking-wider font-semibold">Total Stock Value</p>
            <p className="text-xl font-bold text-[#212529] mt-1">{formatCurrency(stats.inventory.totalStockValue)}</p>
          </div>
          <div className="bg-white rounded-xl border border-[#E9ECEF] shadow-sm p-4">
            <p className="text-xs text-[#6C757D] uppercase tracking-wider font-semibold">Low Stock Items</p>
            <p className="text-xl font-bold text-[#DC3545] mt-1">{stats.inventory.lowStockCount}</p>
          </div>
          <div className="bg-white rounded-xl border border-[#E9ECEF] shadow-sm p-4">
            <p className="text-xs text-[#6C757D] uppercase tracking-wider font-semibold">Expiring Soon (90d)</p>
            <p className="text-xl font-bold text-[#FFC107] mt-1">{stats.inventory.expiringCount}</p>
          </div>
        </div>
      )}
    </div>
  );
}