import { makeAutoObservable, runInAction } from "mobx";
import agent from "../api/agent";
import { Person, PersonFormValues } from "../models/person";
import { Pagination, PagingParams } from "../models/pagination";

export default class PersonStore {
    personRegistry = new Map<string, Person>();
    selectedPerson: Person | undefined = undefined;
    editMode = false;
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
        this.personRegistry.clear();
        return this.loadPeople();
    }

    get axiosParams() {
        const params = new URLSearchParams();
        params.append('pageNumber', this.pagingParams.pageNumber.toString());
        params.append('pageSize', this.pagingParams.pageSize.toString());
        this.predicate.forEach((value, key) => params.append(key, value))
        return params;
    }

    get sortedPeople() {
        if (this.sorting === "name") {
            return Array.from(this.personRegistry.values()).sort((a, b) => {
                if (a.firstName !== b.firstName) {
                    return a.firstName.localeCompare(b.firstName, 'en');
                }
                let surnameA = a.surname === null ? "" : a.surname;
                let surnameB = b.surname === null ? "" : b.surname;
                return surnameA.localeCompare(surnameB, 'en');
            });
        }

        return Array.from(this.personRegistry.values()).sort((a, b) => {
            return (b.created?.getTime() ?? 0) - (a.created?.getTime() ?? 0)
        });
    }

    private listRequestVersion = 0;

    loadPeople = async () => {
        const version = ++this.listRequestVersion;
        this.loadingInitial = true;
        try {
            const result = await agent.People.list(this.axiosParams);
            if (version !== this.listRequestVersion) return;
            result.data.forEach(person => {
                this.setPerson(person);
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

    loadPerson = async (id: string) => {
        let person = this.getPerson(id);
        if (person) {
            this.selectedPerson = person;
            return person; 
        } else {
            this.loadingInitial = true;
            try {
                person = await agent.People.details(id);
                this.setPerson(person);
                runInAction(() => {
                    this.selectedPerson = person;
                })
                this.setLoadingInitial(false);
                return person; 
            } catch (error) {
                console.log(error);
                this.setLoadingInitial(false);
            }
        }
    }

    private setPerson = (person: Person) => {
        person.birthday = person.birthday === null ? null : new Date(person.birthday);
        this.personRegistry.set(person.id, person);
    }

    private getPerson = (id: string) => {
        return this.personRegistry.get(id);
    }

    setLoadingInitial = (state: boolean) => {
        this.loadingInitial = state;
    }

    createPerson = async (person: PersonFormValues) => {
        try {
            await agent.People.create(person);
            const newPerson = new Person(person);
            this.setPerson(newPerson);
            runInAction(() => {
                this.selectedPerson = newPerson;
            })
        } catch(error) {
            console.log(error);
        }
    }

    updatePerson = async (person: PersonFormValues) => {
        try {
            await agent.People.update(person);
            runInAction(() => {
                if (person.id) {
                    let updatedPerson = {...this.getPerson(person.id), ...person}
                    this.personRegistry.set(person.id, updatedPerson as Person);
                    this.selectedPerson = updatedPerson as Person;
                }
            })
        } catch(error) {
            console.log(error);
        }
    }

    deletePerson = async (id: string) => {
        this.loading = true;
        try {
            await agent.People.delete(id);
            runInAction(() => {
                this.personRegistry.delete(id);
                this.loading = false;
            })
        } catch(error) {
            console.log(error);
            runInAction(() => {
                this.loading = false;
            })
        }
    }

    resetPeople = () => {
        this.personRegistry.clear();
        this.selectedPerson = undefined;
        this.pagingParams = new PagingParams();
        this.pagination = null;
    }

    clearSelectedPerson = () => {
        this.selectedPerson = undefined;
    }
}