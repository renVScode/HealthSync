import { useLocation } from 'react-router-dom';

interface HeaderProps {
  onMenuClick: () => void;
}

const pathTitles: Record<string, string> = {
  '/dashboard': 'Dashboard',
  '/patients': 'Patients',
  '/appointments': 'Appointments',
  '/doctors': 'Doctors',
  '/medical-records': 'Medical Records',
  '/lab-tests': 'Lab Tests',
  '/inventory': 'Inventory',
  '/billings': 'Billings',
  '/pharmacy': 'Pharmacy',
  '/reports': 'Reports',
  '/users': 'Users',
  '/archives': 'Archives',
  '/audit-logs': 'Audit Logs',
};

export function Header({ onMenuClick }: HeaderProps) {
  const location = useLocation();
  const title = Object.entries(pathTitles).find(([key]) => location.pathname.startsWith(key))?.[1] || 'Dashboard';

  return (
    <header className="h-16 bg-white border-b border-[#E9ECEF] flex items-center justify-between px-4 md:px-6">
      <div className="flex items-center gap-3">
        <button onClick={onMenuClick} className="lg:hidden w-8 h-8 flex items-center justify-center rounded-md text-[#6C757D] hover:bg-[#F8F9FA] transition-colors">
          <svg xmlns="http://www.w3.org/2000/svg" className="h-5 w-5" viewBox="0 0 24 24" fill="none" stroke="currentColor" strokeWidth="2"><line x1="3" y1="6" x2="21" y2="6" /><line x1="3" y1="12" x2="21" y2="12" /><line x1="3" y1="18" x2="21" y2="18" /></svg>
        </button>
        <h2 className="text-lg font-semibold text-[#212529]">{title}</h2>
      </div>
    </header>
  );
}
