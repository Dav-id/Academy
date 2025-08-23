/**
 * TODO: Update this component to use your client-side framework's link
 * component. We've provided examples of how to do this for Next.js, Remix, and
 * Inertia.js in the Catalyst documentation:
 *
 * https://catalyst.tailwindui.com/docs#client-side-router-integration
 */

import * as Headless from '@headlessui/react'
import React, { forwardRef } from 'react'
import { Link as RouterLink } from 'react-router-dom'

export const Link = forwardRef(function Link(
    props: { href: string } & React.ComponentPropsWithoutRef<'a'>,
    ref: React.ForwardedRef<HTMLAnchorElement>
) {
    // Map 'href' to 'to' for React Router
    const { href, ...rest } = props
    return (
        <Headless.DataInteractive>
            <RouterLink to={href} ref={ref as any} {...rest} />
        </Headless.DataInteractive>
    )
})
