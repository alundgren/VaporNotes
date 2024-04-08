import { Component } from '@angular/core';
import { ShellComponent } from '../shell/shell.component';
import { NotesListComponent } from '../notes/notes-list/notes-list.component';

@Component({
  selector: 'app-ui-prototype',
  standalone: true,
  imports: [ShellComponent, NotesListComponent],
  templateUrl: './ui-prototype.component.html',
  styleUrl: './ui-prototype.component.scss'
})
export class UiPrototypeComponent {

}
