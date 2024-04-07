import { Component } from '@angular/core';
import { ShellComponent } from '../shell/shell.component';

@Component({
  selector: 'app-ui-prototype',
  standalone: true,
  imports: [ShellComponent],
  templateUrl: './ui-prototype.component.html',
  styleUrl: './ui-prototype.component.scss'
})
export class UiPrototypeComponent {

}
