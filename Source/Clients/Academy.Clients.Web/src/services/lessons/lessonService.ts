import api, { ErrorResponse } from '../../lib/axios/axios';

// Request/response types based on LessonContracts.cs
export interface CreateLessonRequest {
    courseModuleId: number;
    title: string;
    summary: string;
    order: number;
    availableFrom?: string | null; // ISO date string or null
    availableTo?: string | null;   // ISO date string or null
}

export interface UpdateLessonRequest {
    id: number;
    courseModuleId: number;
    title: string;
    summary: string;
    order: number;
    availableFrom?: string | null;
    availableTo?: string | null;
}

export interface LessonResponse {
    id: number;
    courseModuleId: number;
    title: string;
    summary: string;
    order: number;
    availableFrom?: string | null;
    availableTo?: string | null;
}

export interface ListLessonsResponse {
    lessons: LessonResponse[];
}

// Fetch all lessons for a module
export const getLessons = async (
    tenant: string,
    moduleId: number
): Promise<ListLessonsResponse> => {
    try {
        const response = await api.get<ListLessonsResponse>(
            `/${encodeURIComponent(tenant)}/api/v1/modules/${moduleId}/lessons`
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

// Fetch a single lesson by ID
export const getLesson = async (
    tenant: string,
    moduleId: number,
    id: number
): Promise<LessonResponse> => {
    try {
        const response = await api.get<LessonResponse>(
            `/${encodeURIComponent(tenant)}/api/v1/modules/${moduleId}/lessons/${id}`
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

// Create a new lesson
export const createLesson = async (
    tenant: string,
    moduleId: number,
    request: CreateLessonRequest
): Promise<LessonResponse> => {
    try {
        const response = await api.post<LessonResponse>(
            `/${encodeURIComponent(tenant)}/api/v1/modules/${moduleId}/lessons`,
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

// Update an existing lesson
export const updateLesson = async (
    tenant: string,
    moduleId: number,
    id: number,
    request: UpdateLessonRequest
): Promise<LessonResponse> => {
    try {
        const response = await api.put<LessonResponse>(
            `/${encodeURIComponent(tenant)}/api/v1/modules/${moduleId}/lessons/${id}`,
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

// Delete a lesson
export const deleteLesson = async (
    tenant: string,
    moduleId: number,
    id: number
): Promise<void> => {
    try {
        await api.delete(
            `/${encodeURIComponent(tenant)}/api/v1/modules/${moduleId}/lessons/${id}`
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