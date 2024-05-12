import { Component, NgZone } from '@angular/core';
import { FormControl, FormGroup, ReactiveFormsModule, Validators } from '@angular/forms';
import { AuthService } from '../auth.service';
import { Router } from '@angular/router';
import { CommonModule } from '@angular/common';
import { debugLog } from '../../../api.service';
import { environment } from '../../../../environments/environment';

const TempKey = '07c4c60c-0b3e-4471-924c-d4537e28636a';

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
        let storedCredential = window.sessionStorage.getItem(TempKey);
        if(storedCredential) {
            this.handleCredentialResponse({ credential: storedCredential });
        } else {
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
    }

    async handleCredentialResponse(response: { credential: string }) {
        this.ngZone.run(async () => {
            debugLog(this.decodeJwtResponse(response.credential));
            window.sessionStorage.setItem(TempKey, response.credential);
            const isAuthenticated = await this.authService.authenticateWithGoogleIdToken(response.credential);
            if(isAuthenticated) {
                this.router.navigate(['/secure/notes']);
            } else {
                debugLog("Authentication failed");
            }
        })
    }

    private decodeJwtResponse(token: string) {
        var base64Url = token.split('.')[1];
        var base64 = base64Url.replace(/-/g, '+').replace(/_/g, '/');
        var jsonPayload = decodeURIComponent(window.atob(base64).split('').map((c) => {
          return '%' + ('00' + c.charCodeAt(0).toString(16)).slice(-2);
        }).join(''));
        return JSON.parse(jsonPayload);
      };
}
