import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';

@Injectable({
  providedIn: 'root'
})
export class ApiService {
  private apiUrl = '/api';

  private publicEndpoints = [
    '/api/Usuarios/login',
    '/api/Usuarios/register',
    '/api/Usuarios/ranking',
    '/api/Retos'
  ];

  private getAuthHeaders(): { [key: string]: string } {
    const token = localStorage.getItem('token');
    const isPublic = this.publicEndpoints.some(endpoint => 
      this.apiUrl.includes(endpoint)
    );
    
    console.log('ApiService getAuthHeaders - Token:', !!token, 'URL pública:', isPublic);
    
    if (token && !isPublic) {
      return { 'Authorization': `Bearer ${token}` };
    }
    return {};
  }

  constructor(private http: HttpClient) {
    console.log('ApiService inicializado con HttpClient');
  }

  getRetos(dificultad?: string, ordenarPor?: string): Observable<any[]> {
    let params = '';
    if (dificultad) params += `dificultad=${dificultad}&`;
    if (ordenarPor) params += `ordenarPor=${ordenarPor}`;
    return this.http.get<any[]>(`${this.apiUrl}/Retos?${params}`);
  }

  getRetosConEstado(idUsuario: number, dificultad?: string, ordenarPor?: string): Observable<any[]> {
    let params = '';
    if (dificultad) params += `dificultad=${dificultad}&`;
    if (ordenarPor) params += `ordenarPor=${ordenarPor}`;
    return this.http.get<any[]>(`${this.apiUrl}/Retos/con-estado/${idUsuario}?${params}`);
  }

  getReto(id: number): Observable<any> {
    return this.http.get<any>(`${this.apiUrl}/Retos/${id}`);
  }

  crearReto(data: { idUsuario: number; titulo: string; descripcion: string; dificultad: string; puntos: number }): Observable<any> {
    return this.http.post(`${this.apiUrl}/Retos`, data, { headers: this.getAuthHeaders() });
  }

  actualizarReto(id: number, data: { titulo: string; descripcion: string; dificultad: string; puntos: number }): Observable<any> {
    console.log('actualizarReto - headers:', this.getAuthHeaders());
    return this.http.put(`${this.apiUrl}/Retos/${id}`, { idReto: id, ...data }, { headers: this.getAuthHeaders() });
  }

  eliminarReto(id: number): Observable<any> {
    return this.http.delete(`${this.apiUrl}/Retos/${id}`, { headers: this.getAuthHeaders() });
  }

  getRanking(): Observable<any[]> {
    return this.http.get<any[]>(`${this.apiUrl}/Usuarios/ranking`);
  }

  getSolucionesPorReto(idReto: number): Observable<any[]> {
    return this.http.get<any[]>(`${this.apiUrl}/Soluciones/por-reto/${idReto}`, { headers: this.getAuthHeaders() });
  }

  getTodasLasSoluciones(): Observable<any[]> {
    return this.http.get<any[]>(`${this.apiUrl}/Soluciones/todas`, { headers: this.getAuthHeaders() });
  }

  evaluarSolucion(id: number, estado: string): Observable<any> {
    return this.http.post(`${this.apiUrl}/Soluciones/evaluar/${id}`, { Estado: estado }, { headers: this.getAuthHeaders() });
  }

  login(username: string, password: string): Observable<any> {
    return this.http.post(`${this.apiUrl}/Usuarios/login`, { username, password });
  }

  getEstadisticas(idUsuario: number): Observable<any> {
    return this.http.get<any>(`${this.apiUrl}/Usuarios/${idUsuario}/estadisticas`, { headers: this.getAuthHeaders() });
  }

  getHistorial(idUsuario: number): Observable<any[]> {
    return this.http.get<any[]>(`${this.apiUrl}/Usuarios/${idUsuario}/historial`, { headers: this.getAuthHeaders() });
  }

  getRetosCompletados(idUsuario: number): Observable<any[]> {
    return this.http.get<any[]>(`${this.apiUrl}/Usuarios/${idUsuario}/retos-completados`, { headers: this.getAuthHeaders() });
  }

  enviarSolucion(idUsuario: number, idReto: number, codigo: string, descripcion?: string): Observable<any> {
    return this.http.post(`${this.apiUrl}/Soluciones/enviar`, {
      idUsuario,
      idReto,
      codigo,
      descripcion
    }, { headers: this.getAuthHeaders() });
  }
}