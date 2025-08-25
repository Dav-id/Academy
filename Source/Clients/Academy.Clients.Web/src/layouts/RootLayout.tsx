import React, { ReactNode, useEffect } from 'react';
import { Avatar } from '../components/avatar'
import {
    Dropdown,
    DropdownButton,
    DropdownDivider,
    DropdownItem,
    DropdownLabel,
    DropdownMenu,
} from '../components/dropdown'
import { Navbar, NavbarItem, NavbarSection, NavbarSpacer } from '../components/navbar'
import {
    Sidebar,
    SidebarBody,
    SidebarFooter,
    SidebarHeader,
    SidebarHeading,
    SidebarItem,
    SidebarLabel,
    SidebarSection,
    SidebarSpacer,
} from '../components/sidebar'
import { SidebarLayout } from '../components/sidebar-layout'
import {
    ArrowRightStartOnRectangleIcon,
    ChevronDownIcon,
    ChevronUpIcon,
    Cog8ToothIcon,
    LightBulbIcon,
    PlusIcon,
    ShieldCheckIcon,
    UserCircleIcon,
} from '@heroicons/react/16/solid'
import {
    Cog6ToothIcon,
    HomeIcon,
    QuestionMarkCircleIcon,
    SparklesIcon,
    Square2StackIcon,
    TicketIcon,
} from '@heroicons/react/20/solid'

import { Outlet, useParams } from 'react-router-dom';
import { useAuth } from '../lib/auth/AuthContext'; // Import your auth context
import { userManager } from '../lib/auth/oidc';
import { useQuery } from '@tanstack/react-query';
import { getTenant, getTenants, TenantResponse } from '../services/tenants/tenantService';

type LayoutProps = {
    children: ReactNode;
};

