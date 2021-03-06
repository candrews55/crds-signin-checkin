﻿using System.Collections.Generic;
using System.Runtime.Serialization;
using Crossroads.Web.Common.MinistryPlatform;
using MinistryPlatform.Translation.Extensions;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace MinistryPlatform.Translation.Models.DTO
{
    [MpRestApiTable(Name = "Event_Groups")]
    public class MpEventGroupDto : MpBaseDto
    {
        [MpRestApiPrimaryKey("Event_Group_ID")]
        [JsonProperty("Event_Group_ID")]
        public int Id { get; set; }
        [JsonIgnore]
        public MpEventDto Event { get; set; } = new MpEventDto();
        [JsonIgnore]
        public MpGroupDto Group { get; set; } = new MpGroupDto();
        [JsonIgnore]
        public MpEventRoomDto RoomReservation { get; set; } = new MpEventRoomDto();

        [JsonProperty("Event_ID")]
        public int EventId
        {
            get { return Event.EventId; }
            set { Event.EventId = value; RoomReservation.EventId = value; }
        }

        [JsonProperty("Group_ID")]
        public int GroupId
        {
            get { return Group.Id; }
            set { Group.Id = value; }
        }

        [JsonProperty("Event_Room_ID")]
        public int? RoomReservationId
        {
            get { return RoomReservation.EventRoomId; }
            set { RoomReservation.EventRoomId = value; }
        }

        [JsonProperty("Room_ID", NullValueHandling = NullValueHandling.Ignore)]
        public int? RoomId
        {
            get { return RoomReservation.RoomId == 0 ? (int?)null : RoomReservation.RoomId; }
            set { RoomReservation.RoomId = value ?? 0; }
        }

        [JsonProperty("Signed_In", NullValueHandling = NullValueHandling.Ignore)]
        public int? SignedIn
        {
            get { return RoomReservation.SignedIn == 0 ? (int?)null : RoomReservation.SignedIn; }
            set { RoomReservation.SignedIn = value ?? 0; }
        }

        [JsonProperty("Checked_In", NullValueHandling = NullValueHandling.Ignore)]
        public int? CheckedIn
        {
            get { return RoomReservation.CheckedIn == 0 ? (int?)null : RoomReservation.CheckedIn; }
            set { RoomReservation.CheckedIn = value ?? 0; }
        }

        public bool HasRoomReservation()
        {
            return RoomReservation != null && RoomReservation.RoomId > 0;
        }

        protected override void ProcessUnmappedData(IDictionary<string, JToken> unmappedData, StreamingContext context)
        {
            RoomReservation.Label = unmappedData.GetUnmappedDataField<string>("Label");
            RoomReservation.AllowSignIn = unmappedData.GetUnmappedDataField<bool>("Allow_Checkin");
            RoomReservation.Capacity = unmappedData.GetUnmappedDataField<int>("Capacity");
            RoomReservation.Volunteers = unmappedData.GetUnmappedDataField<int>("Volunteers");
        }
    }
}
