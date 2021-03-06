//using Crossroads.ApiVersioning;
using Crossroads.Web.Auth.Controllers;
using Crossroads.Web.Common.Security;
using Crossroads.Web.Common.Services;
using MinistryPlatform.Translation.Repositories.Interfaces;
using SignInCheckIn.Exceptions.Models;
using SignInCheckIn.Models.DTO;
using SignInCheckIn.Security;
using SignInCheckIn.Services.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Http;
using System.Web.Http.Description;

namespace SignInCheckIn.Controllers
{
    [RoutePrefix("api")]
    public class ChildSigninController : AuthBaseController
    {
        private readonly IWebsocketService _websocketService;
        private readonly IChildSigninService _childSigninService;
        private readonly IKioskRepository _kioskRepository;
        private readonly IContactRepository _contactRepository;
        private readonly IFamilyService _familyService;
        private const int KidsClubTools = 112;

        public ChildSigninController(IAuthTokenExpiryService authTokenExpiryService, IChildSigninService childSigninService, IWebsocketService websocketService, IAuthenticationRepository authenticationRepository, IKioskRepository kioskRepository, IContactRepository contactRepository, IFamilyService familyService) : base(authenticationRepository, authTokenExpiryService)
        {
            _websocketService = websocketService;
            _childSigninService = childSigninService;
            _kioskRepository = kioskRepository;
            _contactRepository = contactRepository;
            _familyService = familyService;
        }

        [HttpGet]
        [ResponseType(typeof(ParticipantEventMapDto))]
        //[VersionedRoute(template: "events/{eventId}/signin/children/household/{householdId}", minimumVersion: "1.0.0")]
        [Route("events/{eventId}/signin/children/household/{householdId}")]
        public IHttpActionResult GetChildrenAndEventByHousehold(int eventId, int householdId)
        {
            try
            {
                var siteId = 0;
                if (Request.Headers.Contains("Crds-Site-Id"))
                {
                    siteId = int.Parse(Request.Headers.GetValues("Crds-Site-Id").First());
                }

                if (siteId == 0)
                {
                    throw new Exception("Site Id is Invalid");
                }

                string kioskId = "";

                if (Request.Headers.Contains("Crds-Kiosk-Identifier"))
                {
                    kioskId = Request.Headers.GetValues("Crds-Kiosk-Identifier").First();
                }

                var children = _childSigninService.GetChildrenAndEventByHouseholdId(householdId, eventId, siteId, kioskId);
                return Ok(children);
            }
            catch (Exception e)
            {
                var apiError = new ApiErrorDto("Get Children", e);
                throw new HttpResponseException(apiError.HttpResponseMessage);
            }
        }

        [HttpGet]
        [ResponseType(typeof(ParticipantEventMapDto))]
        //[VersionedRoute(template: "signin/children/{phoneNumber}", minimumVersion: "1.0.0")]
        [Route("signin/children/{phoneNumber}")]
        public IHttpActionResult GetChildrenAndEvent(string phoneNumber)
        {
            try
            {
                var siteId = 0;
                if (Request.Headers.Contains("Crds-Site-Id"))
                {
                    siteId = int.Parse(Request.Headers.GetValues("Crds-Site-Id").First());
                }

                if (siteId == 0)
                {
                    throw new Exception("Site Id is Invalid");
                }

                string kioskId = "";

                if (Request.Headers.Contains("Crds-Kiosk-Identifier"))
                {
                    kioskId = Request.Headers.GetValues("Crds-Kiosk-Identifier").First();
                }

                var children = _childSigninService.GetChildrenAndEventByPhoneNumber(phoneNumber, siteId, null, false, kioskId);
                return Ok(children);
            }
            catch (Exception e)
            {
                var apiError = new ApiErrorDto("Get Children", e);
                throw new HttpResponseException(apiError.HttpResponseMessage);
            }
        }

        [HttpPost]
        [ResponseType(typeof(ParticipantEventMapDto))]
        //[VersionedRoute(template: "signin/children", minimumVersion: "1.0.0")]
        [Route("signin/children")]
        public IHttpActionResult SigninParticipants(ParticipantEventMapDto participantEventMapDto)
        {
            try
            {
                var participants = _childSigninService.SigninParticipants(participantEventMapDto);
                PublishSignedInParticipantsToRooms(participants);
                return Ok(participants);
            }
            catch (Exception e)
            {
                var apiError = new ApiErrorDto("Sign In Participants", e);
                throw new HttpResponseException(apiError.HttpResponseMessage);
            }
        }

        [HttpPost]
        [ResponseType(typeof(ParticipantEventMapDto))]
        //[VersionedRoute(template: "signin/familyfinder", minimumVersion: "1.0.0")]
        [Route("signin/familyfinder")]
        public IHttpActionResult SigninFamilyFinder(ParticipantEventMapDto participantEventMapDto)
        {
            try
            {
                string kioskIdentifier = "";
                if (Request.Headers.Contains("Crds-Kiosk-Identifier"))
                {
                    kioskIdentifier = Request.Headers.GetValues("Crds-Kiosk-Identifier").First();
                }

                var participants = _childSigninService.SigninParticipants(participantEventMapDto, true);
                PublishSignedInParticipantsToRooms(participants);
                participantEventMapDto.Participants = participants.Participants;
                _childSigninService.PrintParticipants(participantEventMapDto, kioskIdentifier);
                return Ok(participants);
            }
            catch (Exception e)
            {
                var apiError = new ApiErrorDto("Sign In Participants", e);
                throw new HttpResponseException(apiError.HttpResponseMessage);
            }
        }

