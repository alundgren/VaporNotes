import { Component } from '@angular/core';
import { RouterModule } from '@angular/router';
import { AuthService } from '../auth/auth.service';
import { LoadingService } from '../loading/loading.service';
import { Observable } from 'rxjs';
import { CommonModule } from '@angular/common';

@Component({
    selector: 'v-shell',
    standalone: true,
    imports: [RouterModule, CommonModule],
    templateUrl: './shell.component.html',
    styleUrl: './shell.component.scss'
})
export class ShellComponent {
    constructor(private authService: AuthService, loadingService: LoadingService) {
        this.loading$ = loadingService.loading$;
    }

    loading$: Observable<boolean>;

    logout(evt ?: Event) {
        evt?.preventDefault();
        this.authService.logout();
    }
}
