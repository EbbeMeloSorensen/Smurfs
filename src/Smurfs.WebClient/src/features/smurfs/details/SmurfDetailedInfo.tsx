import React from "react";
import { Table } from "semantic-ui-react";
import { Smurf } from "../../../app/models/smurf";

interface Props {
    smurf: Smurf;
}

export default function SmurfDetailedInfo({ smurf }: Props) {
    return (
        <Table definition>
            <Table.Body>
                <Table.Row><Table.Cell>Name</Table.Cell><Table.Cell>{smurf.name}</Table.Cell></Table.Row>
                <Table.Row><Table.Cell>Health</Table.Cell><Table.Cell>{smurf.health}</Table.Cell></Table.Row>
                <Table.Row><Table.Cell>Birthday</Table.Cell><Table.Cell>
                    {smurf.birthday ? smurf.birthday.slice(0, 10) : "Unknown"}
                </Table.Cell></Table.Row>
                <Table.Row><Table.Cell>Infected</Table.Cell><Table.Cell>
                    {smurf.infected === null ? "Unknown" : smurf.infected ? "Yes" : "No"}
                </Table.Cell></Table.Row>
                <Table.Row><Table.Cell>Gender</Table.Cell><Table.Cell>{smurf.gender ?? "Unknown"}</Table.Cell></Table.Row>
            </Table.Body>
        </Table>
    );
}
