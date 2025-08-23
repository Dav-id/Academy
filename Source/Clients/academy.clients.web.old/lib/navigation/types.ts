// /lib/navigation/types.ts
export type NavContext = {
    tenant?: string | null;
    userId?: number | null;
    courseId?: number | null;
    moduleId?: number | null;
    lessonId?: number | null;
    assessmentId?: number | null;
};

export type IdKey =
    | 'tenant'
    | 'userId'
    | 'courseId'
    | 'moduleId'
    | 'lessonId'
    | 'assessmentId';

export type NavItemResolved = {
    id: string;
    label: string;
    href?: string | null;
    children?: NavItemResolved[];
    disabled?: boolean;
    badge?: string | number | null;
};

export type NavItemSchema = {
    id: string;
    label: string | ((ctx: NavContext) => string);
    /**
     * Template path or function. If you use a template string (with ${...}),
     * set `required` so we know which ctx keys must exist.
     */
    href?:
    | string
    | ((ctx: NavContext) => string | null);

    /** Context keys required for visibility / href interpolation */
    required?: IdKey[];

    /** Hide node if returns false (after `required` check) */
    visibleIf?: (ctx: NavContext) => boolean;

    /** Mark node disabled (but visible) when true */
    disabledIf?: (ctx: NavContext) => boolean;

    /** How to behave if required keys missing. Default: 'hide' */
    showWhenMissing?: 'hide' | 'disable';

    /** Badge / counters (e.g., # of pending items) */
    badge?: (ctx: NavContext) => string | number | null;

    /** Static children or factory based on context */
    children?: NavItemSchema[] | ((ctx: NavContext) => NavItemSchema[]);
};
