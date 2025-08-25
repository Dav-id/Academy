import api, { ErrorResponse } from '../../lib/axios/axios';

export interface UserProfile {
    id: string;
    firstName?: string;
    lastName?: string;    
    email: string;
    roles?: string[];
    isActive?: boolean;
}

export interface ListUserProfilesResponse {
    users: UserProfile[];
    totalCount: number;
}

// Get all user profiles for a tenant (paginated)
export const getUserProfiles = async (
    tenant: string,
    page: number = 1,
    pageSize: number = 20
): Promise<ListUserProfilesResponse> => {
    try {
        const response = await api.get<ListUserProfilesResponse>(
            `/${tenant}/api/v1/users?page=${page}&pageSize=${pageSize}`
        );
        return response.data;
    } catch (err: any) {
        if (err?.response?.data) {
            throw err.response.data as ErrorResponse;
        }
        throw err;
    }
};

// Get a specific user profile by ID
export const getUserProfile = async (
    tenant: string,
    id: string
): Promise<UserProfile> => {
    try {
        const response = await api.get<UserProfile>(`/${tenant}/api/v1/users/${id}`);
        return response.data;
    } catch (err: any) {
        if (err?.response?.data) {
            throw err.response.data as ErrorResponse;
        }
        throw err;
    }
};

// Create a new user profile
export const createUserProfile = async (
    tenant: string,
    data: Partial<UserProfile>
): Promise<UserProfile> => {
    try {
        const response = await api.post<UserProfile>(`/${tenant}/api/v1/users`, data);
        return response.data;
    } catch (err: any) {
        if (err?.response?.data) {
            throw err.response.data as ErrorResponse;
        }
        throw err;
    }
};

// Update a user profile by ID
export const updateUserProfile = async (
    tenant: string,
    id: string,
    data: Partial<UserProfile>
): Promise<UserProfile> => {
    try {
        const response = await api.post<UserProfile>(`/${tenant}/api/v1/users/${id}`, data);
        return response.data;
    } catch (err: any) {
        if (err?.response?.data) {
            throw err.response.data as ErrorResponse;
        }
        throw err;
    }
};

// Delete a user profile by ID
export const deleteUserProfile = async (
    tenant: string,
    id: string
): Promise<void> => {
    try {
        await api.delete(`/${tenant}/api/v1/users/${id}`);
    } catch (err: any) {
        if (err?.response?.data) {
            throw err.response.data as ErrorResponse;
        }
        throw err;
    }
};
