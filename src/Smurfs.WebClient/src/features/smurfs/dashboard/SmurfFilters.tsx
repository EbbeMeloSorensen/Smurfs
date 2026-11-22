import { observer } from "mobx-react-lite";
import React, { useState } from "react";
import {
    Button,
    Checkbox,
    Form,
    Header,
    Radio,
} from "semantic-ui-react";
import { useStore } from "../../../app/stores/store";

export default observer(function SmurfsFilters() {
    const {
        smurfStore: { setPredicate, sorting, setSorting },
    } = useStore();

    const [nameFilter, setNameFilter] = useState("");
    const [infected, setInfected] = useState(false);
    const [notInfected, setNotInfected] = useState(false);
    const [infectionUnspecified, setInfectionUnspecified] = useState(false);
    const [sortingLocal, setSortingLocal] = useState(sorting);

    function handleClick() {
        setSorting(sortingLocal);
        setPredicate(
            nameFilter,
            infected,
            notInfected,
            infectionUnspecified,
        );
    }

    return (
        <>
            <Header icon="filter" attached color="teal" content="Filters" />

            <Header>Name</Header>
            <label htmlFor="nameFilter">Name contains</label>
            <input id="nameFilter"
                value={nameFilter}
                onChange={(e) => setNameFilter(e.target.value)}
            />

            <Header>Infected</Header>
            <Form>
                <Form.Field>
                    <Checkbox
                        label="Yes"
                        checked={infected}
                        onChange={() => setInfected(!infected)}
                    />
                </Form.Field>
                <Form.Field>
                    <Checkbox
                        label="No"
                        checked={notInfected}
                        onChange={() => setNotInfected(!notInfected)}
                    />
                </Form.Field>
                <Form.Field>
                    <Checkbox
                        label="Unspecified"
                        checked={infectionUnspecified}
                        onChange={() => setInfectionUnspecified(!infectionUnspecified)}
                    />
                </Form.Field>
            </Form>

            <Header>Sorting</Header>
            <Form>
                <Form.Field>
                    <Radio
                        label="Name"
                        name="radioGroup"
                        value="this"
                        checked={sortingLocal === "name"}
                        onChange={() => setSortingLocal("name")}
                    />
                </Form.Field>
                <Form.Field>
                    <Radio
                        label="Newest first"
                        name="radioGroup"
                        value="that"
                        checked={sortingLocal === "created"}
                        onChange={() => setSortingLocal("created")}
                    />
                </Form.Field>
            </Form>

            <Button
                floated="right"
                content="Search"
                onClick={() => handleClick()}
            />
        </>
    );
});
