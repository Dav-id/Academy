import React, { createContext, useContext, useEffect, useState } from 'react';
import { userManager } from './oidc';

const AuthContext = createContext<any>(null);

export const AuthProvider = ({ children }: { children: React.ReactNode }) => {
    const [user, setUser] = useState<any>(null);
    const [loading, setLoading] = useState(true);

    useEffect(() => {
        console.log('AuthProvider mounted, checking user status...');
        userManager.getUser().then(user => {
            setUser(user);
            setLoading(false);
        });

        userManager.events.addUserLoaded(user => {
            setUser(user);
            setLoading(false);
        });
        userManager.events.addUserUnloaded(() => {
            setUser(null);
            setLoading(false);
        });
        return () => {
            userManager.events.removeUserLoaded(setUser);
            userManager.events.removeUserUnloaded(() => setUser(null));
        };
    }, []);

    const login = () => userManager.signinRedirect();
    const logout = () => userManager.signoutRedirect();

    return (
        <AuthContext.Provider value={{ user, login, logout, loading }}>
            {children}
        </AuthContext.Provider>
    );
};

export const useAuth = () => useContext(AuthContext);