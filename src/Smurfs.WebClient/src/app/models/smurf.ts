export interface Smurf {
    id: string;
    name: string;
    health: number;
    birthday: string | null;
    infected: boolean | null;
    gender: 'Male' | 'Female' | null;
    petId: string | null;
}

export interface CreateSmurfRequest {
    name: string;
    health: number;
    birthday: string | null;
    infected: boolean | null;
    gender: 'Male' | 'Female' | null;
}
