﻿using Crossroads.Utilities.Services.Interfaces;
using Crossroads.Web.Common.MinistryPlatform;
using MinistryPlatform.Translation.Models.DTO;
using MinistryPlatform.Translation.Repositories.Interfaces;
using System.Collections.Generic;
using System.Linq;

namespace MinistryPlatform.Translation.Repositories
{
    public class GroupRepository : IGroupRepository
    {
        private readonly IMinistryPlatformRestRepository _ministryPlatformRestRepository;
        private readonly IApiUserRepository _apiUserRepository;
        private readonly List<string> _attributeColumns;
        private readonly List<string> _groupColumns;
        private readonly List<string> _groupAttributeColumns;
        private readonly IApplicationConfiguration _applicationConfiguration;

        public GroupRepository(IApiUserRepository apiUserRepository, IMinistryPlatformRestRepository ministryPlatformRestRepository, IApplicationConfiguration applicationConfiguration)
        {
            _ministryPlatformRestRepository = ministryPlatformRestRepository;
            _apiUserRepository = apiUserRepository;

            _attributeColumns = new List<string>
            {
                "Attribute_ID_Table.[Attribute_ID]",
                "Attribute_ID_Table.[Attribute_Name]",
                "Attribute_ID_Table.[Sort_Order]",
                "Attribute_ID_Table_Attribute_Type_ID_Table.[Attribute_Type_ID]",
                "Attribute_ID_Table_Attribute_Type_ID_Table.[Attribute_Type]"
            };

            _groupColumns = new List<string>
            {
                "Groups.Group_ID",
                "Groups.Group_Name"
            };

            _groupAttributeColumns = new List<string>
            {
                "Group_Attributes.Group_ID",
                "Group_ID_Table.Group_Name"
            };
            _groupAttributeColumns.AddRange(_attributeColumns);

            _applicationConfiguration = applicationConfiguration;
        }

        public MpGroupDto GetGroup(string authenticationToken, int groupId, bool includeAttributes = false)
        {
            var token = authenticationToken ?? _apiUserRepository.GetApiClientToken("CRDS.Service.SignCheckIn");
            var group = _ministryPlatformRestRepository.UsingAuthenticationToken(token).Get<MpGroupDto>(groupId, _groupColumns);
            return SetKidsClubGroupAttributes(group, includeAttributes, token);
        }

        public List<MpGroupDto> GetGroups(IEnumerable<int> groupIds, bool includeAttributes = false)
        {
            var token = _apiUserRepository.GetApiClientToken("CRDS.Service.SignCheckIn");
            var searchString = $"Group_ID IN ({string.Join(",", groupIds)})";
            var groups = _ministryPlatformRestRepository.UsingAuthenticationToken(token).Search<MpGroupDto>(searchString, _groupColumns);

            return groups.Select(g => SetKidsClubGroupAttributes(g, includeAttributes, token)).ToList();
        }

        public List<MpGroupDto> GetGroupsByAttribute(IEnumerable<MpAttributeDto> attributes, bool includeAttributes = false)
        {
            var attributesList = attributes.ToList();
            var token = _apiUserRepository.GetApiClientToken("CRDS.Service.SignCheckIn");
            var searchString = string.Empty;
            var first = true;
            foreach (var typeId in attributesList.Select(a => a.Type.Id).Distinct())
            {
                if (!first)
                {
                    searchString = $"{searchString} OR ";
                }
                else
                {
                    first = false;
                }

                searchString = $"{searchString} (Attribute_ID_Table_Attribute_Type_ID_Table.Attribute_Type_ID = {typeId} AND Group_Attributes.Attribute_ID IN ";
                searchString = searchString + "(" + string.Join(",", attributesList.FindAll(a => a.Type.Id == typeId).Select(a => a.Id).ToList()) + ") AND Group_ID_Table_Parent_Group_Table.[Group_ID] IS NULL)";
            }

            var groups = _ministryPlatformRestRepository.UsingAuthenticationToken(token).SearchTable<MpGroupDto>("Group_Attributes", searchString, _groupAttributeColumns);
            var response = groups.Select(g => SetKidsClubGroupAttributes(g, includeAttributes, token)).ToList();
            return response;
        }

        public List<MpGroupDto> GetGroupsForParticipantId(int participantId)
        {
            var token = _apiUserRepository.GetApiClientToken("CRDS.Service.SignCheckIn");

            var mpGroupDtos = _ministryPlatformRestRepository.UsingAuthenticationToken(token)
                .SearchTable<MpGroupDto>("Group_Participants", $"Participant_ID_Table.[Participant_ID]={participantId} AND End_Date IS NULL", "Group_ID_Table.[Group_ID]");

            return mpGroupDtos;
        }

        private MpGroupDto SetKidsClubGroupAttributes(MpGroupDto group, bool includeAttributes, string token)
        {
            if (!includeAttributes)
            {
                return group;
            }

            var attributes = _ministryPlatformRestRepository.UsingAuthenticationToken(token)
                .SearchTable<MpAttributeDto>("Group_Attributes", $"Group_Attributes.Group_ID = {group.Id}", _attributeColumns);
            if (attributes == null || !attributes.Any())
            {
                return group;
            }
            group.AgeRange = attributes.Find(a => a.Type.Id == _applicationConfiguration.AgesAttributeTypeId);
            group.Grade = attributes.Find(a => a.Type.Id == _applicationConfiguration.GradesAttributeTypeId);
            group.BirthMonth = attributes.Find(a => a.Type.Id == _applicationConfiguration.BirthMonthsAttributeTypeId);
            group.NurseryMonth = attributes.Find(a => a.Type.Id == _applicationConfiguration.NurseryAgesAttributeTypeId);

            return group;

        }
    }
}
