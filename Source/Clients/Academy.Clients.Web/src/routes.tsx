import { createBrowserRouter, RouterProvider } from 'react-router-dom';

import RootLayout from './layouts/RootLayout';
import ErrorPage from './pages/errors/error';

// Example loader
import TenantListPage, { loader as tenantListLoader } from './pages/tenants/index';
import CourseListPage, { loader as courseListLoader } from './pages/courses/index';
import CourseDetailsPage, { loader as courseDetailLoader } from './pages/courses/details';

import OidcCallback from './lib/auth/OidcCallback';
import ProtectedRoute from './lib/auth/ProtectedRoute';

const router = createBrowserRouter([
    {
        path: '/callback',
        element: <OidcCallback />,
    },
    {
        path: '/',
        element: <RootLayout />,
        errorElement: <ErrorPage />,
        children: [
            {
                index: true,
                element: (
                    <ProtectedRoute>
                        <TenantListPage />
                    </ProtectedRoute>
                ),
                loader: tenantListLoader,
            },
            {
                path: ':tenantUrlStub/courses',
                element: (
                    <ProtectedRoute>
                        <CourseListPage />
                    </ProtectedRoute>
                ),
                loader: courseListLoader,
            },
            {
                path: ':tenantUrlStub/courses/:courseId',
                element: (
                    <ProtectedRoute>
                        <CourseDetailsPage />
                    </ProtectedRoute>
                ),
                loader: courseDetailLoader,
            },

        ],
    }
]);

export default router;
