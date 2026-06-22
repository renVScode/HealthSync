import { Link } from 'react-router-dom';

export function NotFound() {
  return (
    <div className="flex flex-col items-center justify-center py-20">
      <h1 className="text-6xl font-bold text-[#2C7DA0] mb-4">404</h1>
      <p className="text-xl text-[#6C757D] mb-6">Page not found</p>
      <Link to="/dashboard" className="text-[#2C7DA0] hover:underline">
        Go to Dashboard
      </Link>
    </div>
  );
}
