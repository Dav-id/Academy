import { Heading, Subheading } from '@/components/heading'

export default async function ModuleDetails({
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

    const { moduleId } = await params;

    return (
        <>
            <Heading>Module {moduleId} Details</Heading>
        </>
    )
}
