import { CommonModule } from '@angular/common';
import { RouterLink, RouterLinkActive } from '@angular/router';
import { Component, OnInit, inject } from '@angular/core';
import { ApiService } from '../../core/api.service';

@Component({
  standalone: true,
  selector: 'app-sla',
  imports: [CommonModule, RouterLink, RouterLinkActive],
  templateUrl: './sla.component.html',
  styleUrl: './sla.component.scss'
})
export class SlaComponent implements OnInit {
  private readonly api = inject(ApiService);
  bags: any[] = [];
  oldest = '—';
  breaches = 0;
  selectedHistory: any = null;

  ngOnInit(): void {
    this.api.get<any[]>('dashboard/sla').subscribe({
      next: value => {
        this.bags = value ?? [];
        this.breaches = this.bags.filter(b => b.breached).length;
        this.oldest = this.bags.length ? (this.bags[0]?.dwellTime ?? '—') : '—';
      }
    });
  }

  history(id: string): void {
    this.api.get<any>(`dashboard/bags/${encodeURIComponent(id)}/history`).subscribe({
      next: value => this.selectedHistory = value
    });
  }
}
