import { Observable } from 'rxjs';
import { HouseholdEditComponent } from './household-edit.component';
import { ActivatedRoute, ActivatedRouteSnapshot, Router } from '@angular/router';
import { Household, State, Country } from '../../../../../shared/models';

const eventId = 4335;
const householdId = 1231;
const participantId = 6542;

let apiService = jasmine.createSpyObj('apiService', ['getEvent']);
let adminService = jasmine.createSpyObj('adminService', ['getHouseholdInformation', 'getStates', 'getCountries']);
let headerService = jasmine.createSpyObj('headerService', ['announceEvent']);
let router = jasmine.createSpyObj<Router>('router', ['navigate']);
let route: ActivatedRoute = new ActivatedRoute();
route.snapshot = new ActivatedRouteSnapshot();
route.snapshot.params = {
  eventId: eventId,
  householdId: householdId
};

let household = new Household();
household.HouseholdName = 'test';

let states: Array<State> = [];
states.push(new State());
states[0].StateId = 123;

let countries: Array<Country> = [];
countries.push(new Country());
countries[0].CountryId = 123;

let fixture;

describe('HouseholdComponent', () => {
  beforeEach(() => {
    (<jasmine.Spy>(apiService.getEvent)).and.returnValue(Observable.of());
    (<jasmine.Spy>(adminService.getHouseholdInformation)).and.returnValue(Observable.of(household));
    (<jasmine.Spy>(adminService.getStates)).and.returnValue(Observable.of(states));
    (<jasmine.Spy>(adminService.getCountries)).and.returnValue(Observable.of(countries));
    fixture = new HouseholdEditComponent(apiService, adminService, route, headerService);
  });

  describe('#ngOnInit', () => {
    it('should initialize data', () => {
      fixture.ngOnInit();
      expect(fixture.household.HouseholdName).toEqual(household.HouseholdName);
      expect(fixture.states[0].StateId).toEqual(states[0].StateId);
      expect(fixture.countries[0].CountryId).toEqual(countries[0].CountryId);
    });
  });
});
