'use client';

import React, { useEffect } from "react";
import { useSidebar } from "../../../lib/navigation/sidebar/SidebarContext";
import { useParams } from "next/dist/client/components/navigation";

export default function TenantLayout({
    children
}: {
    children: React.ReactNode;
}) {
    const params = useParams();
    const { setSidebarState } = useSidebar();

    //useEffect(() => {
        setSidebarState({
            tenant: params.tenant as string,
            courseId: params.courseId as string,
            moduleId: params.moduleId as string,
            lessonId: params.lessonId as string,
            userId: 'current-user-id', // Replace with actual user ID logic
        });
    //}, [params]);


    // Pass tree as a prop to children
    return (
        <>
            {children}
        </>
    );
}