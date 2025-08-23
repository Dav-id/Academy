import axios, { AxiosRequestConfig, AxiosResponse } from 'axios';
import { getSession } from 'next-auth/react';

let cachedSession: any = null;
let sessionFetchedAt = 0;
const SESSION_CACHE_DURATION = 60 * 1000; // 1 minute

async function getCachedSession() {
    const now = Date.now();
    if (!cachedSession || now - sessionFetchedAt > SESSION_CACHE_DURATION) {
        cachedSession = await getSession();
        sessionFetchedAt = now;
    }
    return cachedSession;
}

const api = axios.create({
    baseURL: process.env.NEXT_PUBLIC_API_BASE_URL,
    timeout: 10000,
    headers: {
        'Content-Type': 'application/json',
    },
});

// Request interceptor to attach token from NextAuth
api.interceptors.request.use(
    async (config: AxiosRequestConfig) => {
        if (typeof window !== 'undefined') {
            const session = await getCachedSession();
            const token = session?.accessToken; // or session?.user?.accessToken depending on your NextAuth config
            if (token && config.headers) {
                config.headers.Authorization = `Bearer ${token}`;
            }
        }
        return config;
    },
    error => Promise.reject(error)
);

// Response interceptor for error handling and 401 redirect
api.interceptors.response.use(
    (response: AxiosResponse) => response,
    error => {
        if (typeof window !== 'undefined' && error.response?.status === 401) {
            // Redirect to login page (adjust path if needed)
            window.location.href = '/api/auth/signin';
            // Optionally, return here to prevent further error handling
            return;
        }
        console.error('API Error:', error.response?.data || error.message);
        return Promise.reject(error);
    }
);

export default api;

export interface ErrorResponse {
    status: number;
    title: string;
    detail: string;
    traceId?: string;
}