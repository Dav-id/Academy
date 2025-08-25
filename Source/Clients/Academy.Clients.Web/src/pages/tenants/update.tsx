import React, { useState } from 'react'
import { Heading, Subheading } from '../../components/heading'
import { Button } from '../../components/button'
import { useAuth } from '../../lib/auth/AuthContext'
import { useForm } from 'react-hook-form'
import { getTenant, updateTenant, TenantResponse, UpdateTenantRequest } from '../../services/tenants/tenantService'
import { Fieldset, Field, Label, ErrorMessage } from '../../components/fieldset'
import { Input } from '../../components/input'
import { Text } from '../../components/text'
import { Textarea } from '../../components/textarea'
import { Divider } from '../../components/divider'
import { useNavigate, useParams, useLoaderData, LoaderFunctionArgs } from 'react-router-dom'

// Loader for React Router
export async function loader({ params }: LoaderFunctionArgs) {
    const tenantUrlStub = params.tenantUrlStub as string;
    if (!tenantUrlStub) throw new Response('Not Found', { status: 404 });
    try {
        return await getTenant(tenantUrlStub);
    } catch (err: any) {
        throw new Response(err?.title || 'Failed to load tenant', { status: 404 });
    }
}

export default function TenantUpdatePage() {
    const { roles } = useAuth()
    const navigate = useNavigate()
    const { tenantUrlStub } = useParams<{ tenantUrlStub: string }>()
    const initialData = useLoaderData() as TenantResponse
    const [serverError, setServerError] = useState<string | null>(null)

    const {
        register,
        handleSubmit,
        setValue,
        formState: { errors, isSubmitting, isSubmitted }
    } = useForm<UpdateTenantRequest>({
        defaultValues: {
            urlStub: initialData.urlStub,
            title: initialData.title,
            description: initialData.description || ''
        }
    })

    // Keep form in sync if loader data changes (e.g. on fast navigation)
    // This is safe because loader always provides the latest data
    React.useEffect(() => {
        setValue('urlStub', initialData.urlStub)
        setValue('title', initialData.title)
        setValue('description', initialData.description || '')
    }, [initialData, setValue])

    const onSubmit = async (data: UpdateTenantRequest) => {
        setServerError(null)
        try {
            const updated = await updateTenant(tenantUrlStub!, data)
            navigate(`/${updated.urlStub}`)
        } catch (err: any) {
            setServerError(err?.message || 'An error occurred')
        }
    }

    if (!initialData) {
        return <div className="mx-auto max-w-4xl">Loading...</div>
    }

    return (
        <form onSubmit={handleSubmit(onSubmit)} className="mx-auto max-w-4xl">
            <div className="flex w-full flex-wrap items-end justify-between gap-4">
                <Heading>Update Tenant Profile</Heading>
                <div className="flex gap-4">
                    {roles.includes('Administrator') ? (
                        <Button href={`/${tenantUrlStub}`}>Back To Tenant</Button>
                    ) : null}
                </div>
            </div>
            <Divider className="my-10 mt-6" soft />
            <Fieldset className="mt-5">
                <Subheading>Tenant Details</Subheading>
                <Divider className="my-10 mt-6" />
                <Field className="mb-5">
                    <section className="grid gap-x-8 gap-y-6 sm:grid-cols-2">
                        <div className="space-y-1">
                            <Label htmlFor="urlStub">URL Stub</Label>
                            <Text>This will be how the tenant is identified in page URLs.</Text>
                        </div>
                        <div className="pt-5">
                            <Input
                                id="urlStub"
                                type="text"
                                {...register('urlStub', {
                                    required: 'URL Stub is required',
                                    maxLength: { value: 20, message: 'Max 20 characters' }
                                })}
                                data-invalid={errors.urlStub ? true : undefined}
                            />
                            {isSubmitted && errors.urlStub && <ErrorMessage>{errors.urlStub.message}</ErrorMessage>}
                        </div>
                    </section>
                </Field>
                <Field className="mb-5">
                    <section className="grid gap-x-8 gap-y-6 sm:grid-cols-2">
                        <div className="space-y-1">
                            <Label htmlFor="title">Title</Label>
                            <Text>This will be displayed to users.</Text>
                        </div>
                        <div className="pt-5">
                            <Input
                                id="title"
                                type="text"
                                {...register('title', {
                                    required: 'Title is required',
                                    maxLength: { value: 200, message: 'Max 200 characters' }
                                })}
                                data-invalid={errors.title ? true : undefined}
                            />
                            {isSubmitted && errors.title && <ErrorMessage>{errors.title.message}</ErrorMessage>}
                        </div>
                    </section>
                </Field>
                <Field className="mb-5">
                    <section className="grid gap-x-8 gap-y-6 sm:grid-cols-2">
                        <div className="space-y-1">
                            <Label htmlFor="description">Description</Label>
                            <Text>Optional description of the tenant.</Text>
                        </div>
                        <div className="pt-5">
                            <Textarea
                                id="description"
                                {...register('description', {
                                    maxLength: { value: 1000, message: 'Max 1000 characters' }
                                })}
                                data-invalid={errors.description ? true : undefined}
                            />
                            {isSubmitted && errors.description && <ErrorMessage>{errors.description.message}</ErrorMessage>}
                        </div>
                    </section>
                </Field>
            </Fieldset>

            <Divider className="my-10" soft />

            {serverError &&
                <Fieldset>
                    <Field className="mb-5">
                        <ErrorMessage>{serverError}</ErrorMessage>
                    </Field>
                    <Divider className="my-10" soft />
                </Fieldset>
            }
            <div className="flex justify-end gap-4">
                <Button type="submit" disabled={isSubmitting}>
                    Update Tenant
                </Button>
            </div>
        </form>
    )
}