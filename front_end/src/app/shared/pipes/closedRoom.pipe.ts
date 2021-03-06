import { Pipe, PipeTransform } from '@angular/core';

@Pipe({
  name: 'ClosedRoomPipe'
})

export class ClosedRoomPipe implements PipeTransform {
  transform(value, allowSignIn: boolean) {
    return value.filter(room => {

      if (allowSignIn === true) {
        return room.AllowSignIn === allowSignIn;
      } else {
        return (room.AllowSignIn === true || room.AllowSignIn === false);
      }

    });
  }
}
