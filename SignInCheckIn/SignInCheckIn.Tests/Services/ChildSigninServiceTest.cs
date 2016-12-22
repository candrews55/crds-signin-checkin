﻿using System;
using System.Collections.Generic;
using System.Linq;
using Crossroads.Utilities.Services.Interfaces;
using MinistryPlatform.Translation.Models.DTO;
using MinistryPlatform.Translation.Repositories.Interfaces;
using Moq;
using NUnit.Framework;
using Printing.Utilities.Models;
using Printing.Utilities.Services.Interfaces;
using SignInCheckIn.App_Start;
using SignInCheckIn.Models.DTO;
using SignInCheckIn.Services;
using SignInCheckIn.Services.Interfaces;

namespace SignInCheckIn.Tests.Services
{
    public class ChildSigninServiceTest
    {
        private Mock<IChildSigninRepository> _childSigninRepository;
        private Mock<IEventRepository> _eventRepository;
        private Mock<IGroupRepository> _groupRepository;
        private Mock<IEventService> _eventService;
        private Mock<IPdfEditor> _pdfEditor;
        private Mock<IPrintingService> _printingService;
        private Mock<IContactRepository> _contactRepository;
        private Mock<IKioskRepository> _kioskRepository;
        private Mock<IParticipantRepository> _participantRepository;
        private Mock<IApplicationConfiguration> _applicationConfiguration;
        private Mock<IGroupLookupRepository> _groupLookupRepository;
        private Mock<IRoomRepository> _roomRepository;

        private ChildSigninService _fixture;

        [SetUp]
        public void SetUp()
        {
            AutoMapperConfig.RegisterMappings();

            _childSigninRepository = new Mock<IChildSigninRepository>(MockBehavior.Strict);
            _eventRepository = new Mock<IEventRepository>(MockBehavior.Strict);
            _groupRepository = new Mock<IGroupRepository>(MockBehavior.Strict);
            _eventService = new Mock<IEventService>(MockBehavior.Strict);
            _pdfEditor = new Mock<IPdfEditor>(MockBehavior.Strict);
            _printingService = new Mock<IPrintingService>(MockBehavior.Strict);
            _contactRepository = new Mock<IContactRepository>(MockBehavior.Strict);
            _kioskRepository = new Mock<IKioskRepository>(MockBehavior.Strict);
            _participantRepository = new Mock<IParticipantRepository>(MockBehavior.Strict);
            _applicationConfiguration = new Mock<IApplicationConfiguration>();
            _groupLookupRepository = new Mock<IGroupLookupRepository>();
            _roomRepository = new Mock<IRoomRepository>();

            _fixture = new ChildSigninService(_childSigninRepository.Object,_eventRepository.Object, 
                _groupRepository.Object, _eventService.Object, _pdfEditor.Object, _printingService.Object,
                _contactRepository.Object, _kioskRepository.Object, _participantRepository.Object,
                _applicationConfiguration.Object, _groupLookupRepository.Object, _roomRepository.Object);
        }

        [Test]
        public void ShouldGetChildrenByPhoneNumber()
        {
            const int siteId = 1;
            const string phoneNumber = "812-812-8877";
            int? primaryHouseholdId = 123;

            var eventDto = new EventDto();

            var mpHouseholdAndParticipants = new MpHouseholdParticipantsDto
            {
                HouseholdId = primaryHouseholdId.GetValueOrDefault(),
                Participants = new List<MpParticipantDto>
                {
                    new MpParticipantDto
                    {
                        ParticipantId = 12,
                        ContactId = 1443,
                        HouseholdId = primaryHouseholdId.GetValueOrDefault(),
                        HouseholdPositionId = 2,
                        FirstName = "First1",
                        LastName = "Last1",
                        DateOfBirth = new DateTime(),
                        PrimaryHousehold = true
                    }
                }
            };
 
            var contactDtos = new List<MpContactDto>();

            _childSigninRepository.Setup(mocked => mocked.GetChildrenByPhoneNumber(phoneNumber, true)).Returns(mpHouseholdAndParticipants);
            _contactRepository.Setup(m => m.GetHeadsOfHouseholdByHouseholdId(It.IsAny<int>())).Returns(contactDtos);
            _eventService.Setup(m => m.GetCurrentEventForSite(siteId)).Returns(eventDto);
            var result = _fixture.GetChildrenAndEventByPhoneNumber(phoneNumber, siteId, null);
            _childSigninRepository.VerifyAll();

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual(mpHouseholdAndParticipants.Participants[0].ParticipantId, result.Participants[0].ParticipantId);
            Assert.AreEqual(mpHouseholdAndParticipants.Participants[0].ContactId, result.Participants[0].ContactId);
        }

