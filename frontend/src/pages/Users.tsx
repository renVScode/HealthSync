import { useState, useEffect } from 'react';
import { Card } from '../components/common/Card';
import { DataTable } from '../components/common/DataTable';
import { SearchBar } from '../components/common/SearchBar';
import { authService } from '../services/authService';
import { useDebounce } from '../hooks/useDebounce';
import { PAGE_SIZE } from '../utils/constants';

export function Users() {
  const [users, setUsers] = useState<any[]>([]);
  const [search, setSearch] = useState('');
  const [page, setPage] = useState(1);
  const [total, setTotal] = useState(0);
  const debouncedSearch = useDebounce(search);

  useEffect(() => {
    const load = async () => {
      const res = await authService.getAllUsers(page, PAGE_SIZE, debouncedSearch || undefined);
      const data = res.data;
      if (Array.isArray(data)) {
        setUsers(data);
        setTotal(0);
      } else if (data?.items) {
        setUsers(data.items);
        setTotal(data.totalCount);
      }
    };
    load();
  }, [page, debouncedSearch]);

  const columns = [
    { key: 'firstName', header: 'First Name' },
    { key: 'lastName', header: 'Last Name' },
    { key: 'email', header: 'Email' },
    { key: 'role', header: 'Role' },
    { key: 'isActive', header: 'Active', render: (u: any) => u.isActive ? 'Yes' : 'No' },
  ];

  return (
    <div>
      <Card title="Users">
        <div className="mb-4">
          <SearchBar value={search} onChange={(v) => { setSearch(v); setPage(1); }} placeholder="Search by name, username, or email..." />
        </div>
        <DataTable
          columns={columns}
          data={users}
          page={page}
          totalPages={total > 0 ? Math.ceil(total / PAGE_SIZE) : undefined}
          onPageChange={setPage}
        />
      </Card>
    </div>
  );
}