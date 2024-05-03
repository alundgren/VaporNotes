import { Component, inject } from '@angular/core';
import { FormControl, FormGroup, ReactiveFormsModule, Validators } from '@angular/forms';
import { AuthService } from '../auth.service';
import { Router } from '@angular/router';
import { CommonModule } from '@angular/common';

@Component({
    selector: 'app-login',
    standalone: true,
    imports: [ReactiveFormsModule, CommonModule],
    templateUrl: './login.component.html',
    styleUrl: './login.component.scss'
})
export class LoginComponent {
    constructor(private authService: AuthService, private router: Router) {

    }

    authorizeUrl: string | null = null;

    loginForm = new FormGroup({
        code: new FormControl('', [Validators.required])
    });

    ngOnInit() {
        this.authService.beginAuthorization().subscribe(x => this.authorizeUrl = x);
    }

    completeAuthorize(evt ?: Event) {
        evt?.preventDefault();
        this.authService.completeDropboxAuthorization(this.loginForm.value.code!).subscribe(wasLoggedIn => {
            if(wasLoggedIn) {
                this.router.navigate(['secure/notes']);
            }
        });
        /*
        this.authService.login(this.loginForm.value.email ?? '', this.loginForm.value.password ?? '', false).subscribe(wasLoggedIn => {
            if(wasLoggedIn) {
                this.router.navigate(['secure/overview']);
            }
        });
        */
    }
}
