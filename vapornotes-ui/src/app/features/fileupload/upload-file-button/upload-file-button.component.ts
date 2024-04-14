import { CommonModule } from '@angular/common';
import { Component, ElementRef, ViewChild } from '@angular/core';

@Component({
  selector: 'v-upload-file-button',
  standalone: true,
  imports: [CommonModule],
  templateUrl: './upload-file-button.component.html',
  styleUrl: './upload-file-button.component.scss'
})
export class UploadFileButtonComponent {
    @ViewChild('fileInput')
    fileInput: ElementRef<HTMLInputElement> | null = null;

    @ViewChild('fileInputForm')
    fileInputForm: ElementRef<HTMLFormElement> | null = null;

    selectFile(evt ?: Event) {
        evt?.preventDefault();
        this.fileInput?.nativeElement?.click();
    }

    async onFileSelected(evt ?: Event) {
        let target: EventTarget & { files: FileList } = (evt as any).target;
        if((target.files.length ?? 0) !== 1) {
            //TODO: Show message
            return;
        }
        const file = target.files[0];
        //this.fileInputForm?.nativeElement.reset();
        /*
        return new Promise<{ dataUrl: string; filename: string }>((resolve, reject) => {
            if (attachedFiles.length == 1) {
                let r = new FileReader();
                var f = attachedFiles[0];
                if (f.size > 10 * 1024 * 1024) {
                    reject('Attached file is too big!');
                }
                r.onloadend = (e) => {
                    let result = {
                        dataUrl: (<any>e.target).result,
                        filename: f.name,
                    };
                    resolve(result);
                };
                r.readAsDataURL(f);
            } else if (attachedFiles.length == 0) {
                reject('No agreement attached!');
            } else {
                reject('Multiple files have been attached. Please reload the page and only attach a single file.');
            }
        });
       */
    }
}
