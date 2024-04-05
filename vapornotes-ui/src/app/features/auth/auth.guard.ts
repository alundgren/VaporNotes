import { CanActivateFn, Router } from '@angular/router';
import { AuthService } from './auth.service';
import { inject } from '@angular/core';

export const AuthGuard: CanActivateFn = (_, __) => {
    const auth  = inject(AuthService);
    return auth.isAuthenticated()
    ? true
    : auth.getLocalLoginUrl();
};