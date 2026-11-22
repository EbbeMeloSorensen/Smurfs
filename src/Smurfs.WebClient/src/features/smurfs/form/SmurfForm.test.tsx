import React from "react";
import { fireEvent, render, screen, waitFor } from "@testing-library/react";
import { Route, Router } from "react-router-dom";
import { createMemoryHistory } from "history";
import agent from "../../../app/api/agent";
import SmurfForm from "./SmurfForm";

const mockResetSmurfs = jest.fn();
jest.mock("../../../app/stores/store", () => ({
    useStore: () => ({ smurfStore: { resetSmurfs: mockResetSmurfs } })
}));
jest.mock("../../../app/api/agent", () => ({
    __esModule: true, default: { Smurfs: { create: jest.fn(), details: jest.fn(), update: jest.fn() } }
}));

beforeEach(() => jest.clearAllMocks());

test("creates a Smurf with typed values and returns to the list", async () => {
    const history = createMemoryHistory({ initialEntries: ["/smurfs/new"] });
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
    await waitFor(() => expect(history.location.pathname).toBe("/smurfs"));
    expect(mockResetSmurfs).toHaveBeenCalledTimes(1);
});

test("sends null for unknown fields and permits retry after failure", async () => {
    const history = createMemoryHistory({ initialEntries: ["/smurfs/new"] });
    (agent.Smurfs.create as jest.Mock).mockRejectedValueOnce(new Error("offline")).mockResolvedValueOnce(undefined);
    render(<Router history={history}><SmurfForm /></Router>);
    fireEvent.change(screen.getByLabelText("Name"), { target: { value: "Papa" } });
    await waitFor(() => expect(screen.getByRole("button", { name: "Create Smurf" })).toBeEnabled());
    fireEvent.click(screen.getByRole("button", { name: "Create Smurf" }));
    await screen.findByText("Could not create the Smurf. Please try again.");
    expect(history.location.pathname).toBe("/smurfs/new");
    expect(screen.getByLabelText("Name")).toHaveValue("Papa");
    expect(agent.Smurfs.create).toHaveBeenCalledWith({ name: "Papa", health: 0, birthday: null, infected: null, gender: null });
    fireEvent.click(screen.getByRole("button", { name: "Create Smurf" }));
    await waitFor(() => expect(history.location.pathname).toBe("/smurfs"));
});


test("loads a Smurf for editing and saves changed and cleared values through PUT", async () => {
    const history = createMemoryHistory({ initialEntries: ["/smurfs/smurf-id/edit"] });
    (agent.Smurfs.details as jest.Mock).mockResolvedValue({
        id: "smurf-id", name: "Original", health: 42, birthday: "2000-01-02T00:00:00Z",
        infected: false, gender: "Female", petId: null
    });
    (agent.Smurfs.update as jest.Mock).mockResolvedValue(undefined);
    render(<Router history={history}><Route path="/smurfs/:id/edit"><SmurfForm /></Route></Router>);
    await screen.findByDisplayValue("Original");
    expect(screen.getByLabelText("Birthday (optional)")).toHaveValue("2000-01-02");
    expect(screen.getByLabelText("Infected")).toHaveValue("false");
    expect(screen.getByLabelText("Gender")).toHaveValue("Female");
    fireEvent.change(screen.getByLabelText("Name"), { target: { value: "Edited" } });
    fireEvent.change(screen.getByLabelText("Health"), { target: { value: "12" } });
    fireEvent.change(screen.getByLabelText("Birthday (optional)"), { target: { value: "" } });
    fireEvent.change(screen.getByLabelText("Infected"), { target: { value: "true" } });
    fireEvent.change(screen.getByLabelText("Gender"), { target: { value: "" } });
    await waitFor(() => expect(screen.getByRole("button", { name: "Save changes" })).toBeEnabled());
    fireEvent.click(screen.getByRole("button", { name: "Save changes" }));
    await waitFor(() => expect(agent.Smurfs.update).toHaveBeenCalledWith("smurf-id", {
        name: "Edited", health: 12, birthday: null, infected: true, gender: null
    }));
    await waitFor(() => expect(history.location.pathname).toBe("/smurfs/smurf-id"));
    expect(agent.Smurfs.create).not.toHaveBeenCalled();
});

test("keeps edits visible after a failed save and permits retry", async () => {
    const history = createMemoryHistory({ initialEntries: ["/smurfs/smurf-id/edit"] });
    (agent.Smurfs.details as jest.Mock).mockResolvedValue({
        id: "smurf-id", name: "Original", health: 0, birthday: null, infected: null, gender: null
    });
    (agent.Smurfs.update as jest.Mock).mockRejectedValueOnce(new Error("offline")).mockResolvedValueOnce(undefined);
    render(<Router history={history}><Route path="/smurfs/:id/edit"><SmurfForm /></Route></Router>);
    await screen.findByDisplayValue("Original");
    fireEvent.change(screen.getByLabelText("Name"), { target: { value: "Edited" } });
    fireEvent.click(screen.getByRole("button", { name: "Save changes" }));
    await screen.findByText("Could not save the Smurf. Please try again.");
    expect(screen.getByLabelText("Name")).toHaveValue("Edited");
    expect(history.location.pathname).toBe("/smurfs/smurf-id/edit");
    fireEvent.click(screen.getByRole("button", { name: "Save changes" }));
    await waitFor(() => expect(history.location.pathname).toBe("/smurfs/smurf-id"));
});
