import { useState, useEffect } from 'react';
import { Card } from '../components/common/Card';
import { Button } from '../components/common/Button';
import { DataTable } from '../components/common/DataTable';
import { SearchBar } from '../components/common/SearchBar';
import { StockAlertBadge } from '../components/domain-components';
import { inventoryService } from '../services/inventoryService';
import { useDebounce } from '../hooks/useDebounce';
import { useAuth } from '../contexts/auth-context';
import { formatCurrency } from '../utils/formatters';
import { PAGE_SIZE } from '../utils/constants';

export function Inventory() {
  const { hasRole } = useAuth();
  const [batches, setBatches] = useState<any[]>([]);
  const [lowStock, setLowStock] = useState<any[]>([]);
  const [search, setSearch] = useState('');
  const [page, setPage] = useState(1);
  const [total, setTotal] = useState(0);
  const debouncedSearch = useDebounce(search);

  useEffect(() => {
    inventoryService.getLowStock().then((r) => setLowStock(r.data));
  }, []);

  useEffect(() => {
    const load = async () => {
      const res = await inventoryService.getAll(page, PAGE_SIZE, debouncedSearch || undefined);
      const data = res.data;
      if (Array.isArray(data)) {
        setBatches(data);
        setTotal(0);
      } else if (data?.items) {
        setBatches(data.items);
        setTotal(data.totalCount);
      }
    };
    load();
  }, [page, debouncedSearch]);

  const columns = [
    { key: 'medicineName', header: 'Medicine' },
    { key: 'batchNumber', header: 'Batch' },
    { key: 'quantity', header: 'Qty' },
    { key: 'unitPrice', header: 'Unit Price', render: (b: any) => formatCurrency(b.unitPrice) },
    { key: 'expiryDate', header: 'Expiry', render: (b: any) => b.expiryDate ? new Date(b.expiryDate).toLocaleDateString() : '—' },
    { key: 'isLowStock', header: 'Status', render: (b: any) => <StockAlertBadge currentStock={b.quantity} reorderLevel={5} /> },
  ];

  return (
    <div className="space-y-6">
      {lowStock.length > 0 && (
        <Card title="Low Stock Alerts" className="border-yellow-200">
          <div className="space-y-2">
            {lowStock.map((item: any) => (
              <div key={item.medicineId} className="flex justify-between items-center p-2 bg-yellow-50 rounded">
                <span className="font-medium">{item.medicineName}</span>
                <span className="text-sm">Stock: {item.currentStock} / Reorder at: {item.reorderLevel}</span>
              </div>
            ))}
          </div>
        </Card>
      )}
      <Card title="Inventory Batches" actions={
        hasRole('Pharmacist') && <Button>+ Add Stock</Button>
      }>
        <div className="mb-4">
          <SearchBar value={search} onChange={(v) => { setSearch(v); setPage(1); }} placeholder="Search by medicine name, batch, or supplier..." />
        </div>
        <DataTable
          columns={columns}
          data={batches}
          page={page}
          totalPages={total > 0 ? Math.ceil(total / PAGE_SIZE) : undefined}
          onPageChange={setPage}
        />
      </Card>
    </div>
  );
}