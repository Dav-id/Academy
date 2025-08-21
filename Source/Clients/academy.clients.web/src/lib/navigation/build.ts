// /lib/navigation/build.ts
'use client';

import { cache } from 'react';
import type { NavContext, NavItemResolved, NavItemSchema } from './types';
import { labelOf, materializeChildren, hasAll } from './util';
import { navSchema } from './schema';

function resolveNode(node: NavItemSchema, ctx: NavContext): NavItemResolved | null {
    const hasRequired = hasAll(ctx, node.required);
    const mode = node.showWhenMissing ?? 'hide';

    if (!hasRequired && mode === 'hide') return null;
    if (node.visibleIf && !node.visibleIf(ctx)) return null;

    // href resolution is delegated to each node (see schema below),
    // so we just call the function and don't do string templates here.
    const href = typeof node.href === 'function'
        ? node.href(ctx)
        : typeof node.href === 'string'
            ? node.href
            : null;

    const disabled = (!hasRequired && mode === 'disable') || (node.disabledIf?.(ctx) ?? false);

    const resolved: NavItemResolved = {
        id: node.id,
        label: labelOf(node.label, ctx),
        href,
        disabled,
        badge: node.badge ? node.badge(ctx) : null,
    };

    const rawChildren = materializeChildren(node.children, ctx);
    if (rawChildren?.length) {
        const children = rawChildren
            .map((c) => resolveNode(c, ctx))
            .filter(Boolean) as NavItemResolved[];
        if (children.length) resolved.children = children;
    }
    return resolved;
}

export const buildNavTree = cache((ctx: NavContext): NavItemResolved[] =>
    navSchema.map((n) => resolveNode(n, ctx)).filter(Boolean) as NavItemResolved[]
);
