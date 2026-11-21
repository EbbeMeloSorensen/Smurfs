import React from "react";
import { fireEvent, render, screen, waitFor } from "@testing-library/react";
import { Router } from "react-router-dom";
import { createMemoryHistory } from "history";
import agent from "../../../app/api/agent";
import SmurfForm from "./SmurfForm";

const mockResetPeople = jest.fn();
jest.mock("../../../app/stores/store", () => ({
    useStore: () => ({ personStore: { resetPeople: mockResetPeople } })
}));
jest.mock("../../../app/api/agent", () => ({
    __esModule: true, default: { Smurfs: { create: jest.fn() } }
}));

beforeEach(() => jest.clearAllMocks());

test("creates a Smurf with typed values and returns to the list", async () => {
    const history = createMemoryHistory({ initialEntries: ["/createSmurf"] });
    (agent.Smurfs.create as jest.Mock).mockResolvedValue(undefined);
    render(<Router history={history}><SmurfForm /></Router>);
    fireEvent.change(screen.getByLabelText("Name"), { target: { value: "Kylling" } });
    fireEvent.change(screen.getByLabelText("Health"), { target: { value: "42" } });
    fireEvent.change(screen.getByLabelText("Birthday (optional)"), { target: { value: "2000-01-02" } });
    fireEvent.change(screen.getByLabelText("Infected"), { target: { value: "false" } });
    fireEvent.change(screen.getByLabelText("Gender"), { target: { value: "Female" } });
    await waitFor(() => expect(screen.getByRole("button", { name: "Create Smurf" })).toBeEnabled());
    fireEvent.click(screen.getByRole("button", { name: "Create Smurf" }));
    await waitFor(() => expect(agent.Smurfs.create).toHaveBeenCalledWith({
        name: "Kylling", health: 42, birthday: "2000-01-02T00:00:00.000Z", infected: false, gender: "Female"
    }));
    await waitFor(() => expect(history.location.pathname).toBe("/people"));
    expect(mockResetPeople).toHaveBeenCalledTimes(1);
});

test("sends null for unknown fields and permits retry after failure", async () => {
    const history = createMemoryHistory({ initialEntries: ["/createSmurf"] });
    (agent.Smurfs.create as jest.Mock).mockRejectedValueOnce(new Error("offline")).mockResolvedValueOnce(undefined);
    render(<Router history={history}><SmurfForm /></Router>);
    fireEvent.change(screen.getByLabelText("Name"), { target: { value: "Papa" } });
    await waitFor(() => expect(screen.getByRole("button", { name: "Create Smurf" })).toBeEnabled());
    fireEvent.click(screen.getByRole("button", { name: "Create Smurf" }));
    await screen.findByText("Could not create the Smurf. Please try again.");
    expect(history.location.pathname).toBe("/createSmurf");
    expect(screen.getByLabelText("Name")).toHaveValue("Papa");
    expect(agent.Smurfs.create).toHaveBeenCalledWith({ name: "Papa", health: 0, birthday: null, infected: null, gender: null });
    fireEvent.click(screen.getByRole("button", { name: "Create Smurf" }));
    await waitFor(() => expect(history.location.pathname).toBe("/people"));
});
