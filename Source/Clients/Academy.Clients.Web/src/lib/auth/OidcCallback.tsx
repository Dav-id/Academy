import React, { useEffect } from 'react';
import { userManager } from './oidc';
import { useNavigate } from 'react-router-dom';

export default function OidcCallback() {
    const navigate = useNavigate();

    useEffect(() => {
        userManager.signinRedirectCallback()
            .then(() => navigate('/'))
            .catch(err => console.error('OIDC callback error:', err));
    }, [navigate]);

    return <div>Signing in...</div>;
}