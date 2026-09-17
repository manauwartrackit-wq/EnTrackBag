import { HttpInterceptorFn } from "@angular/common/http";
import { inject } from "@angular/core";
import { catchError, throwError } from "rxjs";
import { AuthService } from "../services/auth.service";
import { environment } from "../../environments/environment";
import { BACKGROUND_REQUEST } from "./request-activity";

export const authInterceptor: HttpInterceptorFn = (req, next) => {
  const trusted = [environment.apiUrl, environment.identityApiUrl]
    .some(base => req.url.startsWith(base + "/"));
  const token = localStorage.getItem("access_token");
  if (!trusted || !token || req.url.endsWith("/auth/login")) return next(req);
  const auth = inject(AuthService);
  const foreground = !req.context.get(BACKGROUND_REQUEST) && !req.url.includes("/auth/");
  if (foreground) auth.markRequestActivity();
  if (auth.getAccessToken() !== token) return throwError(() => new Error("Session expired"));
  const headers: Record<string, string> = { Authorization: `Bearer ${token}` };
  if (foreground) headers["X-User-Activity"] = "1";
  return next(req.clone({ setHeaders: headers })).pipe(catchError(error => {
    // An old request must not clear a newer login.
    if (error.status === 401 && auth.getAccessToken() === token) auth.clearSession();
    return throwError(() => error);
  }));
};
