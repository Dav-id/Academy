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

    const { lessonId } = await params;

    return (
        <>
            <Heading>Lesson {lessonId} Details</Heading>
        </>
    )
}
