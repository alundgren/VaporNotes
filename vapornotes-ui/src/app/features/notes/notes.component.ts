import { Component } from '@angular/core';
import { ApiService } from '../../api.service';
import { AuthService } from '../auth/auth.service';

@Component({
    selector: 'app-notes',
    standalone: true,
    imports: [],
    templateUrl: './notes.component.html',
    styleUrl: './notes.component.scss'
})
export class NotesComponent {
    constructor(private api: ApiService, private auth: AuthService) {

    }

    addNote() {
        this.api.post<{ id: string }> ('api/notes/add-text', { text: 'abc123åäö' }).subscribe(x => console.log(x));
    }

    listNotes() {
        this.api.post<{ id: string }> ('api/notes/list', {  }).subscribe(x => console.log(x));
    }

    expireAccessToken() {
        this.auth.expireAccessToken();
    }
}