        [Test]
        public void GetChildrenByPhoneNumberShouldReturnNoParticipants()
        {
            const int siteId = 1;
            const string phoneNumber = "812-812-8877";
            int? primaryHouseholdId = 123;

            var mpHouseholdAndParticipants = new MpHouseholdParticipantsDto
            {
                HouseholdId = primaryHouseholdId.GetValueOrDefault(),
            };
            var eventDto = new EventDto();
            var contactDtos = new List<MpContactDto>();

            _childSigninRepository.Setup(mocked => mocked.GetChildrenByPhoneNumber(phoneNumber, true)).Returns(mpHouseholdAndParticipants);
            _contactRepository.Setup(m => m.GetHeadsOfHouseholdByHouseholdId(primaryHouseholdId.Value)).Returns(contactDtos);
            _eventService.Setup(m => m.GetCurrentEventForSite(siteId)).Returns(eventDto);
            var result = _fixture.GetChildrenAndEventByPhoneNumber(phoneNumber, siteId, null);
            _childSigninRepository.VerifyAll();

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual(false, result.Participants.Any());
        }

        public void GetChildrenByPhoneNumberShouldThrowExceptionIfHouseholdNotFound()
        {
            const int siteId = 1;
            const string phoneNumber = "812-812-8877";

            var mpHouseholdAndParticipants = new MpHouseholdParticipantsDto();

            _childSigninRepository.Setup(mocked => mocked.GetChildrenByPhoneNumber(phoneNumber, true)).Returns(mpHouseholdAndParticipants);
            try
            {
                _fixture.GetChildrenAndEventByPhoneNumber(phoneNumber, siteId, null);
                Assert.Fail("Expected exception was not thrown");
            }
            catch (ApplicationException e)
            {
                _childSigninRepository.VerifyAll();
                Assert.AreEqual($"Could not locate household for phone number {phoneNumber}", e.Message);
            }
        }

        [Test]
        public void ShouldSignInParticipants()
        {
            // Arrange
            var participantDtos = new List<ParticipantDto>
            {
                new ParticipantDto
                {
                    FirstName = "Child1First",
                    AssignedRoomId = 1234567,
                    AssignedRoomName = "TestRoom",
                    AssignedSecondaryRoomId = 2345678,
                    AssignedSecondaryRoomName = "TestSecondaryRoom",
                    ParticipantId = 111,
                    Selected = true
                }
            };

            var contactDtos = new List<ContactDto>
            {
                new ContactDto
                {
                    ContactId = 1234567,
                    LastName = "TestLast",
                    Nickname = "TestNickname"
                }
            };

            var eventDto = new EventDto
            {
                EventTitle = "test event",
                EventId = 321
            };

            var mpEventGroupDtos = new List<MpEventGroupDto>
            {
                new MpEventGroupDto
                {
                    RoomReservation = new MpEventRoomDto
                    {
                        AllowSignIn = true,
                        Capacity = 1,
                        CheckedIn = 2,
                        EventId = 3,
                        EventRoomId = null,
                        Hidden = true,
                        RoomId = 4,
                        RoomName = "name",
                        RoomNumber = "number",
                        SignedIn = 5,
                        Volunteers = 6
                    }
                }
            };

            var mpEventParticipantDtos = new List<MpEventParticipantDto>
            {
                new MpEventParticipantDto
                {
                    GroupId = 432,
                    RoomId = 4
                }
            };

            DateTime currentMpEventDtoStartTime = new DateTime(2016, 12, 1, 9, 0, 0);

            MpEventDto currentMpServiceEventDto = new MpEventDto
            {
                EventId = 1234567,
                ParentEventId = null,
                CongregationId = 8,
                EventTypeId = 123,
                EventStartDate = currentMpEventDtoStartTime
            };

            // current service event, current ac event, trailing service event
            List<MpEventDto> eventDtosBySite = new List<MpEventDto>()
            {
                currentMpServiceEventDto
            };

            _eventRepository.Setup(m => m.GetEvents(It.IsAny<DateTime>(), It.IsAny<DateTime>(), It.IsAny<int>(), true)).Returns(eventDtosBySite);

            var participantEventMapDto = new ParticipantEventMapDto();
            participantEventMapDto.Participants = participantDtos;
            participantEventMapDto.Contacts = contactDtos;
            participantEventMapDto.CurrentEvent = eventDto;

            _eventService.Setup(m => m.GetEvent(eventDto.EventId)).Returns(participantEventMapDto.CurrentEvent);
            _eventService.Setup(m => m.CheckEventTimeValidity(participantEventMapDto.CurrentEvent)).Returns(true);
            _eventRepository.Setup(m => m.GetEventGroupsForEvent(participantEventMapDto.CurrentEvent.EventId)).Returns(mpEventGroupDtos);
            _groupRepository.Setup(m => m.GetGroup(null, 2, false)).Returns((MpGroupDto)null);
            _childSigninRepository.Setup(m => m.CreateEventParticipants(It.IsAny<List<MpEventParticipantDto>>())).Returns(mpEventParticipantDtos);

            // Act
            var response = _fixture.SigninParticipants(participantEventMapDto);

            // Assert
            Assert.IsNotNull(response);
            Assert.IsNull(response.Participants[0].SignInErrorMessage);
        }
        
