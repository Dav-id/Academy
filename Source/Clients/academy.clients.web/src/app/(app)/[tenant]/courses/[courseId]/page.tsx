"use client";
import { Stat } from '@/app/stat'
import { Avatar } from '@/components/avatar'
import { Heading, Subheading } from '@/components/heading'
import React, { useEffect } from "react";
import { useSidebar } from "../../../../../lib/navigation/sidebar/SidebarContext";
import { useParams } from "next/dist/client/components/navigation";

export default function CourseDetails({
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



  return (
    <>
          <Heading>Tenant { tenant } Course {courseId}</Heading>
      
    </>
  )
}