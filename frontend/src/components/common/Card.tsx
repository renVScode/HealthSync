import { ReactNode } from 'react';

interface CardProps {
  title?: string;
  children: ReactNode;
  className?: string;
  actions?: ReactNode;
}

export function Card({ title, children, className, actions }: CardProps) {
  return (
    <div className={`bg-white rounded-xl border border-[#E9ECEF] shadow-sm ${className || ''}`}>
      {(title || actions) && (
        <div className="flex items-center justify-between px-6 py-4 border-b border-[#E9ECEF]">
          {title && <h3 className="text-lg font-semibold text-[#212529]">{title}</h3>}
          {actions && <div className="flex gap-2">{actions}</div>}
        </div>
      )}
      <div className="p-6">{children}</div>
    </div>
  );
}
