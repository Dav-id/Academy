//import { useParams } from "react-router-dom";

//export default function CoursesPage() {
//    const { tenant } = useParams();

//    return (
//        <div>
//            <h1>Courses for Tenant: {tenant}</h1>
//            {/* Your course content here */}
//        </div>
//    );
//}


import type { Route } from "./+types/home";

export function loader() {
    return { name: "React Router" };
}

export default function Tenant({ loaderData }: Route.ComponentProps) {
    return (
        <div className="p-4 text-center">
            <h1 className="text-2xl">Hello, {loaderData.name}</h1>
            <a
                className="mt-2 block text-blue-500 underline hover:text-blue-600"
                href="https://reactrouter.com/docs"
            >
                React Router Docs
            </a>
        </div>
    );
}