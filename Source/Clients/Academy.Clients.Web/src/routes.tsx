import { createBrowserRouter, RouterProvider } from 'react-router-dom';

import RootLayout from './layouts/RootLayout';
import ErrorPage from './pages/errors/error';

// Example loader
import TenantCreatePage from './pages/tenants/create';
import TenantListPage, { loader as tenantListLoader } from './pages/tenants/index';
import TenantDetailsPage, { loader as tenantDetailsLoader } from './pages/tenants/details';
import TenantUpdatePage, { loader as tenantUpdateLoader } from './pages/tenants/update';

import CourseListPage, { loader as courseListLoader } from './pages/courses/index';
import CourseCreatePage from './pages/courses/create';
import CourseDetailsPage, { loader as courseDetailLoader } from './pages/courses/details';
import CourseUpdatePage, { loader as courseUpdateLoader } from './pages/courses/update';

import CourseModuleListPage, { loader as courseModuleListLoader } from './pages/courses/modules/index';
import CourseModuleCreatePage from './pages/courses/modules/create';
import CourseModuleDetailsPage, { loader as courseModuleDetailLoader } from './pages/courses/modules/details';
import CourseModuleUpdatePage, { loader as courseModuleUpdateLoader } from './pages/courses/modules/update';

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
                path: ':tenantUrlStub/update',
                element: (
                    <ProtectedRoute requiredRoles={["Administrator", ":tenantUrlStub:Administrator"]}>
                        <TenantUpdatePage />
                    </ProtectedRoute>
                ),
                loader: tenantUpdateLoader,
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
                path: ':tenantUrlStub/courses/:courseId/update',
                element: (
                    <ProtectedRoute requiredRoles={["Administrator", ":tenantUrlStub:Administrator", ":tenantUrlStub:Instructor"]}>
                        <CourseUpdatePage />
                    </ProtectedRoute>
                ),
                loader: courseUpdateLoader,
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
                path: ':tenantUrlStub/courses/:courseId/modules',
                element: (
                    <ProtectedRoute>
                        <CourseModuleListPage />
                    </ProtectedRoute>
                ),
                loader: courseModuleListLoader,
            },
            {
                path: ':tenantUrlStub/courses/:courseId/modules/:courseModuleId',
                element: (
                    <ProtectedRoute>
                        <CourseModuleDetailsPage />
                    </ProtectedRoute>
                ),
                loader: courseModuleDetailLoader,
            },
            {
                path: ':tenantUrlStub/courses/:courseId/modules/create',
                element: (
                    <ProtectedRoute requiredRoles={["Administrator", ":tenantUrlStub:Administrator", ":tenantUrlStub:Instructor"]}>
                        <CourseModuleCreatePage />
                    </ProtectedRoute>
                ),
                loader: courseModuleListLoader,
            },
            {
                path: ':tenantUrlStub/courses/:courseId/modules/:courseModuleId/update',
                element: (
                    <ProtectedRoute requiredRoles={["Administrator", ":tenantUrlStub:Administrator", ":tenantUrlStub:Instructor"]}>
                        <CourseModuleUpdatePage />
                    </ProtectedRoute>
                ),
                loader: courseModuleUpdateLoader,
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