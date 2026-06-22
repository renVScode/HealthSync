interface EmptyStateProps {
  title: string;
  description?: string;
  action?: { label: string; onClick: () => void };
}

export function EmptyState({ title, description, action }: EmptyStateProps) {
  return (
    <div className="text-center py-12">
      <p className="text-lg font-medium text-[#6C757D]">{title}</p>
      {description && <p className="text-sm text-[#ADB5BD] mt-1">{description}</p>}
      {action && (
        <button onClick={action.onClick} className="mt-4 text-[#2C7DA0] hover:underline text-sm">
          {action.label}
        </button>
      )}
    </div>
  );
}
