import { useParams, useNavigate, useLoaderData, LoaderFunctionArgs, Link, useSearchParams } from 'react-router-dom';
import { useQuery } from '@tanstack/react-query';
import { getCourses, ListCoursesResponse, CourseResponse } from '../../services/courses/courseService';
import { Heading } from '../../components/heading';
import { Button } from '../../components/button';
import { Divider } from '../../components/divider';
import { Table, TableBody, TableCell, TableHead, TableHeader, TableRow } from '../../components/table';
import { Fieldset, Field, ErrorMessage } from '../../components/fieldset';
import { useAuth } from '../../lib/auth/AuthContext';
import {
    Pagination,
    PaginationGap,
    PaginationList,
    PaginationNext,
    PaginationPage,
    PaginationPrevious,
} from '../../components/pagination'

const PAGE_SIZE = 10;

// Loader for React Router
export async function loader({ params, request }: LoaderFunctionArgs) {
    const tenantUrlStub = params.tenantUrlStub as string;
    if (!tenantUrlStub) throw new Response('Not Found', { status: 404 });

    // Get page from query string
    const url = new URL(request?.url || '', window.location.origin);
    const page = parseInt(url.searchParams.get('page') || '1', 10);

    try {
        return await getCourses(tenantUrlStub, page, PAGE_SIZE);
    } catch (err: any) {
        throw new Response(err?.title || 'Failed to load courses', { status: 404 });
    }
}

export default function CourseListPage() {
    const { tenantUrlStub } = useParams<{ tenantUrlStub: string }>();
    const navigate = useNavigate();
    const initialCourses = useLoaderData() as ListCoursesResponse;

    // Pagination state
    const [searchParams, setSearchParams] = useSearchParams();
    const page = parseInt(searchParams.get('page') || '1', 10);

    const {
        data,
        isLoading,
        isError,
        error,
    } = useQuery<ListCoursesResponse, any>({
        queryKey: ['courses', tenantUrlStub, page, PAGE_SIZE],
        queryFn: () => getCourses(tenantUrlStub!, page, PAGE_SIZE),
        initialData: initialCourses,
        enabled: !!tenantUrlStub,
    });

    const { roles } = useAuth();
    const canCreateCourse =
        roles.includes('Administrator') ||
        roles.includes(`${tenantUrlStub}:Administrator`) ||
        roles.includes(`${tenantUrlStub}:Instructor`);

    const courses = data?.courses ?? [];
    const totalCourses = data?.totalCourseCount || 0;
    const totalPages = Math.ceil(totalCourses / PAGE_SIZE);

    const handlePageChange = (newPage: number) => {
        setSearchParams({ page: newPage.toString() });
    };

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

    return (
        <div className="mx-auto">
            <div className="flex w-full flex-wrap items-end justify-between gap-4">
                <Heading>Courses</Heading>
                {canCreateCourse ? <Button href={`/${tenantUrlStub}/courses/create`}>New Course</Button>  : null}
            </div>
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

            {totalPages > 1 && (
                <Pagination className="mt-4">
                    <PaginationPrevious
                        href={`?page=${page > 1 ? page - 1 : 1}`}
                        onClick={e => { e.preventDefault(); if (page > 1) handlePageChange(page - 1); }}
                    />
                    <PaginationList>
                        {Array.from({ length: totalPages }).map((_, idx) => {
                            const pageNum = idx + 1;
                            // Show first, last, current, and neighbors; use PaginationGap for gaps
                            if (
                                pageNum === 1 ||
                                pageNum === totalPages ||
                                Math.abs(pageNum - page) <= 1
                            ) {
                                return (
                                    <PaginationPage
                                        key={pageNum}
                                        href={`?page=${pageNum}`}
                                        current={pageNum === page}
                                        onClick={e => { e.preventDefault(); handlePageChange(pageNum); }}
                                    >
                                        {pageNum}
                                    </PaginationPage>
                                );
                            }
                            if (
                                (pageNum === page - 2 && pageNum > 1) ||
                                (pageNum === page + 2 && pageNum < totalPages)
                            ) {
                                return <PaginationGap key={`gap-${pageNum}`} />;
                            }
                            return null;
                        })}
                    </PaginationList>
                    <PaginationNext
                        href={`?page=${page < totalPages ? page + 1 : totalPages}`}
                        onClick={e => { e.preventDefault(); if (page < totalPages) handlePageChange(page + 1); }}
                    />
                </Pagination>
            )}
        </div>
    );
}
