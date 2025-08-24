import { useParams, useNavigate, useLoaderData, LoaderFunctionArgs } from 'react-router-dom';
import { useQuery } from '@tanstack/react-query';
import { getCourse, CourseResponse } from '../../services/courses/courseService';
import { Heading, Subheading } from '../../components/heading';
import { Button } from '../../components/button';
import { Divider } from '../../components/divider';
import { Fieldset, Field, Label, ErrorMessage } from '../../components/fieldset';
import { Text } from '../../components/text';

// Loader for React Router
export async function loader({ params }: LoaderFunctionArgs) {
    const tenantUrlStub = params.tenantUrlStub as string;
    const courseId = params.courseId as string;
    if (!tenantUrlStub || !courseId) throw new Response('Not Found', { status: 404 });
    try {
        return await getCourse(tenantUrlStub, Number(courseId));
    } catch (err: any) {
        throw new Response(err?.title || 'Failed to load course', { status: 404 });
    }
}

export default function CourseDetailsPage() {
    const { tenantUrlStub, courseId } = useParams<{ tenantUrlStub: string; courseId: string }>();
    const navigate = useNavigate();
    const initialCourse = useLoaderData() as CourseResponse;

    const {
        data: course,
        isLoading,
        isError,
        error,
    } = useQuery<CourseResponse, any>({
        queryKey: ['course', tenantUrlStub, courseId],
        queryFn: () => getCourse(tenantUrlStub!, Number(courseId)),
        initialData: initialCourse,
        enabled: !!tenantUrlStub && !!courseId,
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
                        <ErrorMessage>{error?.title || 'Failed to load course'}</ErrorMessage>
                    </Field>
                </Fieldset>
            </div>
        );
    }

    if (!course) {
        return null;
    }

    return (
        <div className="mx-auto">
            <div className="flex w-full flex-wrap items-end justify-between gap-4">
                <Heading>Course Details</Heading>
                <Button href={`/${tenantUrlStub}/courses`}>Back</Button>
            </div>
            <Divider className="my-10 mt-6" soft />
            <Fieldset>
                <Subheading>Course Information</Subheading>
                <Divider className="my-6" />
                <Field>
                    <Label>ID</Label>
                    <Text>{course.id}</Text>
                </Field>
                <Field>
                    <Label>Title</Label>
                    <Text>{course.title}</Text>
                </Field>
                <Field>
                    <Label>Description</Label>
                    <Text>{course.description || <em>No description</em>}</Text>
                </Field>
            </Fieldset>
        </div>
    );
}
