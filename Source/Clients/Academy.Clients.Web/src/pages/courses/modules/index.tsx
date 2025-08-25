import { useParams, useNavigate, useLoaderData, LoaderFunctionArgs, useSearchParams, Link } from 'react-router-dom';
import { useQuery } from '@tanstack/react-query';
import { getCourseModules, ListCourseModulesResponse, CourseModuleResponse } from '../../../services/courses/courseModuleService';
import { Heading } from '../../../components/heading';
import { Button } from '../../../components/button';
import { Divider } from '../../../components/divider';
import { Table, TableBody, TableCell, TableHead, TableHeader, TableRow } from '../../../components/table';
import { Fieldset, Field, ErrorMessage } from '../../../components/fieldset';
import {
    Pagination,
    PaginationGap,
    PaginationList,
    PaginationNext,
    PaginationPage,
    PaginationPrevious,
} from '../../../components/pagination';
import { useAuth } from '../../../lib/auth/AuthContext';

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
        const modulesResponse = await getCourseModules(tenantUrlStub, Number(courseId), page, PAGE_SIZE);
        return { modules: modulesResponse.modules, totalCount: modulesResponse.totalCount ?? 0, page };
    } catch (err: any) {
        throw new Response(err?.title || 'Failed to load modules', { status: 404 });
    }
}

export default function CourseModuleListPage() {
    const { tenantUrlStub, courseId } = useParams<{ tenantUrlStub: string; courseId: string }>();
    const navigate = useNavigate();
    const loaderData = useLoaderData() as { modules: CourseModuleResponse[]; totalCount: number; page: number };
    const { roles } = useAuth();

    // Pagination state
    const [searchParams, setSearchParams] = useSearchParams();
    const page = parseInt(searchParams.get('page') || loaderData.page?.toString() || '1', 10);

    const {
        data,
        isLoading,
        isError,
        error,
    } = useQuery<ListCourseModulesResponse, any>({
        queryKey: ['courseModules', tenantUrlStub, courseId, page, PAGE_SIZE],
        queryFn: () => getCourseModules(tenantUrlStub!, Number(courseId), page, PAGE_SIZE),
        initialData: { modules: loaderData.modules, totalCount: loaderData.totalCount },
        enabled: !!tenantUrlStub && !!courseId,
    });

    const canCreateModule =
        roles.includes('Administrator') ||
        roles.includes(`${tenantUrlStub}:Administrator`) ||
        roles.includes(`${tenantUrlStub}:Instructor`);

    const modules = data?.modules ?? [];
    const totalModules = data?.totalCount ?? 0;
    const totalPages = Math.ceil(totalModules / PAGE_SIZE);

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
                        <ErrorMessage>{error?.title || 'Failed to load modules'}</ErrorMessage>
                    </Field>
                </Fieldset>
            </div>
        );
    }

    return (
        <div className="mx-auto">
            <div className="flex w-full flex-wrap items-end justify-between gap-4">
                <Heading>Course Modules</Heading>
                {canCreateModule ? (
                    <Button href={`/${tenantUrlStub}/courses/${courseId}/modules/create`}>New Module</Button>
                ) : null}
            </div>
            <Divider className="my-6" soft />
            <Table>
                <TableHead>
                    <TableRow>
                        <TableHeader>Order</TableHeader>
                        <TableHeader>Title</TableHeader>
                        <TableHeader>Description</TableHeader>
                    </TableRow>
                </TableHead>
                <TableBody>
                    {modules.length === 0 ? (
                        <TableRow>
                            <TableCell colSpan={3} className="text-center text-zinc-500">
                                No modules found.
                            </TableCell>
                        </TableRow>
                    ) : (
                        modules
                            .slice()
                            .sort((a, b) => (a.order ?? 0) - (b.order ?? 0))
                            .map((module: CourseModuleResponse) => (
                                <TableRow key={module.id} href={`/${tenantUrlStub}/courses/${courseId}/modules/${module.id}`}>
                                    <TableCell>{module.order}</TableCell>
                                    <TableCell>
                                        <Link to={`/${tenantUrlStub}/courses/${courseId}/modules/${module.id}`}>
                                            {module.title}
                                        </Link>
                                    </TableCell>
                                    <TableCell className="text-zinc-500">{module.description}</TableCell>
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