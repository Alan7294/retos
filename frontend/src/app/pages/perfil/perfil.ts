import { Component, OnInit, ChangeDetectorRef } from '@angular/core';
import { CommonModule } from '@angular/common';
import { Router } from '@angular/router';
import { ApiService } from '../../services/api.service';

@Component({
  selector: 'app-perfil',
  standalone: true,
  imports: [CommonModule],
  templateUrl: './perfil.html',
  styleUrls: ['./perfil.css']
})
export class Perfil implements OnInit {
  usuario: any = null;
  estadisticas: any = null;
  retosCompletados: any[] = [];
  historial: any[] = [];
  loading = true;
  tabActual = 'estadisticas';

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
    this.loading = true;
    this.cdr.detectChanges();

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
        this.retosCompletados = Array.isArray(data) ? data : [];
        this.cdr.detectChanges();
      }
    });

    this.api.getHistorial(this.usuario.id).subscribe({
      next: (data) => {
        this.historial = Array.isArray(data) ? data : [];
        this.cdr.detectChanges();
      }
    });
  }

  cambiarTab(tab: string) {
    this.tabActual = tab;
  }

  getEstadoColor(estado: string): string {
    switch (estado?.toLowerCase()) {
      case 'correcto': return '#4ade80';
      case 'incorrecto': return '#f87171';
      case 'pendiente': return '#fbbf24';
      default: return '#e0ffff';
    }
  }
}
