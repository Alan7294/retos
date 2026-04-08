import { Component, OnInit, ChangeDetectorRef } from '@angular/core';
import { CommonModule } from '@angular/common';
import { Router, RouterModule } from '@angular/router';
import { ApiService } from '../../services/api.service';

@Component({
  selector: 'app-dashboard',
  templateUrl: './dashboard.html',
  styleUrls: ['./dashboard.css'],
  standalone: true,
  imports: [CommonModule, RouterModule]
})
export class Dashboard implements OnInit {
  usuario: any = null;
  estadisticas: any = null;
  retosRecientes: any[] = [];
  loading = true;

  constructor(private router: Router, private api: ApiService, private cdr: ChangeDetectorRef) {}

  ngOnInit() {
    const userData = localStorage.getItem('usuario');
    if (!userData) {
      this.router.navigate(['/login']);
      return;
    }

    this.usuario = JSON.parse(userData);
    this.cargarDatos();
  }

  cargarDatos() {
    this.api.getEstadisticas(this.usuario.id).subscribe({
      next: (data) => {
        this.estadisticas = data;
        this.loading = false;
        this.cdr.detectChanges();
      },
      error: () => {
        this.loading = false;
        this.cdr.detectChanges();
      }
    });

    this.api.getRetosCompletados(this.usuario.id).subscribe({
      next: (data) => {
        this.retosRecientes = data.slice(0, 3);
        this.cdr.detectChanges();
      }
    });
  }
}
