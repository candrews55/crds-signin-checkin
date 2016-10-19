import { Component } from '@angular/core';
import { Router } from '@angular/router';

@Component({
  selector: 'search',
  templateUrl: 'search.component.html'
})
export class SearchComponent {
  private phoneNumber: string = '';
  private error: boolean = false;

  constructor(private router: Router) {}

  setPhoneNumber(num: string) {
    if (this.phoneNumber.length < 10) {
      this.phoneNumber = `${this.phoneNumber}${num}`;
    }
  }

  delete(): void {
    this.phoneNumber = this.phoneNumber.slice(0, this.phoneNumber.length - 1);
  }

  clear(): void {
    this.phoneNumber = '';
  }

  next(): void {
    if (this.phoneNumber.length === 10) {
      this.error = false;
      this.router.navigate(['/child-checkin/results']);
    } else {
      this.error = true;
    }
  }
}
