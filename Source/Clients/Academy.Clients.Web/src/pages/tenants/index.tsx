import { useLoaderData } from 'react-router-dom';
import { Heading, Subheading } from '../../components/heading'
import { Table, TableBody, TableCell, TableHead, TableHeader, TableRow } from '../../components/table'
import { getTenants } from '../../services/tenantService';

export async function loader() {
    const data = await getTenants();

    //return json(await data.json());
}

export default function TenantListPage() {
    const tenants = useLoaderData();

    console.log(tenants);

    return (
        <>
            <Heading>Tenants</Heading>
            
            <Table className="mt-4 [--gutter:--spacing(6)] lg:[--gutter:--spacing(10)]">
                <TableHead>
                    <TableRow>
                        <TableHeader>Order number</TableHeader>
                        <TableHeader>Purchase date</TableHeader>
                        <TableHeader>Customer</TableHeader>
                        <TableHeader>Event</TableHeader>
                        <TableHeader className="text-right">Amount</TableHeader>
                    </TableRow>
                </TableHead>
                <TableBody>
                    {/*{tenants.map((tenant) => (*/}
                    {/*    <TableRow key={tenant.id} href={order.url} title={`Order #${order.id}`}>*/}
                    {/*        <TableCell>{order.id}</TableCell>*/}
                    {/*        <TableCell className="text-zinc-500">{order.date}</TableCell>*/}
                    {/*        <TableCell>{order.customer.name}</TableCell>*/}
                    {/*        <TableCell>*/}
                    {/*            <div className="flex items-center gap-2">*/}
                    {/*                <Avatar src={order.event.thumbUrl} className="size-6" />*/}
                    {/*                <span>{order.event.name}</span>*/}
                    {/*            </div>*/}
                    {/*        </TableCell>*/}
                    {/*        <TableCell className="text-right">US{order.amount.usd}</TableCell>*/}
                    {/*    </TableRow>*/}
                    {/*))}*/}
                </TableBody>
            </Table>
        </>
    );
}
