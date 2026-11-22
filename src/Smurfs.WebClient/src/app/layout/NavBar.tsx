import { observer } from "mobx-react-lite";
import React from "react";
import { NavLink } from "react-router-dom";
import { Button, Container, Menu, Dropdown } from "semantic-ui-react";
import { useStore } from "../stores/store";

export default observer(function NavBar() {
    const {
        userStore: { user, logout },
    } = useStore();
    return (
        <Menu inverted fixed="top">
            <Container>
                <Menu.Item as={NavLink} to="/" exact header>
                    <img
                        src="/assets/logo.png"
                        alt="logo"
                        style={{ marginRight: "10px" }}
                    />
                    Smurfs
                </Menu.Item>
                <Menu.Item as={NavLink} to="/smurfs" name="Smurf Overview" />
                <Menu.Item>
                    <Button
                        as={NavLink}
                        to="/smurfs/new"
                        positive
                        content="Create Smurf"
                    />
                </Menu.Item>
                <Menu.Item as={NavLink} to="/errors" name="Errors" />
                <Menu.Item position="right">
                    <Dropdown pointing="top left" text={user?.displayName}>
                        <Dropdown.Menu>
                            <Dropdown.Item
                                onClick={logout}
                                text="Logout"
                                icon="power"
                            />
                        </Dropdown.Menu>
                    </Dropdown>
                </Menu.Item>
            </Container>
        </Menu>
    );
});
