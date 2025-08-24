import { Heading, Subheading } from '../../components/heading';
import { Button } from '../../components/button';
import { useAuth } from '../../lib/auth/AuthContext';
import { useForm } from 'react-hook-form';
import { createCourse, CreateCourseRequest } from '../../services/courses/courseService';
import { Fieldset, Field, Label, ErrorMessage } from '../../components/fieldset';
import { Input } from '../../components/input';
import { useState } from 'react';
import { Text } from '../../components/text';
import { Textarea } from '../../components/textarea';
import { Divider } from '../../components/divider';
import { useNavigate, useParams } from 'react-router-dom';

export async function loader() {}

export default function CourseCreatePage() {
    const { roles } = useAuth();
    const [serverError, setServerError] = useState<string | null>(null);
    const navigate = useNavigate();
    const { tenantUrlStub } = useParams<{ tenantUrlStub: string }>();

    const {
        register,
        handleSubmit,
        formState: { errors, isSubmitting, isSubmitted }
    } = useForm<CreateCourseRequest>();

    const onSubmit = async (data: CreateCourseRequest) => {
        setServerError(null);
        try {
            if (!tenantUrlStub) throw new Error('Missing tenant');
            const course = await createCourse(tenantUrlStub, data);
            navigate(`/${tenantUrlStub}/courses/${course.id}`);
        } catch (err: any) {
            setServerError(err?.message || 'An error occurred');
        }
    };

    return (
        <form onSubmit={handleSubmit(onSubmit)} className="mx-auto max-w-4xl">
            <div className="flex w-full flex-wrap items-end justify-between gap-4">
                <Heading>New Course</Heading>
                <div className="flex gap-4">
                    <Button href={`/${tenantUrlStub}/courses`}>Back To Courses</Button>
                </div>
            </div>
            <Divider className="my-10 mt-6" soft />
            <Fieldset className="mt-5">
                <Subheading>Course Details</Subheading>
                <Divider className="my-10 mt-6" />
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
                            <Text>Optional description of the course.</Text>
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
                    Create Course
                </Button>
            </div>
        </form>
    );
}