const RootLayout = () => {
    const { user } = useAuth(); // Use the hook inside the component

    // The profile is typically under user.profile
    const profile = user?.profile;
    const name = profile?.name || profile?.preferred_username || profile?.email || 'User';
    const email = profile?.email;

    // Extract params from the current route
    const { tenantUrlStub, courseId, moduleId, lessonId, assessmentId } = useParams<{
        tenantUrlStub?: string;
        courseId?: string;
        moduleId?: string;
        lessonId?: string;
        assessmentId?: string;
    }>();

    useEffect(() => {
        if (user === null) {
            userManager.signinRedirect();
        }
    }, [user]);

    if (user === null) {
        return null; // Prevent rendering while redirecting
    }

    const {
        data: tenant,
    } = useQuery<TenantResponse, any>({
        queryKey: ['tenant', tenantUrlStub],
        queryFn: () => getTenant(tenantUrlStub!),
        enabled: !!tenantUrlStub,
    });

    const {
        data: tenantsData,
        isLoading: isTenantsLoading,
        isError: isTenantsError,
        error: tenantsError,
    } = useQuery<{ tenants: TenantResponse[] }, any>({
        queryKey: ['tenants'],
        queryFn: getTenants,
    });

    const tenants = tenantsData?.tenants ?? [];

    const { roles } = useAuth();

    return (
        <SidebarLayout
            navbar={
                <Navbar>
                    <NavbarSpacer />
                    <NavbarSection>
                        <Dropdown>
                            <DropdownButton as={NavbarItem}>
                                <Avatar src="/users/erica.jpg" square />
                            </DropdownButton>
                            <AccountDropdownMenu anchor="bottom end" />
                        </Dropdown>
                    </NavbarSection>
                </Navbar>
            }
            sidebar={

                <Sidebar>
                    <SidebarHeader>
                        <Dropdown>
                            <DropdownButton as={SidebarItem}>
                                <Avatar initials={
                                    tenant?.title
                                        .split(' ')
                                        .filter(Boolean)
                                        .map(part => part[0])
                                        .join('')
                                        .toUpperCase()
                                } square />
                                {
                                    tenantUrlStub ?
                                        <SidebarLabel>{tenant?.title}</SidebarLabel>
                                        :
                                        <SidebarLabel>Academy</SidebarLabel>
                                }
                                <ChevronDownIcon />
                            </DropdownButton>
                            <DropdownMenu className="min-w-80 lg:min-w-64" anchor="bottom start">
                                {
                                    tenants.length === 0 ? (
                                        <SidebarLabel>
                                            No tenants found.
                                        </SidebarLabel>
                                    ) : (
                                        tenants.map((tenant: TenantResponse) => (
                                            <DropdownItem href={`/${tenant.urlStub}`}>
                                                <Avatar slot="icon" initials={
                                                    tenant?.title
                                                        .split(' ')
                                                        .filter(Boolean)
                                                        .map(part => part[0])
                                                        .join('')
                                                        .toUpperCase()
                                                } square />
                                                <DropdownLabel>{tenant.title}</DropdownLabel>
                                            </DropdownItem>
                                        ))
                                    )
                                }

                                {roles.includes('Administrator') ? (
                                    <>
                                        <DropdownDivider />
                                        <DropdownItem href="/tenants/create">
                                            <PlusIcon />
                                            <DropdownLabel>New Tenant&hellip;</DropdownLabel>
                                        </DropdownItem>
                                    </>
                                ) : null}
                            </DropdownMenu>
                        </Dropdown>

                    </SidebarHeader>

                    <SidebarBody>
                        <SidebarSection>
                            <SidebarItem href="/" current={false}>
                                <HomeIcon />
                                <SidebarLabel>Home</SidebarLabel>
                            </SidebarItem>

                            {/* Show Courses if tenantUrlStub is present */}
                            {tenantUrlStub && (

                                <>
                                    <SidebarHeading>Tenant</SidebarHeading>
                                    <SidebarItem href={`/${tenantUrlStub}`} current={false}>
                                        <HomeIcon />
                                        <SidebarLabel>Dashboard</SidebarLabel>
                                    </SidebarItem>
                                    <SidebarItem href={`/${tenantUrlStub}/courses`} current={false}>
                                        <Square2StackIcon />
                                        <SidebarLabel>Courses</SidebarLabel>
                                    </SidebarItem>
                                    <SidebarItem href={`/${tenantUrlStub}/accounts`} current={false}>
                                        <Square2StackIcon />
                                        <SidebarLabel>Accounts</SidebarLabel>
                                    </SidebarItem>
                                </>
                            )}

                            {/* Show Modules and Assessments if tenantUrlStub and courseId are present */}
                            {tenantUrlStub && courseId && (
                                <>

                                    <SidebarHeading>Course</SidebarHeading>
                                    <SidebarItem href={`/${tenantUrlStub}/courses/${courseId}/modules`} current={false}>
                                        <TicketIcon />
                                        <SidebarLabel>Modules</SidebarLabel>
                                    </SidebarItem>
                                    <SidebarItem href={`/${tenantUrlStub}/courses/${courseId}/assessments`} current={false}>
                                        <TicketIcon />
                                        <SidebarLabel>Assessments</SidebarLabel>
                                    </SidebarItem>
                                </>
                            )}

                            {/* Show Lessons if tenantUrlStub, courseId, and moduleId are present */}
                            {tenantUrlStub && courseId && moduleId && (
                                <SidebarItem href={`/${tenantUrlStub}/courses/${courseId}/modules/${moduleId}/lessons`} current={false}>
                                    <TicketIcon />
                                    <SidebarLabel>Lessons</SidebarLabel>
                                </SidebarItem>
                            )}
                        </SidebarSection>

                        <SidebarSpacer />

                        <SidebarSection>
                            {tenantUrlStub && (
                                <>
                                    <SidebarItem href={`/${tenantUrlStub}/settings`} current={false}>
                                        <Cog6ToothIcon />
                                        <SidebarLabel>Settings</SidebarLabel>
                                    </SidebarItem>
                                </>
                            )}
                            <SidebarItem href="/changelog">
                                <SparklesIcon />
                                <SidebarLabel>Changelog</SidebarLabel>
                            </SidebarItem>
                        </SidebarSection>
                    </SidebarBody>

                    <SidebarFooter className="max-lg:hidden">
                        <Dropdown>
                            <DropdownButton as={SidebarItem}>
                                <span className="flex min-w-0 items-center gap-3">
                                    <Avatar
                                        initials={
                                            name
                                                .split(' ')
                                                .filter(Boolean)
                                                .map(part => part[0])
                                                .slice(0, 2)
                                                .join('')
                                                .toUpperCase()
                                        }
                                        className="size-10"
                                        square
                                        alt=""
                                    />
                                    <span className="min-w-0">
                                        <span className="text-sm/5 block truncate font-medium text-zinc-950 dark:text-white">{name}</span>
                                        <span className="text-xs/5 block truncate font-normal text-zinc-500 dark:text-zinc-400">
                                            {email}
                                        </span>
                                    </span>
                                </span>
                                <ChevronUpIcon />
                            </DropdownButton>
                            <AccountDropdownMenu anchor="top start" />
                        </Dropdown>
                    </SidebarFooter>
                </Sidebar>
            }
        >
            <Outlet />
        </SidebarLayout>
    );
};
export default RootLayout;


function AccountDropdownMenu({ anchor }: { anchor: 'top start' | 'bottom end' }) {
    const { logout } = useAuth(); // Use the hook inside the component

    return (
        <DropdownMenu className="min-w-64" anchor={anchor}>
            {/*<DropdownItem href="#">*/}
            {/*    <UserCircleIcon />*/}
            {/*    <DropdownLabel>My account</DropdownLabel>*/}
            {/*</DropdownItem>*/}
            {/*<DropdownDivider />*/}
            {/*<DropdownItem href="#">*/}
            {/*    <ShieldCheckIcon />*/}
            {/*    <DropdownLabel>Privacy policy</DropdownLabel>*/}
            {/*</DropdownItem>*/}
            {/*<DropdownItem href="#">*/}
            {/*    <LightBulbIcon />*/}
            {/*    <DropdownLabel>Share feedback</DropdownLabel>*/}
            {/*</DropdownItem>*/}
            {/*<DropdownDivider />*/}
            <DropdownItem onClick={logout}>
                <ArrowRightStartOnRectangleIcon />
                <DropdownLabel>Sign out</DropdownLabel>
            </DropdownItem>
        </DropdownMenu>
    )
}
