import api, { ErrorResponse } from '../../lib/axios/axios';

// Request/response types based on CourseModuleContracts.cs
export interface CreateCourseModuleRequest {
    courseId: number;
    title: string;
    description: string;
    order: number;
}

export interface UpdateCourseModuleRequest {
    id: number;
    courseId: number;
    title: string;
    description: string;
    order: number;
}

export interface CourseModuleResponse {
    id: number;
    courseId: number;
    title: string;
    description: string;
    order: number;
}

export interface ListCourseModulesResponse {
    modules: CourseModuleResponse[];
    totalCount?: number;
}

// Fetch all modules for a course (with optional pagination)
export const getCourseModules = async (
    tenant: string,
    courseId: number,
    page: number = 1,
    pageSize: number = 10
): Promise<ListCourseModulesResponse> => {
    try {        
        const response = await api.get<ListCourseModulesResponse>(
            `/${encodeURIComponent(tenant)}/api/v1/courses/${courseId}/modules?page=${page}&pageSize=${pageSize}`
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
export const getCourseModule = async (
    tenant: string,
    courseId: number,
    id: number
): Promise<CourseModuleResponse> => {
    try {
        const response = await api.get<CourseModuleResponse>(
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
    request: CreateCourseModuleRequest
): Promise<CourseModuleResponse> => {
    try {
        const response = await api.post<CourseModuleResponse>(
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
    request: UpdateCourseModuleRequest
): Promise<CourseModuleResponse> => {
    try {
        const response = await api.post<CourseModuleResponse>(
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