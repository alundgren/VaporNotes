import { Component } from '@angular/core';
import { ShellComponent } from '../shell/shell.component';
import { AddNoteComponent } from '../notes/add-note/add-note.component';
import { ApiService } from '../../api.service';
import { HttpClient } from '@angular/common/http';
import { firstValueFrom } from 'rxjs';

@Component({
  selector: 'app-ui-prototype',
  standalone: true,
  imports: [ShellComponent, AddNoteComponent],
  templateUrl: './ui-prototype.component.html',
  styleUrl: './ui-prototype.component.scss'
})
export class UiPrototypeComponent {
    constructor(private httpClient: HttpClient) {

    }

    async testLoader() {
        await firstValueFrom(this.httpClient.get(ApiService.getApiUrl('api/test-delay')));
    }
}
