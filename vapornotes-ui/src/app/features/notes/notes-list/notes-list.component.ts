import { Component, Input } from '@angular/core';
import { ShellComponent } from '../../shell/shell.component';
import { CommonModule } from '@angular/common';
import { Router } from '@angular/router';
import { NotesService } from '../notes.service';
import { QuillModule } from 'ngx-quill';
import { Observable, Subscription, timer } from 'rxjs';
import { formatDistanceToNow } from "date-fns";

@Component({
    selector: 'v-notes-list',
    standalone: true,
    imports: [CommonModule, ShellComponent, QuillModule],
    templateUrl: './notes-list.component.html',
    styleUrl: './notes-list.component.scss'
})
export class NotesListComponent {
    constructor(private router: Router, notesService: NotesService) {
        const notesSub = notesService.notes.subscribe(notes => {
            this.notes = notes.map(note => ({
                isExpanded: false,
                text: note.text,
                durationText: formatDistanceToNow(note.expirationDate)
            }))
        });
        this.subs.push(notesSub);
    }

    everyMinute: Observable<number> = timer(0, 60 * 1000);

    public notes: NoteViewModel[] = [];

    private subs: Subscription[] = [];


    addNote(evt?: Event) {
        evt?.preventDefault();
        this.router.navigateByUrl('/secure/add-note')
    }

    ngOnInit() {

    }

    ngOnDestroy() {
        for(let sub of this.subs) {
            sub.unsubscribe();
        }
    }
}

interface NoteViewModel {
    isExpanded?: boolean
    durationText: string
    text: string
}
