import React from 'react';
import { useLocation } from 'react-router-dom';
import { useAuth } from './AuthContext';

export default function ProtectedRoute({ children }: { children: JSX.Element }) {
    const { user, login, loading} = useAuth();
    const location = useLocation();

    if (loading) {
        return <div>Loading authentication...</div>;
    }

    if (user === undefined) {
        return <div>Loading authentication...</div>;
    }

    if (!user) {
        login();
        return null;
    }

    return children;
}