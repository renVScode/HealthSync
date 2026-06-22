import { Link, useLocation } from 'react-router-dom';

export function Breadcrumb() {
  const location = useLocation();
  const paths = location.pathname.split('/').filter(Boolean);

  return (
    <nav className="flex items-center gap-2 text-sm text-[#6C757D] mb-4">
      <Link to="/dashboard" className="hover:text-[#2C7DA0]">Home</Link>
      {paths.map((path, index) => {
        const url = `/${paths.slice(0, index + 1).join('/')}`;
        const label = path.charAt(0).toUpperCase() + path.slice(1).replace(/-/g, ' ');
        return (
          <span key={path} className="flex items-center gap-2">
            <span>/</span>
            {index === paths.length - 1 ? (
              <span className="text-[#212529] font-medium">{label}</span>
            ) : (
              <Link to={url} className="hover:text-[#2C7DA0]">{label}</Link>
            )}
          </span>
        );
      })}
    </nav>
  );
}