        [Test]
        public void ShouldSignInParticipantsViaBumpingRules()
        {
            // Arrange
            var participantDtos = new List<ParticipantDto>
            {
                new ParticipantDto
                {
                    FirstName = "Child1First",
                    ParticipantId = 111,
                    Selected = true,
                    GroupId = 432
                }
            };

            var contactDtos = new List<ContactDto>
            {
                new ContactDto
                {
                    ContactId = 1234567,
                    LastName = "TestLast",
                    Nickname = "TestNickname"
                }
            };

            var eventDto = new EventDto
            {
                EventTitle = "test event",
                EventId = 321
            };

            var mpEventGroupDtos = new List<MpEventGroupDto>
            {
                new MpEventGroupDto
                {
                    GroupId = 432,
                    RoomReservation = new MpEventRoomDto
                    {
                        AllowSignIn = true,
                        Capacity = 11,
                        CheckedIn = 9,
                        EventId = 321,
                        EventRoomId = 153234,
                        Hidden = true,
                        RoomId = 4,
                        RoomName = "name",
                        RoomNumber = "number",
                        SignedIn = 2,
                        Volunteers = 6
                    }
                }
            };

            var mpEventParticipantDtos = new List<MpEventParticipantDto>
            {
                new MpEventParticipantDto
                {
                    GroupId = 432,
                    RoomId = 3827
                }
            };

            var participantEventMapDto = new ParticipantEventMapDto
            {
                Participants = participantDtos,
                Contacts = contactDtos,
                CurrentEvent = eventDto
            };

            var mpBumpingRooms = new List<MpBumpingRoomsDto>
            {
                new MpBumpingRoomsDto
                {
                    EventRoomId = 5134,
                    RoomId = 161641,
                    PriorityOrder = 2,
                    AllowSignIn = true,
                    Capacity = 32,
                    RoomName = "Test Room 1",
                    SignedIn = 93,
                    CheckedIn = 12
                },
                new MpBumpingRoomsDto
                {
                    EventRoomId = 1248,
                    RoomId = 3877727,
                    PriorityOrder = 1,
                    AllowSignIn = false,
                    Capacity = 10,
                    RoomName = "Test Room 2",
                    SignedIn = 9,
                    CheckedIn = 0
                },
                new MpBumpingRoomsDto
                {
                    EventRoomId = 1248,
                    RoomId = 511,
                    PriorityOrder = 4,
                    AllowSignIn = false,
                    Capacity = 10,
                    RoomName = "Test Room 2",
                    SignedIn = 9,
                    CheckedIn = 0
                },
                new MpBumpingRoomsDto
                {
                    EventRoomId = 1248,
                    RoomId = 3827,
                    PriorityOrder = 3,
                    AllowSignIn = true,
                    Capacity = 10,
                    RoomName = "Test Room 2",
                    SignedIn = 9,
                    CheckedIn = 0
                }
            };

            DateTime currentMpEventDtoStartTime = new DateTime(2016, 12, 1, 9, 0, 0);

            MpEventDto currentMpServiceEventDto = new MpEventDto
            {
                EventId = 1234567,
                ParentEventId = null,
                CongregationId = 8,
                EventTypeId = 123,
                EventStartDate = currentMpEventDtoStartTime
            };

            // current service event, current ac event, trailing service event
            List<MpEventDto> eventDtosBySite = new List<MpEventDto>()
            {
                currentMpServiceEventDto
            };

            _eventRepository.Setup(m => m.GetEvents(It.IsAny<DateTime>(), It.IsAny<DateTime>(), It.IsAny<int>(), true)).Returns(eventDtosBySite);

            _eventService.Setup(m => m.GetEvent(eventDto.EventId)).Returns(participantEventMapDto.CurrentEvent);
            _eventService.Setup(m => m.CheckEventTimeValidity(participantEventMapDto.CurrentEvent)).Returns(true);
            _eventRepository.Setup(m => m.GetEventGroupsForEvent(participantEventMapDto.CurrentEvent.EventId)).Returns(mpEventGroupDtos);
            _groupRepository.Setup(m => m.GetGroup(null, 2, false)).Returns((MpGroupDto)null);
            _roomRepository.Setup(m => m.GetBumpingRoomsForEventRoom(321, 153234)).Returns(mpBumpingRooms);
            _childSigninRepository.Setup(m => m.CreateEventParticipants(It.IsAny<List<MpEventParticipantDto>>())).Returns(mpEventParticipantDtos);

            // Act
            var response = _fixture.SigninParticipants(participantEventMapDto);

            // Assert
            Assert.IsNotNull(response);
            Assert.AreEqual(response.Participants[0].AssignedRoomId, 3827);
        }

