import { useState, useEffect } from 'react';
import { Card } from '../components/common/Card';
import { Button } from '../components/common/Button';
import { DataTable } from '../components/common/DataTable';
import { StockAlertBadge } from '../components/domain-components';
import { inventoryService } from '../services/inventoryService';
import { formatCurrency } from '../utils/formatters';
import type { InventoryBatch } from '../types';

export function Inventory() {
  const [batches, setBatches] = useState<InventoryBatch[]>([]);
  const [lowStock, setLowStock] = useState<any[]>([]);

  useEffect(() => {
    inventoryService.getAll().then((r) => setBatches(r.data));
    inventoryService.getLowStock().then((r) => setLowStock(r.data));
  }, []);

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
        <Button>+ Add Stock</Button>
      }>
        <DataTable columns={columns} data={batches} />
      </Card>
    </div>
  );
}
