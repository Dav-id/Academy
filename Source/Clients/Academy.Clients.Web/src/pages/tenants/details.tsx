import { useParams, useNavigate, useLoaderData, LoaderFunctionArgs, Link } from 'react-router-dom';
import { useQuery } from '@tanstack/react-query';
import { getTenant, TenantResponse } from '../../services/tenants/tenantService';
import { getCourses, ListCoursesResponse, CourseResponse } from '../../services/courses/courseService';
import { Heading, Subheading } from '../../components/heading';
import { Button } from '../../components/button';
import { Divider } from '../../components/divider';
import { Fieldset, Field, Label, ErrorMessage } from '../../components/fieldset';
import { Text } from '../../components/text';
import { Table, TableBody, TableCell, TableHead, TableHeader, TableRow } from '../../components/table';

// Loader for React Router
export async function loader({ params }: LoaderFunctionArgs) {
    const tenantUrlStub = params.tenantUrlStub as string;
    if (!tenantUrlStub) throw new Response('Not Found', { status: 404 });
    try {
        return await getTenant(tenantUrlStub);
    } catch (err: any) {
        throw new Response(err?.title || 'Failed to load tenant', { status: 404 });
    }
}

export default function TenantDetailsPage() {
    const { tenantUrlStub } = useParams<{ tenantUrlStub: string }>();
    const navigate = useNavigate();
    const initialTenant = useLoaderData() as TenantResponse;

    const {
        data: tenant,
        isLoading,
        isError,
        error,
    } = useQuery<TenantResponse, any>({
        queryKey: ['tenant', tenantUrlStub],
        queryFn: () => getTenant(tenantUrlStub!),
        initialData: initialTenant,
        enabled: !!tenantUrlStub,
    });

    // Fetch courses for this tenant
    const {
        data: coursesData,
        isLoading: isCoursesLoading,
        isError: isCoursesError,
        error: coursesError,
    } = useQuery<ListCoursesResponse, any>({
        queryKey: ['courses', tenantUrlStub],
        queryFn: () => getCourses(tenantUrlStub!),
        enabled: !!tenantUrlStub,
    });

    // Mock users data
    const users = [
        { id: 1, name: 'Alice Smith', email: 'alice@example.com', role: 'Instructor' },
        { id: 2, name: 'Bob Johnson', email: 'bob@example.com', role: 'Student' },
        { id: 3, name: 'Carol Lee', email: 'carol@example.com', role: 'Student' },
    ];

    if (isLoading) {
        return <div className="mx-auto max-w-4xl">Loading...</div>;
    }

    if (isError) {
        return (
            <div className="mx-auto max-w-4xl">
                <Button onClick={() => navigate(-1)}>Back</Button>
                <Fieldset>
                    <Field>
                        <ErrorMessage>{error?.title || 'Failed to load tenant'}</ErrorMessage>
                    </Field>
                </Fieldset>
            </div>
        );
    }

    if (!tenant) {
        return null;
    }

    return (
        <div className="mx-auto">
            <div className="flex w-full flex-wrap items-end justify-between gap-4">
            <div>
                <Heading>{ tenant.title }</Heading>
                    <Subheading>{tenant.description || <em></em>}</Subheading>
                </div>
                <Button href={`/`}>Back</Button>
            </div>
            <Divider className="my-10 mt-6" soft />

            <Fieldset>
                <Subheading>Courses</Subheading>
                <Divider className="my-6" />
                {isCoursesLoading ? (
                    <Text>Loading courses...</Text>
                ) : isCoursesError ? (
                    <ErrorMessage>{coursesError?.title || 'Failed to load courses'}</ErrorMessage>
                ) : (
                    <Table>
                        <TableHead>
                            <TableRow>
                                <TableHeader>Title</TableHeader>
                                <TableHeader>Description</TableHeader>
                            </TableRow>
                        </TableHead>
                        <TableBody>
                            {coursesData?.courses && coursesData.courses.length > 0 ? (
                                coursesData.courses.map((course: CourseResponse) => (
                                    <TableRow key={course.id}>
                                        <TableCell>
                                            <Link to={`/${tenant.urlStub}/courses/${course.id}`}>
                                                {course.title}
                                            </Link>
                                        </TableCell>
                                        <TableCell>{course.description}</TableCell>
                                    </TableRow>
                                ))
                            ) : (
                                <TableRow>
                                    <TableCell colSpan={2} className="text-center text-zinc-500">
                                        No courses found.
                                    </TableCell>
                                </TableRow>
                            )}
                        </TableBody>
                    </Table>
                )}
            </Fieldset>

            <Divider className="my-10" soft />

            <Fieldset>
                <Subheading>Users</Subheading>
                <Divider className="my-6" />
                <Table>
                    <TableHead>
                        <TableRow>
                            <TableHeader>Name</TableHeader>
                            <TableHeader>Email</TableHeader>
                            <TableHeader>Role</TableHeader>
                        </TableRow>
                    </TableHead>
                    <TableBody>
                        {users.map(user => (
                            <TableRow key={user.id}>
                                <TableCell>{user.name}</TableCell>
                                <TableCell>{user.email}</TableCell>
                                <TableCell>{user.role}</TableCell>
                            </TableRow>
                        ))}
                    </TableBody>
                </Table>
            </Fieldset>
        </div>
    );
}