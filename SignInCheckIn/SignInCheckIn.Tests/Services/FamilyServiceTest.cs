﻿using Crossroads.Utilities.Services.Interfaces;
using MinistryPlatform.Translation.Models.DTO;
using MinistryPlatform.Translation.Repositories.Interfaces;
using Moq;
using NUnit.Framework;
using SignInCheckIn.Models.DTO;
using SignInCheckIn.Services;
using SignInCheckIn.Services.Interfaces;
using System.Collections.Generic;

namespace SignInCheckIn.Tests.Services
{
    public class FamilyServiceTest
    {
        private Mock<IContactRepository> _contactRepository;
        private Mock<IParticipantRepository> _participantRepository;
        private Mock<IApplicationConfiguration> _applicationConfiguration;
        private Mock<IPasswordService> _passwordService;
        private Mock<IChildSigninService> _childSigninSerivce;

        private FamilyService _fixture;

        [SetUp]
        public void SetUp()
        {
            AutoMapperConfig.RegisterMappings();

            _contactRepository = new Mock<IContactRepository>(MockBehavior.Strict);
            _participantRepository = new Mock<IParticipantRepository>(MockBehavior.Strict);
            _applicationConfiguration = new Mock<IApplicationConfiguration>(MockBehavior.Strict);
            _passwordService = new Mock<IPasswordService>(MockBehavior.Strict);
            _childSigninSerivce = new Mock<IChildSigninService>(MockBehavior.Strict);

            _applicationConfiguration.Setup(m => m.KidsClubRegistrationSourceId).Returns(48);
            _applicationConfiguration.Setup(m => m.AttendeeParticipantType).Returns(2);
            _applicationConfiguration.Setup(m => m.HeadOfHouseholdId).Returns(1);
            _applicationConfiguration.Setup(m => m.KidsClubPublicationId).Returns(3);
            _applicationConfiguration.Setup(m => m.AllPlatformUsersRoleId).Returns(39);
            _applicationConfiguration.Setup(m => m.GeneralPublicationId).Returns(2);
            _applicationConfiguration.Setup(m => m.MarriedToRelationshipId).Returns(1);

            _fixture = new FamilyService(_contactRepository.Object, _participantRepository.Object, _applicationConfiguration.Object, _passwordService.Object, _childSigninSerivce.Object);
        }

        [Test]
        public void ShouldSaveNewFamily()
        {
            // Arrange
            var token = "123abc";
            var kioskId = "aaa";
            var passwordResetToken = "abcdefgh12345678";

            var mpHouseholdDto = new MpHouseholdDto
            {
                HouseholdId = 1234567
            };

            var newParentDtos = new List<NewParentDto>
            {
                new NewParentDto
                {
                    CongregationId = 1,
                    FirstName = "first",
                    LastName = "last",
                    PhoneNumber = "555-555-0987",
                    EmailAddress = "test@test.com"
                }
            };

            var mpNewParticipantDtoFromRepo = new MpNewParticipantDto
            {
                FirstName = "first",
                LastName = "last",
                Contact = new MpContactDto
                {
                    HouseholdId = 1234567
                }
            };

            var mpNewContactDto = new MpContactDto
            {
                ContactId = 5544555
            };

            var mpUserDto = new MpUserDto
            {
                FirstName = "first", // contact?
                LastName = "last", // contact?
                UserEmail = "test@test.com",
                Password = "abcdefghijklmnopq",
                Company = false, //contact?
                DisplayName = "last, first",
                DomainId = 1,
                UserName = "test@test.com",
                ContactId = 5544555,
                PasswordResetToken = "abcdefgh12345678"
            };

            var mpNewUserDto = new MpUserDto
            {
                UserId = 6677667,
                FirstName = "first", // contact?
                LastName = "last", // contact?
                UserEmail = "test@test.com",
                Password = "abcdefghijklmnopq",
                Company = false, //contact?
                DisplayName = "last, first",
                DomainId = 1,
                UserName = "test@test.com",
                ContactId = 5544555,
                PasswordResetToken = "abcdefgh12345678"
            };

            _passwordService.Setup(r => r.GetNewUserPassword(16, 2)).Returns("abcdefghijklmnopq");
            _passwordService.Setup(r => r.GeneratorPasswordResetToken("test@test.com")).Returns("abcdefgh12345678");
            _contactRepository.Setup(r => r.GetUserByEmailAddress("test@test.com")).Returns(new List<MpUserDto>());
            _contactRepository.Setup(m => m.CreateHousehold(It.IsAny<MpHouseholdDto>())).Returns(mpHouseholdDto);
            _contactRepository.Setup(m => m.GetContactById(It.IsAny<int>())).Returns(mpNewContactDto);
            _contactRepository.Setup(m => m.CreateUserRecord(It.IsAny<MpUserDto>())).Returns(mpNewUserDto);
            _contactRepository.Setup(m => m.CreateUserRoles(It.IsAny<List<MpUserRoleDto>>()));
            _contactRepository.Setup(m => m.CreateContactPublications(It.IsAny<List<MpContactPublicationDto>>()));
            _participantRepository.Setup(m => m.CreateParticipantWithContact(It.IsAny<MpNewParticipantDto>(), null)).Returns(mpNewParticipantDtoFromRepo);

            // Act
            var result = _fixture.CreateNewFamily(newParentDtos, kioskId);

            // Assert
            Assert.IsNotNull(result[0].HouseholdId);
            Assert.AreNotEqual(0, result.Count);
        }

