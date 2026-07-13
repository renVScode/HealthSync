import { ReactNode } from 'react';
import { Pagination } from './Pagination';

interface Column<T> {
  key: string;
  header: string;
  render?: (item: T) => ReactNode;
  sortable?: boolean;
  align?: 'left' | 'center' | 'right';
}

interface DataTableProps<T> {
  columns: Column<T>[];
  data: T[];
  page?: number;
  totalPages?: number;
  onPageChange?: (page: number) => void;
  isLoading?: boolean;
  onRowClick?: (item: T) => void;
}

export function DataTable<T extends { id: string }>({
  columns, data, page, totalPages, onPageChange, isLoading, onRowClick,
}: DataTableProps<T>) {
  if (isLoading) {
    return <div className="text-center py-8 text-[#6C757D]">Loading...</div>;
  }

  if (!data.length) {
    return <div className="text-center py-8 text-[#6C757D]">No records found</div>;
  }

  return (
    <div>
      <div className="overflow-x-auto">
        <table className="w-full">
          <thead>
            <tr className="bg-[#F0F4F8]">
              {columns.map((col) => (
                <th key={col.key} className={`px-4 py-3 text-xs font-semibold text-[#6C757D] uppercase tracking-wider text-${col.align || 'left'}`}>
                  {col.header}
                </th>
              ))}
            </tr>
          </thead>
          <tbody>
            {data.map((item, index) => (
              <tr
                key={item.id}
                className={`${index % 2 === 0 ? 'bg-white' : 'bg-gray-50/50'} border-b border-[#E9ECEF] hover:bg-[#F0F4F8] cursor-pointer transition-colors duration-150 group`}
                onClick={() => onRowClick?.(item)}
              >
                {columns.map((col) => (
                  <td key={col.key} className={`px-4 py-3 text-sm text-[#212529] text-${col.align || 'left'}`}>
                    {col.render ? col.render(item) : (item as any)[col.key]}
                  </td>
                ))}
              </tr>
            ))}
          </tbody>
        </table>
      </div>
      {page !== undefined && totalPages !== undefined && onPageChange && (
        <Pagination currentPage={page} totalPages={totalPages} onPageChange={onPageChange} />
      )}
    </div>
  );
}
