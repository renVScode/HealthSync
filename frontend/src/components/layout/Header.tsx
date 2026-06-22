import { useAuth } from '../../contexts/auth-context';

export function Header() {
  const { user, logout } = useAuth();

  return (
    <header className="h-16 bg-white border-b border-[#E9ECEF] flex items-center justify-between px-6">
      <div>
        <h2 className="text-lg font-semibold text-[#212529]">Dashboard</h2>
      </div>
      <div className="flex items-center gap-4">
        <button className="relative text-[#6C757D] hover:text-[#212529]">
          <span className="text-lg">🔔</span>
        </button>
        <div className="flex items-center gap-2">
          <div className="text-right">
            <p className="text-sm font-medium text-[#212529]">{user?.firstName} {user?.lastName}</p>
            <p className="text-xs text-[#6C757D]">{user?.role}</p>
          </div>
          <button onClick={logout} className="text-sm text-[#DC3545] hover:underline">
            Logout
          </button>
        </div>
      </div>
    </header>
  );
}
