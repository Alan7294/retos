import { Component, OnInit, ChangeDetectorRef } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { RouterLink } from '@angular/router';
import { ApiService } from '../../services/api.service';

@Component({
  selector: 'app-retos',
  standalone: true,
  imports: [CommonModule, FormsModule, RouterLink],
  templateUrl: './retos.html',
  styleUrls: ['./retos.css']
})
export class Retos implements OnInit {
  retos: any[] = [];
  usuario: any = null;
  filtroDificultad = '';
  ordenarPor = 'fecha';
  dificultadOptions = ['Facil', 'Media', 'Dificil'];
  loading = true;

  constructor(private api: ApiService, private cdr: ChangeDetectorRef) {}

  ngOnInit() {
    const userData = localStorage.getItem('usuario');
    if (userData) {
      this.usuario = JSON.parse(userData);
    }
    this.cargarRetos();
  }

  cargarRetos() {
    this.loading = true;
    this.cdr.detectChanges();
    
    const observable = this.usuario 
      ? this.api.getRetosConEstado(this.usuario.id)
      : this.api.getRetos();
    
    observable.subscribe({
      next: (data: any) => {
        const rawRetos = Array.isArray(data) ? data : (data?.value || []);
        this.retos = rawRetos.map((r: any) => ({
          idReto: r.IdReto || r.idReto,
          titulo: r.Titulo || r.titulo,
          descripcion: r.Descripcion || r.descripcion,
          dificultad: r.Dificultad || r.dificultad,
          puntos: r.Puntos || r.puntos,
          fechaCreacion: r.FechaCreacion || r.fechaCreacion,
          completado: r.Completado || r.completado
        }));
        this.loading = false;
        this.cdr.detectChanges();
      },
      error: () => {
        this.loading = false;
        this.cdr.detectChanges();
      }
    });
  }

  getDificultadColor(dificultad: string): string {
    switch (dificultad?.toLowerCase()) {
      case 'facil': return '#4ade80';
      case 'media': return '#fbbf24';
      case 'dificil': return '#f87171';
      default: return '#9ca3af';
    }
  }

  aplicarFiltros() {
    this.cargarRetos();
  }
}
