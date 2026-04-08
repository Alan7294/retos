import { Component, OnInit, ChangeDetectorRef } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ActivatedRoute, Router } from '@angular/router';
import { FormsModule } from '@angular/forms';
import { ApiService } from '../../services/api.service';

@Component({
  selector: 'app-detalle-reto',
  standalone: true,
  imports: [CommonModule, FormsModule],
  templateUrl: './detalle-reto.html',
  styleUrls: ['./detalle-reto.css']
})
export class DetalleReto implements OnInit {
  reto: any = null;
  usuario: any = null;
  codigo = '';
  descripcion = '';
  enviando = false;
  mensaje = '';
  tipoMensaje = 'success';
  loading = true;
  yaCompletado = false;

  constructor(
    private route: ActivatedRoute,
    private router: Router,
    private api: ApiService,
    private cdr: ChangeDetectorRef
  ) {}

  ngOnInit() {
    const userData = localStorage.getItem('usuario');
    if (userData) {
      this.usuario = JSON.parse(userData);
    }
    
    const id = Number(this.route.snapshot.paramMap.get('id'));
    this.cargarReto(id);
  }

  cargarReto(id: number) {
    this.loading = true;
    this.cdr.detectChanges();
    
    this.api.getReto(id).subscribe({
      next: (retoData) => {
        this.reto = retoData;
        if (this.usuario) {
          this.api.getRetosCompletados(this.usuario.id).subscribe({
            next: (retosCompletados) => {
              this.yaCompletado = Array.isArray(retosCompletados) && 
                retosCompletados.some((r: any) => r.idReto === id || r.IdReto === id);
              this.loading = false;
              this.cdr.detectChanges();
            },
            error: () => {
              this.loading = false;
              this.cdr.detectChanges();
            }
          });
        } else {
          this.loading = false;
          this.cdr.detectChanges();
        }
      },
      error: () => this.router.navigate(['/retos'])
    });
  }

  enviarSolucion() {
    if (!this.usuario) {
      this.mostrarMensaje('Debes iniciar sesión para enviar una solución', 'error');
      return;
    }

    if (this.yaCompletado) {
      this.mostrarMensaje('Ya has completado este reto', 'error');
      return;
    }

    if (!this.codigo.trim()) {
      this.mostrarMensaje('Ingresa tu código', 'error');
      return;
    }

    this.enviando = true;
    this.cdr.detectChanges();

    this.api.enviarSolucion(this.usuario.id, this.reto.idReto, this.codigo, this.descripcion).subscribe({
      next: (data) => {
        if (data.message && data.message.includes('ya has completado')) {
          this.yaCompletado = true;
          this.mostrarMensaje(data.message, 'error');
        } else {
          this.mostrarMensaje('¡Solución enviada exitosamente!', 'success');
          this.codigo = '';
          this.descripcion = '';
          setTimeout(() => this.router.navigate(['/retos']), 1500);
        }
        this.enviando = false;
        this.cdr.detectChanges();
      },
      error: () => {
        this.enviando = false;
        this.mostrarMensaje('Error al enviar la solución', 'error');
        this.cdr.detectChanges();
      }
    });
  }

  mostrarMensaje(msg: string, tipo: string) {
    this.mensaje = msg;
    this.tipoMensaje = tipo;
    this.cdr.detectChanges();
    setTimeout(() => {
      this.mensaje = '';
      this.cdr.detectChanges();
    }, 3000);
  }

  getDificultadColor(dificultad: string): string {
    switch (dificultad?.toLowerCase()) {
      case 'facil': return '#4ade80';
      case 'media': return '#fbbf24';
      case 'dificil': return '#f87171';
      default: return '#9ca3af';
    }
  }
}
