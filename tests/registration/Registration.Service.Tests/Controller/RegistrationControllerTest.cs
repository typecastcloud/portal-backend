/********************************************************************************
 * Copyright (c) 2021, 2023 Microsoft and BMW Group AG
 * Copyright (c) 2021, 2023 Contributors to the Eclipse Foundation
 *
 * See the NOTICE file(s) distributed with this work for additional
 * information regarding copyright ownership.
 *
 * This program and the accompanying materials are made available under the
 * terms of the Apache License, Version 2.0 which is available at
 * https://www.apache.org/licenses/LICENSE-2.0.
 *
 * Unless required by applicable law or agreed to in writing, software
 * distributed under the License is distributed on an "AS IS" BASIS, WITHOUT
 * WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied. See the
 * License for the specific language governing permissions and limitations
 * under the License.
 *
 * SPDX-License-Identifier: Apache-2.0
 ********************************************************************************/

using AutoFixture;
using FakeItEasy;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.DBAccess.Models;
using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.PortalEntities.Enums;
using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.PortalEntities.Identities;
using Org.Eclipse.TractusX.Portal.Backend.Provisioning.Library.Models;
using Org.Eclipse.TractusX.Portal.Backend.Registration.Service.BusinessLogic;
using Org.Eclipse.TractusX.Portal.Backend.Registration.Service.Controllers;
using Org.Eclipse.TractusX.Portal.Backend.Registration.Service.Model;
using Org.Eclipse.TractusX.Portal.Backend.Tests.Shared.Extensions;
using System.Text;
using Xunit;

namespace Org.Eclipse.TractusX.Portal.Backend.Registration.Service.Tests;

public class RegistrationControllerTest
{
    private readonly IdentityData _identity = new("7478542d-7878-47a8-a931-08bd8779532d", Guid.NewGuid(), IdentityTypeId.COMPANY_USER, Guid.NewGuid());
    private readonly IFixture _fixture;
    private readonly RegistrationController _controller;
    private readonly IRegistrationBusinessLogic _registrationBusinessLogicFake;

    public RegistrationControllerTest()
    {
        _fixture = new Fixture();
        _registrationBusinessLogicFake = A.Fake<IRegistrationBusinessLogic>();
        _controller = new RegistrationController(_registrationBusinessLogicFake);
        _controller.AddControllerContextWithClaimAndBearer("ac-token", _identity);
    }

    [Fact]
    public async Task Get_WhenThereAreInvitedUsers_ShouldReturnActionResultOfInvitedUsersWith200StatusCode()
    {
        //Arrange
        var id = new Guid("7eab8e16-8298-4b41-953b-515745423658");
        var invitedUserMapper = _fixture.CreateMany<InvitedUser>(3).ToAsyncEnumerable();
        A.CallTo(() => _registrationBusinessLogicFake.GetInvitedUsersAsync(id))
            .Returns(invitedUserMapper);

        //Act
        var result = this._controller.GetInvitedUsersAsync(id);
        await foreach (var item in result)
        {
            //Assert
            A.CallTo(() => _registrationBusinessLogicFake.GetInvitedUsersAsync(id)).MustHaveHappenedOnceExactly();
            Assert.NotNull(item);
            Assert.IsType<InvitedUser>(item);
        }
    }

    [Fact]
    public async Task GetInvitedUsersDetail_WhenIdisNull_ShouldThrowException()
    {
        //Arrange
        var id = new Guid("7eab8e16-8298-4b41-953b-515745423658");
        var invitedUserMapper = _fixture.CreateMany<InvitedUser>(3).ToAsyncEnumerable();
        A.CallTo(() => _registrationBusinessLogicFake.GetInvitedUsersAsync(id))
            .Returns(invitedUserMapper);

        //Act
        var result = this._controller.GetInvitedUsersAsync(Guid.Empty);

        //Assert
        await foreach (var item in result)
        {
            A.CallTo(() => _registrationBusinessLogicFake.GetInvitedUsersAsync(Guid.Empty)).Throws(new Exception());
        }
    }

    [Fact]
    public async Task GetUploadedDocumentsAsync_ReturnsExpectedResult()
    {
        //Arrange
        var applicationId = _fixture.Create<Guid>();

        var uploadDocuments = _fixture.CreateMany<UploadDocuments>(3);
        A.CallTo(() => _registrationBusinessLogicFake.GetUploadedDocumentsAsync(applicationId, DocumentTypeId.APP_CONTRACT, _identity.UserId))
            .Returns(uploadDocuments);

        //Act
        var result = await _controller.GetUploadedDocumentsAsync(applicationId, DocumentTypeId.APP_CONTRACT).ConfigureAwait(false);

        //Assert
        A.CallTo(() => _registrationBusinessLogicFake.GetUploadedDocumentsAsync(applicationId, DocumentTypeId.APP_CONTRACT, _identity.UserId)).MustHaveHappenedOnceExactly();

        result.Should().HaveSameCount(uploadDocuments);
        result.Should().ContainInOrder(uploadDocuments);
    }

