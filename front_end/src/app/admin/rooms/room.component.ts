import { Component, Input } from '@angular/core';
import { Router, ActivatedRoute, Params } from '@angular/router';
import { AdminService } from '../admin.service';
import { Room } from './room';

@Component({
  selector: '.room',
  templateUrl: 'room.component.html',
  styleUrls: ['room.component.scss']
})
export class RoomComponent {
  @Input() room: Room;

  // constructor() {
  //   console.log(this)
  // }

  // constructor(
  //   private route: ActivatedRoute,
  //   private adminService: AdminService,
  //   private service: AdminService) {}
  //
  // private getData(): void {
  //   const eventId = this.route.snapshot.params['eventId'];
  //   const roomId = this.route.snapshot.params['roomId'];
  //   this.adminService.getRoom(eventId, roomId).subscribe(
  //     room => {this.room = room},
  //     error => console.error(error)
  //   );
  // }
  ngOnInit(): void {
    // this.getData();
    console.log("room component oninit", this)
  }
}
