import { useParams, useNavigate, useLoaderData, LoaderFunctionArgs, useSearchParams } from 'react-router-dom';
import { useQuery } from '@tanstack/react-query';
import { getCourse, CourseResponse } from '../../services/courses/courseService';
import { getCourseModules, ListCourseModulesResponse, CourseModuleResponse } from '../../services/courses/courseModuleService';
import { Heading } from '../../components/heading';
import { Button } from '../../components/button';
import { Divider } from '../../components/divider';
import { Fieldset, Field, ErrorMessage } from '../../components/fieldset';
import { Table, TableBody, TableCell, TableHead, TableHeader, TableRow } from '../../components/table';
import {
    Pagination,
    PaginationGap,
    PaginationList,
    PaginationNext,
    PaginationPage,
    PaginationPrevious,
} from '../../components/pagination';
import { useAuth } from '../../lib/auth/AuthContext';
import { Link } from '../../components/link';

const PAGE_SIZE = 10;

// Loader for React Router
export async function loader({ params, request }: LoaderFunctionArgs) {
    const tenantUrlStub = params.tenantUrlStub as string;
    const courseId = params.courseId as string;
    if (!tenantUrlStub || !courseId) throw new Response('Not Found', { status: 404 });

    // Get page from query string
    const url = new URL(request?.url || '', window.location.origin);
    const page = parseInt(url.searchParams.get('page') || '1', 10);

    try {
        const [course, modulesResponse] = await Promise.all([
            getCourse(tenantUrlStub, Number(courseId)),
            getCourseModules(tenantUrlStub, Number(courseId), page, PAGE_SIZE),
        ]);
        // modulesResponse is expected to be { modules: CourseModuleResponse[], totalCount: number }
        return { course, modules: modulesResponse.modules, totalCount: modulesResponse.totalCount, page };
    } catch (err: any) {
        throw new Response(err?.title || 'Failed to load course', { status: 404 });
    }
}

export default function CourseDetailsPage() {
    const { tenantUrlStub, courseId } = useParams<{ tenantUrlStub: string; courseId: string }>();
    const navigate = useNavigate();
    const loaderData = useLoaderData() as { course: CourseResponse; modules: CourseModuleResponse[]; totalCount: number; page: number };
    const { roles } = useAuth();

    // Pagination state
    const [searchParams, setSearchParams] = useSearchParams();
    const page = parseInt(searchParams.get('page') || loaderData.page?.toString() || '1', 10);

    const {
        data: course,
        isLoading: isCourseLoading,
        isError: isCourseError,
        error: courseError,
    } = useQuery<CourseResponse, any>({
        queryKey: ['course', tenantUrlStub, courseId],
        queryFn: () => getCourse(tenantUrlStub!, Number(courseId)),
        initialData: loaderData.course,
        enabled: !!tenantUrlStub && !!courseId,
    });

    const {
        data: modulesData,
        isLoading: isModulesLoading,
        isError: isModulesError,
        error: modulesError,
    } = useQuery<ListCourseModulesResponse, any>({
        queryKey: ['courseModules', tenantUrlStub, courseId, page, PAGE_SIZE],
        queryFn: () => getCourseModules(tenantUrlStub!, Number(courseId), page, PAGE_SIZE),
        initialData: { modules: loaderData.modules, totalCount: loaderData.totalCount },
        enabled: !!tenantUrlStub && !!courseId,
    });

    const modules = modulesData?.modules ?? [];
    const totalModules = modulesData?.totalCount ?? 0;
    const totalPages = Math.ceil(totalModules / PAGE_SIZE);

    const handlePageChange = (newPage: number) => {
        setSearchParams({ page: newPage.toString() });
    };

    if (isCourseLoading || isModulesLoading) {
        return <div className="mx-auto max-w-4xl">Loading...</div>;
    }

    if (isCourseError || isModulesError) {
        return (
            <div className="mx-auto max-w-4xl">
                <Button onClick={() => navigate(-1)}>Back</Button>
                <Fieldset>
                    <Field>
                        <ErrorMessage>
                            {courseError?.title || modulesError?.title || 'Failed to load course'}
                        </ErrorMessage>
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
            <div className="mt-4 flex flex-wrap items-end justify-between gap-4">
                <div className="flex flex-wrap items-center gap-6">
                    <div>
                        <div className="flex flex-wrap items-center gap-x-4 gap-y-2">
                            <Heading>{course.title}</Heading>
                        </div>
                        <div className="text-sm/6 mt-2 text-zinc-500">
                            {course.description || <em>No description</em>}
                        </div>
                    </div>
                </div>
                <div className="flex gap-4">
                    <Button outline href={`/${tenantUrlStub}/courses`}>Back</Button>
                    {roles.includes('Administrator') || roles.includes(tenantUrlStub + ':Administrator') || roles.includes(tenantUrlStub + ':Instructor') ? (
                        <Button href={`/${tenantUrlStub}/courses/${course.id}/update`}>Update</Button>
                    ) : null}
                </div>
            </div>

            <Divider className="my-10 mt-6" soft />
            <div>
                <div className="mt-4 flex flex-wrap items-end justify-between gap-4">
                    <div className="flex flex-wrap items-center gap-6">
                        <div>
                            <div className="flex flex-wrap items-center gap-x-4 gap-y-2">
                                <Heading>Course Modules</Heading>
                            </div>

                        </div>
                    </div>
                    <div className="flex gap-4">
                        {roles.includes('Administrator') || roles.includes(tenantUrlStub + ':Administrator') || roles.includes(tenantUrlStub + ':Instructor') ? (
                            <Button href={`/${tenantUrlStub}/courses/${course.id}/modules/create`}>New Module</Button>
                        ) : null}
                    </div>
                </div>
                <Divider className="my-6" />

                <Table>
                    <TableHead>
                        <TableRow>
                            <TableHeader>Title</TableHeader>
                            <TableHeader>Description</TableHeader>
                        </TableRow>
                    </TableHead>
                    <TableBody>
                        {modules && modules.length > 0 ? (
                            modules
                                .slice()
                                .sort((a, b) => (a.order ?? 0) - (b.order ?? 0))
                                .map((module) => (
                                    <TableRow key={module.id}>
                                        <TableCell>
                                            <Link to={`/${tenantUrlStub}/courses/${courseId}/modules/${module.id}`}>
                                                {module.title}
                                            </Link>
                                        </TableCell>
                                        <TableCell>{module.description}</TableCell>
                                    </TableRow>
                                ))
                        ) : (
                            <TableRow>
                                <TableCell colSpan={2} className="text-center text-zinc-500">
                                    No modules found.
                                </TableCell>
                            </TableRow>
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
        </div>
    );
}