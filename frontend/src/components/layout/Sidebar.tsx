import { NavLink } from 'react-router-dom';
import { useAuth } from '../../contexts/auth-context';
import { useState } from 'react';

interface MenuItem {
  path: string;
  label: string;
  icon: string;
  roles: string[];
}

const menuItems: MenuItem[] = [
  { path: '/dashboard', label: 'Dashboard', icon: '📊', roles: ['Admin', 'Doctor', 'Receptionist', 'Pharmacist'] },
  { path: '/patients', label: 'Patients', icon: '👤', roles: ['Admin', 'Doctor', 'Receptionist'] },
  { path: '/doctors', label: 'Doctors', icon: '👨‍⚕️', roles: ['Admin', 'Doctor', 'Receptionist'] },
  { path: '/appointments', label: 'Appointments', icon: '📅', roles: ['Admin', 'Doctor', 'Receptionist', 'Pharmacist'] },
  { path: '/medical-records', label: 'Medical Records', icon: '📋', roles: ['Admin', 'Doctor', 'Receptionist'] },
  { path: '/billings', label: 'Billings', icon: '💰', roles: ['Admin', 'Receptionist'] },
  { path: '/inventory', label: 'Inventory', icon: '💊', roles: ['Admin', 'Pharmacist'] },
  { path: '/reports', label: 'Reports', icon: '📈', roles: ['Admin', 'Doctor', 'Receptionist', 'Pharmacist'] },
  { path: '/users', label: 'Users', icon: '🔐', roles: ['Admin'] },
  { path: '/settings', label: 'Settings', icon: '⚙️', roles: ['Admin'] },
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
            <span className="text-lg">{item.icon}</span>
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
  const { user, hasRole } = useAuth();

  const filteredItems = menuItems.filter((item) =>
    item.roles.some((r) => hasRole(r))
  );

  return (
    <aside className="w-64 bg-[#1A4B61] flex flex-col shadow-[4px_0_12px_rgba(0,0,0,0.08)]">
      <div className="h-16 flex items-center px-6 border-b border-white/10">
        <div className="flex items-center gap-3">
          <div className="w-8 h-8 bg-white/15 rounded-lg flex items-center justify-center">
            <svg xmlns="http://www.w3.org/2000/svg" className="h-4 w-4 text-white" viewBox="0 0 24 24" fill="none" stroke="currentColor" strokeWidth="2">
              <path d="M19 14l-7 7m0 0l-7-7m7 7V3" />
            </svg>
          </div>
          <h1 className="text-lg font-bold text-white">HealthSync</h1>
        </div>
      </div>
      <nav className="flex-1 p-3 space-y-0.5 overflow-y-auto">
        {filteredItems.map((item) => (
          <SidebarItem key={item.path} item={item} />
        ))}
      </nav>
      <div className="p-4 border-t border-white/10">
        <p className="text-sm text-white/60">{user?.firstName} {user?.lastName}</p>
        <p className="text-xs text-white/40">{user?.role}</p>
      </div>
    </aside>
  );
}
