import { useState, useEffect } from 'react';
import { Card } from '../components/common/Card';
import { DataTable } from '../components/common/DataTable';
import { Modal } from '../components/common/Modal';
import { SearchBar } from '../components/common/SearchBar';
import { StatusBadge } from '../components/common/StatusBadge';
import { InvoiceView } from '../components/domain-components';
import { billingService } from '../services/billingService';
import { useDebounce } from '../hooks/useDebounce';
import { formatCurrency, formatDate } from '../utils/formatters';
import { BillingStatus, Billing } from '../types';
import { PAGE_SIZE } from '../utils/constants';

export function Billings() {
  const [billings, setBillings] = useState<Billing[]>([]);
  const [page, setPage] = useState(1);
  const [total, setTotal] = useState(0);
  const [selected, setSelected] = useState<any>(null);
  const [search, setSearch] = useState('');
  const debouncedSearch = useDebounce(search);

  const load = () => billingService.getAll({ page, pageSize: PAGE_SIZE, search: debouncedSearch || undefined }).then((res) => {
    setBillings(res.data.items);
    setTotal(res.data.totalCount);
  });
  useEffect(() => { load(); }, [page, debouncedSearch]);

  const columns = [
    { key: 'invoiceNumber', header: 'Invoice' },
    { key: 'patientName', header: 'Patient' },
    { key: 'total', header: 'Total', render: (b: any) => formatCurrency(b.total) },
    { key: 'amountPaid', header: 'Paid', render: (b: any) => formatCurrency(b.amountPaid) },
    { key: 'balance', header: 'Balance', render: (b: any) => formatCurrency(b.balance) },
    { key: 'status', header: 'Status', render: (b: any) => <StatusBadge status={BillingStatus[b.status]} /> },
    { key: 'createdAt', header: 'Date', render: (b: any) => formatDate(b.createdAt) },
  ];

  return (
    <div>
      <Card title="Billings">
        <div className="mb-4">
          <SearchBar value={search} onChange={(v) => { setSearch(v); setPage(1); }} placeholder="Search by invoice or patient name..." />
        </div>
        <DataTable columns={columns} data={billings} page={page}
          totalPages={Math.ceil(total / PAGE_SIZE)} onPageChange={setPage}
          onRowClick={(b) => setSelected(b)} />
      </Card>
      <Modal isOpen={!!selected} onClose={() => setSelected(null)} title={`Invoice ${selected?.invoiceNumber}`} size="lg">
        {selected && <InvoiceView billing={selected} onRefresh={load} />}
      </Modal>
    </div>
  );
}