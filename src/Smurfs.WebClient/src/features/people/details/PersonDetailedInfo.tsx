import React from "react";
import { Table } from "semantic-ui-react";
import { Person } from "../../../app/models/person";

interface Props {
    person: Person;
}

export default function PersonDetailedInfo({ person }: Props) {
    return (
        <Table definition>
            <Table.Body>
                <Table.Row><Table.Cell>Name</Table.Cell><Table.Cell>{person.firstName}</Table.Cell></Table.Row>
                <Table.Row><Table.Cell>Health</Table.Cell><Table.Cell>{person.health}</Table.Cell></Table.Row>
                <Table.Row><Table.Cell>Birthday</Table.Cell><Table.Cell>
                    {person.birthday ? person.birthday.toISOString().slice(0, 10) : "Unknown"}
                </Table.Cell></Table.Row>
                <Table.Row><Table.Cell>Infected</Table.Cell><Table.Cell>
                    {person.infected === null ? "Unknown" : person.infected ? "Yes" : "No"}
                </Table.Cell></Table.Row>
                <Table.Row><Table.Cell>Gender</Table.Cell><Table.Cell>{person.gender ?? "Unknown"}</Table.Cell></Table.Row>
            </Table.Body>
        </Table>
    );
}
