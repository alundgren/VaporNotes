import { Component, NgZone } from '@angular/core';
import { FormControl, FormGroup, ReactiveFormsModule, Validators } from '@angular/forms';
import { AuthService } from '../auth.service';
import { Router } from '@angular/router';
import { CommonModule } from '@angular/common';
import { debugLog } from '../../../api.service';
import { environment } from '../../../../environments/environment';
import { add, formatISO, parseISO } from 'date-fns';

const NextAllowedLoginAttemptTimeKey = '0bc0783e-c396-4dca-9dd2-e9f9e10d6543';

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
        let nextAllowedLoginAttemptTimeRaw = window.localStorage.getItem(NextAllowedLoginAttemptTimeKey);
        if(nextAllowedLoginAttemptTimeRaw && parseISO(nextAllowedLoginAttemptTimeRaw) > new Date()) {
            debugLog('Next login attempt allowed at: ' + nextAllowedLoginAttemptTimeRaw);
            return;
        }

        window.localStorage.setItem(NextAllowedLoginAttemptTimeKey, formatISO(add(new Date(), { minutes: 1 })));
        debugLog('login attempt allowed');

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

        //TODO: Unsub
        this.authService.authState.subscribe(x => {
            if(this.authService.isStateAuthenticated(x)) {
                this.router.navigate(['/secure/notes']);
            }
        })
    }

    async handleCredentialResponse(response: { credential: string }) {
        this.ngZone.run(async () => {
            debugLog(this.decodeJwtResponse(response.credential));
            this.authService.authenticateWithGoogleIdToken(response.credential);
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
