import { Injectable, EventEmitter } from '@angular/core';
import { Observable } from 'rxjs/Observable';
import '../rxjs-operators';
import { HttpClientService } from '../shared/services';
import { Room, NewFamily, Child } from '../shared/models';

@Injectable()
export class AdminService {
  site: number;

  constructor(private http: HttpClientService) {}

  getRooms(eventId: string) {
    const url = `${process.env.ECHECK_API_ENDPOINT}/events/${eventId}/rooms`;
    return this.http.get(url)
                    .map(res => (<any[]>res.json()).map(r => Room.fromJson(r)))
                    .catch(this.handleError);
  }

  getBumpingRooms(eventId: string, roomId: string) {
    const url = `${process.env.ECHECK_API_ENDPOINT}/events/${eventId}/rooms/${roomId}/bumping`;
    return this.http.get(url)
                    .map(res => res.json())
                    .catch(this.handleError);
  }

  updateBumpingRooms(eventId: string, roomId: string, rooms: Room[]) {
    const url = `${process.env.ECHECK_API_ENDPOINT}/events/${eventId}/rooms/${roomId}/bumping`;
    return this.http.post(url, rooms)
                    .map(res => res.json())
                    .catch(this.handleError);
  }

  updateRoom(eventId: string, roomId: string, body: Room) {
    const url = `${process.env.ECHECK_API_ENDPOINT}/events/${eventId}/rooms/${roomId}`;
    return this.http.put(url, body)
                    .map(res => res.json())
                    .catch(this.handleError);
  }

  getRoomGroups(eventId: string, roomId: string) {
    const url = `${process.env.ECHECK_API_ENDPOINT}/events/${eventId}/rooms/${roomId}/groups`;
    return this.http.get(url)
                    .map(res => Room.fromJson(res.json()))
                    .catch(this.handleError);
  }

  updateRoomGroups(eventId: string, roomId: string, body: Room) {
    body.EventId = eventId;
    body.RoomId = roomId;

    return this.updateRoomGroupsInternal(body);
  }

  importEvent(destinationEventId: number, sourceEventId: number): Observable<Room[]> {
    const url = `${process.env.ECHECK_API_ENDPOINT}/events/${destinationEventId}/import/${sourceEventId}`;
    return this.http.put(url, null, null)
                    .map(res => { (<any[]>res.json()).map(r => Room.fromJson(r)); })
                    .catch(this.handleError);
  }

  createNewFamily(family: NewFamily) {
    const url = `${process.env.ECHECK_API_ENDPOINT}/signin/newfamily`;
    return this.http.post(url, family).map(res => { return res; }).catch(this.handleError);
  }

  getChildrenForEvent(eventId: number) {
    const url = `${process.env.ECHECK_API_ENDPOINT}/events/${eventId}/children`;
    return this.http.get(url)
                    .map(res => { return (<Child[]>res.json()).map(r => Child.fromJson(r)); })
                    .catch(this.handleError);
  }

  reprint(participantEventId: number) {
    const url = `${process.env.ECHECK_API_ENDPOINT}/signin/participant/${participantEventId}/print`;
    return this.http.post(url, {}).catch(this.handleError);
  }

  reverseSignin(eventParticipantId: number) {
    const url = `${process.env.ECHECK_API_ENDPOINT}/signin/reverse/${eventParticipantId}`;
    return this.http.put(url, null).catch(this.handleError);
  }

  private updateRoomGroupsInternal(room: Room) {
    const url = `${process.env.ECHECK_API_ENDPOINT}/events/${room.EventId}/rooms/${room.RoomId}/groups`;
    return this.http.put(url, room)
                    .map(res => Room.fromJson(res.json()))
                    .catch(this.handleError);
  }

  private handleError (error: any) {
    console.error(error);
    return Observable.throw(error || 'Server error');
  }
}
