import { useParams, useNavigate, useLoaderData, LoaderFunctionArgs } from 'react-router-dom';
import { useQuery } from '@tanstack/react-query';
import { getCourseModule, CourseModuleResponse } from '../../../services/courses/courseModuleService';
import { Heading } from '../../../components/heading';
import { Button } from '../../../components/button';
import { Divider } from '../../../components/divider';
import { Fieldset, Field, ErrorMessage } from '../../../components/fieldset';
import { useAuth } from '../../../lib/auth/AuthContext';

// Loader for React Router
export async function loader({ params }: LoaderFunctionArgs) {
    const tenantUrlStub = params.tenantUrlStub as string;
    const courseId = params.courseId as string;
    const courseModuleId = params.courseModuleId as string;
    if (!tenantUrlStub || !courseId || !courseModuleId) throw new Response('Not Found', { status: 404 });
    try {
        return await getCourseModule(tenantUrlStub, Number(courseId), Number(courseModuleId));
    } catch (err: any) {
        throw new Response(err?.title || 'Failed to load module', { status: 404 });
    }
}

export default function CourseModuleDetailsPage() {
    const { tenantUrlStub, courseId, courseModuleId } = useParams<{ tenantUrlStub: string; courseId: string; courseModuleId: string }>();
    const navigate = useNavigate();
    const initialModule = useLoaderData() as CourseModuleResponse;
    const { roles } = useAuth();

    const {
        data: module,
        isLoading,
        isError,
        error,
    } = useQuery<CourseModuleResponse, any>({
        queryKey: ['courseModule', tenantUrlStub, courseId, courseModuleId],
        queryFn: () => getCourseModule(tenantUrlStub!, Number(courseId), Number(courseModuleId)),
        initialData: initialModule,
        enabled: !!tenantUrlStub && !!courseId && !!courseModuleId,
    });

    if (isLoading) {
        return <div className="mx-auto max-w-4xl">Loading...</div>;
    }

    if (isError) {
        return (
            <div className="mx-auto max-w-4xl">
                <Button onClick={() => navigate(-1)}>Back</Button>
                <Fieldset>
                    <Field>
                        <ErrorMessage>{error?.title || 'Failed to load module'}</ErrorMessage>
                    </Field>
                </Fieldset>
            </div>
        );
    }

    if (!module) {
        return null;
    }

    return (
        <div className="mx-auto">
            <div className="mt-4 flex flex-wrap items-end justify-between gap-4">
                <div className="flex flex-wrap items-center gap-6">
                    <div>
                        <div className="flex flex-wrap items-center gap-x-4 gap-y-2">
                            <Heading>{module.title}</Heading>
                        </div>
                        <div className="text-sm/6 mt-2 text-zinc-500">
                            {module.description || <em>No description</em>}
                        </div>
                    </div>
                </div>
                <div className="flex gap-4">
                    <Button outline href={`/${tenantUrlStub}/courses/${courseId}/modules`}>Back</Button>
                    {roles.includes('Administrator') ||
                    roles.includes(tenantUrlStub + ':Administrator') ||
                    roles.includes(tenantUrlStub + ':Instructor') ? (
                        <Button href={`/${tenantUrlStub}/courses/${courseId}/modules/${module.id}/update`}>Update</Button>
                    ) : null}
                </div>
            </div>

            <Divider className="my-10 mt-6" soft />
            <div>
                <Heading>Module Details</Heading>
                <Divider className="my-6" />
                <div className="mb-4">
                    <strong>Order:</strong> {module.order}
                </div>
                <div className="mb-4">
                    <strong>Title:</strong> {module.title}
                </div>
                <div className="mb-4">
                    <strong>Description:</strong> {module.description || <em>No description</em>}
                </div>
            </div>
        </div>
    );
}