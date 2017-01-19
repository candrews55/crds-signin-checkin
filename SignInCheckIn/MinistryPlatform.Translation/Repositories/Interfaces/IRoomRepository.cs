﻿using System.Collections.Generic;
using MinistryPlatform.Translation.Models.DTO;

namespace MinistryPlatform.Translation.Repositories.Interfaces
{
    public interface IRoomRepository
    {
        List<MpEventRoomDto> GetRoomsForEvent(int eventId, int locationId);
        List<MpEventRoomDto> GetRoomsForEvent(List<int> eventId, int locationId);

        MpEventRoomDto CreateOrUpdateEventRoom(string authenticationToken, MpEventRoomDto eventRoom);

        MpEventRoomDto GetEventRoom(int eventId, int roomId);

        MpRoomDto GetRoom(int roomId);

        List<MpBumpingRuleDto> GetBumpingRulesByRoomId(int fromRoomId);

        List<MpRoomDto> GetAvailableRoomsBySite(int locationId);

        void DeleteBumpingRules(string authenticationToken, IEnumerable<int> ruleIds);

        void CreateBumpingRules(string authenticationToken, List<MpBumpingRuleDto> bumpingRules);

        List<MpBumpingRuleDto> GetBumpingRulesForEventRooms(List<int?> eventRoomIds, int? fromEventRoomId);

        MpEventRoomDto GetEventRoomForEventMaps(List<int> eventIds, int roomId);

        List<MpBumpingRoomsDto> GetBumpingRoomsForEventRoom(int eventId, int fromEventRoomId);
    }
}
