import { useParams, useNavigate, useLoaderData, LoaderFunctionArgs, Link } from 'react-router-dom';
import { useQuery } from '@tanstack/react-query';
import { getCourses, ListCoursesResponse, CourseResponse } from '../../services/courses/courseService';
import { Heading, Subheading } from '../../components/heading';
import { Button } from '../../components/button';
import { Divider } from '../../components/divider';
import { Table, TableBody, TableCell, TableHead, TableHeader, TableRow } from '../../components/table';
import { Fieldset, Field, ErrorMessage } from '../../components/fieldset';
import { useAuth } from '../../lib/auth/AuthContext';

// Loader for React Router
export async function loader({ params }: LoaderFunctionArgs) {
    const tenantUrlStub = params.tenantUrlStub as string;
    if (!tenantUrlStub) throw new Response('Not Found', { status: 404 });
    try {
        return await getCourses(tenantUrlStub);
    } catch (err: any) {
        throw new Response(err?.title || 'Failed to load courses', { status: 404 });
    }
}

export default function CourseListPage() {
    const { tenantUrlStub } = useParams<{ tenantUrlStub: string }>();
    const navigate = useNavigate();
    const initialCourses = useLoaderData() as ListCoursesResponse;

    const {
        data,
        isLoading,
        isError,
        error,
    } = useQuery<ListCoursesResponse, any>({
        queryKey: ['courses', tenantUrlStub],
        queryFn: () => getCourses(tenantUrlStub!),
        initialData: initialCourses,
        enabled: !!tenantUrlStub,
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
                        <ErrorMessage>{error?.title || 'Failed to load courses'}</ErrorMessage>
                    </Field>
                </Fieldset>
            </div>
        );
    }
    const { roles } = useAuth();
    const canCreateCourse =
        roles.includes('Administrator') ||
        roles.includes(`${tenantUrlStub}:Administrator`) ||
        roles.includes(`${tenantUrlStub}:Instructor`);

    const courses = data?.courses ?? [];

    return (
        <div className="mx-auto">
            <div className="flex w-full flex-wrap items-end justify-between gap-4">
                <Heading>Courses</Heading>
                {canCreateCourse ? <Button href={`/${tenantUrlStub}/courses/create`}>New Course</Button>  : null}
            </div>
            {/*<Subheading>List of courses for this tenant.</Subheading>*/}
            <Divider className="my-6" soft />
            <Table>
                <TableHead>
                    <TableRow>
                        <TableHeader>Title</TableHeader>
                        <TableHeader>Description</TableHeader>
                    </TableRow>
                </TableHead>
                <TableBody>
                    {courses.length === 0 ? (
                        <TableRow>
                            <TableCell colSpan={2} className="text-center text-zinc-500">
                                No courses found.
                            </TableCell>
                        </TableRow>
                    ) : (
                        courses.map((course: CourseResponse) => (
                            <TableRow key={course.id} href={`/${tenantUrlStub}/courses/${course.id}`}>
                                <TableCell>
                                    <Link to={`/${tenantUrlStub}/courses/${course.id}`}>
                                        {course.title}
                                    </Link>
                                </TableCell>
                                <TableCell className="text-zinc-500">{course.description}</TableCell>
                            </TableRow>
                        ))
                    )}
                </TableBody>
            </Table>
        </div>
    );
}
