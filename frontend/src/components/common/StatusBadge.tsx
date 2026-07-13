interface StatusBadgeProps {
  status: string;
  colorMap?: Record<string, string>;
}

export function StatusBadge({ status, colorMap }: StatusBadgeProps) {
  const defaultColors: Record<string, string> = {
    Paid: 'text-green-700',
    Pending: 'text-yellow-700',
    Cancelled: 'text-red-700',
    Completed: 'text-gray-600',
    Confirmed: 'text-green-700',
    Scheduled: 'text-blue-700',
    Refunded: 'text-gray-500',
    InProgress: 'text-blue-700',
    NoShow: 'text-red-700',
    PartiallyPaid: 'text-yellow-700',
  };
  const colorClass = (colorMap || defaultColors)[status] || 'text-gray-500';
  return (
    <span className={`text-xs font-semibold ${colorClass}`}>
      {status}
    </span>
  );
}
