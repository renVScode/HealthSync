import { BrowserRouter } from 'react-router-dom';
import { AuthProvider } from './contexts/auth-context';
import { AppRoutes } from './routes';
import './styles/global.css';

export function App() {
  return (
    <BrowserRouter>
      <AuthProvider>
        <AppRoutes />
      </AuthProvider>
    </BrowserRouter>
  );
}
