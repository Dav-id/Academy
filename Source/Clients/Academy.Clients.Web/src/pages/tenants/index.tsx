import { useQuery } from '@tanstack/react-query';
import { getTenants, TenantResponse } from '../../services/tenantService';

import { Heading, Subheading } from '../../components/heading'
import { Table, TableBody, TableCell, TableHead, TableHeader, TableRow } from '../../components/table'

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
            <Heading>Tenants</Heading>

            <Subheading>List of tenants in the system. You are logged in as: {roles.join(", ")}</Subheading>

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
