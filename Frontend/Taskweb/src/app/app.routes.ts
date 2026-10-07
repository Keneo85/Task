import { Routes } from '@angular/router';
import { authGuard } from './auth';

// Lazy loading: cada pantalla se descarga solo cuando se visita
export const routes: Routes = [
  { path: 'login', loadComponent: () => import('./login').then((m) => m.Login) },
  {
    path: 'tasks',
    loadComponent: () => import('./tasks').then((m) => m.Tasks),
    canActivate: [authGuard],
  },
  { path: '**', redirectTo: 'tasks' },
];
