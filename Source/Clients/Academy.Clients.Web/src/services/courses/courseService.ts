import api, { ErrorResponse } from '../../lib/axios/axios';

// Request/response types based on CourseContracts.cs
export interface CreateCourseRequest {
    title: string;
    description: string;
}

export interface UpdateCourseRequest {
    id: number;
    title: string;
    description: string;
}

export interface CourseResponse {
    id: number;
    title: string;
    description: string;
}

export interface ListCoursesResponse {
    courses: CourseResponse[];
}

// Fetch all courses for a tenant
export const getCourses = async (tenant: string): Promise<ListCoursesResponse> => {
    try {
        const response = await api.get<ListCoursesResponse>(`/${encodeURIComponent(tenant)}/api/v1/courses`);
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

// Fetch a single course by ID
export const getCourse = async (tenant: string, id: number): Promise<CourseResponse> => {
    try {
        const response = await api.get<CourseResponse>(`/${encodeURIComponent(tenant)}/api/v1/courses/${id}`);
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

// Create a new course
export const createCourse = async (tenant: string, request: CreateCourseRequest): Promise<CourseResponse> => {
    try {
        const response = await api.post<CourseResponse>(`/${encodeURIComponent(tenant)}/api/v1/courses`, request);
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

// Update an existing course
export const updateCourse = async (tenant: string, id: number, request: UpdateCourseRequest): Promise<CourseResponse> => {
    try {
        const response = await api.post<CourseResponse>(`/${encodeURIComponent(tenant)}/api/v1/courses/${id}`, request);
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

// Delete a course
export const deleteCourse = async (tenant: string, id: number): Promise<void> => {
    try {
        await api.delete(`/${encodeURIComponent(tenant)}/api/v1/courses/${id}`);
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