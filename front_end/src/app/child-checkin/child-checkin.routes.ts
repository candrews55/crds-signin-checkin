import { ModuleWithProviders } from '@angular/core';
import { Routes, RouterModule } from '@angular/router';

import { ChildCheckinComponent } from './child-checkin.component';
import { SearchComponent } from './search';
import { ResultsComponent } from './results';
import { AssignmentComponent } from './assignment';

const childCheckinRoutes: Routes = [
  {
    path: 'child-checkin',
    component: ChildCheckinComponent,
    children: [
      {
        path: '',
        component: SearchComponent
      },
      {
        path: 'results',
        component: ResultsComponent
      },
      {
        path: 'assignment',
        component: AssignmentComponent
      }
    ]
  }
];

export const childCheckinRouting: ModuleWithProviders = RouterModule.forChild(childCheckinRoutes);
