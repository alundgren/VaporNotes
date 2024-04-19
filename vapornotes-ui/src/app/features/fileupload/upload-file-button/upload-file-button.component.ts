import { CommonModule } from '@angular/common';
import { Component, ElementRef, ViewChild } from '@angular/core';
import { ApiService } from '../../../api.service';
import { Observable, firstValueFrom, lastValueFrom, switchMap, tap } from 'rxjs';
import { HttpClient, HttpEvent, HttpEventType, HttpHeaders, HttpRequest, HttpResponse } from '@angular/common/http';
import { AuthService } from '../../auth/auth.service';
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
        let target: EventTarget & { files: FileList } = (evt as any).target;
        if ((target.files.length ?? 0) !== 1) {
            //TODO: Show message
            return;
        }
        await firstValueFrom(this.notesService.uploadFile(target.files[0], x=> {
            console.log(x);
        }));
        this.router.navigateByUrl('/secure/notes');
    }
}
