import { TestBed } from '@angular/core/testing';
import * as DateFns from 'date-fns';
import { LocalStorageItem, LocalStorageOptions } from './localStorageItem';

const Key = 'test1';

describe('LocalStorage', () => {
    let now: Date;

    let moveTimeForward = (d: DateFns.Duration, item: LocalStorageItem<TestItem>) => {
        now = DateFns.add(now, d);
        item.now = () => now;
    };

    let wipeLocalAndSessionStorage = () => {
        window.localStorage.removeItem(Key);
        window.sessionStorage.removeItem(Key);
    };

    let createItem = (options ?: LocalStorageOptions) => new LocalStorageItem<TestItem>(Key, options);

    beforeEach(() => {
        TestBed.configureTestingModule({});
        now = new Date();

    });

    it('stored items preserve values correctly', () => {
        const item = createItem();

        item.set(testItem1);

        const value = item.get();
        expect(value?.a).toBe(testItem1.a);
        expect(value?.b).toBe(testItem1.b);
    });

    it('can use local storage', () => {
        const item = createItem({ useSessionStorage: false });

        wipeLocalAndSessionStorage();
        item.set(testItem1);

        expect(window.localStorage.getItem(Key)).toBeTruthy();
        expect(window.sessionStorage.getItem(Key)).toBeFalsy();
    });

    it('can use session storage', () => {
        const item = createItem({ useSessionStorage: true });

        wipeLocalAndSessionStorage();
        item.set(testItem1);

        expect(window.localStorage.getItem(Key)).toBeFalsy();
        expect(window.sessionStorage.getItem(Key)).toBeTruthy();
    });

    it('returns items with expiration before expiration date', () => {
        const item = createItem({ expiresAfterMinutes : 1 });
        item.set(testItem1);

        moveTimeForward({ minutes: 1 }, item);

        expect(item.get()).toBeTruthy();
    });

    it('does not return items with expiration after expiration date', () => {
        const item = createItem({ expiresAfterMinutes : 1 });
        item.set(testItem1);

        moveTimeForward({ minutes: 1, seconds: 1 }, item);

        expect(item.get()).toBeFalsy();
    });
});

interface TestItem {
    a: string
    b: number
}
const testItem1 : TestItem = { a: 'abcåäö', b: 0 };

