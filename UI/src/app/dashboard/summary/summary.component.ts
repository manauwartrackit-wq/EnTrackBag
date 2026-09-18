import { Component, OnInit, inject } from "@angular/core";
import { RouterLink, RouterLinkActive } from "@angular/router";
import { ApiService } from "../../core/api.service";

@Component({
  standalone: true,
  selector: "app-summary",
  imports: [RouterLink, RouterLinkActive],
  templateUrl: "./summary.component.html",
  styleUrl: "./summary.component.scss",
})
export class SummaryComponent implements OnInit {
  private readonly api = inject(ApiService);
  kpi: any;

  ngOnInit(): void {
    this.api.get<any>("dashboard/kpis").subscribe({
      next: (value) => (this.kpi = value),
    });
  }
}
