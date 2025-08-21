'use client';

import * as React from 'react';
import { usePathname } from 'next/navigation';
import Link from 'next/link';

import type { NavItemResolved } from '@/lib/navigation/types';
import {
    SidebarSection,
    SidebarItem,
    SidebarLabel,
} from '@/components/sidebar';

// Optional: map node ids to icons from your set
import {
    HomeIcon,
    Square2StackIcon,
    TicketIcon,
    Cog6ToothIcon,
} from '@heroicons/react/20/solid';

const ICONS: Record<string, React.ComponentType<React.SVGProps<SVGSVGElement>>> = {
    home: HomeIcon,
    courses: Square2StackIcon,
    modules: Square2StackIcon,
    lessons: Square2StackIcon,
    enrollments: TicketIcon,
    'course-assessments': Cog6ToothIcon,
    users: HomeIcon,
    // add more ids => icons as you like
};

function isActive(href: string | undefined, pathname: string) {
    if (!href) return false;
    return pathname === href || pathname.startsWith(href + '/');
}

export function SidebarTree({ tree }: { tree: NavItemResolved[] }) {
    const pathname = usePathname();
    return (
        <SidebarSection>
            {tree.map((n) => (
                <Node key={n.id} node={n} pathname={pathname} depth={0} />
            ))}
        </SidebarSection>
    );
}

function Node({
    node,
    pathname,
    depth,
}: {
    node: NavItemResolved;
    pathname: string;
    depth: number;
}) {
    const active = isActive(node.href, pathname);
    const hasChildren = Boolean(node.children?.length);
    const [open, setOpen] = React.useState(active); // open the active branch by default
    const Icon = ICONS[node.id];

    const content = node.href ? (
        <SidebarItem href={node.href} current={active}>
            {Icon ? <Icon className="mr-2 h-4 w-4 shrink-0" /> : null}
            <SidebarLabel>{node.label}</SidebarLabel>
        </SidebarItem>
    ) : (
        <SidebarItem current={active}>
            {Icon ? <Icon className="mr-2 inline-block h-4 w-4" /> : null}
            <SidebarLabel>{node.label}</SidebarLabel>
        </SidebarItem>
    );

    return (
        <div className={depth ? 'ml-3' : ''}>
            <div className="flex items-center">
                {hasChildren && (
                    <button
                        type="button"
                        aria-label="Toggle section"
                        onClick={() => setOpen((v) => !v)}
                        className="mr-1 rounded px-1 text-xs text-zinc-500 hover:bg-zinc-100 dark:hover:bg-zinc-800"
                    >
                        {open ? '?' : '?'}
                    </button>
                )}
                {content}
            </div>

            {hasChildren && open && (
                <div className="mt-1">
                    {node.children!.map((child) => (
                        <Node key={child.id} node={child} pathname={pathname} depth={depth + 1} />
                    ))}
                </div>
            )}
        </div>
    );
}
