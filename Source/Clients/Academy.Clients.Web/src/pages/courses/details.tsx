import { useLoaderData } from 'react-router-dom';


export function loader({ params }) {
    const { tenantUrlStub, courseId } = params;
    return { tenantUrlStub: tenantUrlStub, courseId: courseId }
}

export default function CoursePage() {
    const course = useLoaderData();
    return (
        <div>
            <h2>Course: {course.tenantUrlStub}</h2>
            <p>{course.courseId}</p>
        </div>
    );
}
