import axios, { AxiosRequestConfig, AxiosResponse } from 'axios';
const config = window.appConfig;
import { userManager } from '../auth/oidc';

// ... inside your AuthProvider or as a standalone export:
export async function getAccessToken(): Promise<string | null> {
    const user = await userManager.getUser();
    return user?.access_token ?? null;
}

async function ensureValidToken() {
    let user = await userManager.getUser();
    if (!user || user.expired) {
        try {
            user = await userManager.signinSilent();
        } catch (err) {
            // Silent refresh failed, fallback to interactive login if needed
            console.error('Silent token refresh failed:', err);
            await userManager.signinRedirect();
        }
    }
    return user?.access_token ?? null;
}

const api = axios.create({
    baseURL: config.api.API_BASE_URL,
    timeout: 10000,
    headers: {
        'Content-Type': 'application/json',
    },
});

api.interceptors.request.use(
    async (config: AxiosRequestConfig) => {
        const token = await ensureValidToken();
        if (token && config.headers) {
            config.headers.Authorization = `Bearer ${token}`;
        }
        return config;
    },
    error => Promise.reject(error)
);

//// Response interceptor for error handling and 401 redirect
//api.interceptors.response.use(
//    (response: AxiosResponse) => response,
//    error => {
//        if (typeof window !== 'undefined' && error.response?.status === 401) {
//            // Redirect to login page (adjust path if needed)
//            window.location.href = '/api/auth/signin';
//            // Optionally, return here to prevent further error handling
//            return;
//        }
//        console.error('API Error:', error.response?.data || error.message);
//        return Promise.reject(error);
//    }
//);

export default api;

export interface ErrorResponse {
    status: number;
    title: string;
    detail: string;
    traceId?: string;
}