import { CommonModule } from "@angular/common";
import { Component, OnInit, inject } from "@angular/core";
import { FormsModule } from "@angular/forms";
import { ApiService } from "../core/api.service";

interface JourneyThreshold { code: string; from: string; to: string; normalSeconds: number; delaySeconds: number; }
interface JourneyConfiguration { currentPrimaryServer: string | null; lastUpdated: string | null; thresholds: JourneyThreshold[]; }

@Component({
  selector: "app-bag-journey-configuration",
  standalone: true,
  imports: [CommonModule, FormsModule],
  templateUrl: "./bag-journey-configuration.component.html",
  styleUrl: "./bag-journey-configuration.component.scss",
})
export class BagJourneyConfigurationComponent implements OnInit {
  private readonly api = inject(ApiService);
  configuration: JourneyConfiguration | null = null;
  loading = true; saving = false; errorMessage = ""; successMessage = "";
  ngOnInit(): void { this.load(); }
  load(): void {
    this.loading = true; this.errorMessage = "";
    this.api.get<JourneyConfiguration>("bag-journey-configuration").subscribe({
      next: (value) => { this.configuration = value; this.loading = false; },
      error: (error) => { this.errorMessage = error?.error?.message ?? "Bag journey settings could not be loaded."; this.loading = false; },
    });
  }
  save(): void {
    if (!this.configuration || this.configuration.thresholds.some(x => x.normalSeconds < 1 || x.delaySeconds < 1)) { this.errorMessage = "Every threshold must be at least one second."; return; }
    this.saving = true; this.errorMessage = ""; this.successMessage = "";
    this.api.put<JourneyConfiguration, { thresholds: JourneyThreshold[] }>("bag-journey-configuration", { thresholds: this.configuration.thresholds }).subscribe({
      next: (value) => { this.configuration = value; this.successMessage = "Bag journey settings updated successfully."; this.saving = false; },
      error: (error) => { this.errorMessage = error?.error?.message ?? "Settings could not be updated."; this.saving = false; },
    });
  }
  formatDate(value: string | null): string { return value ? new Intl.DateTimeFormat("en-AE", { dateStyle: "medium", timeStyle: "medium" }).format(new Date(value)) : "—"; }
  icon(code: string): string { return code.includes("tag") ? "◇" : code.includes("lounge") ? "▰" : code.includes("exit") ? "⇥" : code.includes("return") ? "⌘" : "⌂"; }
  threshold(code: string): JourneyThreshold { return this.configuration!.thresholds.find(item => item.code === code)!; }
  get firstMissThreshold(): number { return this.configuration ? this.threshold("dog-land-exit").normalSeconds : 0; }
  set firstMissThreshold(value: number) { if (this.configuration) this.threshold("dog-land-exit").normalSeconds = value; }
  get secondMissThreshold(): number { return this.configuration ? this.threshold("exit-recheck").normalSeconds : 0; }
  set secondMissThreshold(value: number) { if (this.configuration) this.threshold("exit-recheck").normalSeconds = value; }
}
