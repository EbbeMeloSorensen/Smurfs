import axios, { AxiosResponse, AxiosError } from 'axios';
import { toast } from 'react-toastify';
import { history } from '../..';
import { CreateSmurfRequest, Smurf } from '../models/smurf';
import { Person, PersonFormValues } from '../models/person';
import { PaginatedResult } from '../models/pagination';
import { User, UserFormValues } from '../models/user';
import { store } from '../stores/store';

const sleep = (delay: number) => {
    return new Promise((resolve) => {
        setTimeout(resolve, delay)
    })
}

const currentAccountUrl = '/account';

axios.defaults.baseURL = process.env.REACT_APP_API_URL;

axios.interceptors.request.use(config => {
    const token = store.commonStore.token;
    if (token && config.headers) {
        config.headers.Authorization = `Bearer ${token}`;
    } 
    return config;
})

axios.interceptors.response.use(async response => {
    if (process.env.NODE_ENV === 'development') await sleep(1000);
    const pagination = response.headers['pagination'];
    if (pagination) {
        response.data = new PaginatedResult(response.data, JSON.parse(pagination));
        return response as AxiosResponse<PaginatedResult<any>>
    }
    return response;
}, (error: AxiosError) => {
    if (!error.response) {
        return Promise.reject(error);
    }
    const {data, status, config} = error.response;
    let data_as_any = data as any; // The instructor doesn't do this, but I couldn't get it to compile without casting
    switch (status) {
        case 400:
            if (typeof data === 'string') {
                toast.error(data);
            }
            if (config.method === 'get' && data_as_any?.errors?.hasOwnProperty('id')) {
                history.push('/not-found');
            }
            if (data_as_any.errors) {
                const modalStateErrors = [];
                for (const key in data_as_any.errors) {
                    if (data_as_any.errors[key]) {
                        modalStateErrors.push(data_as_any.errors[key])
                    }
                }
                throw modalStateErrors.flat();
            }
            break;
        case 401:
            if (config.method !== 'get' || config.url !== currentAccountUrl) {
                toast.error('unauthorized');
            }
            break;
        case 404:
            history.push('/not-found');
            break;
        case 500:
            store.commonStore.setServerError(data_as_any);
            history.push('/server-error');
            break;
    }
    return Promise.reject(error);
})

const responseBody = <T> (response: AxiosResponse<T>) => response.data;

const requests = {
    get: <T> (url: string) => axios.get<T>(url).then(responseBody),
    post: <T> (url: string, body: {}) => axios.post<T>(url, body).then(responseBody),
    put: <T> (url: string, body: {}) => axios.put<T>(url, body).then(responseBody),
    del: <T> (url: string) => axios.delete<T>(url).then(responseBody),
}

function toPerson(smurf: Smurf): Person {
    return {
        ...smurf,
        firstName: smurf.name,
        surname: null,
        birthday: smurf.birthday ? new Date(smurf.birthday) : null,
        start: null,
        end: null,
        created: null,
        superseded: null,
        nickname: null,
        address: null,
        zipCode: null,
        city: null,
        category: null,
        description: null,
        dead: null,
        latitude: null,
        longitude: null
    };
}

const People = {
    list: (params: URLSearchParams) =>
        axios.get<PaginatedResult<Smurf[]>>('/smurfs', { params })
            .then(responseBody)
            .then(result => new PaginatedResult(result.data.map(toPerson), result.pagination)),
    details: (id: string) => requests.get<Smurf>('/smurfs/' + id).then(toPerson),
    create: (person: PersonFormValues) => requests.post<void>('/people', person),
    update: (person: PersonFormValues) => requests.put<void>(`/people/${person.id}`, person),
    delete: (id: string) => requests.del<void>(`/smurfs/${id}`)
}

const Account = {
    current: () => requests.get<User>(currentAccountUrl),
    login: (user: UserFormValues) => requests.post<User>('/account/login', user),
    register: (user: UserFormValues) => requests.post<User>('/account/register', user),
}

const agent = {
    Smurfs: {
        create: (request: CreateSmurfRequest) => requests.post<void>('/smurfs', request)
    },
    People,
    Account
}

export default agent;