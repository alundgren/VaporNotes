import { Component } from '@angular/core';
import { ShellComponent } from '../shell/shell.component';
import { AddNoteComponent } from '../notes/add-note/add-note.component';

@Component({
  selector: 'app-ui-prototype',
  standalone: true,
  imports: [ShellComponent, AddNoteComponent],
  templateUrl: './ui-prototype.component.html',
  styleUrl: './ui-prototype.component.scss'
})
export class UiPrototypeComponent {

}
