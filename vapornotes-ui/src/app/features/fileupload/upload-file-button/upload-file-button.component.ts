import { CommonModule } from '@angular/common';
import { Component, ElementRef, ViewChild } from '@angular/core';
import { ApiService } from '../../../api.service';
import { Observable, firstValueFrom, lastValueFrom, switchMap, tap } from 'rxjs';
import { HttpClient, HttpEvent, HttpEventType, HttpHeaders, HttpRequest, HttpResponse } from '@angular/common/http';
import { AuthService } from '../../auth/auth.service';

@Component({
    selector: 'v-upload-file-button',
    standalone: true,
    imports: [CommonModule],
    templateUrl: './upload-file-button.component.html',
    styleUrl: './upload-file-button.component.scss'
})
export class UploadFileButtonComponent {
    constructor(private authService: AuthService, private httpClient: HttpClient, private apiService: ApiService) {

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
        const file = target.files[0];
        const uploadKey = await firstValueFrom(this.apiService.post<string>('api/upload/begin', { fileName: file.name }));

        await lastValueFrom(this.upload(uploadKey, file).pipe(tap(event => {
            if (event.type === HttpEventType.UploadProgress) {
                const percentDone = Math.round(100 * event.loaded / event.total!);
                console.log(`File is ${percentDone}% uploaded.`);
            } else if (event instanceof HttpResponse) {
                console.log('File is completely uploaded!', event.body);
            }
        })));
    }

    private upload(uploadKey: string, file: File): Observable<HttpEvent<any>> {
        const formData: FormData = new FormData();
        formData.append('file', file);

        return this.authService.getAccessTokenOrRedirectToLogin().pipe(switchMap(accessToken => {
            const headers = new HttpHeaders({
                Authorization: `Bearer ${accessToken}`,
            });
            const req = new HttpRequest('POST', ApiService.getApiUrl(`api/upload/${uploadKey}`), formData, {
                reportProgress: true,
                responseType: 'json',
                headers: headers
            });
            return this.httpClient.request(req);
        }));
    }
}
