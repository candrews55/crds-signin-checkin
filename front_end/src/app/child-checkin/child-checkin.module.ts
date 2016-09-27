import { NgModule } from '@angular/core';
import { SharedModule } from '../shared/shared.module';

import { ChildCheckinComponent } from './child-checkin.component';
import { SearchComponent } from './search';
import { ResultsComponent } from './results';
import { AssignmentComponent } from './assignment';
import { childCheckinRouting } from './child-checkin.routes';

@NgModule({
  declarations: [
    ChildCheckinComponent,
    SearchComponent,
    ResultsComponent,
    AssignmentComponent,
  ],
  imports: [
    SharedModule,
    childCheckinRouting
  ],
})

export class ChildCheckinModule { }
