import { Component, OnInit, ChangeDetectorRef } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { Router } from '@angular/router';
import { ApiService } from '../../services/api.service';

@Component({
  selector: 'app-admin',
  standalone: true,
  imports: [CommonModule, FormsModule],
  templateUrl: './admin.html',
  styleUrls: ['./admin.css']
})
export class Admin implements OnInit {
  titulo = '';
  descripcion = '';
  dificultad = 'Facil';
  puntos = 10;
  mensaje = '';
  tipoMensaje = 'success';
  enviando = false;
  retos: any[] = [];
  loading = true;
  usuario: any = null;
  isAdmin = false;

  editandoId: number | null = null;
  editTitulo = '';
  editDescripcion = '';
  editDificultad = 'Facil';
  editPuntos = 10;

  mostrarSoluciones = false;
  soluciones: any[] = [];
  solucionesLoading = false;
  verSolucionesRetoId: number | null = null;

  dificultadOptions = ['Facil', 'Medio', 'Dificil'];

  constructor(private router: Router, private api: ApiService, private cdr: ChangeDetectorRef) {}

  ngOnInit() {
    const userData = localStorage.getItem('usuario');
    if (!userData) {
      this.router.navigate(['/login']);
      return;
    }
    
    this.usuario = JSON.parse(userData);
    if (!this.usuario.nombre.toLowerCase().includes('admin')) {
      this.router.navigate(['/retos']);
      return;
    }
    this.isAdmin = true;

    this.cargarRetos();
  }

  cargarRetos() {
    this.loading = true;
    this.api.getRetos().subscribe({
      next: (data) => {
        this.retos = Array.isArray(data) ? data : [];
        this.loading = false;
      },
      error: () => this.loading = false
    });
  }

  crearReto() {
    if (!this.titulo || !this.descripcion) {
      this.mostrarMensaje('Completa todos los campos', 'error');
      return;
    }

    this.enviando = true;

    this.api.crearReto({
      idUsuario: this.usuario.id,
      titulo: this.titulo,
      descripcion: this.descripcion,
      dificultad: this.dificultad,
      puntos: this.puntos
    }).subscribe({
      next: (data) => {
        if (data.message && data.message.includes('administrador')) {
          this.mostrarMensaje(data.message, 'error');
        } else {
          this.mostrarMensaje('¡Reto creado exitosamente!', 'success');
          this.titulo = '';
          this.descripcion = '';
          this.dificultad = 'Facil';
          this.puntos = 10;
          this.cargarRetos();
        }
        this.enviando = false;
      },
      error: () => {
        this.mostrarMensaje('Error al crear el reto', 'error');
        this.enviando = false;
      }
    });
  }

  iniciarEdicion(reto: any) {
    this.editandoId = reto.idReto;
    this.editTitulo = reto.titulo;
    this.editDescripcion = reto.descripcion;
    this.editDificultad = reto.dificultad;
    this.editPuntos = reto.puntos;
  }

  cancelarEdicion() {
    this.editandoId = null;
    this.editTitulo = '';
    this.editDescripcion = '';
    this.editDificultad = 'Facil';
    this.editPuntos = 10;
  }

  guardarEdicion(retoId: number) {
    const token = localStorage.getItem('token');
    console.log('Token guardado:', token ? 'sí' : 'no');
    
    this.api.actualizarReto(retoId, {
      titulo: this.editTitulo,
      descripcion: this.editDescripcion,
      dificultad: this.editDificultad,
      puntos: this.editPuntos
    }).subscribe({
      next: () => {
        this.mostrarMensaje('Reto actualizado correctamente', 'success');
        this.cancelarEdicion();
        this.cargarRetos();
      },
      error: () => this.mostrarMensaje('Error al actualizar el reto', 'error')
    });
  }

  eliminarReto(retoId: number) {
    if (!confirm('¿Estás seguro de eliminar este reto? Esta acción no se puede deshacer.')) {
      return;
    }

    this.api.eliminarReto(retoId).subscribe({
      next: () => {
        this.mostrarMensaje('Reto eliminado correctamente', 'success');
        this.cargarRetos();
      },
      error: () => this.mostrarMensaje('Error al eliminar el reto', 'error')
    });
  }

  mostrarMensaje(msg: string, tipo: string) {
    this.mensaje = msg;
    this.tipoMensaje = tipo;
    setTimeout(() => this.mensaje = '', 3000);
  }

  verSoluciones(retoId: number) {
    this.verSolucionesRetoId = retoId;
    this.mostrarSoluciones = true;
    this.solucionesLoading = true;
    console.log('Cargando soluciones del reto:', retoId);
    
    this.api.getSolucionesPorReto(retoId).subscribe({
      next: (data) => {
        console.log('Soluciones del reto:', data);
        this.soluciones = Array.isArray(data) ? data : [];
        this.solucionesLoading = false;
        this.cdr.detectChanges();
      },
      error: (err) => {
        console.error('Error:', err);
        this.solucionesLoading = false;
        this.mostrarMensaje('Error al cargar soluciones', 'error');
        this.cdr.detectChanges();
      }
    });
  }

  verTodasSoluciones() {
    this.verSolucionesRetoId = null;
    this.mostrarSoluciones = true;
    this.solucionesLoading = true;
    console.log('Cargando todas las soluciones...');
    
    this.api.getTodasLasSoluciones().subscribe({
      next: (data) => {
        console.log('Soluciones recibidas:', data);
        this.soluciones = Array.isArray(data) ? data : [];
        this.solucionesLoading = false;
        console.log('Agrupadas:', this.getSolucionesPorReto());
        this.cdr.detectChanges();
      },
      error: (err) => {
        console.error('Error al cargar soluciones:', err);
        this.solucionesLoading = false;
        this.mostrarMensaje('Error al cargar soluciones', 'error');
        this.cdr.detectChanges();
      }
    });
  }

  cerrarSoluciones() {
    this.mostrarSoluciones = false;
    this.soluciones = [];
    this.verSolucionesRetoId = null;
  }

  getSolucionesPorReto(): { titulo: string; soluciones: any[] }[] {
    const grouped: { [key: string]: any[] } = {};
    
    this.soluciones.forEach(sol => {
      const titulo = sol.retoTitulo || sol.RetoTitulo || 'Sin título';
      if (!grouped[titulo]) {
        grouped[titulo] = [];
      }
      grouped[titulo].push(sol);
    });

    return Object.keys(grouped).map(titulo => ({
      titulo,
      soluciones: grouped[titulo]
    }));
  }

  getEstadoColor(estado: string): string {
    switch (estado?.toLowerCase()) {
      case 'correcto': return '#4ade80';
      case 'incorrecto': return '#f87171';
      case 'pendiente': return '#fbbf24';
      default: return '#9ca3af';
    }
  }

  evaluarSolucion(solucionId: number, nuevoEstado: string) {
    console.log('Evaluando solución:', solucionId, 'estado:', nuevoEstado);
    
    this.api.evaluarSolucion(solucionId, nuevoEstado).subscribe({
      next: (data) => {
        console.log('Evaluación resultado:', data);
        this.mostrarMensaje(`Solución marcada como ${nuevoEstado}`, 'success');
        this.verTodasSoluciones();
      },
      error: (err) => {
        console.error('Error:', err);
        this.mostrarMensaje('Error al evaluar solución', 'error');
      }
    });
  }
}
