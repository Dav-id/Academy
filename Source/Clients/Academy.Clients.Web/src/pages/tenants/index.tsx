import { useLoaderData } from 'react-router-dom';
import { Heading, Subheading } from '../../components/heading'
import { Table, TableBody, TableCell, TableHead, TableHeader, TableRow } from '../../components/table'
import { getTenants, TenantResponse } from '../../services/tenantService';
import { useAuth } from '../../lib/auth/AuthContext';

export async function loader() {
    return await getTenants();
}

export default function TenantListPage() {
    const data = useLoaderData() as { tenants: TenantResponse[] };
    const tenants = data.tenants;

    console.log('Loader data:', tenants);

    return (
        <>
            <Heading>Tenants</Heading>

            <Table className="mt-4 [--gutter:--spacing(6)] lg:[--gutter:--spacing(10)]">
                <TableHead>
                    <TableRow>
                        {/*<TableHeader>Url Stub</TableHeader>*/}
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
                                {/*<TableCell>{tenant.urlStub}</TableCell>*/}
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
