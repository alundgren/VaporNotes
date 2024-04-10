import { CommonModule } from '@angular/common';
import { Component } from '@angular/core';
import { ShellComponent } from '../../shell/shell.component';
import { QuillModule } from 'ngx-quill';
import { NotesService } from '../notes.service';
import * as Quill from 'quill'
import { FormsModule } from '@angular/forms';
import { Router } from '@angular/router';

@Component({
    selector: 'v-add-note',
    standalone: true,
    imports: [CommonModule, ShellComponent, QuillModule, FormsModule],
    templateUrl: './add-note.component.html',
    styleUrl: './add-note.component.scss'
})
export class AddNoteComponent {
    constructor(private notes: NotesService, private router: Router) {

    }

    editorContent: string = '';
    private editor: Quill.default | null = null;

    saveNote(evt ?: Event) {
        evt?.preventDefault();
        if(!this.editor) {
            throw new Error('Missing editor');
        }
        this.notes.addNote(this.editorContent).subscribe(() => {
            this.router.navigateByUrl('/secure/notes');
        })
    }

    onQuillEditorCreated(quillInstance: Quill.default) {
        this.editor = quillInstance;
    }
}
