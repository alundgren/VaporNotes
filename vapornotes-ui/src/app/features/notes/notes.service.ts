import { Injectable } from "@angular/core";
import { BehaviorSubject, Observable, firstValueFrom, map, of, switchMap, timer } from "rxjs";
import { ApiService, debugLog } from "../../api.service";
import { formatDistanceToNow, isBefore } from "date-fns";

@Injectable({
    providedIn: 'root'
})
export class NotesService {
    constructor(private api: ApiService) {
        this.everyMinute = timer(0, 30 * 1000);
        this.everyMinute.subscribe(() => {
            if(this.isInitRequired) {
                return;
            }
            const expiredIds : string[] = [];
            const now = new Date();
            for(let note of this.notes.value) {
                if(isBefore(note.serverNote.expirationDate, now)) {
                    expiredIds.push(note.serverNote.id);
                } else {
                    const newDurationText = formatDistanceToNow(note.serverNote.expirationDate);
                    if(newDurationText !== note.durationText) {
                        note.durationText = newDurationText;
                    }
                }
            }
            if(expiredIds.length > 0) {
                this.notes.next(this.notes.value.filter(x => !expiredIds.includes(x.serverNote.id)));
            }
        });
    }

    private isInitRequired = true;
    private everyMinute: Observable<number>;

    notes: BehaviorSubject<UiNote[]> = new BehaviorSubject<UiNote[]>([]);

    init() {
        if(!this.isInitRequired) {
            return of(true);
        }
        return this.refreshNotes().pipe(map(() => {
            this.isInitRequired = false;
            return true;
        }))
    }

    addNote(text: string) {
        return this.handleRefresh(this.api.post<ServerNote[]> ('api/notes/add-text', { text: text }));
    }

    refreshNotes() {
        return this.handleRefresh(this.api.post<ServerNote[]> ('api/notes/list', {  }));
    }

    uploadFile(file: File, observeProgressPercent: (progressPercent: number) => void)  {
        return this.api.post<{ uploadKey: string }>('api/upload/begin', { fileName: file.name }).pipe(switchMap(({uploadKey}) => {
            return this.handleRefresh(this.api.upload(`/api/upload/${uploadKey}`, file, { observeProgressPercent: observeProgressPercent }));
        }));
    }

    private handleRefresh(result: Observable<ServerNote[]>) {
        return result.pipe(map(serverNotes => {
            this.notes.next(serverNotes.map(n => new UiNote(n)));
            debugLog('handleRefresh: done');
            return true;
        }))
    }
}

interface ServerNote {
    id: string
    text: string
    creationDate: Date
    expirationDate: Date
    attachedDropboxFile: { path: string }
}
export class UiNote {
    constructor(public serverNote: ServerNote) {
        this.text = serverNote.text
        this.isExpanded = false;
        this.durationText = formatDistanceToNow(serverNote.expirationDate)
        this.isFile = !!serverNote.attachedDropboxFile;
    }

    text: string;
    durationText: string
    isExpanded: boolean;
    isFile: boolean;
}