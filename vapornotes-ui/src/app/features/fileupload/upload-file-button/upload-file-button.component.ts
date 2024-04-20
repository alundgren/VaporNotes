import { CommonModule } from '@angular/common';
import { Component, ElementRef, ViewChild } from '@angular/core';
import { debugLog } from '../../../api.service';
import { firstValueFrom } from 'rxjs';
import { Router } from '@angular/router';
import { NotesService } from '../../notes/notes.service';

@Component({
    selector: 'v-upload-file-button',
    standalone: true,
    imports: [CommonModule],
    templateUrl: './upload-file-button.component.html',
    styleUrl: './upload-file-button.component.scss'
})
export class UploadFileButtonComponent {
    constructor(private notesService: NotesService,
        private router: Router
    ) {

    }

    @ViewChild('fileInput')
    fileInput: ElementRef<HTMLInputElement> | null = null;

    @ViewChild('fileInputForm')
    fileInputForm: ElementRef<HTMLFormElement> | null = null;

    selectFile(evt?: Event) {
        evt?.preventDefault();
        this.fileInput?.nativeElement?.click();
    }

    async onFileSelected(evt?: Event) {
        console.log(evt);
        let target: EventTarget & { files: FileList } = (evt as any).target;
        if ((target.files.length ?? 0) !== 1) {
            debugLog('No file selected');
            return;
        }
        await firstValueFrom(this.notesService.uploadFile(target.files[0], x => {
            debugLog(`upload progress: ${x}`);
        }));
        this.router.navigateByUrl('/secure/notes');
    }
}
