import { Component } from '@angular/core';
import { RouterModule } from '@angular/router';
import { AuthService } from '../auth/auth.service';

@Component({
    selector: 'v-shell',
    standalone: true,
    imports: [RouterModule],
    templateUrl: './shell.component.html',
    styleUrl: './shell.component.scss'
})
export class ShellComponent {
    constructor(private authService: AuthService) {

    }

    logout(evt ?: Event) {
        evt?.preventDefault();
        this.authService.logout();
    }
}
