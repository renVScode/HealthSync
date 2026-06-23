export function Header() {
  return (
    <header className="h-16 bg-white border-b border-[#E9ECEF] flex items-center justify-between px-6">
      <div>
        <h2 className="text-lg font-semibold text-[#212529]">Dashboard</h2>
      </div>
      <div className="flex items-center gap-4">
        <button className="relative text-[#6C757D] hover:text-[#212529]">
          <svg xmlns="http://www.w3.org/2000/svg" className="h-5 w-5" viewBox="0 0 24 24" fill="none" stroke="currentColor" strokeWidth="2" strokeLinecap="round" strokeLinejoin="round">
            <path d="M18 8A6 6 0 0 0 6 8c0 7-3 9-3 9h18s-3-2-3-9" />
            <path d="M13.73 21a2 2 0 0 1-3.46 0" />
          </svg>
        </button>
      </div>
    </header>
  );
}
