import PersonStore from "./personStore";
import agent from "../api/agent";
import { Person } from "../models/person";
import { PagingParams } from "../models/pagination";

jest.mock("../api/agent", () => ({
    __esModule: true, default: { People: { list: jest.fn() } }
}));

function result(name: string) {
    return { data: [{ id: name, firstName: name, birthday: null } as Person],
        pagination: { currentPage: 1, itemsPerPage: 25, totalItems: 1, totalPages: 1 } };
}

beforeEach(() => jest.clearAllMocks());

test("Search resets paging and changes values even when the filter keys stay the same", async () => {
    const store = new PersonStore();
    (agent.People.list as jest.Mock).mockResolvedValue(result("Papa"));
    store.setPagingParams(new PagingParams(3));
    await store.setPredicate(" Papa ", true, false, true);
    const first = (agent.People.list as jest.Mock).mock.calls[0][0] as URLSearchParams;
    expect(first.get("name")).toBe("Papa");
    expect(first.get("infected")).toBe("true|null");
    expect(first.get("pageNumber")).toBe("1");
    expect(first.has("dead")).toBe(false);
    await store.setPredicate("Smurf", false, true, false);
    const second = (agent.People.list as jest.Mock).mock.calls[1][0] as URLSearchParams;
    expect(second.get("name")).toBe("Smurf");
    expect(second.get("infected")).toBe("false");
    await store.setPredicate("", false, false, false);
    const cleared = (agent.People.list as jest.Mock).mock.calls[2][0] as URLSearchParams;
    expect(cleared.has("name")).toBe(false);
    expect(cleared.has("infected")).toBe(false);
});

test("a previous page response cannot reintroduce rows after a new search", async () => {
    let finishOld: (value: ReturnType<typeof result>) => void = () => {};
    (agent.People.list as jest.Mock)
        .mockReturnValueOnce(new Promise(resolve => { finishOld = resolve; }))
        .mockResolvedValueOnce(result("New"));
    const store = new PersonStore();
    const oldRequest = store.loadPeople();
    await store.setPredicate("New", true, false, false);
    finishOld(result("Old"));
    await oldRequest;
    expect(Array.from(store.personRegistry.keys())).toEqual(["New"]);
});


test("Created sorting is sent to the API and the list retains server ordering", async () => {
    const rows = [
        { id: "new", firstName: "Z", birthday: null, created: new Date("2026-01-02T00:00:00Z") } as Person,
        { id: "old", firstName: "A", birthday: null, created: new Date("2026-01-01T00:00:00Z") } as Person
    ];
    (agent.People.list as jest.Mock).mockResolvedValue({ ...result("unused"), data: rows });
    const store = new PersonStore();
    store.setPagingParams(new PagingParams(3));
    store.setSorting("created");
    await store.setPredicate("", false, false, false);
    const parameters = (agent.People.list as jest.Mock).mock.calls[0][0] as URLSearchParams;
    expect(parameters.get("sorting")).toBe("created");
    expect(parameters.get("pageNumber")).toBe("1");
    expect(store.sortedPeople.map(row => row.id)).toEqual(["new", "old"]);
    store.setSorting("name");
    await store.setPredicate("", false, false, false);
    const next = (agent.People.list as jest.Mock).mock.calls[1][0] as URLSearchParams;
    expect(next.get("sorting")).toBe("name");
    expect(store.sortedPeople.map(row => row.id)).toEqual(["new", "old"]);
});
