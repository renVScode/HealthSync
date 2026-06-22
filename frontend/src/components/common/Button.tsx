import { ButtonHTMLAttributes, ReactNode } from 'react';

interface ButtonProps extends ButtonHTMLAttributes<HTMLButtonElement> {
  variant?: 'primary' | 'secondary' | 'danger' | 'ghost';
  size?: 'sm' | 'md' | 'lg';
  isLoading?: boolean;
  children: ReactNode;
}

const variants = {
  primary: 'bg-[#2C7DA0] text-white hover:bg-[#1F5E7A] active:bg-[#184B61]',
  secondary: 'bg-white text-[#2C7DA0] border border-[#2C7DA0] hover:bg-gray-50 active:bg-gray-100',
  danger: 'bg-[#DC3545] text-white hover:bg-red-700 active:bg-red-800',
  ghost: 'bg-transparent text-[#6C757D] hover:bg-gray-100 active:bg-gray-200',
};

const sizes = {
  sm: 'px-3 py-1.5 text-sm',
  md: 'px-4 py-2 text-sm',
  lg: 'px-6 py-3 text-base',
};

export function Button({ variant = 'primary', size = 'md', isLoading, children, className, disabled, ...props }: ButtonProps) {
  return (
    <button
      className={`inline-flex items-center justify-center rounded-lg font-medium transition-all duration-200
        ${variants[variant]} ${sizes[size]} ${(disabled || isLoading) ? 'opacity-50 cursor-not-allowed' : ''}
        ${className || ''}`}
      disabled={disabled || isLoading}
      {...props}
    >
      {isLoading && <svg className="animate-spin -ml-1 mr-2 h-4 w-4" viewBox="0 0 24 24" />}
      {children}
    </button>
  );
}
