﻿using System;
using System.Linq;
using System.Web.Http;
using System.Web.Http.Description;
using SignInCheckIn.Exceptions.Models;
using SignInCheckIn.Models.DTO;
using SignInCheckIn.Services.Interfaces;

namespace SignInCheckIn.Controllers
{
    public class ChildCheckinController : ApiController
    {
        private readonly IChildCheckinService _childCheckinService;

        public ChildCheckinController(IChildCheckinService childCheckinService)
        {
            _childCheckinService = childCheckinService;
        }

        [HttpGet]
        [ResponseType(typeof(ParticipantEventMapDto))]
        [Route("checkin/children/{roomId}")]
        public IHttpActionResult GetCheckedInChildrenForEventAndRoom(int roomId, int? eventId)
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

                var children = _childCheckinService.GetChildrenForCurrentEventAndRoom(roomId, siteId, eventId);
                return Ok(children);
            }
            catch (Exception e)
            {
                var apiError = new ApiErrorDto("Get Children", e);
                throw new HttpResponseException(apiError.HttpResponseMessage);
            }
        }

        [HttpPut]
        [ResponseType(typeof(ParticipantEventMapDto))]
        [Route("checkin/event/participant/{eventParticipantId}/{checkIn}")]
        public IHttpActionResult CheckinChildrenForCurrentEventAndRoom(bool checkIn, int eventParticipantId)
        {
            try
            {
                _childCheckinService.CheckinChildrenForCurrentEventAndRoom(checkIn, eventParticipantId);
                return Ok();
            }
            catch (Exception e)
            {
                var apiError = new ApiErrorDto("Checking in Children", e);
                throw new HttpResponseException(apiError.HttpResponseMessage);
            }
        }
    }
}
