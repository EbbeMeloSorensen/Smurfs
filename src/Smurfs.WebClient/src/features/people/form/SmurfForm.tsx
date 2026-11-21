import axios from "axios";
import { Form, Formik } from "formik";
import React, { useRef } from "react";
import { Link, useHistory } from "react-router-dom";
import { Button, Header, Message, Segment } from "semantic-ui-react";
import * as Yup from "yup";
import agent from "../../../app/api/agent";
import MyTextInput from "../../../app/common/form/MyTextInput";
import { useStore } from "../../../app/stores/store";

const validationSchema = Yup.object({
    name: Yup.string()
        .test("not-blank", "Name is required", value => !!value && value.trim().length > 0)
        .max(20, "Name must contain at most 20 characters"),
    health: Yup.number().typeError("Health must be a whole number").required("Health is required")
        .integer("Health must be a whole number")
        .min(-2147483648, "Health is outside the supported range")
        .max(2147483647, "Health is outside the supported range"),
    birthday: Yup.string().test("birthday", "Enter a valid birthday", value => {
        if (!value) return true;
        const date = new Date(value + "T00:00:00.000Z");
        return !Number.isNaN(date.getTime()) && date.toISOString().slice(0, 10) === value;
    })
});

export default function SmurfForm() {
    const history = useHistory();
    const { personStore } = useStore();
    const saving = useRef(false);

    return (
        <Segment clearing>
            <Header as="h2" content="Create Smurf" />
            <Formik
                initialValues={{ name: "", health: 0, birthday: "", infected: "", gender: "" }}
                validationSchema={validationSchema}
                validateOnMount
                onSubmit={async (values, helpers) => {
                    if (saving.current) return;
                    saving.current = true;
                    helpers.setStatus(undefined);
                    try {
                        await agent.Smurfs.create({
                            name: values.name,
                            health: Number(values.health),
                            birthday: values.birthday ? values.birthday + "T00:00:00.000Z" : null,
                            infected: values.infected === "" ? null : values.infected === "true",
                            gender: values.gender === "Male" ? "Male" : values.gender === "Female" ? "Female" : null
                        });
                        personStore.resetPeople();
                        history.push("/people");
                    } catch (error) {
                        const message = Array.isArray(error) ? error.join(" ")
                            : axios.isAxiosError(error) && typeof error.response?.data === "string"
                                ? error.response.data : "Could not create the Smurf. Please try again.";
                        helpers.setStatus(message);
                    } finally {
                        saving.current = false;
                    }
                }}
            >
                {({ isSubmitting, isValid, values, setFieldValue, status }) => (
                    <Form className="ui form" autoComplete="off">
                        <fieldset disabled={isSubmitting} style={{ border: 0, padding: 0, margin: 0 }}>
                            <MyTextInput name="name" label="Name" placeholder="Name" />
                            <MyTextInput name="health" label="Health" placeholder="Health" type="number" />
                            <MyTextInput name="birthday" label="Birthday (optional)" placeholder="Birthday" type="date" />
                            <div className="field">
                                <label htmlFor="infected">Infected</label>
                                <select id="infected" name="infected" value={values.infected}
                                    onChange={event => setFieldValue("infected", event.target.value)}>
                                    <option value="">Unknown</option>
                                    <option value="true">Yes</option>
                                    <option value="false">No</option>
                                </select>
                            </div>
                            <div className="field">
                                <label htmlFor="gender">Gender</label>
                                <select id="gender" name="gender" value={values.gender}
                                    onChange={event => setFieldValue("gender", event.target.value)}>
                                    <option value="">Unknown</option>
                                    <option value="Male">Male</option>
                                    <option value="Female">Female</option>
                                </select>
                            </div>
                            {status && <Message negative content={status} />}
                            <Button positive type="submit" loading={isSubmitting}
                                disabled={isSubmitting || !isValid} content="Create Smurf" />
                            <Button as={Link} to="/people" type="button" disabled={isSubmitting} content="Cancel" />
                        </fieldset>
                    </Form>
                )}
            </Formik>
        </Segment>
    );
}
