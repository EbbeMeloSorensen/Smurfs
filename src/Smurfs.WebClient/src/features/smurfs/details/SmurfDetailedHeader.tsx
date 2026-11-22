import { observer } from "mobx-react-lite";
import React, { SyntheticEvent } from "react";
import { Link, useHistory } from "react-router-dom";
import { Button, Header, Segment } from "semantic-ui-react";
import { Smurf } from "../../../app/models/smurf";
import { useStore } from "../../../app/stores/store";

interface Props {
  smurf: Smurf;
}

export default observer(function SmurfDetailedHeader({ smurf }: Props) {
  const history = useHistory();
  const { smurfStore } = useStore();
  const { deleteSmurf, loading } = smurfStore;

  function handleSmurfDelete(
    e: SyntheticEvent<HTMLButtonElement>,
    id: string
  ) {
    if (window.confirm("Do you want to delete this Smurf?") === true) {
      deleteSmurf(id).then(() => history.push(`/smurfs`));
    }
  }

  return (
    <Segment.Group>
      <Segment clearing attached="bottom">
        <Header>
          {smurf.name}
        </Header>
        <Button.Group floated="right">
          <Button
            loading={loading}
            onClick={(e) => handleSmurfDelete(e, smurf.id)}
            color="red"
            type="button"
          >
            Delete
          </Button>
          <Button
            as={Link}
            to={`/smurfs/${smurf.id}/edit`}
            color="orange"
            floated="right"
            type="button"
          >
            Edit
          </Button>
        </Button.Group>
      </Segment>
    </Segment.Group>
  );
});