        [Test]
        public void ShouldNotSignInParticipantsDueToCapacity()
        {
            // Arrange
            var participantDtos = new List<ParticipantDto>
            {
                new ParticipantDto
                {
                    FirstName = "Child1First",
                    ParticipantId = 111,
                    Selected = true,
                    GroupId = 432
                }
            };

            var contactDtos = new List<ContactDto>
            {
                new ContactDto
                {
                    ContactId = 1234567,
                    LastName = "TestLast",
                    Nickname = "TestNickname"
                }
            };

            var eventDto = new EventDto
            {
                EventTitle = "test event",
                EventId = 321
            };

            var mpEventGroupDtos = new List<MpEventGroupDto>
            {
                new MpEventGroupDto
                {
                    GroupId = 432,
                    RoomReservation = new MpEventRoomDto
                    {
                        AllowSignIn = true,
                        Capacity = 11,
                        CheckedIn = 9,
                        EventId = 321,
                        EventRoomId = 153234,
                        Hidden = true,
                        RoomId = 4,
                        RoomName = "name",
                        RoomNumber = "number",
                        SignedIn = 2,
                        Volunteers = 6
                    }
                }
            };

            var mpEventParticipantDtos = new List<MpEventParticipantDto>();

            var participantEventMapDto = new ParticipantEventMapDto
            {
                Participants = participantDtos,
                Contacts = contactDtos,
                CurrentEvent = eventDto
            };

            DateTime currentMpEventDtoStartTime = new DateTime(2016, 12, 1, 9, 0, 0);

            MpEventDto currentMpServiceEventDto = new MpEventDto
            {
                EventId = 1234567,
                ParentEventId = null,
                CongregationId = 8,
                EventTypeId = 123,
                EventStartDate = currentMpEventDtoStartTime
            };

            // current service event, current ac event, trailing service event
            List<MpEventDto> eventDtosBySite = new List<MpEventDto>()
            {
                currentMpServiceEventDto
            };

            _eventRepository.Setup(m => m.GetEvents(It.IsAny<DateTime>(), It.IsAny<DateTime>(), It.IsAny<int>(), true)).Returns(eventDtosBySite);

            _eventService.Setup(m => m.GetEvent(eventDto.EventId)).Returns(participantEventMapDto.CurrentEvent);
            _eventService.Setup(m => m.CheckEventTimeValidity(participantEventMapDto.CurrentEvent)).Returns(true);
            _eventRepository.Setup(m => m.GetEventGroupsForEvent(participantEventMapDto.CurrentEvent.EventId)).Returns(mpEventGroupDtos);
            _groupRepository.Setup(m => m.GetGroup(null, 2, false)).Returns((MpGroupDto)null);
            _roomRepository.Setup(m => m.GetBumpingRoomsForEventRoom(321, 153234)).Returns(new List<MpBumpingRoomsDto>());
            _childSigninRepository.Setup(m => m.CreateEventParticipants(It.IsAny<List<MpEventParticipantDto>>())).Returns(mpEventParticipantDtos);

            // Act
            var response = _fixture.SigninParticipants(participantEventMapDto);

            // Assert
            Assert.IsNotNull(response);
            Assert.IsNull(response.Participants[0].AssignedRoomId);
        }

