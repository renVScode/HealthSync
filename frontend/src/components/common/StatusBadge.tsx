interface StatusBadgeProps {
  status: string;
  colorMap?: Record<string, string>;
}

export function StatusBadge({ status, colorMap }: StatusBadgeProps) {
  const defaultColors: Record<string, string> = {
    Paid: 'bg-green-50 text-green-700',
    Pending: 'bg-yellow-50 text-yellow-700',
    Cancelled: 'bg-red-50 text-red-700',
    Completed: 'bg-gray-100 text-gray-600',
    Confirmed: 'bg-green-50 text-green-700',
    Scheduled: 'bg-blue-50 text-blue-700',
    Refunded: 'bg-gray-100 text-gray-500',
    InProgress: 'bg-blue-50 text-blue-700',
    NoShow: 'bg-red-50 text-red-700',
    PartiallyPaid: 'bg-yellow-50 text-yellow-700',
  };
  const colorClass = (colorMap || defaultColors)[status] || 'bg-gray-50 text-gray-500';
  return (
    <span className={`inline-flex items-center px-2.5 py-0.5 text-xs font-medium rounded-full ${colorClass}`}>
      {status}
    </span>
  );
}
