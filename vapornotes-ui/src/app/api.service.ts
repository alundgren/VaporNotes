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

    upload<TResponse>(relativeUrl: string, file: File, options ?: { observeProgressPercent: (progressPercent: number) => void }): Observable<TResponse> {
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
        if(baseUrl.endsWith('/')) {
            baseUrl = baseUrl.substring(0, baseUrl.length - 1);
        }

        const trimmedRelativeUrl = relativeUrl.startsWith('/')
            ? relativeUrl.substring(1)
            : relativeUrl;

        return `${baseUrl}/${trimmedRelativeUrl}`
    }
}

export function debugLog(text: string) {
    if(environment.isDebugLogEnabled) {
        console.log(text);
    }
}

export interface UploadProgressEvent {
    percentDone: number
    isComplete: boolean
}