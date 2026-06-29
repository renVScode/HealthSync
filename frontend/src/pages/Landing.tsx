import { useNavigate } from 'react-router-dom';

export function Landing() {
  const navigate = useNavigate();

  return (
    <div className="min-h-screen bg-[#F8F9FA] font-['Inter',system-ui,sans-serif]">

      {/* ── NAV BAR ──────────────────────────────────────── */}
      <nav className="h-16 bg-white border-b border-[#E9ECEF] flex items-center justify-between px-8 lg:px-16 sticky top-0 z-50">
        <div className="flex items-center gap-3">
          <img src="/healthsync-icon.png" alt="HealthSync" className="w-8 h-8 object-contain" />
          <span className="text-lg font-bold text-[#212529]">HealthSync</span>
        </div>
        <div className="flex items-center gap-4">
          <button
            onClick={() => navigate('/login')}
            className="px-5 py-2 bg-[#2C7DA0] text-white text-sm font-medium rounded-lg shadow-sm hover:bg-[#1F5E7A] transition-all duration-200"
          >
            Sign In
          </button>
        </div>
      </nav>

      {/* ── SECTION 1: HERO ───────────────────────────────── */}
      <section className="max-w-7xl mx-auto px-8 lg:px-16 py-20 lg:py-28">
        <div className="flex flex-col lg:flex-row items-center gap-16">

          {/* Left: Text + CTAs */}
          <div className="flex-1 lg:max-w-[55%]">
            <h1 className="text-4xl lg:text-5xl font-extrabold text-[#212529] leading-tight tracking-tight">
              The ERP Clinic Staff{' '}
              <span className="text-[#2C7DA0]">Actually Enjoy Using</span>
            </h1>
            <p className="mt-6 text-lg text-[#6C757D] leading-relaxed max-w-xl">
              Streamline patient workflows, manage billing, and track inventory without the system fatigue.
              Built for modern medical workstations.
            </p>
            <div className="mt-10 flex flex-wrap items-center gap-4">
              <button className="px-8 py-3.5 bg-[#2C7DA0] text-white font-semibold rounded-lg shadow-md shadow-[#2C7DA0]/20 hover:bg-[#1F5E7A] hover:shadow-lg hover:shadow-[#2C7DA0]/30 transition-all duration-200">
                Book a Live Demo
              </button>
              <button className="px-8 py-3.5 bg-white text-[#2C7DA0] font-semibold rounded-lg border border-[#80CED7] hover:bg-[#F0F9FA] hover:border-[#2C7DA0] transition-all duration-200">
                Watch 2-Min Tour
              </button>
            </div>
          </div>

          {/* Right: Dashboard Mockup */}
          <div className="flex-1 lg:max-w-[45%]">
            <div className="relative">
              <div className="absolute -inset-4 bg-gradient-to-br from-[#80CED7]/20 to-[#2C7DA0]/10 rounded-3xl blur-2xl" />
              <div className="relative bg-white rounded-2xl shadow-2xl shadow-black/10 overflow-hidden rotate-[1.5deg] hover:rotate-0 transition-transform duration-500">
                {/* Mockup Top Bar */}
                <div className="h-8 bg-[#F8F9FA] border-b border-[#E9ECEF] flex items-center px-4 gap-1.5">
                  <div className="w-2.5 h-2.5 rounded-full bg-[#DC3545]" />
                  <div className="w-2.5 h-2.5 rounded-full bg-[#FFC107]" />
                  <div className="w-2.5 h-2.5 rounded-full bg-[#28A745]" />
                </div>
                <div className="flex" style={{ height: 280 }}>
                  {/* Mockup Sidebar */}
                  <div className="w-16 bg-[#1A4B61] flex flex-col items-center py-3 gap-3 shrink-0">
                    {[...Array(6)].map((_, i) => (
                      <div key={i} className={`w-8 h-8 rounded-lg ${i === 0 ? 'bg-white/20' : 'bg-white/10'} flex items-center justify-center`}>
                        <div className="w-4 h-4 rounded bg-white/30" />
                      </div>
                    ))}
                  </div>
                  {/* Mockup Content */}
                  <div className="flex-1 p-4 space-y-3">
                    {/* Stat cards row */}
                    <div className="flex gap-3">
                      {[1, 2, 3].map((s) => (
                        <div key={s} className="flex-1 bg-white rounded-lg border border-[#E9ECEF] p-3">
                          <div className="h-1 w-8 rounded-full mb-2" style={{ backgroundColor: s === 1 ? '#2C7DA0' : s === 2 ? '#28A745' : '#95D5B2' }} />
                          <div className="h-3 w-16 bg-[#F0F4F8] rounded mb-2" />
                          <div className="h-5 w-10 bg-[#E9ECEF] rounded" />
                        </div>
                      ))}
                    </div>
                    {/* Table header */}
                    <div className="flex gap-4">
                      {[1, 2, 3, 4].map((h) => (
                        <div key={h} className="h-3 flex-1 bg-[#F0F4F8] rounded" />
                      ))}
                    </div>
                    {/* Table rows */}
                    {[1, 2, 3].map((r) => (
                      <div key={r} className={`flex gap-4 py-2 ${r % 2 === 0 ? 'bg-[#F8F9FA]' : ''} rounded px-2 -mx-2`}>
                        {[1, 2, 3, 4].map((c) => (
                          <div key={c} className={`h-3 flex-1 rounded ${c === 3 ? 'bg-[#80CED7]/30 w-12' : 'bg-[#E9ECEF]'}`} />
                        ))}
                      </div>
                    ))}
                    {/* Status badges */}
                    <div className="flex gap-2 pt-1">
                      <div className="h-5 w-16 bg-green-50 rounded-full flex items-center justify-center">
                        <div className="h-2 w-8 bg-green-300 rounded" />
                      </div>
                      <div className="h-5 w-16 bg-yellow-50 rounded-full flex items-center justify-center">
                        <div className="h-2 w-8 bg-yellow-300 rounded" />
                      </div>
                    </div>
                  </div>
                </div>
              </div>
            </div>
          </div>
        </div>
      </section>

      {/* ── SECTION 2: TRUST BAR ─────────────────────────── */}
      <section className="bg-white border-y border-[#E9ECEF] py-10">
        <div className="max-w-5xl mx-auto px-8 text-center">
          <p className="text-xs text-[#ADB5BD] uppercase tracking-[0.15em] font-semibold mb-6">
            Trusted by over 1,200+ clinical professionals nationwide
          </p>
          <div className="flex flex-wrap items-center justify-center gap-x-12 gap-y-6 opacity-40">
            {[' ManilaMed', 'StLuke\'s', 'MakatiMed', 'AsianHosp', 'TheMedCity', 'Perpetual'].map((name) => (
              <div key={name} className="h-8 flex items-center">
                <div className="w-28 h-6 bg-[#CED4DA] rounded" />
              </div>
            ))}
          </div>
        </div>
      </section>

      {/* ── SECTION 3: FEATURE GRID ────────────────────────── */}
      <section className="max-w-7xl mx-auto px-8 lg:px-16 py-24">
        <div className="text-center mb-16">
          <p className="text-sm font-semibold text-[#2C7DA0] uppercase tracking-[0.12em] mb-3">Tailored to the Team</p>
          <h2 className="text-3xl lg:text-4xl font-bold text-[#212529]">One platform, every role</h2>
        </div>

        <div className="grid grid-cols-1 md:grid-cols-3 gap-6">
          {/* Card 1: Doctors */}
          <div className="group bg-white rounded-2xl border border-[#E9ECEF] p-8 hover:-translate-y-1 hover:shadow-lg transition-all duration-300 relative overflow-hidden">
            <div className="absolute top-0 left-0 right-0 h-1 bg-[#80CED7] scale-x-0 group-hover:scale-x-100 transition-transform duration-300 origin-left" />
            <div className="w-12 h-12 bg-[#F0F9FA] rounded-xl flex items-center justify-center mb-5">
              <svg xmlns="http://www.w3.org/2000/svg" className="h-6 w-6 text-[#2C7DA0]" viewBox="0 0 24 24" fill="none" stroke="currentColor" strokeWidth="2" strokeLinecap="round" strokeLinejoin="round">
                <path d="M22 12h-4l-3 9L9 3l-3 9H2" />
              </svg>
            </div>
            <h3 className="text-lg font-semibold text-[#212529] mb-3">For Doctors</h3>
            <p className="text-sm text-[#6C757D] leading-relaxed">
              Real-time patient history at your fingertips. Access lab results, past diagnoses, and prescriptions in one click — no more flipping through paper charts.
            </p>
          </div>

          {/* Card 2: Administrators */}
          <div className="group bg-white rounded-2xl border border-[#E9ECEF] p-8 hover:-translate-y-1 hover:shadow-lg transition-all duration-300 relative overflow-hidden">
            <div className="absolute top-0 left-0 right-0 h-1 bg-[#80CED7] scale-x-0 group-hover:scale-x-100 transition-transform duration-300 origin-left" />
            <div className="w-12 h-12 bg-[#F0F9FA] rounded-xl flex items-center justify-center mb-5">
              <svg xmlns="http://www.w3.org/2000/svg" className="h-6 w-6 text-[#2C7DA0]" viewBox="0 0 24 24" fill="none" stroke="currentColor" strokeWidth="2" strokeLinecap="round" strokeLinejoin="round">
                <rect x="3" y="11" width="18" height="11" rx="2" ry="2" />
                <path d="M7 11V7a5 5 0 0 1 10 0v4" />
              </svg>
            </div>
            <h3 className="text-lg font-semibold text-[#212529] mb-3">For Administrators</h3>
            <p className="text-sm text-[#6C757D] leading-relaxed">
              Role-based access control, financial analytics dashboards, and full audit trails. Know exactly who accessed what, and when — compliance made simple.
            </p>
          </div>

          {/* Card 3: Inventory Managers */}
          <div className="group bg-white rounded-2xl border border-[#E9ECEF] p-8 hover:-translate-y-1 hover:shadow-lg transition-all duration-300 relative overflow-hidden">
            <div className="absolute top-0 left-0 right-0 h-1 bg-[#80CED7] scale-x-0 group-hover:scale-x-100 transition-transform duration-300 origin-left" />
            <div className="w-12 h-12 bg-[#F0F9FA] rounded-xl flex items-center justify-center mb-5">
              <svg xmlns="http://www.w3.org/2000/svg" className="h-6 w-6 text-[#2C7DA0]" viewBox="0 0 24 24" fill="none" stroke="currentColor" strokeWidth="2" strokeLinecap="round" strokeLinejoin="round">
                <path d="M10.5 4.5L3 9l7.5 4.5L18 9l-7.5-4.5z" />
                <path d="M3 9v6l7.5 4.5L18 15V9" />
                <line x1="10.5" y1="13.5" x2="10.5" y2="22" />
                <line x1="18" y1="9" x2="18" y2="15" />
              </svg>
            </div>
            <h3 className="text-lg font-semibold text-[#212529] mb-3">For Inventory Managers</h3>
            <p className="text-sm text-[#6C757D] leading-relaxed">
              Automated low-stock alerts with color-coded thresholds. Track batch expiry dates, supplier performance, and dispensing history — never run out of essentials.
            </p>
          </div>
        </div>
      </section>

      {/* ── SECTION 4: INTERFACE PREVIEW ───────────────────── */}
      <section className="bg-white py-24">
        <div className="max-w-7xl mx-auto px-8 lg:px-16">
          <div className="text-center mb-16">
            <h2 className="text-3xl lg:text-4xl font-bold text-[#212529] mb-4">Say Goodbye to Clutter</h2>
            <p className="text-lg text-[#6C757D] max-w-2xl mx-auto">
              No more juggling windows. Everything lives inside a single, high-density interface designed for speed.
            </p>
          </div>

          <div className="grid grid-cols-1 lg:grid-cols-2 gap-12 items-center">
            {/* Left: High-density DataTable showcase */}
            <div className="bg-white rounded-2xl border border-[#E9ECEF] shadow-lg overflow-hidden">
              <div className="px-6 py-4 border-b border-[#E9ECEF]">
                <div className="flex items-center gap-3">
                  <div className="w-8 h-8 bg-[#F0F4F8] rounded-lg flex items-center justify-center">
                    <svg xmlns="http://www.w3.org/2000/svg" className="h-4 w-4 text-[#2C7DA0]" viewBox="0 0 24 24" fill="none" stroke="currentColor" strokeWidth="2" strokeLinecap="round" strokeLinejoin="round">
                      <rect x="3" y="4" width="18" height="18" rx="2" ry="2" />
                      <line x1="16" y1="2" x2="16" y2="6" />
                      <line x1="8" y1="2" x2="8" y2="6" />
                      <line x1="3" y1="10" x2="21" y2="10" />
                    </svg>
                  </div>
                  <span className="text-sm font-semibold text-[#212529]">Today's Appointments</span>
                </div>
              </div>
              <div className="overflow-x-auto">
                <table className="w-full">
                  <thead>
                    <tr className="bg-[#F0F4F8]">
                      {['Patient', 'Doctor', 'Time', 'Status'].map((h) => (
                        <th key={h} className="px-4 py-3 text-left text-xs font-semibold text-[#6C757D] uppercase tracking-wider">{h}</th>
                      ))}
                    </tr>
                  </thead>
                  <tbody>
                    {[
                      { name: 'Maria Santos', doctor: 'Dr. Smith', time: '09:00', status: 'Completed' },
                      { name: 'Juan Dela Cruz', doctor: 'Dr. Chen', time: '09:15', status: 'Confirmed' },
                      { name: 'Pedro Gonzales', doctor: 'Dr. Smith', time: '10:00', status: 'InProgress' },
                      { name: 'Ana Reyes', doctor: 'Dr. Chen', time: '10:30', status: 'Scheduled' },
                      { name: 'Carlos Mendoza', doctor: 'Dr. Smith', time: '11:00', status: 'Scheduled' },
                    ].map((row, i) => (
                      <tr key={i} className={`${i % 2 === 0 ? 'bg-white' : 'bg-gray-50/50'} border-b border-[#E9ECEF]`}>
                        <td className="px-4 py-3 text-sm text-[#212529] font-medium">{row.name}</td>
                        <td className="px-4 py-3 text-sm text-[#6C757D]">{row.doctor}</td>
                        <td className="px-4 py-3 text-sm text-[#6C757D]">{row.time}</td>
                        <td className="px-4 py-3">
                          <span className={`inline-flex items-center px-2.5 py-0.5 text-xs font-medium rounded-full ${row.status === 'Completed' ? 'bg-green-50 text-green-700' :
                            row.status === 'Confirmed' ? 'bg-blue-50 text-blue-700' :
                              row.status === 'InProgress' ? 'bg-yellow-50 text-yellow-700' :
                                'bg-gray-100 text-gray-500'
                            }`}>
                            {row.status === 'InProgress' ? 'In Progress' : row.status}
                          </span>
                        </td>
                      </tr>
                    ))}
                  </tbody>
                </table>
              </div>
            </div>

            {/* Right: Modal + blur effect showcase */}
            <div className="relative">
              <div className="bg-white rounded-2xl border border-[#E9ECEF] shadow-lg overflow-hidden">
                <div className="px-6 py-4 border-b border-[#E9ECEF]">
                  <span className="text-sm font-semibold text-[#212529]">New Patient Registration</span>
                </div>
                <div className="p-6 space-y-4 relative">
                  <div className="grid grid-cols-2 gap-4">
                    {['First Name', 'Last Name', 'Phone', 'Email'].map((label) => (
                      <div key={label}>
                        <label className="block text-xs font-medium text-[#6C757D] mb-1">{label}</label>
                        <div className="h-9 bg-[#F8F9FA] border border-[#E9ECEF] rounded-lg" />
                      </div>
                    ))}
                  </div>
                  <div>
                    <label className="block text-xs font-medium text-[#6C757D] mb-1">Address</label>
                    <div className="h-16 bg-[#F8F9FA] border border-[#E9ECEF] rounded-lg" />
                  </div>
                  <div className="flex justify-end gap-2 pt-2">
                    <div className="h-9 w-20 bg-[#F8F9FA] border border-[#E9ECEF] rounded-lg" />
                    <div className="h-9 w-28 bg-[#2C7DA0] rounded-lg" />
                  </div>

                  {/* Backdrop blur overlay simulation */}
                  <div className="absolute inset-0 bg-black/20 backdrop-blur-sm rounded-2xl flex items-center justify-center opacity-0 group-hover:opacity-100 transition-opacity">
                    <div className="bg-white rounded-xl shadow-xl p-6 max-w-sm mx-4">
                      <div className="flex items-center justify-between mb-4">
                        <div className="h-5 w-32 bg-[#E9ECEF] rounded" />
                        <div className="w-6 h-6 bg-[#F8F9FA] rounded-lg" />
                      </div>
                      <div className="h-8 bg-[#2C7DA0] rounded-lg w-full" />
                    </div>
                  </div>
                </div>
              </div>
              <div className="mt-4 flex items-center gap-2 text-sm text-[#6C757D]">
                <svg xmlns="http://www.w3.org/2000/svg" className="h-4 w-4 text-[#28A745]" viewBox="0 0 24 24" fill="none" stroke="currentColor" strokeWidth="2" strokeLinecap="round" strokeLinejoin="round">
                  <polyline points="20 6 9 17 4 12" />
                </svg>
                Modal forms with soft backdrop blur keep you focused on the task
              </div>
            </div>
          </div>
        </div>
      </section>

      {/* ── SECTION 5: FINAL CTA ──────────────────────────── */}
      <section className="bg-gradient-to-br from-[#1A4B61] to-[#2C7DA0] py-20">
        <div className="max-w-3xl mx-auto px-8 text-center">
          <h2 className="text-3xl lg:text-4xl font-bold text-white mb-4">
            Ready to transform your clinic's operations?
          </h2>
          <p className="text-lg text-[#80CED7] mb-10 max-w-xl mx-auto">
            Join hundreds of clinical teams already using HealthSync.
          </p>
          <button className="px-10 py-4 bg-white text-[#1A4B61] font-bold rounded-lg shadow-lg hover:shadow-xl hover:-translate-y-0.5 transition-all duration-200">
            Schedule a Consultation
          </button>
        </div>
      </section>

      {/* ── FOOTER ──────────────────────────────────────────── */}
      <footer className="bg-white border-t border-[#E9ECEF] py-12">
        <div className="max-w-7xl mx-auto px-8 lg:px-16">
          <div className="flex flex-wrap items-start justify-between gap-8 mb-10">
            <div>
              <div className="flex items-center gap-2 mb-4">
                <img src="/healthsync-icon.png" alt="HealthSync" className="w-7 h-7 object-contain" />
                <span className="text-sm font-bold text-[#212529]">HealthSync</span>
              </div>
              <p className="text-xs text-[#6C757D] max-w-xs">© 2026 HealthSync. All rights reserved.</p>
            </div>
            <div className="flex flex-wrap gap-x-10 gap-y-4">
              {['Features', 'Pricing', 'Security', 'Compliance', 'Contact'].map((link) => (
                <button key={link} className="text-sm text-[#6C757D] hover:text-[#212529] transition-colors">
                  {link}
                </button>
              ))}
            </div>
          </div>
          <div className="border-t border-[#E9ECEF] pt-6 flex items-center justify-center gap-2 text-xs text-[#6C757D]">
            <svg xmlns="http://www.w3.org/2000/svg" className="h-3.5 w-3.5 text-[#28A745]" viewBox="0 0 24 24" fill="none" stroke="currentColor" strokeWidth="2" strokeLinecap="round" strokeLinejoin="round">
              <path d="M22 11.08V12a10 10 0 1 1-5.93-9.14" />
              <polyline points="22 4 12 14.01 9 11.01" />
            </svg>
            HIPAA Compliant & Fully Secure Connection Verified
          </div>
        </div>
      </footer>
    </div>
  );
}
