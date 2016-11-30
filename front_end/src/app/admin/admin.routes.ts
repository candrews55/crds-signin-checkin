import { ModuleWithProviders } from '@angular/core';
import { Routes, RouterModule } from '@angular/router';

import { AdminComponent } from './admin.component';
import { SignInComponent } from './sign-in';
import { GuestCheckInComponent } from './guest-checkin';

import { eventsRoutes } from './events/events.routes';

import { CanActivateIfLoggedInGuard } from '../shared/guards';

const adminRoutes: Routes = [
  {
    path: 'admin', component: AdminComponent,
    children: [
      { path: 'dashboard', redirectTo: 'events' },
      { path: 'sign-in', component: SignInComponent },
      { path: 'events', children: [...eventsRoutes], canActivate: [CanActivateIfLoggedInGuard] },
      { path: 'guest-checkin', component: GuestCheckInComponent }
    ]
  }
];

export const adminRouting: ModuleWithProviders = RouterModule.forChild(adminRoutes);