        [HttpPost]
        [ResponseType(typeof(ParticipantEventMapDto))]
        //[VersionedRoute(template: "signin/participants/print", minimumVersion: "1.0.0")]
        [Route("signin/participants/print")]
        public IHttpActionResult PrintParticipants(ParticipantEventMapDto participantEventMapDto)
        {
            try
            {
                string kioskIdentifier;

                if (Request.Headers.Contains("Crds-Kiosk-Identifier"))
                {
                    kioskIdentifier = Request.Headers.GetValues("Crds-Kiosk-Identifier").First();
                }
                else
                {
                    throw new Exception("No kiosk identifier");
                }

                return Ok(_childSigninService.PrintParticipants(participantEventMapDto, kioskIdentifier));
            }
            catch (Exception e)
            {
                var apiError = new ApiErrorDto("Print Participants", e);
                throw new HttpResponseException(apiError.HttpResponseMessage);
            }
        }

        [HttpPost]
        [ResponseType(typeof(ParticipantEventMapDto))]
        //[VersionedRoute(template: "signin/participant/{eventParticipantId}/print", minimumVersion: "1.0.0")]
        [Route("signin/participant/{eventParticipantId}/print")]
        public IHttpActionResult PrintParticipant(int eventParticipantId)
        {
            return Authorized(authDto =>
            {
                string kioskIdentifier;

                // make sure kiosk is admin type and configured for printing
                if (Request.Headers.Contains("Crds-Kiosk-Identifier"))
                {
                    kioskIdentifier = Request.Headers.GetValues("Crds-Kiosk-Identifier").First();
                    var kioskConfig = _kioskRepository.GetMpKioskConfigByIdentifier(Guid.Parse(kioskIdentifier));
                    // must be kiosk type admin and have a printer set up
                    if (kioskConfig.PrinterMapId == null || kioskConfig.KioskTypeId != 3)
                    {
                        throw new HttpResponseException(System.Net.HttpStatusCode.PreconditionFailed);
                    }
                }
                else
                {
                    throw new HttpResponseException(System.Net.HttpStatusCode.PreconditionFailed);
                }

                try
                {
                    VerifyRoles.KidsClubTools(authDto);

                    return Ok(_childSigninService.PrintParticipant(eventParticipantId, kioskIdentifier));
                }
                catch (Exception e)
                {
                    var apiError = new ApiErrorDto("Print Participants", e);
                    throw new HttpResponseException(apiError.HttpResponseMessage);
                }
            });
        }

        [HttpPut]
        [ResponseType(typeof(ParticipantEventMapDto))]
        //[VersionedRoute(template: "signin/event/{eventId}/room/{roomId}/reverse/{eventparticipantId}", minimumVersion: "1.0.0")]
        [Route("signin/event/{eventId}/room/{roomId}/reverse/{eventparticipantId}")]
        public IHttpActionResult ReverseSignin(int eventId, int roomId, int eventparticipantId)
        {
            return Authorized(authDto =>
            {
                try
                {
                    VerifyRoles.KidsClubTools(authDto);

                    var reverseSuccess = _childSigninService.ReverseSignin(eventparticipantId);

                    if (reverseSuccess == true)
                    {
                        var data = new ParticipantDto();
                        data.EventParticipantId = eventparticipantId;
                        data.OriginalRoomId = roomId;

                        _websocketService.PublishCheckinParticipantsSignedInRemove(eventId, roomId, data);
                        return Ok();
                    }
                    else
                    {
                        return Conflict();
                    }
                }
                catch (Exception e)
                {
                    var apiError = new ApiErrorDto("Error reversing signin for event participant ", e);
                    throw new HttpResponseException(apiError.HttpResponseMessage);
                }
            });
        }

        private void PublishSignedInParticipantsToRooms(ParticipantEventMapDto participants)
        {
            foreach (var p in participants.Participants)
            {
                if (p.AssignedRoomId != null)
                {
                    // ignores the site id if there is an event id so therefore we can put a random 0 here
                    var updatedParticipants = participants.Participants.Where(pp => pp.AssignedRoomId == p.AssignedRoomId);
                    _websocketService.PublishCheckinParticipantsAdd(p.EventId, p.AssignedRoomId.Value, new List<ParticipantDto>() { p });
                }

                if (p.AssignedSecondaryRoomId != null)
                {
                    // ignores the site id if there is an event id so therefore we can put a random 0 here
                    var updatedParticipants = participants.Participants.Where(pp => pp.AssignedSecondaryRoomId == p.AssignedSecondaryRoomId);
                    _websocketService.PublishCheckinParticipantsAdd(p.EventIdSecondary, p.AssignedSecondaryRoomId.Value, new List<ParticipantDto>() { p });
                }
            }
        }
    }
}
