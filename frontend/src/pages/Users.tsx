import { useState, useEffect } from 'react';
import { Card } from '../components/common/Card';
import { DataTable } from '../components/common/DataTable';
import { authService } from '../services/authService';
import type { UserInfo } from '../types';

export function Users() {
  const [users, setUsers] = useState<UserInfo[]>([]);

  useEffect(() => {
    authService.getAllUsers().then((res) => setUsers(res.data));
  }, []);

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
        <DataTable columns={columns} data={users} />
      </Card>
    </div>
  );
}
