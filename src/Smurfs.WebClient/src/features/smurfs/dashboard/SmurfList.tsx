import { observer } from 'mobx-react-lite';
import React from 'react';
import { List } from 'semantic-ui-react';
import { useStore } from '../../../app/stores/store';
import SmurfListItem from './SmurfListItem';

export default observer(function SmurfList() {
    const {smurfStore} = useStore();
    const {smurfs} = smurfStore;

    return (
        <List divided>
            {smurfs.map(smurf => (
                <SmurfListItem key={smurf.id} smurf={smurf} />
            ))}
        </List>
    )
})