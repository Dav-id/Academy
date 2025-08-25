import { useParams, useNavigate } from 'react-router-dom';
import { useForm } from 'react-hook-form';
import { createModule, CreateCourseModuleRequest } from '../../../services/courses/courseModuleService';
import { Heading } from '../../../components/heading';
import { Button } from '../../../components/button';
import { Divider } from '../../../components/divider';
import { Fieldset, Field, Label, ErrorMessage } from '../../../components/fieldset';
import { Input } from '../../../components/input';
import { Text } from '../../../components/text';
import { Textarea } from '../../../components/textarea';
import { useState } from 'react';

export default function CourseModuleCreatePage() {
    const { tenantUrlStub, courseId } = useParams<{ tenantUrlStub: string; courseId: string }>();    
    const navigate = useNavigate();
    const [serverError, setServerError] = useState<string | null>(null);

    const {
        register,
        handleSubmit,
        formState: { errors, isSubmitting, isSubmitted }
    } = useForm<CreateCourseModuleRequest>();

    const onSubmit = async (data: CreateCourseModuleRequest) => {
        setServerError(null);
        try {
            const module = await createModule(tenantUrlStub!, Number(courseId), data);
            navigate(`/${tenantUrlStub}/courses/${courseId}/modules`);
        } catch (err: any) {
            setServerError(err?.message || err?.title || 'An error occurred');
        }
    };

    return (
        <form onSubmit={handleSubmit(onSubmit)} className="mx-auto max-w-4xl">
            <div className="flex w-full flex-wrap items-end justify-between gap-4">
                <Heading>New Course Module</Heading>
                <div className="flex gap-4">
                    <Button href={`/${tenantUrlStub}/courses/${courseId}/modules`}>Back To Modules</Button>
                </div>
            </div>
            <Divider className="my-10 mt-6" soft />
            <Fieldset className="mt-5">
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
                            <Text>Optional description of the module.</Text>
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
                <Field className="mb-5">
                    <section className="grid gap-x-8 gap-y-6 sm:grid-cols-2">
                        <div className="space-y-1">
                            <Label htmlFor="order">Order</Label>
                            <Text>Order in which this module appears in the course.</Text>
                        </div>
                        <div className="pt-5">
                            <Input
                                id="order"
                                type="number"
                                min={1}
                                {...register('order', {
                                    required: 'Order is required',
                                    min: { value: 1, message: 'Order must be at least 1' }
                                })}
                                data-invalid={errors.order ? true : undefined}
                            />
                            {isSubmitted && errors.order && <ErrorMessage>{errors.order.message}</ErrorMessage>}
                        </div>
                    </section>
                </Field>
            </Fieldset>

            {serverError &&
                <Fieldset>
                    <Field className="mb-5">
                        <ErrorMessage>{serverError}</ErrorMessage>
                    </Field>
                    <Divider className="my-10" soft />
                </Fieldset>
            }
                <Input id="courseId" type="hidden" value={courseId} {...register('courseId')} />
            <div className="flex justify-end gap-4">
                <Button type="submit" disabled={isSubmitting}>
                    Create Module
                </Button>
            </div>
        </form>
    );
}