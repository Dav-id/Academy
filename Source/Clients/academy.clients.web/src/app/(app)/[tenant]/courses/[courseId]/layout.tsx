'use client';

import React, { useEffect } from "react";
import { useSidebar } from "../../../../../lib/navigation/sidebar/SidebarContext";
import { useParams } from "next/dist/client/components/navigation";
import { buildNavTree } from "../../../../../lib/navigation/build";
import { ApplicationLayout } from "../../../application-layout";

export default function CourseDetailLayout({
    children,
    params,
}: {
    children: React.ReactNode;
    params: Promise<{
        tenant: string
        courseId?: number;
        moduleId?: number;
        lessonId?: number;
        assessmentId?: number;
        userId?: string; // Assuming userId is a string, adjust as necessary
    }>;
}) {
    const unwrappedParams = useParams();
    const { setSidebarState } = useSidebar();
    const { tenant, courseId, moduleId, lessonId, assessmentId, userId } = unwrappedParams;

    console.log('CourseDetailLayout', { tenant, courseId, moduleId, lessonId, assessmentId, userId });
    useEffect(() => {
        setSidebarState({
            tenant: tenant as string,
            courseId: courseId as string,
            moduleId: moduleId as string,
            lessonId: lessonId as string,
            userId: userId as string,
        });
    }, [tenant, courseId, moduleId, lessonId, assessmentId, userId, setSidebarState]);


    const tree = buildNavTree({ tenant: tenant, courseId: Number(courseId), moduleId: Number(moduleId), lessonId: Number(lessonId), assessmentId: Number(assessmentId), userId: Number(userId) });

    console.log('TenantLayout', { tenant, courseId, moduleId, lessonId, assessmentId, userId });

    // Pass tree as a prop to children
    return (
        <>
            <ApplicationLayout tree={tree}>
                {children}
            </ApplicationLayout>
        </>
    );
}