        [Test]
        public void ShouldPrintLabelsForAllParticipants()
        {
            // Arrange
            var kioskId = Guid.Parse("1a11a1a1-a11a-1a1a-11a1-a111a111a11a");

            var mpKioskConfigDto = new MpKioskConfigDto
            {
                KioskIdentifier = kioskId,
                CongregationId = 1,
                PrinterMapId = 1111111
            };

            _kioskRepository.Setup(m => m.GetMpKioskConfigByIdentifier(It.IsAny<Guid>())).Returns(mpKioskConfigDto);

            var mpPrinterMapDto = new MpPrinterMapDto
            {
                PrinterMapId = 1111111
            };

            _kioskRepository.Setup(m => m.GetPrinterMapById(mpKioskConfigDto.PrinterMapId.GetValueOrDefault())).Returns(mpPrinterMapDto);

            var participantDtos = new List<ParticipantDto>
            {
                new ParticipantDto
                {
                    FirstName = "Child1First",
                    AssignedRoomId = 1234567,
                    AssignedRoomName = "TestRoom",
                    AssignedSecondaryRoomId = 2345678,
                    AssignedSecondaryRoomName = "TestSecondaryRoom",
                    ParticipantId = 111,
                    Selected = true
                }
            };

            var contactDtos = new List<ContactDto>
            {
                new ContactDto
                {
                    ContactId = 1234567,
                    LastName = "TestLast",
                    Nickname = "TestNickname"
                }
            };

            var eventDto = new EventDto
            {
                EventTitle = "test event"
            };

            var participantEventMapDto = new ParticipantEventMapDto();
            participantEventMapDto.Participants = participantDtos;
            participantEventMapDto.Contacts = contactDtos;
            participantEventMapDto.CurrentEvent = eventDto;

            const string base64Pdf = "aaa";
            _pdfEditor.Setup(m => m.PopulatePdfMergeFields(It.IsAny<Byte[]>(), It.IsAny<Dictionary<string, string>>())).Returns(base64Pdf);

            _printingService.Setup(m => m.SendPrintRequest(It.IsAny<PrintRequestDto>())).Returns(1234567);

            // Act
            _fixture.PrintParticipants(participantEventMapDto, kioskId.ToString());

            // Assert
            _kioskRepository.VerifyAll();
            _pdfEditor.VerifyAll();
            _printingService.VerifyAll();
        }

        [Test]
        public void ShouldPrintLabelsForNoParticipants()
        {
            // Arrange
            var kioskId = Guid.Parse("1a11a1a1-a11a-1a1a-11a1-a111a111a11a");

            var mpKioskConfigDto = new MpKioskConfigDto
            {
                KioskIdentifier = kioskId,
                CongregationId = 1,
                PrinterMapId = 1111111
            };

            _kioskRepository.Setup(m => m.GetMpKioskConfigByIdentifier(It.IsAny<Guid>())).Returns(mpKioskConfigDto);

            var mpPrinterMapDto = new MpPrinterMapDto
            {
                PrinterMapId = 1111111
            };

            _kioskRepository.Setup(m => m.GetPrinterMapById(mpKioskConfigDto.PrinterMapId.GetValueOrDefault())).Returns(mpPrinterMapDto);

            var participantDtos = new List<ParticipantDto>
            {
                new ParticipantDto
                {
                    FirstName = "Child1First",
                    AssignedRoomId = 1234567,
                    AssignedRoomName = "TestRoom",
                    AssignedSecondaryRoomId = 2345678,
                    AssignedSecondaryRoomName = "TestSecondaryRoom",
                    ParticipantId = 111,
                    Selected = false
                }
            };

            var contactDtos = new List<ContactDto>
            {
                new ContactDto
                {
                    ContactId = 1234567,
                    LastName = "TestLast",
                    Nickname = "TestNickname"
                }
            };

            var eventDto = new EventDto
            {
                EventTitle = "test event"
            };

            var participantEventMapDto = new ParticipantEventMapDto
            {
                Participants = participantDtos,
                Contacts = contactDtos,
                CurrentEvent = eventDto
            };

            // Act
            _fixture.PrintParticipants(participantEventMapDto, kioskId.ToString());

            // Assert
            _kioskRepository.VerifyAll();
            _pdfEditor.VerifyAll();
            _printingService.VerifyAll();
        }

