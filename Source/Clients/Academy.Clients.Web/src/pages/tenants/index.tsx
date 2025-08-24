import { useQuery } from '@tanstack/react-query';
import { getTenants, TenantResponse } from '../../services/tenants/tenantService';

import { Heading, Subheading } from '../../components/heading'
import { Table, TableBody, TableCell, TableHead, TableHeader, TableRow } from '../../components/table'
import { Button } from '../../components/button';

import { useAuth } from '../../lib/auth/AuthContext';

export async function loader() {
    return await getTenants();
}

export default function TenantListPage() {

    const { roles } = useAuth();

    const { data, isLoading, isError } = useQuery({
        queryKey: ['tenants'],
        queryFn: getTenants
    });

    if (isLoading) {
        return <div>Loading...</div>;
    }

    if (isError) {
        return <div>Error loading page.</div>;
    }
    const tenants = data?.tenants ?? [];

    return (
        <>
            <div className="flex w-full flex-wrap items-end justify-between gap-4">
                <Heading>Tenants</Heading>
                <div className="flex gap-4">
                    {roles.includes('Administrator') ? (
                        <Button href="/tenants/create">New Tenant</Button>
                    ) : null}
                </div>
            </div>            

            <Table className="mt-4 [--gutter:--spacing(6)] lg:[--gutter:--spacing(10)]">
                <TableHead>
                    <TableRow>
                        <TableHeader>Name</TableHeader>
                        <TableHeader>Description</TableHeader>
                    </TableRow>
                </TableHead>
                <TableBody>
                    {tenants.length === 0 ? (
                        <TableRow>
                            <TableCell colSpan={2} className="text-center text-zinc-500">
                                No tenants found.
                            </TableCell>
                        </TableRow>
                    ) : (
                        tenants.map((tenant: TenantResponse) => (
                            <TableRow key={tenant.id} href={"/" + tenant.urlStub} title={`Tenant ${tenant.title}`}>
                                <TableCell>{tenant.title}</TableCell>
                                <TableCell className="text-zinc-500">{tenant.description}</TableCell>
                            </TableRow>
                        ))
                    )}
                </TableBody>
            </Table>
        </>
    );
}
