import { Routes } from '@angular/router';
import { authGuard } from './core/auth.guard';

export const routes: Routes = [
    { path: '', redirectTo: '/studio', pathMatch: 'full' },
    {
        path: 'login',
        loadComponent: () => import('./pages/login/login.component').then(m => m.LoginComponent)
    },
    {
        path: 'register',
        loadComponent: () => import('./pages/register/register.component').then(m => m.RegisterComponent)
    },
    {
        path: 'studio',
        loadComponent: () => import('./pages/studio/studio.component').then(m => m.StudioComponent),
        canActivate: [authGuard]
    },
    {
        path: 'gallery',
        loadComponent: () => import('./pages/gallery/gallery.component').then(m => m.GalleryComponent),
        canActivate: [authGuard]
    },
    {
        path: 'share/:token',
        loadComponent: () => import('./pages/share/share.component').then(m => m.ShareComponent)
    },
    { path: '**', redirectTo: '/studio' }
];
