import { CommonModule } from "@angular/common";
import { Component, OnInit, inject } from "@angular/core";
import { FormsModule } from "@angular/forms";
import { ApiService } from "../core/api.service";
import { AuthService } from "../services/auth.service";

interface JourneyThreshold {
  code: string;
  from: string;
  to: string;
  normalSeconds: number;
  delaySeconds: number;
}

interface JourneyConfiguration {
  currentPrimaryServer: string | null;
  lastUpdated: string | null;
  thresholds: JourneyThreshold[];
}

@Component({
  selector: "app-bag-journey-configuration",
  standalone: true,
  imports: [CommonModule, FormsModule],
  templateUrl: "./bag-journey-configuration.component.html",
  styleUrl: "./bag-journey-configuration.component.scss",
})
export class BagJourneyConfigurationComponent implements OnInit {
  readonly auth = inject(AuthService);
  private readonly api = inject(ApiService);

  configuration: JourneyConfiguration | null = null;
  loading = true;
  saving = false;
  errorMessage = "";
  successMessage = "";

  ngOnInit(): void {
    this.load();
  }

  load(): void {
    this.loading = true;
    this.errorMessage = "";
    this.api.get<JourneyConfiguration>("bag-journey-configuration").subscribe({
      next: (value) => {
        this.configuration = value;
        this.loading = false;
      },
      error: (error) => {
        this.errorMessage =
          error?.error?.message ?? "Bag journey settings could not be loaded.";
        this.loading = false;
      },
    });
  }

  save(): void {
    if (!this.auth.hasAccess("BagJourney.Configuration", "EDIT")) return;
    if (
      !this.configuration ||
      this.configuration.thresholds.some(
        (x) => x.normalSeconds < 1 || x.delaySeconds < 1
      )
    ) {
      this.errorMessage = "Every threshold must be at least one second.";
      return;
    }

    this.saving = true;
    this.errorMessage = "";
    this.successMessage = "";

    this.api
      .put<JourneyConfiguration, { thresholds: JourneyThreshold[] }>(
        "bag-journey-configuration",
        { thresholds: this.configuration.thresholds }
      )
      .subscribe({
        next: (value) => {
          this.configuration = value;
          this.successMessage = "Bag journey settings updated successfully.";
          this.saving = false;
        },
        error: (error) => {
          this.errorMessage =
            error?.error?.message ?? "Settings could not be updated.";
          this.saving = false;
        },
      });
  }

  formatDate(value: string | null): string {
    return value
      ? new Intl.DateTimeFormat("en-AE", {
          dateStyle: "medium",
          timeStyle: "medium",
        }).format(new Date(value))
      : "—";
  }

  threshold(code: string): JourneyThreshold {
    return this.configuration!.thresholds.find((item) => item.code === code)!;
  }

  bump(
    code: string,
    field: "normalSeconds" | "delaySeconds",
    delta: number
  ): void {
    const t = this.threshold(code);
    if (!t) return;
    const next = Math.max(1, Math.min(86400, (t[field] || 0) + delta));
    t[field] = next;
  }
}
