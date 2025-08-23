import React, { useEffect } from 'react';
import { userManager } from './oidc';

export default function CallbackPage() {
    useEffect(() => {
        userManager.signinRedirectCallback().then(() => {
            window.location.replace('/');
        });
    }, []);
    return <div>Signing in...</div>;
}