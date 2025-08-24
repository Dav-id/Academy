import api, { ErrorResponse } from '../lib/axios/axios';

// Request/response types based on CourseCompletionContracts.cs
export interface SubmitCompletionRequest {
    courseId: number;
    userProfileId: number;
    finalScore: number;
    isPassed: boolean;
    feedback: string;
}

export interface CompletionResponse {
    id: number;
    courseId: number;
    userProfileId: number;
    submittedOn: string; // ISO date string
    isPassed: boolean;
    finalScore: number;
    feedback: string;
}

export interface ListCompletionsResponse {
    completions: CompletionResponse[];
}

// Submit a course completion for a user
export const submitCompletion = async (
    tenant: string,
    courseId: number,
    request: SubmitCompletionRequest
): Promise<CompletionResponse> => {
    try {
        const response = await api.post<CompletionResponse>(
            `/${encodeURIComponent(tenant)}/api/v1/courses/${courseId}/complete`,
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

// Get all completions for a course (instructor only)
export const getCourseCompletions = async (
    tenant: string,
    courseId: number
): Promise<ListCompletionsResponse> => {
    try {
        const response = await api.get<ListCompletionsResponse>(
            `/${encodeURIComponent(tenant)}/api/v1/courses/${courseId}/completions`
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