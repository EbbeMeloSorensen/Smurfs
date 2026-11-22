import { makeAutoObservable, runInAction } from "mobx";
import agent from "../api/agent";
import { Smurf } from "../models/smurf";
import { Pagination, PagingParams } from "../models/pagination";

export default class SmurfStore {
    smurfRegistry = new Map<string, Smurf>();
    selectedSmurf: Smurf | undefined = undefined;
    loading = false;
    loadingInitial = false;
    pagination: Pagination | null = null;
    pagingParams = new PagingParams();
    predicate = new Map();
    sorting = 'name';

    constructor() {
        makeAutoObservable(this);
    }

    setPagingParams = (pagingParams: PagingParams) => {
        this.pagingParams = pagingParams;
    }

    setSorting = (sorting: string) => {
        this.sorting = sorting;
    }

    resetPredicate = () => {
        this.predicate.forEach((value, key) => {
            this.predicate.delete(key);
        })
    }

    setPredicate = (
        nameFilter: string,
        infected: boolean,
        notInfected: boolean,
        infectionUnspecified: boolean) => {
        this.resetPredicate();
        if (nameFilter.trim()) {
            this.predicate.set('name', nameFilter.trim());
        }
        const states: string[] = [];
        if (infected) states.push("true");
        if (notInfected) states.push("false");
        if (infectionUnspecified) states.push("null");
        if (states.length) this.predicate.set('infected', states.join('|'));

        this.predicate.set('sorting', this.sorting);
        this.pagingParams = new PagingParams();
        this.pagination = null;
        this.smurfRegistry.clear();
        return this.loadSmurfs();
    }

    get axiosParams() {
        const params = new URLSearchParams();
        params.append('pageNumber', this.pagingParams.pageNumber.toString());
        params.append('pageSize', this.pagingParams.pageSize.toString());
        this.predicate.forEach((value, key) => params.append(key, value))
        params.set('sorting', this.sorting);
        return params;
    }

    get smurfs() {
        return Array.from(this.smurfRegistry.values());
    }

    private listRequestVersion = 0;

    loadSmurfs = async () => {
        const version = ++this.listRequestVersion;
        this.loadingInitial = true;
        try {
            const result = await agent.Smurfs.list(this.axiosParams);
            if (version !== this.listRequestVersion) return;
            result.data.forEach(smurf => {
                this.setSmurf(smurf);
            })
            this.setPagination(result.pagination);
            this.setLoadingInitial(false);
        } catch(error) {
            if (version !== this.listRequestVersion) return;
            console.log(error);
            this.setLoadingInitial(false);
        }
    }

    setPagination = (pagination: Pagination) => {
        this.pagination = pagination;
    }

    loadSmurf = async (id: string) => {
        let smurf = this.getSmurf(id);
        if (smurf) {
            this.selectedSmurf = smurf;
            return smurf; 
        } else {
            this.loadingInitial = true;
            try {
                smurf = await agent.Smurfs.details(id);
                this.setSmurf(smurf);
                runInAction(() => {
                    this.selectedSmurf = smurf;
                })
                this.setLoadingInitial(false);
                return smurf; 
            } catch (error) {
                console.log(error);
                this.setLoadingInitial(false);
            }
        }
    }

    private setSmurf = (smurf: Smurf) => {
        this.smurfRegistry.set(smurf.id, smurf);
    }

    private getSmurf = (id: string) => {
        return this.smurfRegistry.get(id);
    }

    setLoadingInitial = (state: boolean) => {
        this.loadingInitial = state;
    }

    deleteSmurf = async (id: string) => {
        this.loading = true;
        try {
            await agent.Smurfs.delete(id);
            runInAction(() => {
                this.smurfRegistry.delete(id);
                this.loading = false;
            })
        } catch(error) {
            console.log(error);
            runInAction(() => {
                this.loading = false;
            })
        }
    }

    resetSmurfs = () => {
        this.smurfRegistry.clear();
        this.selectedSmurf = undefined;
        this.pagingParams = new PagingParams();
        this.pagination = null;
    }

    clearSelectedSmurf = () => {
        this.selectedSmurf = undefined;
    }
}