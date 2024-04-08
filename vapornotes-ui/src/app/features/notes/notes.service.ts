import { Inject, Injectable } from "@angular/core";
import { BehaviorSubject, Observable } from "rxjs";

@Injectable({
    providedIn: 'root'
})
export class NotesService {
    notes: BehaviorSubject<Note[]> = new BehaviorSubject<Note[]>([]);

    addNote(text: string) : Observable<boolean> {
        return new Observable<boolean>(x => {
            this.notes.next([...this.notes.value, new Note(text, new Date(), new Date(new Date().getTime() + 5 * 60000))]);
            x.next(true);
            x.complete();
        })
    }
}

export class Note {
    constructor(public text: string, public creationDate: Date, public expirationDate: Date, public attachedDropboxFile ?: { path: string }) {

    }
}

/*
string Text, DateTimeOffset CreationDate, DateTimeOffset ExpirationDate, DropboxFileReference? AttachedDropboxFile
*/