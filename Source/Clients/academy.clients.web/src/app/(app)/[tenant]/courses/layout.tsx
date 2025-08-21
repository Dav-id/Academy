'use client';

import React, { useEffect } from "react";
import { useSidebar } from "../../../../lib/navigation/sidebar/SidebarContext";
import { useParams } from "next/dist/client/components/navigation";
import { buildNavTree } from "../../../../lib/navigation/build";
import { ApplicationLayout } from "../../application-layout";

export default function CourseLayout({
    children
}: {
    children: React.ReactNode;
}) {
    const params = useParams();
    const { setSidebarState } = useSidebar();

    useEffect(() => {
        setSidebarState({
            tenant: params.tenant as string,
            courseId: params.courseId as string,
            moduleId: params.moduleId as string,
            lessonId: params.lessonId as string,
            userId: 'current-user-id', // Replace with actual user ID logic
        });
    }, [params]);


    // Pass tree as a prop to children
    const { tenant, courseId, moduleId, lessonId, assessmentId, userId } = useSidebar();
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