import { Card } from '../components/common/Card';
import { EmptyState } from '../components/common/EmptyState';

export function MedicalRecords() {
  return (
    <Card title="Medical Records">
      <EmptyState
        title="No medical records yet"
        description="Select a patient to view their medical records"
      />
    </Card>
  );
}
