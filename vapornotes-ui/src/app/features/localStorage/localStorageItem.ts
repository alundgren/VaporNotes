import { Injectable } from '@angular/core';

//Change to expire all existing data
const CurrentVersion = '20240513.1'
const OneMinutesMs = 60000;

export class LocalStorageItem<T> {
    constructor(private key: string, private options ?: LocalStorageOptions) {

    }

    now = () => new Date();

    set(item: T) {
        let expiresAfterEpoch: number | null = null;
        if(this.options?.expiresAfterMinutes || this.options?.expiresAfterHours) {
            expiresAfterEpoch = this.now().valueOf();
            expiresAfterEpoch += OneMinutesMs * 60 * (this.options.expiresAfterHours ?? 0)
            expiresAfterEpoch += OneMinutesMs * (this.options.expiresAfterMinutes ?? 0);
        }
        const storedData: StoredData<T> = {
            version: CurrentVersion,
            item: item,
            expiresAfterEpoch: expiresAfterEpoch
        }
        this.storage().setItem(this.key, JSON.stringify(storedData))
    }

    get() : T | null {
        const storedData = this.storage().getItem(this.key);
        if(!storedData) {
            return null;
        }
        const parsedData : StoredData<T> = JSON.parse(storedData);
        if(parsedData?.version !== CurrentVersion) {
            return null;
        }
        let nowEpoch = this.now().valueOf();
        if(parsedData.expiresAfterEpoch === null || nowEpoch <= parsedData.expiresAfterEpoch) {
            return parsedData.item;
        } else {
            this.storage().removeItem(this.key);
            return null;
        }
    }

    storage() {
        return this.options?.useSessionStorage ? window.sessionStorage : window.localStorage;
    }


/*
    set<T>(key: string, item: T, options ?: LocalStorageOptions) {
        let expiresAfterEpoch: number | null = null;
        if(options?.expiresAfterMinutes || options?.expiresAfterHours) {
            expiresAfterEpoch = this.now().valueOf();
            expiresAfterEpoch += OneMinutesMs * 60 * (options.expiresAfterHours ?? 0)
            expiresAfterEpoch += OneMinutesMs * (options.expiresAfterMinutes ?? 0);
        }
        const storedData: StoredData<T> = {
            version: CurrentVersion,
            item: item,
            expiresAfterEpoch: expiresAfterEpoch
        }
        window.localStorage.setItem(key, JSON.stringify(storedData))
    }

    get<T>(key: string) : T | null {
        const storedData = window.localStorage.getItem(key);
        if(!storedData) {
            return null;
        }
        const parsedData : StoredData<T> = JSON.parse(storedData);
        if(parsedData?.version !== CurrentVersion) {
            return null;
        }
        let nowEpoch = this.now().valueOf();
        if(parsedData.expiresAfterEpoch === null || nowEpoch <= parsedData.expiresAfterEpoch) {
            return parsedData.item;
        } else {
            window.localStorage.removeItem(key);
            return null;
        }
    }
    */
}

export interface LocalStorageOptions {
    expiresAfterMinutes ?: number,
    expiresAfterHours ?: number
    useSessionStorage ?: boolean
}

interface StoredData<T> {
    version: string
    item: T
    expiresAfterEpoch: number | null
}
