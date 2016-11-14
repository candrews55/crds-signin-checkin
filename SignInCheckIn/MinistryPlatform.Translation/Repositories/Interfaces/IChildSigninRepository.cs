﻿using System.Collections.Generic;
using MinistryPlatform.Translation.Models.DTO;

namespace MinistryPlatform.Translation.Repositories.Interfaces
{
    public interface IChildSigninRepository
    {
        List<MpParticipantDto> GetChildrenByPhoneNumber(string phoneNumber);
        List<MpEventParticipantDto> CreateEventParticipants(List<MpEventParticipantDto> mpEventParticipantDtos);
    }
}
