// /lib/navigation/util.ts
import type { IdKey, NavContext } from './types';

/** Checks all provided keys exist and are not null/undefined */
export const hasAll = (ctx: NavContext, keys: IdKey[] | undefined): boolean =>
    !keys || keys.every((k) => ctx[k] !== null && ctx[k] !== undefined);

/**
 * Interpolates a template like "/t/${tenant}/courses/${courseId}" with ctx values.
 * If a required key is missing, returns null.
 */
export function interpolate(
    tpl: string,
    ctx: NavContext,
    required?: IdKey[]
): string | null {
    if (!hasAll(ctx, required)) return null;
    return tpl.replace(/\$\{(tenant|userId|courseId|moduleId|lessonId|assessmentId)\}/g, (_, k: IdKey) => {
        const v = ctx[k];
        return encodeURIComponent(String(v));
    });
}

/** Resolve a label that may be a string or a function */
export const labelOf = (
    label: NavItemSchema['label'],
    ctx: NavContext
): string => (typeof label === 'function' ? label(ctx) : label);

/** Materialize children that may be static array or factory */
export const materializeChildren = (
    children: NavItemSchema['children'],
    ctx: NavContext
): NavItemSchema[] | undefined =>
    typeof children === 'function' ? children(ctx) : children;
