import api, { ErrorResponse } from '../../lib/axios/axios';

// Types based on LessonContentContracts.cs and endpoints
export interface LessonContentResponse {
    id: number;
    lessonId: number;
    contentType: string;
    contentData: string;
}

export interface ListLessonContentsResponse {
    contents: LessonContentResponse[];
}

export interface CreateLessonContentRequest {
    contentType: string;
    contentData: string;
}

export interface UpdateLessonContentRequest {
    id: number;
    lessonId: number;
    contentType: string;
    contentData: string;
}

// List all content items for a lesson
export const getLessonContents = async (
    tenant: string,
    lessonId: number
): Promise<ListLessonContentsResponse> => {
    try {
        const response = await api.get<ListLessonContentsResponse>(
            `/${encodeURIComponent(tenant)}/api/v1/lessons/${lessonId}/contents`
        );
        return response.data;
    } catch (error: any) {
        if (error.response) throw error.response.data as ErrorResponse;
        throw {
            status: 0,
            title: 'Network or unexpected error',
            detail: error.message,
        } as ErrorResponse;
    }
};

// Get a specific content item for a lesson
export const getLessonContent = async (
    tenant: string,
    lessonId: number,
    id: number
): Promise<LessonContentResponse> => {
    try {
        const response = await api.get<LessonContentResponse>(
            `/${encodeURIComponent(tenant)}/api/v1/lessons/${lessonId}/contents/${id}`
        );
        return response.data;
    } catch (error: any) {
        if (error.response) throw error.response.data as ErrorResponse;
        throw {
            status: 0,
            title: 'Network or unexpected error',
            detail: error.message,
        } as ErrorResponse;
    }
};

// Create a new content item for a lesson
export const createLessonContent = async (
    tenant: string,
    lessonId: number,
    request: CreateLessonContentRequest
): Promise<LessonContentResponse> => {
    try {
        const response = await api.post<LessonContentResponse>(
            `/${encodeURIComponent(tenant)}/api/v1/lessons/${lessonId}/contents`,
            request
        );
        return response.data;
    } catch (error: any) {
        if (error.response) throw error.response.data as ErrorResponse;
        throw {
            status: 0,
            title: 'Network or unexpected error',
            detail: error.message,
        } as ErrorResponse;
    }
};

// Update an existing content item for a lesson
export const updateLessonContent = async (
    tenant: string,
    lessonId: number,
    id: number,
    request: UpdateLessonContentRequest
): Promise<LessonContentResponse> => {
    try {
        const response = await api.post<LessonContentResponse>(
            `/${encodeURIComponent(tenant)}/api/v1/lessons/${lessonId}/contents/${id}`,
            request
        );
        return response.data;
    } catch (error: any) {
        if (error.response) throw error.response.data as ErrorResponse;
        throw {
            status: 0,
            title: 'Network or unexpected error',
            detail: error.message,
        } as ErrorResponse;
    }
};

// Delete a content item for a lesson
export const deleteLessonContent = async (
    tenant: string,
    lessonId: number,
    id: number
): Promise<void> => {
    try {
        await api.delete(
            `/${encodeURIComponent(tenant)}/api/v1/lessons/${lessonId}/contents/${id}`
        );
    } catch (error: any) {
        if (error.response) throw error.response.data as ErrorResponse;
        throw {
            status: 0,
            title: 'Network or unexpected error',
            detail: error.message,
        } as ErrorResponse;
    }
};