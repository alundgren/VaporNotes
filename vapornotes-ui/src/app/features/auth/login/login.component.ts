import { Component, NgZone } from '@angular/core';
import { FormControl, FormGroup, ReactiveFormsModule, Validators } from '@angular/forms';
import { AuthService } from '../auth.service';
import { Router } from '@angular/router';
import { CommonModule } from '@angular/common';
import { debugLog } from '../../../api.service';
import { environment } from '../../../../environments/environment';
import { LocalStorageItem } from '../../localStorage/localStorageItem';

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

    isLoginOnCooldown = new LocalStorageItem<boolean>('isLoginOnCooldown', { expiresAfterMinutes: 1 });
    cachedGoogleIdToken = new LocalStorageItem<string>('cachedGoogleIdToken', { expiresAfterMinutes: 30 });

    authorizeUrl: string | null = null;

    loginForm = new FormGroup({
        code: new FormControl('', [Validators.required])
    });

    async ngOnInit() {
        const cachedIdToken = this.cachedGoogleIdToken.get();
        if(cachedIdToken) {
            this.authenticateWithGoogleIdToken(cachedIdToken)
            return;
        }

        if(this.isLoginOnCooldown.get() === true) {
            return;
        }
        this.isLoginOnCooldown.set(true);

        this.initiateGoogleLogin();

    }

    private initiateGoogleLogin() {
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

    private async handleCredentialResponse(response: { credential: string }) {
        this.ngZone.run(async () => {
            if(!response.credential) {
                return;
            }
            this.cachedGoogleIdToken.set(response.credential);
            this.authenticateWithGoogleIdToken(response.credential);
        })
    }

    private async authenticateWithGoogleIdToken(idToken: string) {
        const isLoggedIn = await this.authService.authenticateWithGoogleIdToken(idToken);
        if(isLoggedIn) {
            this.router.navigate(['/secure/notes']);
        }
    }

    /* Kept around for future use. Can get email / user name for instance
    private decodeJwtResponse(token: string) {
        var base64Url = token.split('.')[1];
        var base64 = base64Url.replace(/-/g, '+').replace(/_/g, '/');
        var jsonPayload = decodeURIComponent(window.atob(base64).split('').map((c) => {
          return '%' + ('00' + c.charCodeAt(0).toString(16)).slice(-2);
        }).join(''));
        return JSON.parse(jsonPayload);
      };
      */
}
