import { useState, useEffect, useCallback } from 'react';
import { Card } from '../components/common/Card';
import { DataTable } from '../components/common/DataTable';
import { Modal } from '../components/common/Modal';
import { SearchBar } from '../components/common/SearchBar';
import { auditService } from '../services/auditService';
import { useDebounce } from '../hooks/useDebounce';
import { AuditLog } from '../types';
import { formatDate } from '../utils/formatters';
import { PAGE_SIZE } from '../utils/constants';

export function AuditLogs() {
  const [logs, setLogs] = useState<AuditLog[]>([]);
  const [total, setTotal] = useState(0);
  const [page, setPage] = useState(1);
  const [selected, setSelected] = useState<AuditLog | null>(null);
  const [entityFilter, setEntityFilter] = useState('');
  const [search, setSearch] = useState('');
  const debouncedSearch = useDebounce(search);

  const fetchLogs = useCallback(async () => {
    const params: any = { page, pageSize: PAGE_SIZE };
    if (entityFilter) params.entityType = entityFilter;
    if (debouncedSearch) params.search = debouncedSearch;
    const res = await auditService.getAll(params);
    setLogs(res.data.items);
    setTotal(res.data.totalCount);
  }, [page, entityFilter, debouncedSearch]);

  useEffect(() => { fetchLogs(); }, [fetchLogs]);

  const columns = [
    { key: 'createdAt', header: 'Timestamp', render: (l: AuditLog) => formatDate(l.createdAt) },
    { key: 'action', header: 'Action' },
    { key: 'entityType', header: 'Entity' },
    { key: 'entityId', header: 'Entity ID', render: (l: AuditLog) => l.entityId ? l.entityId.slice(0, 8) + '...' : '-' },
    { key: 'user', header: 'User', render: (l: AuditLog) => l.user ? `${l.user.firstName} ${l.user.lastName}` : '-' },
    { key: 'ipAddress', header: 'IP Address', render: (l: AuditLog) => l.ipAddress || '-' },
  ];

  const entityTypes = ['Patient', 'Appointment', 'Billing', 'User', 'MedicalRecord', 'InventoryBatch', 'Prescription'];

  return (
    <div className="space-y-4">
      <Card title="Audit Logs">
        <div className="flex items-center gap-3 mb-4 flex-wrap">
          <div className="flex-1 min-w-[200px]">
            <SearchBar value={search} onChange={(v) => { setSearch(v); setPage(1); }} placeholder="Search by action, user, or IP..." />
          </div>
          <div className="flex items-center gap-2">
            <label className="text-sm text-[#6C757D]">Entity:</label>
            <select
              value={entityFilter}
              onChange={(e) => { setEntityFilter(e.target.value); setPage(1); }}
              className="px-3 py-1.5 border border-[#E9ECEF] rounded text-sm bg-white"
            >
              <option value="">All</option>
              {entityTypes.map((t) => (
                <option key={t} value={t}>{t}</option>
              ))}
            </select>
          </div>
          <span className="text-sm text-[#6C757D]">{total} entries</span>
        </div>
        <DataTable
          columns={columns}
          data={logs}
          page={page}
          totalPages={Math.ceil(total / PAGE_SIZE)}
          onPageChange={setPage}
          onRowClick={(l) => setSelected(l)}
        />
      </Card>

      <Modal isOpen={!!selected} onClose={() => setSelected(null)} title="Audit Log Details" size="lg">
        {selected && (
          <div className="space-y-4 text-sm">
            <div className="grid grid-cols-2 gap-4">
              <div><span className="text-[#6C757D]">Action:</span> <span className="font-medium">{selected.action}</span></div>
              <div><span className="text-[#6C757D]">Entity:</span> <span className="font-medium">{selected.entityType}</span></div>
              <div><span className="text-[#6C757D]">Entity ID:</span> <span className="font-medium">{selected.entityId || '-'}</span></div>
              <div><span className="text-[#6C757D]">User:</span> <span className="font-medium">{selected.user ? `${selected.user.firstName} ${selected.user.lastName}` : '-'}</span></div>
              <div><span className="text-[#6C757D]">IP Address:</span> <span className="font-medium">{selected.ipAddress || '-'}</span></div>
              <div><span className="text-[#6C757D]">Timestamp:</span> <span className="font-medium">{formatDate(selected.createdAt)}</span></div>
            </div>
            {selected.oldValues && (
              <div>
                <span className="block text-xs font-semibold text-[#6C757D] uppercase tracking-wider mb-1">Old Values</span>
                <pre className="bg-[#F8F9FA] border border-[#E9ECEF] rounded p-3 text-xs overflow-auto max-h-40">{JSON.stringify(JSON.parse(selected.oldValues), null, 2)}</pre>
              </div>
            )}
            {selected.newValues && (
              <div>
                <span className="block text-xs font-semibold text-[#6C757D] uppercase tracking-wider mb-1">New Values</span>
                <pre className="bg-[#F8F9FA] border border-[#E9ECEF] rounded p-3 text-xs overflow-auto max-h-40">{JSON.stringify(JSON.parse(selected.newValues), null, 2)}</pre>
              </div>
            )}
          </div>
        )}
      </Modal>
    </div>
  );
}