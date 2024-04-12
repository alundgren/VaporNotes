import { Component } from '@angular/core';
import { RouterModule } from '@angular/router';
import { AuthService } from '../auth/auth.service';
import { LoadingService } from '../loading/loading.service';
import { Observable, firstValueFrom } from 'rxjs';
import { CommonModule } from '@angular/common';
import { environment } from '../../../environments/environment';
import { NotesService } from '../notes/notes.service';

@Component({
    selector: 'v-shell',
    standalone: true,
    imports: [RouterModule, CommonModule],
    templateUrl: './shell.component.html',
    styleUrl: './shell.component.scss'
})
export class ShellComponent {
    constructor(private authService: AuthService, loadingService: LoadingService, private noteService: NotesService) {
        this.loading$ = loadingService.loading$;
    }

    loading$: Observable<boolean>;
    isTest = !environment.isProduction;

    logout(evt ?: Event) {
        evt?.preventDefault();
        this.authService.logout();
    }

    async addTestData(evt ?: Event) {
        evt?.preventDefault();
        for(var i=0; i < 3; i++) {
            let text = '';
            for(var j=0;j<50; j++) {
                text += `[${j+1}]: Row Row RowRowRowRowRowRowRowRow Rowv Row  Row Row Row Row RowRow Row <br>`
            }
            await firstValueFrom(this.noteService.addNote(text));
        }
    }
}
