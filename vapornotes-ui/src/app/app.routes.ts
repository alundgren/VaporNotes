import { Routes } from '@angular/router';
import { LoginComponent } from './features/auth/login/login.component';
import { AuthGuard } from './features/auth/auth.guard';
import { UiPrototypeComponent } from './features/ui-prototype/ui-prototype.component';
import { NotesListComponent } from './features/notes/notes-list/notes-list.component';
import { AddNoteComponent } from './features/notes/add-note/add-note.component';

export const routes: Routes = [
    { path: '', redirectTo: 'secure/notes', pathMatch: 'full' },
    { path: 'ui-prototype', component: UiPrototypeComponent },
    { path: 'login', component: LoginComponent },
    { path: 'secure', canActivate: [], children: [
        { path: 'notes', component: NotesListComponent },
        { path: 'add-note', component: AddNoteComponent }
    ]}
];
