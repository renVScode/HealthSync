import { useEffect, useRef, useCallback } from 'react';
import * as signalR from '@microsoft/signalr';

const HUB_URL = import.meta.env.VITE_API_URL?.replace('/api', '') || 'http://localhost:5000';

export function useSignalR(hubName: string) {
  const connectionRef = useRef<signalR.HubConnection | null>(null);

  useEffect(() => {
    const token = sessionStorage.getItem('accessToken');
    const connection = new signalR.HubConnectionBuilder()
      .withUrl(`${HUB_URL}/hubs/${hubName}`, {
        accessTokenFactory: () => token || '',
      })
      .withAutomaticReconnect()
      .build();

    connectionRef.current = connection;
    connection.start().catch((err) => console.error('SignalR connection error:', err));

    return () => {
      connection.stop();
    };
  }, [hubName]);

  const on = useCallback((event: string, callback: (...args: any[]) => void) => {
    connectionRef.current?.on(event, callback);
    return () => connectionRef.current?.off(event, callback);
  }, []);

  const invoke = useCallback((method: string, ...args: any[]) => {
    return connectionRef.current?.invoke(method, ...args);
  }, []);

  return { on, invoke };
}
