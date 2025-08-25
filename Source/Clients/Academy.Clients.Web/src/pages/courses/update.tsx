import { useParams, useNavigate, useLoaderData, LoaderFunctionArgs } from 'react-router-dom';
import { useForm } from 'react-hook-form';
import { getCourse, updateCourse, UpdateCourseRequest, CourseResponse } from '../../services/courses/courseService';
import { Heading } from '../../components/heading';
import { Button } from '../../components/button';
import { Divider } from '../../components/divider';
import { Fieldset, Field, Label, ErrorMessage } from '../../components/fieldset';
import { Input } from '../../components/input';
import { useState, useEffect } from 'react';
import { Text } from '../../components/text';
import { Textarea } from '../../components/textarea';

// Loader for React Router
export async function loader({ params }: LoaderFunctionArgs) {
    const tenantUrlStub = params.tenantUrlStub as string;
    const courseId = params.courseId as string;
    if (!tenantUrlStub || !courseId) throw new Response('Not Found', { status: 404 });
    try {
        return await getCourse(tenantUrlStub, Number(courseId));
    } catch (err: any) {
        throw new Response(err?.title || 'Failed to load course', { status: 404 });
    }
}

export default function CourseUpdatePage() {
    const { tenantUrlStub, courseId } = useParams<{ tenantUrlStub: string; courseId: string }>();
    const navigate = useNavigate();
    const initialCourse = useLoaderData() as CourseResponse;
    const [serverError, setServerError] = useState<string | null>(null);

    const {
        register,
        handleSubmit,
        setValue,
        formState: { errors, isSubmitting, isSubmitted }
    } = useForm<UpdateCourseRequest>({
        defaultValues: {
            id: initialCourse.id,
            title: initialCourse.title,
            description: initialCourse.description,
        }
    });

    // Keep form in sync if loader data changes (e.g. on fast navigation)
    useEffect(() => {
        setValue('id', initialCourse.id);
        setValue('title', initialCourse.title);
        setValue('description', initialCourse.description);
    }, [initialCourse, setValue]);

    const onSubmit = async (data: UpdateCourseRequest) => {
        setServerError(null);
        try {
            await updateCourse(tenantUrlStub!, Number(courseId), data);
            navigate(`/${tenantUrlStub}/courses/${courseId}`);
        } catch (err: any) {
            setServerError(err?.message || err?.title || 'An error occurred');
        }
    };

    return (
        <form onSubmit={handleSubmit(onSubmit)} className="mx-auto max-w-4xl">
            <div className="flex w-full flex-wrap items-end justify-between gap-4">
                <Heading>Update Course</Heading>
                <div className="flex gap-4">
                    <Button href={`/${tenantUrlStub}/courses/${courseId}`}>Back To Course</Button>
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
                    Update Course
                </Button>
            </div>
        </form>
    );
}