        [Test]
        public void ShouldPrintLabelsForSomeParticipants()
        {
            // Arrange
            var kioskId = Guid.Parse("1a11a1a1-a11a-1a1a-11a1-a111a111a11a");

            var mpKioskConfigDto = new MpKioskConfigDto
            {
                KioskIdentifier = kioskId,
                CongregationId = 1,
                PrinterMapId = 1111111
            };

            _kioskRepository.Setup(m => m.GetMpKioskConfigByIdentifier(It.IsAny<Guid>())).Returns(mpKioskConfigDto);

            var mpPrinterMapDto = new MpPrinterMapDto
            {
                PrinterMapId = 1111111
            };

            _kioskRepository.Setup(m => m.GetPrinterMapById(mpKioskConfigDto.PrinterMapId.GetValueOrDefault())).Returns(mpPrinterMapDto);

            var participantDtos = new List<ParticipantDto>
            {
                new ParticipantDto
                {
                    FirstName = "Child1First",
                    AssignedRoomId = 1234567,
                    AssignedRoomName = "TestRoom",
                    AssignedSecondaryRoomId = 2345678,
                    AssignedSecondaryRoomName = "TestSecondaryRoom",
                    ParticipantId = 111,
                    Selected = true
                },
                new ParticipantDto
                {
                    FirstName = "Child2First",
                    AssignedRoomId = null,
                    AssignedSecondaryRoomId = 2345678,
                    AssignedSecondaryRoomName = "TestSecondaryRoom",
                    ParticipantId = 222,
                    Selected = true
                },
                new ParticipantDto
                {
                    FirstName = "Child3First",
                    AssignedRoomId = null,
                    AssignedSecondaryRoomId = 2345678,
                    AssignedSecondaryRoomName = "TestSecondaryRoom",
                    ParticipantId = 333,
                    Selected = false
                },
                new ParticipantDto
                {
                    FirstName = "Child4First",
                    AssignedRoomId = null,
                    AssignedSecondaryRoomId = 2345678,
                    AssignedSecondaryRoomName = "TestSecondaryRoom",
                    ParticipantId = 333,
                    Selected = true,
                    SignInErrorMessage = "testerror"
                }
            };

            var contactDtos = new List<ContactDto>
            {
                new ContactDto
                {
                    ContactId = 1234567,
                    LastName = "TestLast",
                    Nickname = "TestNickname"
                }
            };

            var eventDto = new EventDto
            {
                EventTitle = "test event"
            };

            var participantEventMapDto = new ParticipantEventMapDto
            {
                Participants = participantDtos,
                Contacts = contactDtos,
                CurrentEvent = eventDto
            };

            const string successLabel = "aaa";
            _pdfEditor.Setup(m => m.PopulatePdfMergeFields(It.IsAny<byte[]>(), It.IsAny<Dictionary<string, string>>())).Returns(successLabel);
            _printingService.Setup(m => m.SendPrintRequest(It.IsAny<PrintRequestDto>())).Returns(1234567);

            // Act
            _fixture.PrintParticipants(participantEventMapDto, kioskId.ToString());

            // Assert

            // Verify specific calls, to make sure we populated the right PDFs for the right participants
            _pdfEditor.Verify(
                mocked =>
                    mocked.PopulatePdfMergeFields(Properties.Resources.Checkin_KC_Label, It.Is<Dictionary<string, string>>(d => d["ChildName"].Equals(participantDtos[0].FirstName))));
            _pdfEditor.Verify(
                mocked =>
                    mocked.PopulatePdfMergeFields(Properties.Resources.Activity_Kit_Label, It.Is<Dictionary<string, string>>(d => d["ChildName"].Equals(participantDtos[1].FirstName))));
            _pdfEditor.Verify(
                mocked =>
                    mocked.PopulatePdfMergeFields(Properties.Resources.Error_Label, It.Is<Dictionary<string, string>>(d => d["ChildName"].Equals(participantDtos[3].FirstName))));
            _pdfEditor.VerifyAll();

            // Verify that we called the print service for each expected participant
            _printingService.Verify(mocked => mocked.SendPrintRequest(It.Is<PrintRequestDto>(p => p.title.Contains(participantDtos[0].FirstName))));
            _printingService.Verify(mocked => mocked.SendPrintRequest(It.Is<PrintRequestDto>(p => p.title.Contains(participantDtos[1].FirstName))));
            _printingService.Verify(mocked => mocked.SendPrintRequest(It.Is<PrintRequestDto>(p => p.title.Contains(participantDtos[3].FirstName))));
            _printingService.VerifyAll();

            _kioskRepository.VerifyAll();
        }

