import { useParams, useLoaderData, LoaderFunctionArgs, useNavigate, useSearchParams, Link } from 'react-router-dom';
import { useQuery } from '@tanstack/react-query';
import { getAccounts, AccountResponse, ListAccountsResponse } from '../../services/accounts/accountService';
import { Heading } from '../../components/heading';
import { Button } from '../../components/button';
import { Divider } from '../../components/divider';
import { Table, TableBody, TableCell, TableHead, TableHeader, TableRow } from '../../components/table';
import { Fieldset, Field, ErrorMessage } from '../../components/fieldset';
import {
    Pagination,
    PaginationGap,
    PaginationList,
    PaginationNext,
    PaginationPage,
    PaginationPrevious,
} from '../../components/pagination';
import { useAuth } from '../../lib/auth/AuthContext';

const PAGE_SIZE = 10;

// Loader for React Router
export async function loader({ request, params }: LoaderFunctionArgs) {
    const tenantUrlStub = params.tenantUrlStub as string;
    // Get page from query string
    const url = new URL(request?.url || '', window.location.origin);
    const page = parseInt(url.searchParams.get('page') || '1', 10);

    try {
        return await getAccounts(tenantUrlStub!, page, PAGE_SIZE);
    } catch (err: any) {
        throw new Response(err?.title || 'Failed to load accounts', { status: 404 });
    }
}

export default function AccountListPage() {

    const { tenantUrlStub } = useParams<{ tenantUrlStub: string;}>();

    const navigate = useNavigate();
    const initialAccounts = useLoaderData() as ListAccountsResponse;

    // Pagination state
    const [searchParams, setSearchParams] = useSearchParams();
    const page = parseInt(searchParams.get('page') || '1', 10);

    const {
        data,
        isLoading,
        isError,
        error,
    } = useQuery<ListAccountsResponse, any>({
        queryKey: ['accounts', page, PAGE_SIZE],
        queryFn: () => getAccounts(tenantUrlStub!, page, PAGE_SIZE),
        initialData: initialAccounts,
    });

    const { roles } = useAuth();
    const canCreateAccount = roles.includes('Administrator');

    const accounts = data?.users ?? [];
    const totalAccounts = data?.totalCount ?? 0;
    const totalPages = Math.ceil(totalAccounts / PAGE_SIZE);

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
                        <ErrorMessage>{error?.title || 'Failed to load accounts'}</ErrorMessage>
                    </Field>
                </Fieldset>
            </div>
        );
    }

    return (
        <div className="mx-auto">
            <div className="flex w-full flex-wrap items-end justify-between gap-4">
                <Heading>Accounts</Heading>
                {canCreateAccount ? (
                    <Button href={`/${tenantUrlStub}/accounts/create`}>New Account</Button>
                ) : null}
            </div>
            <Divider className="my-6" soft />
            <Table>
                <TableHead>
                    <TableRow>
                        <TableHeader>Name</TableHeader>
                        <TableHeader>Email</TableHeader>
                    </TableRow>
                </TableHead>
                <TableBody>
                    {accounts.length === 0 ? (
                        <TableRow>
                            <TableCell colSpan={3} className="text-center text-zinc-500">
                                No accounts found.
                            </TableCell>
                        </TableRow>
                    ) : (
                        accounts.map((account: AccountResponse) => (
                            <TableRow key={account.id} href={`/${tenantUrlStub}/accounts/${account.id}`}>
                                <TableCell>
                                    <Link to={`/${tenantUrlStub}/accounts/${account.id}`}>{account.firstName} {account.lastName}</Link>
                                </TableCell>
                                <TableCell>{account.email}</TableCell>
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