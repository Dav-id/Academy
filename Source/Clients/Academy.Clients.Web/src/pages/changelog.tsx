import { Heading, Subheading } from '../components/heading';
import { Divider } from '../components/divider';
import { Fieldset, Field, Label } from '../components/fieldset';
import { Text } from '../components/text';

export default function ChangelogPage() {
    return (
        <div className="mx-auto max-w-3xl">
            <Heading>Changelog</Heading>
            <Divider className="my-6" />
            <Fieldset>
                <Subheading>v1.0.0 - Initial Release</Subheading>
                <Field>
                    <Label>Features</Label>
                    <Text>
                        <ul className="ml-6 list-disc">
                            <li>Multi-tenant support</li>
                            <li>Course and module management</li>
                            <li>Lesson management</li>
                            <li>User authentication and roles</li>
                        </ul>
                    </Text>
                </Field>
            </Fieldset>
        </div>
    );
}