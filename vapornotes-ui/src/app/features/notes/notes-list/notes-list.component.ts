import { Component, Input } from '@angular/core';
import { ShellComponent } from '../../shell/shell.component';
import { CommonModule } from '@angular/common';
import { Router } from '@angular/router';
import { NotesService } from '../notes.service';

@Component({
    selector: 'v-notes-list',
    standalone: true,
    imports: [CommonModule, ShellComponent],
    templateUrl: './notes-list.component.html',
    styleUrl: './notes-list.component.scss'
})
export class NotesListComponent {
    constructor(private router: Router, public notesService: NotesService) {

    }

    notes: NoteModel[] | null = (() => {
        let  n = [
            { text: 'Test 123', durationText: '10 seconds' },
            { text: 'Test q334f423', durationText: '5 minutes' },
            { text: 'Test q3f23r', durationText: '19 hours' },
            { text: 'Test 123', durationText: '10 seconds' },
            { text: 'Test q334f423', durationText: '5 minutes' },
            { text: 'Test q3f23r', durationText: '19 hours' },
            { text: 'Test 123', durationText: '10 seconds' },
            { text: 'Test q334f423', durationText: '5 minutes' },
            { text: 'Test q3f23r', durationText: '19 hours' },
            { text: 'Test 123', durationText: '10 seconds' },
            { text: 'Test q334f423', durationText: '5 minutes' },
            { text: 'Test q3f23r', durationText: '19 hours' },
            { text: 'Test 123', durationText: '10 seconds' },
            { text: 'Test q334f423', durationText: '5 minutes' },
            { text: 'Test q3f23r', durationText: '19 hours' },
            { text: 'Test 123', durationText: '10 seconds' },
            { text: 'Test q334f423', durationText: '5 minutes' },
            { text: 'Test q3f23r', durationText: '19 hours' },
            { text: 'Test 123', durationText: '10 seconds' },
            { text: 'Test q334f423', durationText: '5 minutes' },
            { text: 'Test q3f23r', durationText: '19 hours' },
            { text: 'Test 123', durationText: '10 seconds' },
            { text: 'Test q334f423', durationText: '5 minutes' },
            { text: 'Test q3f23r', durationText: '19 hours' },
            { text: 'Test 123', durationText: '10 seconds' },
            { text: 'Test q334f423', durationText: '5 minutes' },
            { text: 'Test q3f23r', durationText: '19 hours' },
            { text: 'Test 123', durationText: '10 seconds' },
            { text: 'Test q334f423', durationText: '5 minutes' },
            { text: 'Test q3f23r', durationText: '19 hours' },
            { text: 'Test 123', durationText: '10 seconds' },
            { text: 'Test q334f423', durationText: '5 minutes' },
            { text: 'Test q3f23r', durationText: '19 hours' },
            { text: 'Test 123', durationText: '10 seconds' },
            { text: 'Test q334f423', durationText: '5 minutes' },
            { text: 'Test q3f23r', durationText: '19 hours' },
        ];

        for(var i=0;i < 50; i++) {
            n[3].text += 'New line ' + '\r\n';
        }

        return n;
    })();

    addNote(evt ?: Event) {
        this.router.navigateByUrl('/secure/add-note')
    }
}

interface NoteModel {
    isExpanded ?: boolean
    durationText: string
    text: string
}
