import { useLocation } from 'react-router-dom';

const pathTitles: Record<string, string> = {
  '/dashboard': 'Dashboard',
  '/patients': 'Patients',
  '/appointments': 'Appointments',
  '/doctors': 'Doctors',
  '/medical-records': 'Medical Records',
  '/inventory': 'Inventory',
  '/billings': 'Billings',
  '/pharmacy': 'Pharmacy',
  '/reports': 'Reports',
  '/users': 'Users',
  '/archives': 'Archives',
  '/audit-logs': 'Audit Logs',
};

export function Header() {
  const location = useLocation();
  const title = pathTitles[location.pathname] || 'Dashboard';

  return (
    <header className="h-16 bg-white border-b border-[#E9ECEF] flex items-center justify-between px-6">
      <div>
        <h2 className="text-lg font-semibold text-[#212529]">{title}</h2>
      </div>
    </header>
  );
}
