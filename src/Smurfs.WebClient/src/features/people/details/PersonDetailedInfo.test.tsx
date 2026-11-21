import React from "react";
import { render, screen, within } from "@testing-library/react";
import { Person } from "../../../app/models/person";
import PersonDetailedInfo from "./PersonDetailedInfo";

test("displays the Smurf fields, false infection, and birthday without a timezone shift", () => {
    render(<PersonDetailedInfo person={{
        firstName: "Kylling", health: 42, birthday: new Date("2000-01-02T00:00:00Z"),
        infected: false, gender: "Female"
    } as Person} />);
    expect(screen.getByText("Kylling")).toBeInTheDocument();
    expect(screen.getByText("42")).toBeInTheDocument();
    expect(screen.getByText("2000-01-02")).toBeInTheDocument();
    expect(screen.getByText("No")).toBeInTheDocument();
    expect(screen.getByText("Female")).toBeInTheDocument();
    expect(screen.queryByText("Address")).not.toBeInTheDocument();
    expect(screen.queryByText("Dead")).not.toBeInTheDocument();
});

test("displays unknown optional values and true infection", () => {
    const { rerender } = render(<PersonDetailedInfo person={{
        firstName: "Papa", health: 0, birthday: null, infected: null, gender: null
    } as Person} />);
    expect(screen.getAllByText("Unknown")).toHaveLength(3);
    const row = screen.getByText("Health").closest("tr")!;
    expect(within(row).getByText("0")).toBeInTheDocument();
    rerender(<PersonDetailedInfo person={{
        firstName: "Papa", health: 0, birthday: null, infected: true, gender: "Male"
    } as Person} />);
    expect(screen.getByText("Yes")).toBeInTheDocument();
    expect(screen.getByText("Male")).toBeInTheDocument();
});
