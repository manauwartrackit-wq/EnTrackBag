import { Injectable, inject, signal } from "@angular/core";
import { ApiService } from "../core/api.service";
import { AuthService } from "./auth.service";

@Injectable({ providedIn: "root" })
export class NotificationService {
  private readonly api = inject(ApiService);
  private readonly auth = inject(AuthService);
  readonly count = signal(0);

  refresh(): void {
    if (!this.auth.hasPermission("Dashboard.SLA")) { this.clear(); return; }
    this.api.get<{ breached?: boolean }[]>("dashboard/sla", true).subscribe({
      next: (bags) => this.setCount((bags ?? []).filter((bag) => bag.breached).length),
      error: () => this.clear(),
    });
  }

  setCount(value: number): void {
    this.count.set(Math.max(0, Math.trunc(value)));
  }

  add(amount = 1): void {
    this.setCount(this.count() + amount);
  }

  clear(): void {
    this.count.set(0);
  }
}
