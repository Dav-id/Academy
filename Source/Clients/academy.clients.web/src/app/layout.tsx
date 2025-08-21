import '@/styles/tailwind.css'
import type { Metadata } from 'next'

export const metadata: Metadata = {
    title: {
        template: '%s - Academy',
        default: 'Academy',
    },
    description: '',
}

export default async function RootLayout({ children }: { children: React.ReactNode }) {
    return (
        <html
            lang="en"
            className="text-zinc-950 antialiased dark:bg-zinc-900 dark:text-white lg:bg-zinc-100 dark:lg:bg-zinc-950"
        >
            <head>
                <link rel="preconnect" href="https://rsms.me/" />
                <link rel="stylesheet" href="https://rsms.me/inter/inter.css" />
            </head>
            <body>
                {children}                
            </body>
        </html>
    )
}