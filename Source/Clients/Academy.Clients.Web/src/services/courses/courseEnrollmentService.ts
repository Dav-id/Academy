import api, { ErrorResponse } from '../../lib/axios/axios';

// Request/response types based on CourseEnrollmentContracts.cs
export interface EnrollRequest {
    courseId: number;
}

export interface EnrollmentResponse {
    id: number;
    courseId: number;
    userProfileId: number;
    enrolledOn: string; // ISO date string
    isCompleted: boolean;
}

// Enroll the current user in a course
export const enrollInCourse = async (
    tenant: string,
    courseId: number,
    request: EnrollRequest
): Promise<EnrollmentResponse> => {
    try {
        const response = await api.post<EnrollmentResponse>(
            `/${encodeURIComponent(tenant)}/api/v1/courses/${courseId}/enroll`,
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

// Unenroll the current user from a course
export const unenrollFromCourse = async (
    tenant: string,
    courseId: number
): Promise<void> => {
    try {
        await api.delete(`/${encodeURIComponent(tenant)}/api/v1/courses/${courseId}/enroll`);
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

// Get all enrollments for a course (instructor only)
export const getCourseEnrollments = async (
    tenant: string,
    courseId: number
): Promise<EnrollmentResponse[]> => {
    try {
        const response = await api.get<EnrollmentResponse[]>(
            `/${encodeURIComponent(tenant)}/api/v1/courses/${courseId}/enrollments`
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