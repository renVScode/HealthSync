export const requried = (value: any): string | undefined =>
  value ? undefined : 'This field is required';

export const email = (value: string): string | undefined =>
  /^[^\s@]+@[^\s@]+\.[^\s@]+$/.test(value) ? undefined : 'Invalid email address';

export const phone = (value: string): string | undefined =>
  /^(\+63|0)\d{10}$/.test(value.replace(/\s/g, '')) ? undefined : 'Invalid phone number';

export const minLength = (min: number) => (value: string): string | undefined =>
  value.length >= min ? undefined : `Must be at least ${min} characters`;

export const maxLength = (max: number) => (value: string): string | undefined =>
  value.length <= max ? undefined : `Must be at least ${max} characters`;
