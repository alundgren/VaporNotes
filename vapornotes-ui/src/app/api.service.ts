import { HttpClient, HttpEventType, HttpHeaders, HttpRequest, HttpResponse } from "@angular/common/http";
import { environment } from "../environments/environment";
import { AuthService } from "./features/auth/auth.service";
import { Observable, delay, switchMap, tap } from "rxjs";
import { Injectable } from "@angular/core";

@Injectable({
    providedIn: 'root'
})
export class ApiService {
    constructor(private httpClient: HttpClient, private authService: AuthService) {

    }

    post<TResponse>(relativeUrl: string, body: any) {
        return this.authService.getAccessTokenOrRedirectToLogin().pipe(switchMap(accessToken => {
            const headers = new HttpHeaders({
                'Content-Type': 'application/json',
                Authorization: `Bearer ${accessToken}`,
            });
            return this.httpClient.post<TResponse>(ApiService.getApiUrl(relativeUrl), body, { headers: headers });
        }));
    }

    download(relativeUrl: string) {
        const getFileNameFromContentDisposition = (disposition: string | null): string | null => {
            if (!disposition) {
                return null;
            }

            const utf8FilenameRegex = /filename\*=UTF-8''([\w%\-\.]+)(?:; ?|$)/i;
            const asciiFilenameRegex = /^filename=(["']?)(.*?[^\\])\1(?:; ?|$)/i;

            let fileName: string | null = null;
            if (utf8FilenameRegex.test(disposition)) {
                fileName = decodeURIComponent(utf8FilenameRegex.exec(disposition)![1]);
            } else {
                const filenameStart = disposition.toLowerCase().indexOf('filename=');
                if (filenameStart >= 0) {
                    const partialDisposition = disposition.slice(filenameStart);
                    const matches = asciiFilenameRegex.exec(partialDisposition);
                    if (matches != null && matches[2]) {
                        fileName = matches[2];
                    }
                }
            }
            return fileName;
        }

        return this.authService.getAccessTokenOrRedirectToLogin().pipe(switchMap(accessToken => {
            const headers = new HttpHeaders({
                'Content-Type': 'application/json',
                Authorization: `Bearer ${accessToken}`,
            });
            return new Observable<{ fileData: Blob, fileName: string } | null>(ob => {
                this.httpClient
                    .get(ApiService.getApiUrl(relativeUrl), { headers: headers, responseType: 'blob', observe: 'response' })
                    .subscribe(x => {
                        const fileName = getFileNameFromContentDisposition(x.headers.get('Content-Disposition'));
                        if(x.body && fileName) {
                            ob.next({ fileData: x.body, fileName: fileName });
                        } else {
                            ob.next(null);
                        }
                        ob.complete();
                    });
            });
        }));
    }

    upload<TResponse>(relativeUrl: string, file: File, options?: { observeProgressPercent: (progressPercent: number) => void }): Observable<TResponse> {
        const formData: FormData = new FormData();
        formData.append('file', file);
        return new Observable<TResponse>(ob => {
            this.authService.getAccessTokenOrRedirectToLogin().pipe(switchMap(accessToken => {
                const headers = new HttpHeaders({
                    Authorization: `Bearer ${accessToken}`,
                });
                const req = new HttpRequest('POST', ApiService.getApiUrl(relativeUrl), formData, {
                    reportProgress: true,
                    responseType: 'json',
                    headers: headers
                });
                return this.httpClient.request<TResponse>(req);
            })).subscribe(event => {
                if (event instanceof HttpResponse) {
                    ob.next((event as HttpResponse<TResponse>).body!);
                    ob.complete();
                } else if (event.type === HttpEventType.UploadProgress && event.total) {
                    options?.observeProgressPercent(100 * event.loaded / event.total!)
                }
            });
        })
    }

    static getApiUrl(relativeUrl: string) {
        let baseUrl = environment.apiBaseUrl;
        if (baseUrl.endsWith('/')) {
            baseUrl = baseUrl.substring(0, baseUrl.length - 1);
        }

        const trimmedRelativeUrl = relativeUrl.startsWith('/')
            ? relativeUrl.substring(1)
            : relativeUrl;

        return `${baseUrl}/${trimmedRelativeUrl}`
    }
}

export function debugLog(content: any) {
    if (environment.isDebugLogEnabled) {
        console.log(content);
    }
}

export interface UploadProgressEvent {
    percentDone: number
    isComplete: boolean
}