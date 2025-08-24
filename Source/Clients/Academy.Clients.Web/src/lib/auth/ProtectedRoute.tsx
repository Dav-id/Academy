import React from 'react';
import { useAuth } from './AuthContext';
import { useParams } from 'react-router-dom';

type ProtectedRouteProps = {
    children: React.ReactNode;
    requiredRoles?: string[];
};

const ProtectedRoute: React.FC<ProtectedRouteProps> = ({ children, requiredRoles }) => {
    const { user, roles, loading, login } = useAuth();
    const params = useParams();

    // Example: replace :tenantUrlStub in requiredRoles with actual value
    const effectiveRoles = requiredRoles?.map(role =>
        role.replace(':tenantUrlStub', params.tenantUrlStub || '')
    );

    if (loading) return <div>Loading...</div>;
    if (!user && !loading) {
        login(location.pathname + location.search);
        return null;
    }

    if (effectiveRoles && effectiveRoles.length > 0) {
        const hasRole = roles.some(role => effectiveRoles.includes(role));
        if (!hasRole) {
            return <div className="text-white">Access denied: insufficient permissions.</div>;
        }
    }

    return <>{children}</>;
};

export default ProtectedRoute;