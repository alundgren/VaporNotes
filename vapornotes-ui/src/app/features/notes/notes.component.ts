import { Component } from '@angular/core';
import { AuthService } from '../auth/auth.service';
import { HttpClient } from '@angular/common/http';
import { getApiUrl } from '../../common';

@Component({
    selector: 'app-notes',
    standalone: true,
    imports: [],
    templateUrl: './notes.component.html',
    styleUrl: './notes.component.scss'
})
export class NotesComponent {
    constructor(private authService: AuthService, private httpClient: HttpClient) {

    }

    addNote() {
        this.httpClient.post<{ id: string }> (getApiUrl('api/notes/add-text'), { accessToken: this.authService.accessToken(), text: 'abc123åäö' }).subscribe(x => console.log(x));
    }

    listNotes() {
        this.httpClient.post<{ id: string }> (getApiUrl('api/notes/list'), { accessToken: this.authService.accessToken() }).subscribe(x => console.log(x));
    }
}
