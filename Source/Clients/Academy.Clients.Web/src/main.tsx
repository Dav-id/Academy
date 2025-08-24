import { StrictMode } from 'react'
import { createRoot } from 'react-dom/client'
import App from './App'
import { AuthProvider } from './lib/auth/AuthContext'
import { QueryClient, QueryClientProvider } from '@tanstack/react-query';

const queryClient = new QueryClient();

createRoot(document.getElementById('root')!).render(
    <StrictMode>
        <AuthProvider>
            <QueryClientProvider client={queryClient}>
                <App />
            </QueryClientProvider>
        </AuthProvider>
    </StrictMode>,
)