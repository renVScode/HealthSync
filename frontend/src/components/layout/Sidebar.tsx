import { NavLink } from 'react-router-dom';
import { useAuth } from '../../contexts/auth-context';
import { useState } from 'react';

interface MenuItem {
  path: string;
  label: string;
  icon: React.ReactNode;
  roles: string[];
}

function Icon({ children }: { children: React.ReactNode }) {
  return <svg xmlns="http://www.w3.org/2000/svg" className="h-5 w-5 shrink-0" viewBox="0 0 24 24" fill="none" stroke="currentColor" strokeWidth="2" strokeLinecap="round" strokeLinejoin="round">{children}</svg>;
}

const menuItems: MenuItem[] = [
  { path: '/dashboard', label: 'Dashboard', icon: <Icon><rect x="3" y="3" width="7" height="7" /><rect x="14" y="3" width="7" height="7" /><rect x="14" y="14" width="7" height="7" /><rect x="3" y="14" width="7" height="7" /></Icon>, roles: ['Admin', 'Doctor', 'Receptionist', 'Pharmacist'] },
  { path: '/patients', label: 'Patients', icon: <Icon><path d="M20 21v-2a4 4 0 0 0-4-4H8a4 4 0 0 0-4 4v2" /><circle cx="12" cy="7" r="4" /></Icon>, roles: ['Admin', 'Doctor', 'Receptionist'] },
  { path: '/doctors', label: 'Doctors', icon: <Icon><path d="M12 2a4 4 0 1 0 0 8 4 4 0 0 0 0-8z" /><path d="M16 21v-2a4 4 0 0 0-4-4h-2a4 4 0 0 0-4 4v2" /><path d="M19 12v6" /><path d="M22 15h-6" /></Icon>, roles: ['Admin', 'Doctor', 'Receptionist'] },
  { path: '/appointments', label: 'Appointments', icon: <Icon><rect x="3" y="4" width="18" height="18" rx="2" ry="2" /><line x1="16" y1="2" x2="16" y2="6" /><line x1="8" y1="2" x2="8" y2="6" /><line x1="3" y1="10" x2="21" y2="10" /></Icon>, roles: ['Admin', 'Doctor', 'Receptionist', 'Pharmacist'] },
  { path: '/medical-records', label: 'Medical Records', icon: <Icon><path d="M16 4h2a2 2 0 0 1 2 2v14a2 2 0 0 1-2 2H6a2 2 0 0 1-2-2V6a2 2 0 0 1 2-2h2" /><rect x="8" y="2" width="8" height="4" rx="1" ry="1" /><line x1="8" y1="12" x2="16" y2="12" /><line x1="8" y1="16" x2="14" y2="16" /></Icon>, roles: ['Admin', 'Doctor', 'Receptionist'] },
  { path: '/lab-tests', label: 'Lab Tests', icon: <Icon><path d="M10 2v6l-4 4v2h12v-2l-4-4V2" /><path d="M4 22h16" /><path d="M9 2h6" /></Icon>, roles: ['Admin', 'Doctor', 'Receptionist'] },
  { path: '/billings', label: 'Billings', icon: <Icon><line x1="12" y1="1" x2="12" y2="23" /><path d="M17 5H9.5a3.5 3.5 0 0 0 0 7h5a3.5 3.5 0 0 1 0 7H6" /></Icon>, roles: ['Admin', 'Receptionist'] },
  { path: '/inventory', label: 'Inventory', icon: <Icon><path d="M10.5 4.5L3 9l7.5 4.5L18 9l-7.5-4.5z" /><path d="M3 9v6l7.5 4.5L18 15V9" /><line x1="10.5" y1="13.5" x2="10.5" y2="22" /><line x1="18" y1="9" x2="18" y2="15" /></Icon>, roles: ['Admin', 'Pharmacist'] },
  { path: '/pharmacy', label: 'Pharmacy', icon: <Icon><polygon points="13 2 3 14 12 14 11 22 21 10 12 10 13 2" /></Icon>, roles: ['Admin', 'Pharmacist'] },
  { path: '/reports', label: 'Reports', icon: <Icon><polyline points="23 6 13.5 15.5 8.5 10.5 1 18" /><polyline points="17 6 23 6 23 12" /></Icon>, roles: ['Admin', 'Doctor', 'Receptionist', 'Pharmacist'] },
  { path: '/audit-logs', label: 'Audit Logs', icon: <Icon><path d="M14 2H6a2 2 0 0 0-2 2v16a2 2 0 0 0 2 2h12a2 2 0 0 0 2-2V8z" /><polyline points="14 2 14 8 20 8" /><line x1="16" y1="13" x2="8" y2="13" /><line x1="16" y1="17" x2="8" y2="17" /><polyline points="10 9 9 9 8 9" /></Icon>, roles: ['Admin'] },
  { path: '/archives', label: 'Archives', icon: <Icon><polyline points="21 8 21 21 3 21 3 8" /><rect x="1" y="3" width="22" height="5" /><line x1="10" y1="12" x2="14" y2="12" /></Icon>, roles: ['Admin', 'Doctor', 'Receptionist', 'Pharmacist'] },
  { path: '/users', label: 'Users', icon: <Icon><rect x="3" y="11" width="18" height="11" rx="2" ry="2" /><path d="M7 11V7a5 5 0 0 1 10 0v4" /></Icon>, roles: ['Admin'] },
];

