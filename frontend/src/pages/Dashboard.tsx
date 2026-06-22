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
      <div className="p-5">
        <div className="flex items-start justify-between mb-3">
          <p className="text-sm text-[#6C757D]">{title}</p>
          <div className="w-9 h-9 rounded-full flex items-center justify-center shrink-0" style={{ backgroundColor: `${accentColor}15` }}>
            <span style={{ color: accentColor }}>{icon}</span>
          </div>
        </div>
        <p className="text-3xl font-bold text-[#212529] mb-1">{value}</p>
        {trend && (
          <div className="flex items-center gap-1 text-xs">
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
  );
}

export function Dashboard() {
  const { user, hasRole } = useAuth();
  const [stats, setStats] = useState<any>(null);

  useEffect(() => {
    const loadStats = async () => {
      try {
        const summary = await reportService.getAppointmentSummary();
        const revenue = hasRole('Admin') || hasRole('Receptionist') ? await reportService.getRevenue() : null;
        const inventory = hasRole('Pharmacist') ? await reportService.getInventorySummary() : null;
        setStats({ summary: summary.data, revenue: revenue?.data, inventory: inventory?.data });
      } catch { /* ignore */ }
    };
    loadStats();
  }, []);

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
          trend="+12% vs yesterday"
          trendUp={true}
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
          trend="+5% vs yesterday"
          trendUp={true}
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
          trend="-2% vs yesterday"
          trendUp={false}
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
          trend="+8% vs yesterday"
          trendUp={true}
          icon={
            <svg xmlns="http://www.w3.org/2000/svg" className="h-4 w-4" viewBox="0 0 24 24" fill="none" stroke="currentColor" strokeWidth="2">
              <line x1="12" y1="1" x2="12" y2="23" />
              <path d="M17 5H9.5a3.5 3.5 0 0 0 0 7h5a3.5 3.5 0 0 1 0 7H6" />
            </svg>
          }
        />
      </div>
    </div>
  );
}
