import { HttpContextToken, HttpEvent, HttpHandler, HttpInterceptor, HttpRequest } from "@angular/common/http";
import { Injectable } from "@angular/core";
import { Observable, finalize } from "rxjs";
import { LoadingService } from "./loading.service";

export const SkipLoading = new HttpContextToken<boolean>(() => false);

@Injectable()
export class LoadingHttpInterceptor implements HttpInterceptor {
    constructor(private loadingService: LoadingService) {
    }

    intercept(req: HttpRequest<any>, next: HttpHandler): Observable<HttpEvent<any>> {
        if (req.context.get(SkipLoading)) {
            return next.handle(req);
        }

        //todo make this work https://blog.angular-university.io/angular-loading-indicator/ ... intercepetor does not work

        this.loadingService.loadingOn();

        return next.handle(req).pipe(
            finalize(() => {
                this.loadingService.loadingOff();
            })
        );
    }
}