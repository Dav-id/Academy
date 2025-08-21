import { Heading, Subheading } from '@/components/heading'
import { getRecentOrders } from '@/data'

export default async function ModuleList({
    children,
    params,
}: {
    children: React.ReactNode;
    // I hate this but !
    params: Promise<{
        tenant: string
        courseId?: number;
        moduleId?: number;
        lessonId?: number;
        assessmentId?: number;
    }>;
}) {

    const { tenant, courseId, moduleId, lessonId, assessmentId } = await params;

    return (
        <>
            <Heading>Tenant {tenant} Modules List</Heading>

        </>
    )
}