        [Test]
        public void ShouldSaveNewFamilyWithTwoParents()
        {
            // Arrange
            var token = "123abc";
            var kioskId = "aaa";
            var passwordResetToken = "abcdefgh12345678";

            var mpHouseholdDto = new MpHouseholdDto
            {
                HouseholdId = 1234567
            };

            var newParentDtos = new List<NewParentDto>
            {
                new NewParentDto
                {
                    CongregationId = 1,
                    FirstName = "first_one",
                    LastName = "last",
                    PhoneNumber = "555-555-0987",
                    EmailAddress = "test1@test.com"
                },
                new NewParentDto
                {
                    CongregationId = 1,
                    FirstName = "first_two",
                    LastName = "last",
                    PhoneNumber = "555-555-0986",
                    EmailAddress = "test2@test.com"
                }
            };

            //////////////////////

            var mpNewParticipantDtoFromRepo_1 = new MpNewParticipantDto
            {
                FirstName = "first_one",
                LastName = "last",
                ContactId = 5544555,
                Contact = new MpContactDto
                {
                    HouseholdId = 1234567,
                    ContactId = 5544555
                }
            };

            var mpNewParticipantDtoFromRepo_2 = new MpNewParticipantDto
            {
                FirstName = "first_two",
                LastName = "last",
                ContactId = 4433444,
                Contact = new MpContactDto
                {
                    HouseholdId = 1234567,
                    ContactId = 4433444
                }
            };

            //////////////////////

            var mpNewContactDto_1 = new MpContactDto
            {
                ContactId = 5544555
            };

            var mpNewContactDto_2 = new MpContactDto
            {
                ContactId = 4433444
            };

            /////////////////////

            var mpUserDtoSave_1 = new MpUserDto
            {
                FirstName = "first_one", // contact?
                LastName = "last", // contact?
                UserEmail = "test1@test.com",
                Password = "abcdefghijklmnopq",
                Company = false, //contact?
                DisplayName = "last, first",
                DomainId = 1,
                UserName = "test1@test.com",
                ContactId = 5544555,
                PasswordResetToken = "abcdefgh12345678"
            };

            var mpNewUserDtoReturn_1 = new MpUserDto
            {
                UserId = 6677667,
                FirstName = "first_one", // contact?
                LastName = "last", // contact?
                UserEmail = "test1@test.com",
                Password = "abcdefghijklmnopq",
                Company = false, //contact?
                DisplayName = "last, first",
                DomainId = 1,
                UserName = "test1@test.com",
                ContactId = 5544555,
                PasswordResetToken = "abcdefgh12345678"
            };

            var mpUserDtoSave_2 = new MpUserDto
            {
                FirstName = "first_two", // contact?
                LastName = "last", // contact?
                UserEmail = "test2@test.com",
                Password = "abcdefghijklmnopq",
                Company = false, //contact?
                DisplayName = "last, first",
                DomainId = 1,
                UserName = "test2@test.com",
                ContactId = 4433444,
                PasswordResetToken = "abcdefgh12345678"
            };

            var mpNewUserDtoReturn_2 = new MpUserDto
            {
                UserId = 8877887,
                FirstName = "first_two", // contact?
                LastName = "last", // contact?
                UserEmail = "test2@test.com",
                Password = "abcdefghijklmnopq",
                Company = false, //contact?
                DisplayName = "last, first",
                DomainId = 1,
                UserName = "test2@test.com",
                ContactId = 4433444,
                PasswordResetToken = "abcdefgh12345678"
            };

            // one password is fine in the test
            _passwordService.Setup(r => r.GetNewUserPassword(16, 2)).Returns("abcdefghijklmnopq");

            // the same token is fine in a test
            _passwordService.Setup(r => r.GeneratorPasswordResetToken("test1@test.com")).Returns("abcdefgh12345678");
            _passwordService.Setup(r => r.GeneratorPasswordResetToken("test2@test.com")).Returns("abcdefgh12345678");

            // these need to return an empty list, as we expect that no parents will be found if they're new
            _contactRepository.Setup(r => r.GetUserByEmailAddress( "test1@test.com")).Returns(new List<MpUserDto>());
            _contactRepository.Setup(r => r.GetUserByEmailAddress("test2@test.com")).Returns(new List<MpUserDto>());

            // only one household gets created
            _contactRepository.Setup(m => m.CreateHousehold(It.IsAny<MpHouseholdDto>())).Returns(mpHouseholdDto);

            // the new contact is created as part of creating the participant - would be nice if we can pass down the first name as an arg?
            _participantRepository.Setup(m => m.CreateParticipantWithContact(It.Is<MpNewParticipantDto>(r => r.Contact.FirstName == "first_one"), null)).Returns(mpNewParticipantDtoFromRepo_1);
            _participantRepository.Setup(m => m.CreateParticipantWithContact(It.Is<MpNewParticipantDto>(r => r.Contact.FirstName == "first_two"), null)).Returns(mpNewParticipantDtoFromRepo_2);

            // these are created off of the new participant object - the contact id is on the new participant object
            _contactRepository.Setup(m => m.GetContactById(5544555)).Returns(mpNewContactDto_1);
            _contactRepository.Setup(m => m.GetContactById(4433444)).Returns(mpNewContactDto_2);

            // test creating the actual user records - the email address is the comparator
            _contactRepository.Setup(m => m.CreateUserRecord(It.Is<MpUserDto>(r => r.UserEmail == "test1@test.com"))).Returns(mpNewUserDtoReturn_1);
            _contactRepository.Setup(m => m.CreateUserRecord(It.Is<MpUserDto>(r => r.UserEmail == "test2@test.com"))).Returns(mpNewUserDtoReturn_2);

            _contactRepository.Setup(m => m.CreateUserRoles(It.IsAny<List<MpUserRoleDto>>()));
            _contactRepository.Setup(m => m.CreateContactPublications(It.IsAny<List<MpContactPublicationDto>>()));
            //_contactRepository.Setup(m => m.CreateContactRelationships(token, It.IsAny<List<MpContactRelationshipDto>>()));
            
            // Act
            var result = _fixture.CreateNewFamily(newParentDtos, kioskId);

            // Assert
            _contactRepository.VerifyAll();

            Assert.IsNotNull(result[0].HouseholdId);
            Assert.AreEqual(2, result.Count);
        }

