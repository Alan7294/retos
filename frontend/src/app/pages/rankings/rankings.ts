import { Component, OnInit, ChangeDetectorRef } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ApiService } from '../../services/api.service';

@Component({
  selector: 'app-rankings',
  standalone: true,
  imports: [CommonModule],
  templateUrl: './rankings.html',
  styleUrls: ['./rankings.css']
})
export class Rankings implements OnInit {
  ranking: any[] = [];
  loading = true;

  constructor(private api: ApiService, private cdr: ChangeDetectorRef) {}

  ngOnInit() {
    this.cargarRanking();
  }

  cargarRanking() {
    this.loading = true;
    this.cdr.detectChanges();
    
    this.api.getRanking().subscribe({
      next: (data) => {
        this.ranking = data;
        this.loading = false;
        this.cdr.detectChanges();
      },
      error: () => {
        this.loading = false;
        this.cdr.detectChanges();
      }
    });
  }

  getMedal(posicion: number): string {
    switch (posicion) {
      case 1: return '🥇';
      case 2: return '🥈';
      case 3: return '🥉';
      default: return `#${posicion}`;
    }
  }
}
