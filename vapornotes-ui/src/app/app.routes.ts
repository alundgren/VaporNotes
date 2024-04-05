import { Routes } from '@angular/router';
import { LoginComponent } from './features/auth/login/login.component';
import { AuthGuard } from './features/auth/auth.guard';
import { NotesComponent } from './features/notes/notes.component';

export const routes: Routes = [
    { path: '', redirectTo: 'secure/notes', pathMatch: 'full' },
    { path: 'login', component: LoginComponent },
    { path: 'secure', canActivate: [AuthGuard], children: [
        { path: 'notes', component: NotesComponent }
    ]}
];
