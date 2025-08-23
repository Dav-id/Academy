import { useLoaderData } from 'react-router-dom';


export function loader({ params }) {
    const { tenantUrlStub } = params;
    return { tenantUrlStub: tenantUrlStub}
}

export default function CoursePage() {
    const course = useLoaderData();
    return (
        <div>
            <h2>Courses - Tenant: {course.tenantUrlStub}</h2>
        </div>
    );
}
