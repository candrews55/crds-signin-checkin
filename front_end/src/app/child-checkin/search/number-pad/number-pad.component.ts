import { Component, Output, EventEmitter } from '@angular/core';

@Component({
  selector: 'number-pad',
  templateUrl: 'number-pad.component.html',
  styleUrls: ['../../scss/_number-pad.scss', ]
})
export class NumberPadComponent {
  @Output() addNumber: EventEmitter<any> = new EventEmitter();
  @Output() deleteNumber: EventEmitter<any> = new EventEmitter();
  @Output() clearNumber: EventEmitter<any> = new EventEmitter();

  constructor() { }

  setNumber(num: number): void {
    this.addNumber.emit(num);
  }

  delete(): void {
    this.deleteNumber.emit();
  }

  clear(): void {
    this.clearNumber.emit();
  }
}
