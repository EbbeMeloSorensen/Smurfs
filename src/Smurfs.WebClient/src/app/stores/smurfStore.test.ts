import SmurfStore from "./smurfStore";
import agent from "../api/agent";
import { Smurf } from "../models/smurf";
import { PagingParams } from "../models/pagination";

jest.mock("../api/agent", () => ({
    __esModule: true, default: { Smurfs: { list: jest.fn() } }
}));

function result(name: string) {
    return { data: [{ id: name, name: name, birthday: null } as Smurf],
        pagination: { currentPage: 1, itemsPerPage: 25, totalItems: 1, totalPages: 1 } };
}

beforeEach(() => jest.clearAllMocks());

test("Search resets paging and changes values even when the filter keys stay the same", async () => {
    const store = new SmurfStore();
    (agent.Smurfs.list as jest.Mock).mockResolvedValue(result("Papa"));
    store.setPagingParams(new PagingParams(3));
    await store.setPredicate(" Papa ", true, false, true);
    const first = (agent.Smurfs.list as jest.Mock).mock.calls[0][0] as URLSearchParams;
    expect(first.get("name")).toBe("Papa");
    expect(first.get("infected")).toBe("true|null");
    expect(first.get("pageNumber")).toBe("1");
    expect(first.has("dead")).toBe(false);
    await store.setPredicate("Smurf", false, true, false);
    const second = (agent.Smurfs.list as jest.Mock).mock.calls[1][0] as URLSearchParams;
    expect(second.get("name")).toBe("Smurf");
    expect(second.get("infected")).toBe("false");
    await store.setPredicate("", false, false, false);
    const cleared = (agent.Smurfs.list as jest.Mock).mock.calls[2][0] as URLSearchParams;
    expect(cleared.has("name")).toBe(false);
    expect(cleared.has("infected")).toBe(false);
});

test("a previous page response cannot reintroduce rows after a new search", async () => {
    let finishOld: (value: ReturnType<typeof result>) => void = () => {};
    (agent.Smurfs.list as jest.Mock)
        .mockReturnValueOnce(new Promise(resolve => { finishOld = resolve; }))
        .mockResolvedValueOnce(result("New"));
    const store = new SmurfStore();
    const oldRequest = store.loadSmurfs();
    await store.setPredicate("New", true, false, false);
    finishOld(result("Old"));
    await oldRequest;
    expect(Array.from(store.smurfRegistry.keys())).toEqual(["New"]);
});


test("Created sorting is sent to the API and the list retains server ordering", async () => {
    const rows = [
        { id: "new", name: "Z", birthday: null, createdUtc: "2026-01-02T00:00:00Z" } as Smurf,
        { id: "old", name: "A", birthday: null, createdUtc: "2026-01-01T00:00:00Z" } as Smurf
    ];
    (agent.Smurfs.list as jest.Mock).mockResolvedValue({ ...result("unused"), data: rows });
    const store = new SmurfStore();
    store.setPagingParams(new PagingParams(3));
    store.setSorting("created");
    await store.setPredicate("", false, false, false);
    const parameters = (agent.Smurfs.list as jest.Mock).mock.calls[0][0] as URLSearchParams;
    expect(parameters.get("sorting")).toBe("created");
    expect(parameters.get("pageNumber")).toBe("1");
    expect(store.smurfs.map(row => row.id)).toEqual(["new", "old"]);
    store.setSorting("name");
    await store.setPredicate("", false, false, false);
    const next = (agent.Smurfs.list as jest.Mock).mock.calls[1][0] as URLSearchParams;
    expect(next.get("sorting")).toBe("name");
    expect(store.smurfs.map(row => row.id)).toEqual(["new", "old"]);
});
