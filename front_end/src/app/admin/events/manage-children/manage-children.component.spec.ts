import { ManageChildrenComponent } from './manage-children.component';
import { ActivatedRoute, ActivatedRouteSnapshot, Router } from '@angular/router';
import { Child } from '../../../shared/models';
import { HeaderService } from '../../header/header.service';
import { ApiService, RootService } from '../../../shared/services';
import { AdminService } from '../../admin.service';
import { Observable } from 'rxjs';

describe('ManageChildrenComponent', () => {
  let fixture: ManageChildrenComponent;
  let eventId: 54321;

  let route: ActivatedRoute;

  let apiService: ApiService;
  let adminService: AdminService;
  let headerService: HeaderService;
  let router: Router;
  let rootService: RootService;

  beforeEach(() => {
    // Can't jasmine spy on properties, so have to create a stub for route.snapshot.params
    route = new ActivatedRoute();
    route.snapshot = new ActivatedRouteSnapshot();
    route.snapshot.params = {
      eventId: eventId
    };

    adminService = <AdminService>jasmine.createSpyObj('adminService', ['reverseSignin', 'getChildrenForEvent']);
    apiService = <ApiService>jasmine.createSpyObj('apiService', ['getEvent']);
    headerService = <HeaderService>jasmine.createSpyObj('headerService', ['announceEvent']);
    router = <Router>jasmine.createSpyObj('router', ['navigate']);
    rootService = <RootService>jasmine.createSpyObj('rootService', ['announceEvent']);
    fixture = new ManageChildrenComponent(route, apiService, headerService, rootService, adminService, router);
  });

  describe('#reverseSignin', () => {
    it('should sign out a child', () => {
      let children: Child[] = [ new Child(), new Child() ];
      children[0].EventParticipantId = 123;
      children[1].EventParticipantId = 456;
      children[1].AssignedRoomId = 456;
      fixture.childrenByRoom = children;
      fixture.eventId = 20;
      let eventParticipantId = children[1].EventParticipantId;
      let roomId = children[1].AssignedRoomId;

      (<jasmine.Spy>(adminService.reverseSignin)).and.returnValue(Observable.of());
      fixture.reverseSignin(children[1]);

      expect(adminService.reverseSignin).toHaveBeenCalledWith(fixture.eventId, roomId, eventParticipantId);
      expect(fixture.childrenByRoom[1] === undefined);
    });
  });

  describe('#executeSearch', () => {
    it('should call the backend', () => {
      fixture.eventId = 433;
      fixture.searchString = 'Bluiett';
      (<jasmine.Spy>(adminService.getChildrenForEvent)).and.returnValue(Observable.of());
      fixture.onSearch();

      expect(adminService.getChildrenForEvent).toHaveBeenCalledWith(fixture.eventId, fixture.searchString);
    });
  });

  describe('#children', () => {
    it('should be sorted', () => {
      let unsortedChildren: Array<any> = [];
      unsortedChildren.push({KCSortOrder: 4, AssignedRoomName: 'four'});
      unsortedChildren.push({KCSortOrder: 2, AssignedRoomName: 'two'});
      unsortedChildren.push({KCSortOrder: 1, AssignedRoomName: 'one'});
      unsortedChildren.push({KCSortOrder: 3, AssignedRoomName: 'three'});

      fixture.children = unsortedChildren;

      expect(fixture.children[0].AssignedRoomName).toBe('one');
      expect(fixture.children[1].AssignedRoomName).toBe('two');
      expect(fixture.children[2].AssignedRoomName).toBe('three');
      expect(fixture.children[3].AssignedRoomName).toBe('four');
    });
  });
});
