import api, { ErrorResponse } from '../lib/axios/axios';

export interface CreateTenantRequest {
    urlStub: string;
    title: string;
    description?: string;
}

export interface UpdateTenantRequest {
    urlStub: string;
    title: string;
    description?: string;
}

export interface TenantResponse {
    id: number;
    urlStub: string;
    title: string;
    description: string;
}

export const getTenants = async () => {
    try {
        const response = await api.get('/v1/tenants');
        return response.data;
    } catch (error: any) {
        if (error.response) {
            const err: ErrorResponse = error.response.data;
            throw err;
        }
        throw {
            status: 0,
            title: 'Network or unexpected error',
            detail: error.message,
        } as ErrorResponse;
    }
};

export const getTenant = async (urlStub: string): Promise<TenantResponse> => {
    try {
        const response = await api.get<TenantResponse>(`/v1/tenants/${encodeURIComponent(urlStub)}`);
        return response.data;
    } catch (error: any) {
        if (error.response) {
            const err: ErrorResponse = error.response.data;
            throw err;
        }
        throw {
            status: 0,
            title: 'Network or unexpected error',
            detail: error.message,
        } as ErrorResponse;
    }
};

export const createTenant = async (request: CreateTenantRequest): Promise<TenantResponse> => {
    try {
        const response = await api.post<TenantResponse>('/v1/tenants', request);
        return response.data;
    } catch (error: any) {
        if (error.response) {
            const err: ErrorResponse = error.response.data;
            throw err;
        }
        throw {
            status: 0,
            title: 'Network or unexpected error',
            detail: error.message,
        } as ErrorResponse;
    }
};

export const updateTenant = async (urlStub: string, request: UpdateTenantRequest): Promise<TenantResponse> => {
    try {
        const response = await api.put<TenantResponse>(`/v1/tenants/${encodeURIComponent(urlStub)}`, request);
        return response.data;
    } catch (error: any) {
        if (error.response) {
            const err: ErrorResponse = error.response.data;
            throw err;
        }
        throw {
            status: 0,
            title: 'Network or unexpected error',
            detail: error.message,
        } as ErrorResponse;
    }
};

export const deleteTenant = async (urlStub: string): Promise<void> => {
    try {
        await api.delete(`/v1/tenants/${encodeURIComponent(urlStub)}`);
    } catch (error: any) {
        if (error.response) {
            const err: ErrorResponse = error.response.data;
            throw err;
        }
        throw {
            status: 0,
            title: 'Network or unexpected error',
            detail: error.message,
        } as ErrorResponse;
    }
};