        [Test]
        public void ItShouldSaveNewFamilyData()
        {
            // Arrange
            string token = "123abc";

            EventDto eventDto = new EventDto
            {
                EventSiteId = 1
            };

            NewParentDto newParentDto = new NewParentDto
            {
                FirstName = "TestParentFirst",
                LastName = "TestParentLast",
                PhoneNumber = "123-456-7890"
            };

            List<NewChildDto> newChildDtos = new List<NewChildDto>
            {
                new NewChildDto
                {
                    DateOfBirth = new DateTime(2016, 12, 1, 00, 00, 00),
                    FirstName = "TestChildFirst",
                    LastName = "TestChildLast",
                    YearGrade = 1
                }
            };

            NewFamilyDto newFamilyDto = new NewFamilyDto
            {
                EventDto = eventDto,
                ParentContactDto = newParentDto,
                ChildContactDtos = newChildDtos
            };

            MpHouseholdDto mpHouseholdDto = new MpHouseholdDto();
            MpNewParticipantDto newParticipantDto = new MpNewParticipantDto();

            _contactRepository.Setup(m => m.CreateHousehold(token, It.IsAny<MpHouseholdDto>())).Returns(mpHouseholdDto);
            _participantRepository.Setup(m => m.CreateParticipantWithContact(token, It.IsAny<MpNewParticipantDto>())).Returns(newParticipantDto);

            // Act
            var result = _fixture.SaveNewFamilyData(token, newFamilyDto);

            // Assert
            _contactRepository.VerifyAll();
            _participantRepository.VerifyAll();
            Assert.IsNotNull(result);
        }

        [Test]
        public void CheckAcEventDuringCurrentService()
        {
            // Arrange
            _applicationConfiguration.Setup(m => m.AdventureClubEventTypeId).Returns(20);

            DateTime currentMpEventDtoStartTime = new DateTime(2016, 12, 1, 9, 0, 0);
            DateTime futureMpEventDtoStartTime = new DateTime(2016, 12, 1, 11, 0, 0);

            MpEventDto currentMpServiceEventDto = new MpEventDto
            {
                EventId = 1234567,
                ParentEventId = null,
                CongregationId = 8,
                EventTypeId = 123,
                EventStartDate = currentMpEventDtoStartTime
            };

            MpEventDto currentMpAdventureClubEventDto = new MpEventDto
            {
                EventId = 7654321,
                ParentEventId = 1234567,
                CongregationId = 8,
                EventTypeId = 20,
                EventStartDate = currentMpEventDtoStartTime
            };

            MpEventDto futureMpServiceEventDto = new MpEventDto
            {
                EventId = 2345678,
                ParentEventId = null,
                CongregationId = 8,
                EventTypeId = 20,
                EventStartDate = futureMpEventDtoStartTime
            };

            // current service event, current ac event, trailing service event
            List<MpEventDto> eventDtosBySite = new List<MpEventDto>()
            {
                currentMpServiceEventDto,
                currentMpAdventureClubEventDto,
                futureMpServiceEventDto
            };

            _eventRepository.Setup(m => m.GetEvents(It.IsAny<DateTime>(), It.IsAny<DateTime>(), 8, true)).Returns(eventDtosBySite);

            EventDto signingInEventDto = new EventDto
            {
                EventId = 1234567,
                EventSiteId = 8
            };

            ParticipantEventMapDto participantEventMapDto = new ParticipantEventMapDto
            {
                CurrentEvent = signingInEventDto,
                ServicesAttended = 2
            };

            // Act
            var result = _fixture.CheckAcEventStatus(participantEventMapDto);

            // Assert

            // we expect the child to be signed into the current ac event and future service event
            Assert.AreEqual(result[0], futureMpServiceEventDto.EventId);
            Assert.AreEqual(result[1], currentMpAdventureClubEventDto.EventId);
        }

        [Test]
        public void CheckAcEventAfterCurrentService()
        {
            // Arrange
            _applicationConfiguration.Setup(m => m.AdventureClubEventTypeId).Returns(20);

            DateTime currentMpEventDtoStartTime = new DateTime(2016, 12, 1, 9, 0, 0);
            DateTime futureMpEventDtoStartTime = new DateTime(2016, 12, 1, 11, 0, 0);

            MpEventDto currentMpServiceEventDto = new MpEventDto
            {
                EventId = 1234567,
                ParentEventId = null,
                CongregationId = 8,
                EventTypeId = 123,
                EventStartDate = currentMpEventDtoStartTime
            };

            MpEventDto futureMpAdventureClubEventDto = new MpEventDto
            {
                EventId = 7654321,
                ParentEventId = 1234567,
                CongregationId = 8,
                EventTypeId = 20,
                EventStartDate = futureMpEventDtoStartTime
            };

            MpEventDto futureMpServiceEventDto = new MpEventDto
            {
                EventId = 2345678,
                ParentEventId = null,
                CongregationId = 8,
                EventTypeId = 20,
                EventStartDate = futureMpEventDtoStartTime
            };

            // current service event, current ac event, trailing service event
            List<MpEventDto> eventDtosBySite = new List<MpEventDto>()
            {
                currentMpServiceEventDto,
                futureMpAdventureClubEventDto,
                futureMpServiceEventDto
            };

            _eventRepository.Setup(m => m.GetEvents(It.IsAny<DateTime>(), It.IsAny<DateTime>(), 8, true)).Returns(eventDtosBySite);

            EventDto signingInEventDto = new EventDto
            {
                EventId = 1234567,
                EventSiteId = 8
            };

            ParticipantEventMapDto participantEventMapDto = new ParticipantEventMapDto
            {
                CurrentEvent = signingInEventDto,
                ServicesAttended = 2
            };

            // Act
            var result = _fixture.CheckAcEventStatus(participantEventMapDto);

            // Assert

            // we expect the child to be signed into the current ac event and future service event
            Assert.AreEqual(result[0], futureMpServiceEventDto.EventId);
            Assert.AreEqual(result[1], futureMpAdventureClubEventDto.EventId);
        }

