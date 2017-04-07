import { Component, OnInit } from '@angular/core';
import { ActivatedRoute, Router } from '@angular/router';
import { NgForm } from '@angular/forms';

import { ApiService } from '../../../../../shared/services';
import { HeaderService } from '../../../../header/header.service';
import { AdminService } from '../../../../admin.service';
import { Household } from '../../../../../shared/models';

@Component({
  selector: 'household-edit',
  templateUrl: 'household-edit.component.html'
})
export class HouseholdEditComponent implements OnInit {
  private maskPhoneNumber: any = [/[1-9]/, /\d/, /\d/, '-', /\d/, /\d/, /\d/, '-', /\d/, /\d/, /\d/, /\d/];
  private processing: boolean;
  private eventId: number;
  private householdId: number;
  private household: Household = new Household();

  constructor( private apiService: ApiService,
               private adminService: AdminService,
               private route: ActivatedRoute,
               private headerService: HeaderService) {}

 ngOnInit() {
   this.processing = false;
   this.eventId = +this.route.snapshot.params['eventId'];
   this.householdId = +this.route.snapshot.params['householdId'];

   this.apiService.getEvent(String(this.eventId)).subscribe((event) => {
     this.headerService.announceEvent(event);
   }, error => console.error(error));

   this.adminService.getHouseholdInformation(this.householdId).subscribe((household) => {
      this.household = household;
   }, error => console.error(error));
 }

  onPhoneBlur(e, household) {
    try {
      if (household.HomePhone.indexOf('_') > -1) {
        e.target.value = '';
        household.HomePhone = undefined;
      }
    } catch (e) { }
  }
}
