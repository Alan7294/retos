import { Routes } from '@angular/router';
import { Presentacion } from './pages/presentacion/presentacion';
import { Login } from './pages/login/login';
import { Register } from './pages/register/register';
import { Dashboard } from './pages/dashboard/dashboard';
import { Retos } from './pages/retos/retos';
import { DetalleReto } from './pages/detalle-reto/detalle-reto';
import { Rankings } from './pages/rankings/rankings';
import { Perfil } from './pages/perfil/perfil';
import { Admin } from './pages/admin/admin';
import { authGuard } from './guards/auth.guard';

export const routes: Routes = [
  { path: 'presentacion', component: Presentacion },
  { path: 'login', component: Login },
  { path: 'register', component: Register },
  { path: 'dashboard', canActivate: [authGuard], component: Dashboard },
  { path: 'retos', canActivate: [authGuard], component: Retos },
  { path: 'reto/:id', canActivate: [authGuard], component: DetalleReto },
  { path: 'rankings', canActivate: [authGuard], component: Rankings },
  { path: 'perfil', canActivate: [authGuard], component: Perfil },
  { path: 'admin', canActivate: [authGuard], component: Admin },
  { path: '', redirectTo: 'presentacion', pathMatch: 'full' }
];