        [Test]
        public void CheckAcEventNoAcEvent()
        {
            // Arrange
            _applicationConfiguration.Setup(m => m.AdventureClubEventTypeId).Returns(20);

            DateTime currentMpEventDtoStartTime = new DateTime(2016, 12, 1, 9, 0, 0);
            DateTime futureMpEventDtoStartTime = new DateTime(2016, 12, 1, 11, 0, 0);

            MpEventDto currentMpServiceEventDto = new MpEventDto
            {
                EventId = 1234567,
                ParentEventId = null,
                CongregationId = 8,
                EventTypeId = 123,
                EventStartDate = currentMpEventDtoStartTime
            };

            MpEventDto futureMpAdventureClubEventDto = new MpEventDto
            {
                EventId = 7654321,
                ParentEventId = 1234567,
                CongregationId = 8,
                EventTypeId = 20,
                EventStartDate = futureMpEventDtoStartTime
            };

            MpEventDto futureMpServiceEventDto = new MpEventDto
            {
                EventId = 2345678,
                ParentEventId = null,
                CongregationId = 8,
                EventTypeId = 20,
                EventStartDate = futureMpEventDtoStartTime
            };

            // current service event, future service event
            List<MpEventDto> eventDtosBySite = new List<MpEventDto>()
            {
                currentMpServiceEventDto,
                futureMpServiceEventDto
            };

            _eventRepository.Setup(m => m.GetEvents(It.IsAny<DateTime>(), It.IsAny<DateTime>(), 8, true)).Returns(eventDtosBySite);

            EventDto signingInEventDto = new EventDto
            {
                EventId = 1234567,
                EventSiteId = 8
            };

            ParticipantEventMapDto participantEventMapDto = new ParticipantEventMapDto
            {
                CurrentEvent = signingInEventDto,
                ServicesAttended = 2
            };

            // Act
            var result = _fixture.CheckAcEventStatus(participantEventMapDto);

            // Assert

            // we expect the child to be signed into the current ac event and future service event
            Assert.AreEqual(result[0], currentMpServiceEventDto.EventId);
            Assert.AreEqual(result.Count, 1);
        }

        [Test]
        public void CheckAcEventCurrentService()
        {
            // Arrange
            _applicationConfiguration.Setup(m => m.AdventureClubEventTypeId).Returns(20);

            DateTime currentMpEventDtoStartTime = new DateTime(2016, 12, 1, 9, 0, 0);
            
            MpEventDto currentMpServiceEventDto = new MpEventDto
            {
                EventId = 1234567,
                ParentEventId = null,
                CongregationId = 8,
                EventTypeId = 123,
                EventStartDate = currentMpEventDtoStartTime
            };

            MpEventDto currentMpAdventureClubEventDto = new MpEventDto
            {
                EventId = 7654321,
                ParentEventId = 1234567,
                CongregationId = 8,
                EventTypeId = 20,
                EventStartDate = currentMpEventDtoStartTime
            };

            // current service event, future service event
            List<MpEventDto> eventDtosBySite = new List<MpEventDto>()
            {
                currentMpServiceEventDto,
                currentMpAdventureClubEventDto
            };

            _eventRepository.Setup(m => m.GetEvents(It.IsAny<DateTime>(), It.IsAny<DateTime>(), 8, true)).Returns(eventDtosBySite);

            EventDto signingInEventDto = new EventDto
            {
                EventId = 1234567,
                EventSiteId = 8
            };

            ParticipantEventMapDto participantEventMapDto = new ParticipantEventMapDto
            {
                CurrentEvent = signingInEventDto,
                ServicesAttended = 2
            };

            // Act
            var result = _fixture.CheckAcEventStatus(participantEventMapDto);

            // Assert

            // we expect the child to be signed into the current ac event and future service event
            Assert.AreEqual(result[0], currentMpServiceEventDto.EventId);
            Assert.AreEqual(result.Count, 1);
        }
    }
}
