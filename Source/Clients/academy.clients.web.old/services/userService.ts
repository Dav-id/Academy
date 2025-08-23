import api from '@/lib/axios/axios';

export interface UserProfile {
    id: string;
    name: string;
    email: string;
    role: string;
}

export const getMyUserProfile = async (): Promise<UserProfile> => {
    const response = await api.get<UserProfile>('/users/profile');
    return response.data;
};

export const updateMyUserProfile = async (data: Partial<UserProfile>): Promise<UserProfile> => {
    const response = await api.put<UserProfile>('/users/profile/' + data.id, data);
    return response.data;
};