function SidebarItem({ item }: { item: MenuItem }) {
  const [showTooltip, setShowTooltip] = useState(false);
  let tooltipTimer: ReturnType<typeof setTimeout>;

  return (
    <div className="relative">
      <NavLink
        to={item.path}
        onMouseEnter={() => {
          tooltipTimer = setTimeout(() => setShowTooltip(true), 150);
        }}
        onMouseLeave={() => {
          clearTimeout(tooltipTimer);
          setShowTooltip(false);
        }}
        className={({ isActive }) =>
          `group flex items-center gap-3 px-4 py-2.5 rounded-md text-sm transition-all duration-200 relative
          ${isActive
            ? 'text-white bg-white/10'
            : 'text-white/40 hover:text-white/80 hover:bg-white/5'}`
        }
      >
        {({ isActive }) => (
          <>
            {isActive && (
              <span className="absolute left-0 top-1/2 -translate-y-1/2 w-1 h-6 bg-[#80CED7] rounded-r-full" />
            )}
            {item.icon}
            <span>{item.label}</span>
          </>
        )}
      </NavLink>
      {showTooltip && (
        <div className="absolute left-full ml-3 top-1/2 -translate-y-1/2 z-50 pointer-events-none">
          <div className="bg-[#212529] text-white text-xs px-2.5 py-1.5 rounded-md shadow-lg whitespace-nowrap">
            {item.label}
          </div>
        </div>
      )}
    </div>
  );
}

export function Sidebar() {
  const { user, hasRole, logout } = useAuth();

  const filteredItems = menuItems.filter((item) =>
    item.roles.some((r) => hasRole(r))
  );

  return (
    <aside className="w-64 bg-[#1A4B61] flex flex-col shadow-[4px_0_12px_rgba(0,0,0,0.08)]">
      <div className="h-16 flex items-center px-6 border-b border-white/10 shrink-0">
        <div className="flex items-center gap-3">
          <img src="/healthsync-icon.png" alt="HealthSync" className="w-8 h-8 object-contain" />
          <h1 className="text-lg font-bold text-white">HealthSync</h1>
        </div>
      </div>

      <nav className="flex-1 p-3 space-y-0.5 overflow-y-auto">
        {filteredItems.map((item) => (
          <SidebarItem key={item.path} item={item} />
        ))}
      </nav>

      <div className="p-3 pt-0 mt-auto">
        <button
          onClick={logout}
          className="flex items-center gap-3 w-full px-4 py-2.5 rounded-md text-sm text-white/40 hover:text-[#DC3545] hover:bg-white/5 transition-all duration-200"
        >
          <svg xmlns="http://www.w3.org/2000/svg" className="h-5 w-5 shrink-0" viewBox="0 0 24 24" fill="none" stroke="currentColor" strokeWidth="2" strokeLinecap="round" strokeLinejoin="round">
            <path d="M9 21H5a2 2 0 0 1-2-2V5a2 2 0 0 1 2-2h4" />
            <polyline points="16 17 21 12 16 7" />
            <line x1="21" y1="12" x2="9" y2="12" />
          </svg>
          <span>Logout</span>
        </button>
      </div>

      <div className="p-4 border-t border-white/10 shrink-0">
        <p className="text-sm text-white/60">{user?.firstName} {user?.lastName}</p>
        <p className="text-xs text-white/40">{user?.role}</p>
      </div>
    </aside>
  );
}
