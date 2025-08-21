import { NavItemSchema } from './types';

export const navSchema: NavItemSchema[] = [
    {
        id: 'courses',
        label: 'Courses',
        href: ({ tenant }) => (tenant ? `/${tenant}/courses` : null),
        visibleIf: (ctx) => Boolean(ctx.tenant),
        children: [
            // Course-scoped section, visible only when a course is selected
            {
                id: 'course-scope',
                label: (ctx) => (ctx.courseId !== null && ctx.courseId !== undefined ? `Course - ${ctx.courseId}` : 'Course'),
                visibleIf: (ctx) =>  ctx.courseId !== null && ctx.courseId !== undefined,
                // Nested children for a selected course
                children: (ctx) => [
                    {
                        id: 'modules',
                        label: 'Modules',
                        href: ({ tenant, courseId }) => (courseId ? `/${tenant}/courses/${courseId}/modules` : null),
                        children: [
                            {
                                id: 'module-scope',
                                label: (ctx) => (ctx.moduleId ? `Module - ${ctx.moduleId}` : 'Module'),
                                visibleIf: (ctx) => ctx.moduleId !== null,
                                children: (ctx) => [
                                    {
                                        id: 'lessons',
                                        label: 'Lessons',
                                        href: ({ tenant, courseId, moduleId }) => tenant && courseId && moduleId ? `/${tenant}/courses/${courseId}/modules/${moduleId}/lessons` : null,
                                        children: [
                                            {
                                                id: 'lesson-scope',
                                                label: (ctx) => (ctx.lessonId ? `Lesson - ${ctx.lessonId}` : 'Lesson'),
                                                visibleIf: (ctx) => ctx.lessonId !== null,
                                                children: (ctx) => [
                                                    {
                                                        id: 'lesson-assessments',
                                                        label: 'Lesson Assessments',
                                                        href: ({ tenant, courseId, moduleId, lessonId }) =>
                                                            tenant && courseId && moduleId && lessonId ? `/${tenant}/courses/${courseId}/modules/${moduleId}/lessons/${lessonId}/assessments` : null,
                                                    },
                                                ],
                                            },
                                        ],
                                    },
                                ],
                            },
                        ],
                    },
                    {
                        id: 'enrollments',
                        label: 'Enrollments',
                        href: ({ tenant, courseId }) => (tenant && courseId ? `/${tenant}/courses/${courseId}/enrollments` : null),
                    },
                    {
                        id: 'course-assessments',
                        label: 'Course Assessments',
                        href: ({ tenant, courseId }) => (tenant && courseId ? `/${tenant}/courses/${courseId}/assessments` : null),
                    },
                ],
            },
        ],
    },
    {
        id: 'users',
        label: 'Users',
        href: ({ tenant }) => (tenant ? `/${tenant}/users` : null),
        visibleIf: (ctx) => Boolean(ctx.tenant),
        children: [
            {
                id: 'user-profile',
                label: 'Profile',
                visibleIf: (ctx) => ctx.userId !== null,
                href: ({ tenant, userId }) => (tenant ? `/${tenant}/users/${userId}/profile` : null),
            },
            {
                id: 'user-settings',
                label: 'Settings',
                visibleIf: (ctx) => ctx.userId !== null,
                href: ({ tenant, userId }) => (tenant ? `/${tenant}/users/${userId}/settings` : null),
            },
        ],
    },
];
