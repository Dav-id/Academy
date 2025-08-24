import { createBrowserRouter, RouterProvider } from 'react-router-dom';

import RootLayout from './layouts/RootLayout';
import ErrorPage from './pages/errors/error';

// Example loader
import TenantListPage, { loader as tenantListLoader } from './pages/tenants/index';
import TenantDetailsPage, { loader as tenantDetailsLoader } from './pages/tenants/details';
import TenantCreatePage from './pages/tenants/create';

import CourseListPage, { loader as courseListLoader } from './pages/courses/index';
import CourseCreatePage from './pages/courses/create';
import CourseDetailsPage, { loader as courseDetailLoader } from './pages/courses/details';

import ChangelogPage from './pages/changelog';

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
                path: 'tenants/create',
                element: (
                    <ProtectedRoute requiredRoles={['Administrator']}>
                        <TenantCreatePage />
                    </ProtectedRoute>
                ),
            },
            {
                path: ':tenantUrlStub',
                element: (
                    <ProtectedRoute>
                        <TenantDetailsPage />
                    </ProtectedRoute>
                ),
                loader: tenantDetailsLoader,
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
                path: ':tenantUrlStub/courses/create',
                element: (
                    <ProtectedRoute requiredRoles={["Administrator", ":tenantUrlStub:Administrator", ":tenantUrlStub:Instructor"]}>
                        <CourseCreatePage />
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
            {
                path: 'changelog',
                element: (
                    <ProtectedRoute>
                        <ChangelogPage />
                    </ProtectedRoute>
                ),
            },
        ],
    },
]);

export default router;