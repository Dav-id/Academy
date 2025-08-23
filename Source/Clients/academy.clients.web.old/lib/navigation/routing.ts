// /lib/navigation/routing.ts
import type { NavContext } from './types';

export const RESERVED_PREFIXES = new Set(['api', 'account']);

/** True if a segment is reserved globally (not tenant-scoped) */
export const isReservedPrefix = (seg: string | null | undefined): boolean =>
    !!seg && RESERVED_PREFIXES.has(seg);

/**
 * Build a tenant-scoped path: "/:tenant/seg1/seg2..."
 * Returns null if tenant missing.
 * Throws if the tenant accidentally equals a reserved prefix.
 */
export function tenantPath(
    ctx: Pick<NavContext, 'tenant'>,
    ...segments: (string | number | false | null | undefined)[]
): string | null {
    const { tenant } = ctx;
    if (!tenant) return null;

    if (isReservedPrefix(tenant)) {
        // You said these are reserved as route roots; treat that as invalid tenant.
        // Change to `return null` if you prefer silent failure.
        throw new Error(
            `Invalid tenant "${tenant}" � conflicts with reserved top-level routes: ${[
                ...RESERVED_PREFIXES,
            ].join(', ')}`
        );
    }

    const tail = segments
        .filter((s) => s !== false && s !== null && s !== undefined)
        .map((s) => encodeURIComponent(String(s)))
        .join('/');

    return `/${encodeURIComponent(tenant)}${tail ? `/${tail}` : ''}`;
}

/** Build a global path that is explicitly *not* tenant-scoped (e.g. "/account/...") */
export function globalPath(
    ...segments: (string | number | false | null | undefined)[]
): string {
    const tail = segments
        .filter((s) => s !== false && s !== null && s !== undefined)
        .map((s) => encodeURIComponent(String(s)))
        .join('/');
    return `/${tail}`;
}
