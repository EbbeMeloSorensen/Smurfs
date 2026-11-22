import React from "react";
import { render, screen, waitFor } from "@testing-library/react";
import { Router } from "react-router-dom";
import { createMemoryHistory } from "history";
import App from "./App";

jest.mock("../stores/store", () => ({
    useStore: () => ({
        commonStore: { token: null, appLoaded: true, setAppLoaded: jest.fn() },
        userStore: { isLoggedIn: true }
    })
}));
jest.mock("./NavBar", () => () => null);
jest.mock("../common/modals/ModalContainer", () => () => null);
jest.mock("react-toastify", () => ({ ToastContainer: () => null }));
jest.mock("../../features/home/HomePage", () => () => "Home");
jest.mock("../../features/errors/TestError", () => () => "Errors");
jest.mock("../../features/errors/ServerError", () => () => "Server error");
jest.mock("../../features/smurfs/dashboard/SmurfDashboard", () => () => "Smurf overview");
jest.mock("../../features/smurfs/details/SmurfDetails", () => () => "Smurf details");
jest.mock("../../features/smurfs/form/SmurfForm", () => () => "Smurf form");

test.each([
    ["/smurfs", "/smurfs", "Smurf overview"],
    ["/smurfs/new", "/smurfs/new", "Smurf form"],
    ["/smurfs/123", "/smurfs/123", "Smurf details"],
    ["/smurfs/123/edit", "/smurfs/123/edit", "Smurf form"],
    ["/people", "/smurfs", "Smurf overview"],
    ["/people/123", "/smurfs/123", "Smurf details"],
    ["/manage/123", "/smurfs/123/edit", "Smurf form"],
    ["/createPerson", "/smurfs/new", "Smurf form"],
    ["/createSmurf", "/smurfs/new", "Smurf form"]
])("routes %s to the correct Smurf screen", async (path, expected, screenName) => {
    const history = createMemoryHistory({ initialEntries: [path] });
    render(<Router history={history}><App /></Router>);
    await waitFor(() => expect(history.location.pathname).toBe(expected));
    expect(screen.getByText(screenName)).toBeInTheDocument();
});
