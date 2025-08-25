import api, { ErrorResponse } from '../../lib/axios/axios';

// Account response types
export interface AccountResponse {
    id: number;
    firstName: string;
    lastName: string;
    email: string;
    role: string;
}

export interface ListAccountsResponse {
    users: AccountResponse[];
    totalCount: number;
}

export interface CreateAccountRequest {
    firstName: string;
    lastName: string;
    email: string;
    password?: string;
}

export interface UpdateAccountRequest {
    id: number;
    firstName?: string;
    lastName?: string;
    email?: string;
    password?: string;
}

// Fetch all accounts (paginated)
export const getAccounts = async (
    tenantUrlStub: string,
    page: number = 1,
    pageSize: number = 10
): Promise<ListAccountsResponse> => {
    try {
        const response = await api.get<ListAccountsResponse>(
            `/${tenantUrlStub}/api/v1/users?page=${page}&pageSize=${pageSize}`
        );
        return response.data;
    } catch (error: any) {
        if (error.response) {
            throw error.response.data as ErrorResponse;
        }
        throw {
            status: 0,
            title: 'Network or unexpected error',
            detail: error.message,
        } as ErrorResponse;
    }
};

// Fetch a single account by ID
export const getAccount = async (
    tenantUrlStub: string,
    id: number
): Promise<AccountResponse> => {
    try {
        const response = await api.get<AccountResponse>(
            `/${tenantUrlStub}/api/v1/users/${id}`
        );
        return response.data;
    } catch (error: any) {
        if (error.response) {
            throw error.response.data as ErrorResponse;
        }
        throw {
            status: 0,
            title: 'Network or unexpected error',
            detail: error.message,
        } as ErrorResponse;
    }
};

// Create a new account
export const createAccount = async (
    tenantUrlStub: string,
    request: CreateAccountRequest
): Promise<AccountResponse> => {
    try {
        const response = await api.post<AccountResponse>(
            `/${tenantUrlStub}/api/v1/users`,
            request
        );
        return response.data;
    } catch (error: any) {
        if (error.response) {
            throw error.response.data as ErrorResponse;
        }
        throw {
            status: 0,
            title: 'Network or unexpected error',
            detail: error.message,
        } as ErrorResponse;
    }
};

// Update an existing account
export const updateAccount = async (
    tenantUrlStub: string,
    id: number,
    request: UpdateAccountRequest
): Promise<AccountResponse> => {
    try {
        const response = await api.put<AccountResponse>(
            `/${tenantUrlStub}/api/v1/users/${id}`,
            request
        );
        return response.data;
    } catch (error: any) {
        if (error.response) {
            throw error.response.data as ErrorResponse;
        }
        throw {
            status: 0,
            title: 'Network or unexpected error',
            detail: error.message,
        } as ErrorResponse;
    }
};

// Delete an account
export const deleteAccount = async (
    tenantUrlStub: string,
    id: number
): Promise<void> => {
    try {
        await api.delete(`/api/v1/users/${id}`);
    } catch (error: any) {
        if (error.response) {
            throw error.response.data as ErrorResponse;
        }
        throw {
            status: 0,
            title: 'Network or unexpected error',
            detail: error.message,
        } as ErrorResponse;
    }
};