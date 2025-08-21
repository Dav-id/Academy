'use client';

import React, { createContext, useContext, useState } from 'react';

type SidebarState = {
    tenant?: string;
    courseId?: string;
    moduleId?: string;
    lessonId?: string;
    assessmentId?: string;
    userId?: string;
    setSidebarState: (state: Partial<SidebarState>) => void;
};

const SidebarContext = createContext<SidebarState>({
    setSidebarState: () => { },
});

export const useSidebar = () => useContext(SidebarContext);

export default function SidebarProvider({ children }: { children: React.ReactNode }) {
    const [state, setState] = useState<SidebarState>({});

    const setSidebarState = (newState: Partial<SidebarState>) => {
        setState((prev) => ({ ...prev, ...newState }));
    };

    return (
        <SidebarContext.Provider value={{ ...state, setSidebarState }}>
            {children}
        </SidebarContext.Provider>
    );
}


//'use client';
//// app/SidebarContext.tsx
//import React, { createContext, useContext, useState } from 'react';
//import { NavItemResolved } from '../types';

//const SidebarContext = createContext({
//    tree: [] as NavItemResolved[],
//    setTree: (tree: NavItemResolved[]) => { },
//});

//export const useSidebar = () => useContext(SidebarContext);

//export default function SidebarProvider({ children }: { children: React.ReactNode }) {
//    const [tree, setTree] = useState<NavItemResolved[]>([]);

//    return (
//        <SidebarContext.Provider value={{ tree, setTree }}>
//            {children}
//        </SidebarContext.Provider>
//    );
//}