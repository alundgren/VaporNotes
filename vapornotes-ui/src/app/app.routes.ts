import { Routes } from '@angular/router';
import { LoginComponent } from './features/auth/login/login.component';
import { AuthGuard } from './features/auth/auth.guard';
import { NotesComponent } from './features/notes/notes.component';
import { UiPrototypeComponent } from './features/ui-prototype/ui-prototype.component';

export const routes: Routes = [
    { path: '', redirectTo: 'ui-prototype', pathMatch: 'full' },
    { path: 'ui-prototype', component: UiPrototypeComponent },
    { path: 'login', component: LoginComponent },
    { path: 'secure', canActivate: [AuthGuard], children: [
        { path: 'notes', component: NotesComponent }
    ]}
];
