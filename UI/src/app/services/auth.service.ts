import { Injectable, inject } from "@angular/core";
import { HttpClient } from "@angular/common/http";
import { Observable, tap } from "rxjs";
import { environment } from "../../environments/environment";

export interface LoginRequest {
  userName: string;
  password: string;
}

export interface LoginResponse {
  accessToken: string;
  expiresAtUtc: string;
  userName: string;
  displayName: string;
  roles: string[];
  permissions: { code: string; accessType: string }[];
  sessionId: string;
}

@Injectable({ providedIn: "root" })
export class AuthService {
  private readonly http = inject(HttpClient);
  private readonly baseUrl = environment.identityApiUrl;

  login(request: LoginRequest): Observable<LoginResponse> {
    return this.http
      .post<LoginResponse>(`${this.baseUrl}/auth/login`, request)
      .pipe(
        tap((response) => {
          localStorage.setItem("access_token", response.accessToken);
          localStorage.setItem(
            "permissions",
            JSON.stringify(response.permissions ?? []),
          );
          localStorage.setItem("user_name", response.userName);
          localStorage.setItem("display_name", response.displayName);
          localStorage.setItem("roles", JSON.stringify(response.roles ?? []));
          localStorage.setItem("session_id", String(response.sessionId));
          localStorage.setItem("expires_at_utc", response.expiresAtUtc);
        }),
      );
  }

  logout(): void {
    localStorage.removeItem("access_token");
    localStorage.removeItem("permissions");
    localStorage.removeItem("user_name");
    localStorage.removeItem("display_name");
    localStorage.removeItem("roles");
    localStorage.removeItem("session_id");
    localStorage.removeItem("expires_at_utc");
  }

  getAccessToken(): string | null {
    return localStorage.getItem("access_token");
  }

  isAuthenticated(): boolean {
    return !!this.getAccessToken();
  }

  hasPermission(permission: string, accessType: string = "VIEW"): boolean {
    return this.getPermissions().some(x => x.code === permission && x.accessType === accessType);
  }

  getPermissions(): { code: string; accessType: string }[] {
    const value = localStorage.getItem("permissions");
    if (!value) return [];
    try {
      const parsed = JSON.parse(value);
      return Array.isArray(parsed) ? parsed : [];
    } catch {
      return [];
    }
  }

  getUserName(): string { return localStorage.getItem("user_name") || this.tokenClaims()?.["http://schemas.xmlsoap.org/ws/2005/05/identity/claims/name"] || "—"; }
  getDisplayName(): string { return localStorage.getItem("display_name") || this.tokenClaims()?.["display_name"] || this.getUserName(); }
  getSessionId(): string { return localStorage.getItem("session_id") || String(this.tokenClaims()?.["session_id"] ?? "—"); }
  getExpiresAt(): string | null {
    const stored = localStorage.getItem("expires_at_utc");
    if (stored) return stored;
    const expiry = Number(this.tokenClaims()?.["exp"]);
    return Number.isFinite(expiry) && expiry > 0 ? new Date(expiry * 1000).toISOString() : null;
  }
  getRoles(): string[] {
    try {
      const parsed = JSON.parse(localStorage.getItem("roles") || "[]");
      if (Array.isArray(parsed) && parsed.length) return parsed;
    } catch { /* Use the current token for sessions created before this UI update. */ }
    const role = this.tokenClaims()?.["http://schemas.microsoft.com/ws/2008/06/identity/claims/role"];
    return Array.isArray(role) ? role : role ? [String(role)] : [];
  }

  private tokenClaims(): Record<string, any> | null {
    const token = this.getAccessToken();
    if (!token) return null;
    try {
      const payload = token.split(".")[1].replace(/-/g, "+").replace(/_/g, "/");
      return JSON.parse(decodeURIComponent(Array.from(atob(payload.padEnd(Math.ceil(payload.length / 4) * 4, "="))).map((character) => `%${character.charCodeAt(0).toString(16).padStart(2, "0")}`).join("")));
    } catch { return null; }
  }
}
