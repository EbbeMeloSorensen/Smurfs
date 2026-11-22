import React from "react";
import { Link } from "react-router-dom";
import { List } from "semantic-ui-react";
import { Smurf } from "../../../app/models/smurf";

interface Props {
    smurf: Smurf;
}

export default function SmurfListItem({ smurf }: Props) {
    return (
        <List.Item>
            <List.Content>
                <List.Header as={Link} to={"/smurfs/" + smurf.id}>
                    {smurf.name}
                </List.Header>
            </List.Content>
        </List.Item>
    );
}
