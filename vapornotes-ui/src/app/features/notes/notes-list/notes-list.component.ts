import { Component } from '@angular/core';
import { ShellComponent } from '../../shell/shell.component';
import { CommonModule } from '@angular/common';
import { Router } from '@angular/router';
import { NotesService, UiNote } from '../notes.service';
import { QuillModule } from 'ngx-quill';
import { Subscription, firstValueFrom } from 'rxjs';

@Component({
    selector: 'v-notes-list',
    standalone: true,
    imports: [CommonModule, ShellComponent, QuillModule],
    templateUrl: './notes-list.component.html',
    styleUrl: './notes-list.component.scss'
})
export class NotesListComponent {
    constructor(private router: Router, private notesService: NotesService) {

    }

    public notes: UiNote[] = [];

    private subs: Subscription[] = [];

    addNote(evt?: Event) {
        evt?.preventDefault();
        this.router.navigateByUrl('/secure/add-note')
    }

    async ngOnInit() {
        const notesSub = this.notesService.notes.subscribe(notes => {
            this.notes = notes;
        });
        this.subs.push(notesSub);
        await firstValueFrom(this.notesService.init());
    }

    ngOnDestroy() {
        for(let sub of this.subs) {
            sub.unsubscribe();
        }
    }
}