    [Fact]
    public async Task SubmitCompanyRoleConsentToAgreementsAsync_WithValidData_ReturnsExpected()
    {
        // Arrange
        var applicationId = _fixture.Create<Guid>();
        var data = _fixture.Create<CompanyRoleAgreementConsents>();
        A.CallTo(() => _registrationBusinessLogicFake.SubmitRoleConsentAsync(applicationId, data, _identity.UserId, _identity.CompanyId))
            .Returns(1);

        //Act
        var result = await this._controller.SubmitCompanyRoleConsentToAgreementsAsync(applicationId, data).ConfigureAwait(false);

        // Assert
        A.CallTo(() => _registrationBusinessLogicFake.SubmitRoleConsentAsync(applicationId, data, _identity.UserId, _identity.CompanyId)).MustHaveHappenedOnceExactly();
        result.Should().Be(1);
    }

    [Fact]
    public async Task GetCompanyIdentifiers_WithValidData_ReturnsExpected()
    {
        // Arrange
        var applicationId = _fixture.Create<Guid>();
        var data = _fixture.CreateMany<UniqueIdentifierData>(1);
        A.CallTo(() => _registrationBusinessLogicFake.GetCompanyIdentifiers("DE"))
            .Returns(data);

        //Act
        var result = await this._controller.GetCompanyIdentifiers("DE").ConfigureAwait(false);

        // Assert
        foreach (var item in result)
        {
            A.CallTo(() => _registrationBusinessLogicFake.GetCompanyIdentifiers("DE")).MustHaveHappenedOnceExactly();
            Assert.NotNull(item);
            Assert.IsType<UniqueIdentifierData?>(item);
        }
    }

    [Fact]
    public async Task GetDocumentContentFileAsync_WithValidData_ReturnsOk()
    {
        //Arrange
        const string fileName = "test.pdf";
        const string contentType = "application/pdf";
        var id = Guid.NewGuid();
        var content = Encoding.UTF8.GetBytes("This is just test content");
        A.CallTo(() => _registrationBusinessLogicFake.GetDocumentContentAsync(id, _identity.UserId))
            .Returns((fileName, content, contentType));

        //Act
        var result = await this._controller.GetDocumentContentFileAsync(id).ConfigureAwait(false);

        //Assert
        A.CallTo(() => _registrationBusinessLogicFake.GetDocumentContentAsync(id, _identity.UserId)).MustHaveHappenedOnceExactly();
        result.Should().BeOfType<FileContentResult>();
        ((FileContentResult)result).ContentType.Should().Be(contentType);
    }

    [Fact]
    public async Task GetRegistrationDocumentAsync_WithValidData_ReturnsExpected()
    {
        // Arrange
        var documentId = _fixture.Create<Guid>();
        var content = new byte[7];
        A.CallTo(() => _registrationBusinessLogicFake.GetRegistrationDocumentAsync(documentId))
            .Returns(new ValueTuple<string, byte[], string>("test.json", content, "application/json"));

        //Act
        var result = await this._controller.GetRegistrationDocumentAsync(documentId).ConfigureAwait(false);

        // Assert
        A.CallTo(() => _registrationBusinessLogicFake.GetRegistrationDocumentAsync(documentId)).MustHaveHappenedOnceExactly();
        result.Should().NotBeNull();
    }

    [Fact]
    public async Task InviteNewUserAsync_WithValidData_ReturnsExpected()
    {
        // Arrange
        var applicationId = _fixture.Create<Guid>();
        var creationInfo = _fixture.Create<UserCreationInfoWithMessage>();

        A.CallTo(() => _registrationBusinessLogicFake.InviteNewUserAsync(A<Guid>._, A<UserCreationInfoWithMessage>._, A<(Guid, Guid)>._))
            .Returns(1);

        //Act
        var result = await this._controller.InviteNewUserAsync(applicationId, creationInfo).ConfigureAwait(false);

        // Assert
        A.CallTo(() => _registrationBusinessLogicFake.InviteNewUserAsync(applicationId, creationInfo, new(_identity.UserId, _identity.CompanyId))).MustHaveHappenedOnceExactly();
        result.Should().Be(1);
    }

    [Fact]
    public async Task GetApplicationsWithStatusAsync_WithValidData_ReturnsExpected()
    {
        // Arrange
        var companyApplicationWithStatus = _fixture.CreateMany<CompanyApplicationWithStatus>(2);
        A.CallTo(() => _registrationBusinessLogicFake.GetAllApplicationsForUserWithStatus(_identity.CompanyId))
            .Returns(companyApplicationWithStatus.ToAsyncEnumerable());

        //Act
        var result = await this._controller.GetApplicationsWithStatusAsync().ToListAsync().ConfigureAwait(false);

        // Assert
        A.CallTo(() => _registrationBusinessLogicFake.GetAllApplicationsForUserWithStatus(_identity.CompanyId)).MustHaveHappenedOnceExactly();
        result.Should().HaveCount(2);
    }
}
