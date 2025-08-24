import api, { ErrorResponse } from '../lib/axios/axios';

// Request/response types based on CourseModuleContracts.cs
export interface CreateModuleRequest {
    courseId: number;
    title: string;
    description: string;
    order: number;
}

export interface UpdateModuleRequest {
    id: number;
    courseId: number;
    title: string;
    description: string;
    order: number;
}

export interface ModuleResponse {
    id: number;
    courseId: number;
    title: string;
    description: string;
    order: number;
}

export interface ListModulesResponse {
    modules: ModuleResponse[];
}

// Fetch all modules for a course
export const getModules = async (
    tenant: string,
    courseId: number
): Promise<ListModulesResponse> => {
    try {
        const response = await api.get<ListModulesResponse>(
            `/${encodeURIComponent(tenant)}/api/v1/courses/${courseId}/modules`
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

// Fetch a single module by ID
export const getModule = async (
    tenant: string,
    courseId: number,
    id: number
): Promise<ModuleResponse> => {
    try {
        const response = await api.get<ModuleResponse>(
            `/${encodeURIComponent(tenant)}/api/v1/courses/${courseId}/modules/${id}`
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

// Create a new module
export const createModule = async (
    tenant: string,
    courseId: number,
    request: CreateModuleRequest
): Promise<ModuleResponse> => {
    try {
        const response = await api.post<ModuleResponse>(
            `/${encodeURIComponent(tenant)}/api/v1/courses/${courseId}/modules`,
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

// Update an existing module
export const updateModule = async (
    tenant: string,
    courseId: number,
    id: number,
    request: UpdateModuleRequest
): Promise<ModuleResponse> => {
    try {
        const response = await api.put<ModuleResponse>(
            `/${encodeURIComponent(tenant)}/api/v1/courses/${courseId}/modules/${id}`,
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

// Delete a module
export const deleteModule = async (
    tenant: string,
    courseId: number,
    id: number
): Promise<void> => {
    try {
        await api.delete(
            `/${encodeURIComponent(tenant)}/api/v1/courses/${courseId}/modules/${id}`
        );
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