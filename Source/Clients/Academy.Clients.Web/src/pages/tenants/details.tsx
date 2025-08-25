import React from 'react';
import { useParams, useNavigate, useLoaderData, LoaderFunctionArgs, Link, useSearchParams } from 'react-router-dom';
import { useQuery } from '@tanstack/react-query';
import { getTenant, TenantResponse } from '../../services/tenants/tenantService';
import { getCourses, ListCoursesResponse, CourseResponse } from '../../services/courses/courseService';
import { getAccounts, AccountResponse, ListAccountsResponse } from '../../services/accounts/accountService';
import { Heading, Subheading } from '../../components/heading';
import { Button } from '../../components/button';
import { Divider } from '../../components/divider';
import { Fieldset, Field, Label, ErrorMessage } from '../../components/fieldset';
import { Text } from '../../components/text';
import { Table, TableBody, TableCell, TableHead, TableHeader, TableRow } from '../../components/table';

import {
    Pagination,
    PaginationGap,
    PaginationList,
    PaginationNext,
    PaginationPage,
    PaginationPrevious,
} from '../../components/pagination'

import { useAuth } from '../../lib/auth/AuthContext';

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

const PAGE_SIZE = 10;

export default function TenantDetailsPage() {
    const { tenantUrlStub } = useParams<{ tenantUrlStub: string }>();
    const navigate = useNavigate();
    const initialTenant = useLoaderData() as TenantResponse;
    const { roles } = useAuth();

    // Pagination state
    const [searchParams, setSearchParams] = useSearchParams();
    const page = parseInt(searchParams.get('page') || '1', 10);
    const accountPage = parseInt(searchParams.get('accountPage') || '1', 10);

    // Fetch tenant
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

    // Fetch courses for this tenant with pagination
    const {
        data: coursesData,
        isLoading: isCoursesLoading,
        isError: isCoursesError,
        error: coursesError,
    } = useQuery<ListCoursesResponse, any>({
        queryKey: ['courses', tenantUrlStub, page, PAGE_SIZE],
        queryFn: () => getCourses(tenantUrlStub!, page, PAGE_SIZE),
        enabled: !!tenantUrlStub,
    });

    // Fetch user profiles for this tenant (accounts section)
    const {
        data: usersData,
        isLoading: isUsersLoading,
        isError: isUsersError,
        error: usersError,
    } = useQuery<ListAccountsResponse, any>({
        queryKey: ['users', tenantUrlStub, accountPage, PAGE_SIZE],
        queryFn: () => getAccounts(tenantUrlStub!, accountPage, PAGE_SIZE),
        enabled: !!tenantUrlStub,
    });

    // Pagination helpers
    const totalCourses = coursesData?.totalCourseCount || 0;
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

            <div className="mt-4 flex flex-wrap items-end justify-between gap-4">
                <div className="flex flex-wrap items-center gap-6">
                    <div>
                        <div className="flex flex-wrap items-center gap-x-4 gap-y-2">
                            <Heading>{tenant.title}</Heading>
                        </div>
                        <div className="text-sm/6 mt-2 text-zinc-500">
                            {tenant.description || <em>No description</em>}
                        </div>
                    </div>
                </div>
                <div className="flex gap-4">
                    <Button outline href={`/`}>Back</Button>
                    {roles.includes('Administrator') || roles.includes(tenant.urlStub + ':Administrator') ? (
                        <Button href={`/${tenant.urlStub}/update`}>Update</Button>
                    ) : null}
                </div>
            </div>

            <Divider className="my-10 mt-6" soft />

            <Fieldset>
                <div className="mt-4 flex flex-wrap items-end justify-between gap-4">
                    <Heading>Courses</Heading>
                    {roles.includes('Administrator') || roles.includes(tenant.urlStub + ':Instructor') ? (
                        <div className="mt-4 flex flex-wrap items-end justify-between gap-4">
                            <div className="flex gap-4">
                                <Button href={`/${tenant.urlStub}/courses/create`}>New Course</Button>
                            </div>
                        </div>
                    ) : null}
                </div>

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


            </Fieldset>

            {roles.includes('Administrator') || roles.includes(tenant.urlStub + ':Instructor') ? (
                <>
                    <Divider className="my-10" soft />

                    <Fieldset>
                        <div className="mt-4 flex flex-wrap items-end justify-between gap-4">
                            <Heading>Accounts</Heading>
                            <div className="mt-4 flex flex-wrap items-end justify-between gap-4">
                                <div className="flex gap-4">
                                    <Button href={`/${tenant.urlStub}/accounts/invite`}>Add Account</Button>
                                </div>
                            </div>
                        </div>
                        <Divider className="my-6" />
                        {isUsersLoading ? (
                            <Text>Loading accounts...</Text>
                        ) : isUsersError ? (
                            <ErrorMessage>{usersError?.title || 'Failed to load accounts'}</ErrorMessage>
                        ) : (
                            <Table>
                                <TableHead>
                                    <TableRow>
                                        <TableHeader>Name</TableHeader>
                                        <TableHeader>Email</TableHeader>
                                    </TableRow>
                                </TableHead>
                                <TableBody>
                                            {usersData?.users && usersData.users.length > 0 ? (
                                                usersData.users.map(user => (
                                            <TableRow key={user.id}>
                                                <TableCell>
                                                    <Link to={`/${tenant.urlStub}/accounts/${user.id}`}>
                                                        {`${user.firstName ?? ''} ${user.lastName ?? ''}`.trim()}
                                                    </Link>
                                                </TableCell>
                                                <TableCell>{user.email}</TableCell>
                                            </TableRow>
                                        ))
                                    ) : (
                                        <TableRow>
                                            <TableCell colSpan={2} className="text-center text-zinc-500">
                                                No accounts found.
                                            </TableCell>
                                        </TableRow>
                                    )}
                                </TableBody>
                            </Table>
                        )}
                    </Fieldset>
                </>
            ) : null}
        </div>
    );
}