        [Test]
        public void ShouldNotSaveExistingFamily()
        {
            // Arrange
            var token = "123abc";
            var kioskId = "aaa";
            var passwordResetToken = "abcdefgh12345678";

            var mpHouseholdDto = new MpHouseholdDto
            {
                HouseholdId = 1234567
            };

            var newParentDtos = new List<NewParentDto>
            {
                new NewParentDto
                {
                    CongregationId = 1,
                    FirstName = "first",
                    LastName = "last",
                    PhoneNumber = "555-555-0987",
                    EmailAddress = "test@test.com"
                }
            };

            var mpNewParticipantDtoFromRepo = new MpNewParticipantDto
            {
                FirstName = "first",
                LastName = "last",
                Contact = new MpContactDto
                {
                    HouseholdId = 1234567
                }
            };

            var mpNewContactDto = new MpContactDto
            {
                ContactId = 5544555
            };

            var mpUserDto = new MpUserDto
            {
                FirstName = "first", // contact?
                LastName = "last", // contact?
                UserEmail = "test@test.com",
                Password = "abcdefghijklmnopq",
                Company = false, //contact?
                DisplayName = "last, first",
                DomainId = 1,
                UserName = "test@test.com",
                ContactId = 5544555,
                PasswordResetToken = "abcdefgh12345678"
            };

            var mpNewUserDto = new MpUserDto
            {
                UserId = 6677667,
                FirstName = "first", // contact?
                LastName = "last", // contact?
                UserEmail = "test@test.com",
                Password = "abcdefghijklmnopq",
                Company = false, //contact?
                DisplayName = "last, first",
                DomainId = 1,
                UserName = "test@test.com",
                ContactId = 5544555,
                PasswordResetToken = "abcdefgh12345678"
            };

            _passwordService.Setup(r => r.GetNewUserPassword(16, 2)).Returns("abcdefghijklmnopq");
            _passwordService.Setup(r => r.GeneratorPasswordResetToken("test@test.com")).Returns("abcdefgh12345678");
            _contactRepository.Setup(r => r.GetUserByEmailAddress("test@test.com")).Returns(new List<MpUserDto> { new MpUserDto() });
            _contactRepository.Setup(m => m.CreateHousehold(It.IsAny<MpHouseholdDto>())).Returns(mpHouseholdDto);
            _contactRepository.Setup(m => m.GetContactById(It.IsAny<int>())).Returns(mpNewContactDto);
            _contactRepository.Setup(m => m.CreateUserRecord(It.IsAny<MpUserDto>())).Returns(mpNewUserDto);
            _contactRepository.Setup(m => m.CreateUserRoles(It.IsAny<List<MpUserRoleDto>>()));
            _contactRepository.Setup(m => m.CreateContactPublications(It.IsAny<List<MpContactPublicationDto>>()));
            _participantRepository.Setup(m => m.CreateParticipantWithContact(It.IsAny<MpNewParticipantDto>(), token)).Returns(mpNewParticipantDtoFromRepo);

            // Act
            var result = _fixture.CreateNewFamily(newParentDtos, kioskId);

            // Assert
            Assert.AreEqual(0, result.Count);
        }

