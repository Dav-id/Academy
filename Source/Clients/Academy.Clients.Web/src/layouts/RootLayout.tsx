import React, { ReactNode } from 'react';
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

import { Outlet, NavLink } from 'react-router-dom';
import { useAuth } from '../lib/auth/AuthContext'; // Import your auth context
import { userManager } from '../lib/auth/oidc';

type LayoutProps = {
    children: ReactNode;
};

const RootLayout = () => {
    const { user, logout } = useAuth(); // Use the hook inside the component

    // The profile is typically under user.profile
    const profile = user?.profile;
    const name = profile?.name || profile?.preferred_username || profile?.email || 'User';
    const email = profile?.email;

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
                                <Avatar src="/teams/catalyst.svg" />
                                <SidebarLabel>Catalyst</SidebarLabel>
                                <ChevronDownIcon />
                            </DropdownButton>
                            <DropdownMenu className="min-w-80 lg:min-w-64" anchor="bottom start">
                                <DropdownItem href="/settings">
                                    <Cog8ToothIcon />
                                    <DropdownLabel>Settings</DropdownLabel>
                                </DropdownItem>
                                <DropdownDivider />
                                <DropdownItem href="#">
                                    <Avatar slot="icon" src="/teams/catalyst.svg" />
                                    <DropdownLabel>Catalyst</DropdownLabel>
                                </DropdownItem>
                                <DropdownItem href="#">
                                    <Avatar slot="icon" initials="BE" className="bg-purple-500 text-white" />
                                    <DropdownLabel>Big Events</DropdownLabel>
                                </DropdownItem>
                                <DropdownDivider />
                                <DropdownItem href="#">
                                    <PlusIcon />
                                    <DropdownLabel>New team&hellip;</DropdownLabel>
                                </DropdownItem>
                            </DropdownMenu>
                        </Dropdown>
                    </SidebarHeader>

                    <SidebarBody>
                        <SidebarSection>
                            <SidebarItem href="/" current={ false /*pathname === '/'*/}>
                                <HomeIcon />
                                <SidebarLabel>Home</SidebarLabel>
                            </SidebarItem>
                            <SidebarItem href="/events" current={false/*pathname.startsWith('/events')*/}>
                                <Square2StackIcon />
                                <SidebarLabel>Events</SidebarLabel>
                            </SidebarItem>
                            <SidebarItem href="/orders" current={false/*pathname.startsWith('/orders')*/}>
                                <TicketIcon />
                                <SidebarLabel>Orders</SidebarLabel>
                            </SidebarItem>
                            <SidebarItem href="/settings" current={false/*pathname.startsWith('/settings')*/}>
                                <Cog6ToothIcon />
                                <SidebarLabel>Settings</SidebarLabel>
                            </SidebarItem>
                        </SidebarSection>

                        <SidebarSpacer />

                        <SidebarSection>
                            <SidebarItem href="#">
                                <QuestionMarkCircleIcon />
                                <SidebarLabel>Support</SidebarLabel>
                            </SidebarItem>
                            <SidebarItem href="#">
                                <SparklesIcon />
                                <SidebarLabel>Changelog</SidebarLabel>
                            </SidebarItem>
                        </SidebarSection>
                    </SidebarBody>

                    <SidebarFooter className="max-lg:hidden">
                        <Dropdown>
                            <DropdownButton as={SidebarItem}>
                                <span className="flex min-w-0 items-center gap-3">
                                    <Avatar src="/users/erica.jpg" className="size-10" square alt="" />
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
    const logout = () => userManager.signoutRedirect();

    return (
        <DropdownMenu className="min-w-64" anchor={anchor}>
            <DropdownItem href="#">
                <UserCircleIcon />
                <DropdownLabel>My account</DropdownLabel>
            </DropdownItem>
            <DropdownDivider />
            <DropdownItem href="#">
                <ShieldCheckIcon />
                <DropdownLabel>Privacy policy</DropdownLabel>
            </DropdownItem>
            <DropdownItem href="#">
                <LightBulbIcon />
                <DropdownLabel>Share feedback</DropdownLabel>
            </DropdownItem>
            <DropdownDivider />
            <DropdownItem onClick={logout}>
                <ArrowRightStartOnRectangleIcon />
                <DropdownLabel>Sign out</DropdownLabel>
            </DropdownItem>
        </DropdownMenu>
    )
}


