import { Heading, Subheading } from '../../components/heading'
import { Button } from '../../components/button';

import { useAuth } from '../../lib/auth/AuthContext';
import { useForm } from 'react-hook-form';
import { createTenant, CreateTenantRequest } from '../../services/tenants/tenantService';
import { Fieldset, Field, Label, ErrorMessage } from '../../components/fieldset';
import { Input } from '../../components/input';
import { useState } from 'react';
import { Text } from '../../components/text';
import { Textarea } from '../../components/textarea';
import { Divider } from '../../components/divider';
import { useNavigate } from 'react-router-dom';

export async function loader() {
}

export default function TenantCreatePage() {
    const { roles } = useAuth();
    const [serverError, setServerError] = useState<string | null>(null);
    const navigate = useNavigate();

    const {
        register,
        handleSubmit,
        formState: { errors, isSubmitting, isSubmitted }
    } = useForm<CreateTenantRequest>();

    const onSubmit = async (data: CreateTenantRequest) => {
        setServerError(null);
        try {
            const tenant = await createTenant(data);

            navigate(`/${tenant.urlStub}`);
        } catch (err: any) {
            setServerError(err?.message || 'An error occurred');
        }
    };

    return (
        <>

            <form onSubmit={handleSubmit(onSubmit)} className="mx-auto max-w-4xl">
                <div className="flex w-full flex-wrap items-end justify-between gap-4">
                    <Heading>New Tenant Profile</Heading>
                    <div className="flex gap-4">
                        {roles.includes('Administrator') ? (
                            <Button href="/">Back To Tenants</Button>
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
                                <Text>This will be displayed on to users.</Text>
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

                <Fieldset className="mt-10">
                    <Subheading>Account Owner</Subheading>
                    <Divider className="my-10 mt-6" />
                    <Field className="mb-5">
                        <section className="grid gap-x-8 gap-y-6 sm:grid-cols-2">
                            <div className="space-y-1">
                                <Label htmlFor="tenantAccountOwnerFirstName">First Name</Label>
                                <Text>Account owner's first name.</Text>
                            </div>
                            <div className="pt-5">
                                <Input
                                    id="tenantAccountOwnerFirstName"
                                    type="text"
                                    {...register('tenantAccountOwnerFirstName', {
                                        required: 'First name is required',
                                        maxLength: { value: 200, message: 'Max 200 characters' }
                                    })}
                                    data-invalid={errors.tenantAccountOwnerFirstName ? true : undefined}
                                />
                                {isSubmitted && errors.tenantAccountOwnerFirstName && (
                                    <ErrorMessage>{errors.tenantAccountOwnerFirstName.message}</ErrorMessage>
                                )}
                            </div>
                        </section>
                    </Field>
                    <Field className="mb-5">
                        <section className="grid gap-x-8 gap-y-6 sm:grid-cols-2">
                            <div className="space-y-1">
                                <Label htmlFor="tenantAccountOwnerLastName">Last Name</Label>
                                <Text>Account owner's last name.</Text>
                            </div>
                            <div className="pt-5">
                                <Input
                                    id="tenantAccountOwnerLastName"
                                    type="text"
                                    {...register('tenantAccountOwnerLastName', {
                                        required: 'Last name is required',
                                        maxLength: { value: 200, message: 'Max 200 characters' }
                                    })}
                                    data-invalid={errors.tenantAccountOwnerLastName ? true : undefined}
                                />
                                {isSubmitted && errors.tenantAccountOwnerLastName && (
                                    <ErrorMessage>{errors.tenantAccountOwnerLastName.message}</ErrorMessage>
                                )}
                            </div>
                        </section>
                    </Field>
                    <Field className="mb-5">
                        <section className="grid gap-x-8 gap-y-6 sm:grid-cols-2">
                            <div className="space-y-1">
                                <Label htmlFor="tenantAccountOwnerEmail">Account Owner Email</Label>
                                <Text>Email address of the account owner. They will use this to login with.</Text>
                            </div>
                            <div className="pt-5">
                                <Input
                                    id="tenantAccountOwnerEmail"
                                    type="email"
                                    {...register('tenantAccountOwnerEmail', {
                                        required: 'Email is required',
                                        pattern: {
                                            value: /^[^\s@]+@[^\s@]+\.[^\s@]+$/,
                                            message: 'Invalid email address'
                                        }
                                    })}
                                    data-invalid={errors.tenantAccountOwnerEmail ? true : undefined}
                                />
                                {isSubmitted && errors.tenantAccountOwnerEmail && (
                                    <ErrorMessage>{errors.tenantAccountOwnerEmail.message}</ErrorMessage>
                                )}
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
                        Create Tenant
                    </Button>
                </div>

                <div className="mt-8 flex gap-4">
                </div>
            </form>
        </>
    );
}
