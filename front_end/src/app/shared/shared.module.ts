import { NgModule } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { Ng2BootstrapModule } from 'ng2-bootstrap/ng2-bootstrap';

import { PhoneNumberPipe } from './pipes/phoneNumber.pipe';
import { OneBasedIndexPipe } from './pipes/one-based-index.pipe';
import { MonthNamePipe } from './pipes/monthName.pipe';
import { DashedPipe } from './pipes/dashed.pipe';
import { ClosedRoomPipe } from './pipes/closedRoom.pipe';
import { ApiService, LoginRedirectService } from './services';

import { PreloaderModule } from './preloader';
import { CrdsDatePickerModule, LoadingButtonModule, NumberPadModule } from './components';
import { CanActivateIfLoggedInGuard } from './guards';

@NgModule({
  imports: [
    CommonModule,
  ],
  declarations: [
    PhoneNumberPipe,
    OneBasedIndexPipe,
    MonthNamePipe,
    DashedPipe,
    ClosedRoomPipe
  ],
  exports: [
    PhoneNumberPipe,
    OneBasedIndexPipe,
    MonthNamePipe,
    DashedPipe,
    ClosedRoomPipe,
    CommonModule,
    FormsModule,
    Ng2BootstrapModule,
    PreloaderModule,
    CrdsDatePickerModule,
    LoadingButtonModule,
    NumberPadModule
  ],
  providers:  [
    ApiService,
    CanActivateIfLoggedInGuard,
    LoginRedirectService
  ]
})
export class SharedModule {
}
