import { Component, NgZone, inject } from '@angular/core';
import { FormControl, FormGroup, ReactiveFormsModule, Validators } from '@angular/forms';
import { AuthService } from '../auth.service';
import { Router } from '@angular/router';
import { CommonModule } from '@angular/common';
import { debugLog } from '../../../api.service';
import { environment } from '../../../../environments/environment';

@Component({
    selector: 'app-login',
    standalone: true,
    imports: [ReactiveFormsModule, CommonModule],
    templateUrl: './login.component.html',
    styleUrl: './login.component.scss'
})
export class LoginComponent {
    constructor(private authService: AuthService, private router: Router, private ngZone: NgZone) {

    }

    authorizeUrl: string | null = null;

    loginForm = new FormGroup({
        code: new FormControl('', [Validators.required])
    });

    ngOnInit() {
        // @ts-ignore
        google.accounts.id.initialize({
            client_id: environment.googleClientId,
            callback: this.handleCredentialResponse.bind(this),
            use_fedcm_for_prompt: true
        });
        // @ts-ignore
        google.accounts.id.renderButton(
            // @ts-ignore
            document.getElementById("google-button"),
            { theme: "outline", size: "large", width: "100%" }
        );
        // @ts-ignore
        google.accounts.id.prompt((notification: PromptMomentNotification) => {
            debugLog(notification)
        });
    }

    async handleCredentialResponse(response: { credential: string }) {
        this.ngZone.run(() => {
            // Here will be your response from Google.
            debugLog(response.credential);
        })
      }
}
