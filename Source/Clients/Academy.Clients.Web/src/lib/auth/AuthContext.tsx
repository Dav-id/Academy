import React, { createContext, useContext, useState, useEffect, ReactNode } from 'react';
import { userManager } from './oidc';

// Helper to decode JWT
function parseJwt(token: string): any {
  try {
    const base64Url = token.split('.')[1];
    const base64 = base64Url.replace(/-/g, '+').replace(/_/g, '/');
    const jsonPayload = decodeURIComponent(
      atob(base64)
        .split('')
        .map(c => '%' + ('00' + c.charCodeAt(0).toString(16)).slice(-2))
        .join('')
    );
    return JSON.parse(jsonPayload);
  } catch {
    return null;
  }
}

type AuthContextType = {
  user: any;
  roles: string[];
  setRoles: (roles: string[]) => void;
  login: (redirectPath?: string) => void;
  logout: () => void;
  loading: boolean;
};

const AuthContext = createContext<AuthContextType | undefined>(undefined);

export const AuthProvider = ({ children }: { children: ReactNode }) => {
  const [user, setUser] = useState<any>(null);
  const [roles, setRoles] = useState<string[]>([]);
  const [loading, setLoading] = useState(true);

  // Extract roles from access_token
  const extractRoles = (user: any) => {
    if (user && user.access_token) {
      const payload = parseJwt(user.access_token);
      // Adjust claim name as needed: 'role', 'roles', or custom
      const roleClaim = payload?.role || payload?.roles || [];
      return Array.isArray(roleClaim) ? roleClaim : [roleClaim];
    }
    return [];
  };

  useEffect(() => {
    userManager.getUser().then(user => {
      setUser(user);
      setRoles(extractRoles(user));
      setLoading(false);
    });

    const onUserLoaded = (user: any) => {
      setUser(user);
      setRoles(extractRoles(user));
      setLoading(false);
    };
    const onUserUnloaded = () => {
      setUser(null);
      setRoles([]);
      setLoading(false);
    };

    userManager.events.addUserLoaded(onUserLoaded);
    userManager.events.addUserUnloaded(onUserUnloaded);

    return () => {
      userManager.events.removeUserLoaded(onUserLoaded);
      userManager.events.removeUserUnloaded(onUserUnloaded);
    };
  }, []);

  const login = (redirectPath?: string) => userManager.signinRedirect({
    state: redirectPath || window.location.pathname + window.location.search
  });
  const logout = () => userManager.signoutRedirect();

  return (
    <AuthContext.Provider value={{ user, roles, setRoles, login, logout, loading }}>
      {children}
    </AuthContext.Provider>
  );
};

export const useAuth = () => {
  const context = useContext(AuthContext);
  if (!context) {
    throw new Error('useAuth must be used within an AuthProvider');
  }
  return context;
};