        [Test]
        public void ShouldNotCreateNewUserWithoutEmailAddress()
        {
            // Arrange
            var token = "123abc";
            var kioskId = "aaa";
            var passwordResetToken = "abcdefgh12345678";

            var mpHouseholdDto = new MpHouseholdDto
            {
                HouseholdId = 1234567
            };

            var newParentDtos = new List<NewParentDto>
            {
                new NewParentDto
                {
                    CongregationId = 1,
                    FirstName = "first",
                    LastName = "last",
                    PhoneNumber = "555-555-0987",
                }
            };

            var mpNewParticipantDtoFromRepo = new MpNewParticipantDto
            {
                FirstName = "first",
                LastName = "last",
                Contact = new MpContactDto
                {
                    HouseholdId = 1234567
                }
            };

            var mpNewContactDto = new MpContactDto
            {
                ContactId = 5544555
            };

            var mpUserDto = new MpUserDto
            {
                FirstName = "first", // contact?
                LastName = "last", // contact?
                UserEmail = "test@test.com",
                Password = "abcdefghijklmnopq",
                Company = false, //contact?
                DisplayName = "last, first",
                DomainId = 1,
                UserName = "test@test.com",
                ContactId = 5544555,
                PasswordResetToken = "abcdefgh12345678"
            };

            var mpNewUserDto = new MpUserDto
            {
                UserId = 6677667,
                FirstName = "first", // contact?
                LastName = "last", // contact?
                UserEmail = "test@test.com",
                Password = "abcdefghijklmnopq",
                Company = false, //contact?
                DisplayName = "last, first",
                DomainId = 1,
                UserName = "test@test.com",
                ContactId = 5544555,
                PasswordResetToken = "abcdefgh12345678"
            };

            _passwordService.Setup(r => r.GetNewUserPassword(16, 2)).Returns("abcdefghijklmnopq");
            _passwordService.Setup(r => r.GeneratorPasswordResetToken("test@test.com")).Returns("abcdefgh12345678");
            _contactRepository.Setup(r => r.GetUserByEmailAddress("test@test.com")).Returns(new List<MpUserDto> { new MpUserDto() });
            _contactRepository.Setup(m => m.CreateHousehold(It.IsAny<MpHouseholdDto>())).Returns(mpHouseholdDto);
            _contactRepository.Setup(m => m.GetContactById(It.IsAny<int>())).Returns(mpNewContactDto);
            _contactRepository.Setup(m => m.CreateContactPublications(It.IsAny<List<MpContactPublicationDto>>()));
            _participantRepository.Setup(m => m.CreateParticipantWithContact(It.IsAny<MpNewParticipantDto>(), null)).Returns(mpNewParticipantDtoFromRepo);

            // Act
            var result = _fixture.CreateNewFamily(newParentDtos, kioskId);

            // Assert
            Assert.AreEqual(1, result.Count);
        }
    }
}
