/********************************************************************************
 * Copyright (c) 2021, 2024 Contributors to the Eclipse Foundation
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

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Org.Eclipse.TractusX.Portal.Backend.Administration.Service.BusinessLogic;
using Org.Eclipse.TractusX.Portal.Backend.Administration.Service.Models;
using Org.Eclipse.TractusX.Portal.Backend.Framework.ErrorHandling.Web;
using Org.Eclipse.TractusX.Portal.Backend.Framework.Models;
using Org.Eclipse.TractusX.Portal.Backend.Framework.Web;
using Org.Eclipse.TractusX.Portal.Backend.Keycloak.Authentication;
using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.DBAccess.Models;
using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.PortalEntities.Enums;
using Org.Eclipse.TractusX.Portal.Backend.Web.Identity;
using System.Net;

namespace Org.Eclipse.TractusX.Portal.Backend.Administration.Service.Controllers;

/// <summary>
/// Creates a new instance of <see cref="CompanyDataController"/>
/// </summary>
[ApiController]
[EnvironmentRoute("MVC_ROUTING_BASEPATH", "companydata")]
[Produces("application/json")]
[Consumes("application/json")]
public class CompanyDataController : ControllerBase
{
    private readonly ICompanyDataBusinessLogic _logic;

    /// <summary>
    /// Creates a new instance of <see cref="CompanyDataController"/>
    /// </summary>
    /// <param name="logic">The company data business logic</param>
    public CompanyDataController(ICompanyDataBusinessLogic logic)
    {
        _logic = logic;
    }

    /// <summary>
    /// Gets the company with its address
    /// </summary>
    /// <returns>the company with its address</returns>
    /// <remarks>Example: GET: api/administration/companydata/ownCompanyDetails</remarks>
    /// <response code="200">Returns the company with its address.</response>
    /// <response code="409">user is not associated with  company.</response>
    [HttpGet]
    [Authorize(Roles = "view_company_data")]
    [Authorize(Policy = PolicyTypes.ValidCompany)]
    [Route("ownCompanyDetails")]
    [ProducesResponseType(typeof(CompanyAddressDetailData), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status409Conflict)]
    public Task<CompanyAddressDetailData> GetOwnCompanyDetailsAsync() =>
        _logic.GetCompanyDetailsAsync();

    /// <summary>
    /// Gets the CompanyAssigned UseCase details
    /// </summary>
    /// <returns>the CompanyAssigned UseCase details</returns>
    /// <remarks>Example: GET: api/administration/companydata/preferredUseCases</remarks>
    /// <response code="200">Returns the CompanyAssigned UseCase details.</response>
    [HttpGet]
    [Authorize(Roles = "view_use_cases")]
    [Authorize(Policy = PolicyTypes.ValidCompany)]
    [Route("preferredUseCases")]
    [ProducesResponseType(typeof(CompanyAssignedUseCaseData), StatusCodes.Status200OK)]
    public IAsyncEnumerable<CompanyAssignedUseCaseData> GetCompanyAssigendUseCaseDetailsAsync() =>
       _logic.GetCompanyAssigendUseCaseDetailsAsync();

    /// <summary>
    /// Create the CompanyAssigned UseCase details
    /// </summary>
    /// <remarks>Example: POST: api/administration/companydata/preferredUseCases</remarks>
    /// <response code="204">NoContentResult</response>
    /// <response code="208">UseCaseId already existis</response>
    /// <response code="409">Company Status is Incorrect</response>
    [HttpPost]
    [Authorize(Roles = "set_company_use_cases")]
    [Authorize(Policy = PolicyTypes.ValidCompany)]
    [Route("preferredUseCases")]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status409Conflict)]
    public async Task<StatusCodeResult> CreateCompanyAssignedUseCaseDetailsAsync([FromBody] UseCaseIdDetails data) =>
        await _logic.CreateCompanyAssignedUseCaseDetailsAsync(data.useCaseId).ConfigureAwait(ConfigureAwaitOptions.None)
            ? StatusCode((int)HttpStatusCode.Created)
            : NoContent();

    /// <summary>
    /// Remove the CompanyAssigned UseCase details by UseCaseId
    /// </summary>
    /// <remarks>Example: DELETE: api/administration/companydata/preferredUseCases</remarks>
    /// <response code="204">NoContentResult</response>
    /// <response code="409">
    /// Company Status is Incorrect <br />
    /// UseCaseId is not available
    /// </response>
    [HttpDelete]
    [Authorize(Roles = "set_company_use_cases")]
    [Authorize(Policy = PolicyTypes.ValidCompany)]
    [Route("preferredUseCases")]
    [ProducesResponseType(typeof(NoContentResult), StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status409Conflict)]
    public async Task<NoContentResult> RemoveCompanyAssignedUseCaseDetailsAsync([FromBody] UseCaseIdDetails data)
    {
        await _logic.RemoveCompanyAssignedUseCaseDetailsAsync(data.useCaseId).ConfigureAwait(ConfigureAwaitOptions.None);
        return NoContent();
    }

    /// <summary>
    /// Gets the companyrole and ConsentAgreement Details
    /// </summary>
    /// <returns>the Companyrole and ConsentAgreement details</returns>
    /// <remarks>Example: GET: api/administration/companydata/companyRolesAndConsents</remarks>
    /// <response code="200">Returns the Companyrole and Consent details.</response>
    /// <response code="400">languageShortName is not valid</response>
    /// <response code="404">CompanyId does not exist in company</response>
    /// <response code="409">No Companyrole or Incorrect Status</response>
    [HttpGet]
    [Authorize(Roles = "view_company_data")]
    [Authorize(Policy = PolicyTypes.ValidCompany)]
    [Route("companyRolesAndConsents")]
    [ProducesResponseType(typeof(CompanyRoleConsentViewData), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status409Conflict)]
    public IAsyncEnumerable<CompanyRoleConsentViewData> GetCompanyRoleAndConsentAgreementDetailsAsync([FromQuery] string? languageShortName = null) =>
        _logic.GetCompanyRoleAndConsentAgreementDetailsAsync(languageShortName);

    /// <summary>
    /// Post the companyrole and Consent Details
    /// </summary>
    /// <returns>Create Companyrole and Consent details</returns>
    /// <remarks>Example: POST: api/administration/companydata/companyRolesAndConsents</remarks>
    /// <response code="204">Created the Companyrole and Consent details.</response>
    /// <response code="400">
    /// All agreement need to get signed as Active or InActive <br />
    /// Agreements not associated with requested companyRoles
    /// </response>
    /// <response code="409">
    /// Company does not exists <br />
    /// Company is in Incorrect state <br />
    /// Company can't unassign from all roles
    /// </response>
    [HttpPost]
    [Authorize(Roles = "view_company_data")]
    [Authorize(Policy = PolicyTypes.ValidIdentity)]
    [Authorize(Policy = PolicyTypes.ValidCompany)]
    [Route("companyRolesAndConsents")]
    [ProducesResponseType(typeof(NoContentResult), StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status409Conflict)]
    public async Task<NoContentResult> CreateCompanyRoleAndConsentAgreementDetailsAsync([FromBody] IEnumerable<CompanyRoleConsentDetails> companyRoleConsentDetails)
    {
        await _logic.CreateCompanyRoleAndConsentAgreementDetailsAsync(companyRoleConsentDetails);
        return NoContent();
    }

    /// <summary>
    /// Gets all use case frameworks and the participation status of the acting company
    /// </summary>
    /// <remarks>Example: Get: api/administration/companydata/useCaseParticipation</remarks>
    /// <returns>All UseCaseParticipations and the particiation status of the acting company</returns>
    /// <response code="200">Returns a collection of UseCaseParticipation.</response>
    /// <response code="409">There should only be one pending or active SSI detail be assigned</response>
    [HttpGet]
    [Authorize(Roles = "view_use_case_participation")]
    [Authorize(Policy = PolicyTypes.ValidCompany)]
    [Route("useCaseParticipation")]
    [ProducesResponseType(typeof(IEnumerable<UseCaseParticipationData>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status409Conflict)]
    public Task<IEnumerable<UseCaseParticipationData>> GetUseCaseParticipation([FromQuery] string? language) =>
        _logic.GetUseCaseParticipationAsync(language);

    /// <summary>
    /// Gets all company certificate requests and their status
    /// </summary>
    /// <returns>All SSI certifications of the own company</returns>
    /// <remarks>Example: Get: api/administration/companydata/certificates</remarks>
    /// <response code="200">Returns a collection of certificates.</response>
    /// <response code="409">There should only be one pending or active SSI detail be assigned</response>
    [HttpGet]
    [Authorize(Roles = "view_certificates")]
    [Authorize(Policy = PolicyTypes.ValidCompany)]
    [Route("certificates")]
    [ProducesResponseType(typeof(IEnumerable<SsiCertificateTransferData>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status409Conflict)]
    public Task<IEnumerable<SsiCertificateData>> GetSsiCertificationData() =>
        _logic.GetSsiCertificatesAsync();

    /// <summary>
    /// Gets the certificate types for which the company can apply for
    /// </summary>
    /// <returns>All certificate types for which the company can apply for</returns>
    /// <remarks>Example: Get: api/administration/companydata/certificateTypes</remarks>
    /// <response code="200">Returns a collection of VerifiedCredentialTypeIds.</response>
    [HttpGet]
    [Authorize(Roles = "request_ssicredential")]
    [Authorize(Policy = PolicyTypes.ValidCompany)]
    [Route("certificateTypes")]
    [ProducesResponseType(typeof(IEnumerable<SsiCertificateTransferData>), StatusCodes.Status200OK)]
    public IAsyncEnumerable<VerifiedCredentialTypeId> GetCertificateTypes() =>
        _logic.GetCertificateTypes();

    /// <summary>
    /// Creates the use case participation request
    /// </summary>
    /// <param name="data">The type and document</param>
    /// <param name="cancellationToken">Cancellation Token</param>
    /// <returns>The id of the created use case participation request</returns>
    /// <remarks>Example: POST: api/administration/companydata/useCaseParticipation</remarks>
    /// <response code="204">Successfully created the use case participation request.</response>
    /// <response code="400">
    /// VerifiedCredentialExternalTypeDetailId does not exist <br />
    /// Credential request already exist
    /// </response>
    [HttpPost]
    [Consumes("multipart/form-data")]
    [Authorize(Roles = "request_ssicredential")]
    [Authorize(Policy = PolicyTypes.ValidIdentity)]
    [Authorize(Policy = PolicyTypes.ValidCompany)]
    [Route("useCaseParticipation")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    public Task<Guid> CreateUseCaseParticipation([FromForm] UseCaseParticipationCreationData data, CancellationToken cancellationToken) =>
        this.WithBearerToken(token => _logic.CreateUseCaseParticipation(data, token, cancellationToken));

    /// <summary>
    /// Creates the SSI Certificate request
    /// </summary>
    /// <param name="data">The type and document</param>
    /// <param name="cancellationToken">Cancellation Token</param>
    /// <returns>The id of the created SSI certificate request</returns>
    /// <remarks>Example: POST: api/administration/companydata/certificates</remarks>
    /// <response code="204">Successfully created the SSI certificate request.</response>
    /// <response code="400">
    /// credentialTypeId is not assigned to a certificate <br />
    /// Credential request already exist
    /// </response>
    [HttpPost]
    [Consumes("multipart/form-data")]
    [Authorize(Roles = "request_ssicredential")]
    [Authorize(Policy = PolicyTypes.ValidIdentity)]
    [Authorize(Policy = PolicyTypes.ValidCompany)]
    [Route("certificates")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    public async Task<NoContentResult> CreateSsiCertificate([FromForm] SsiCertificateCreationData data, CancellationToken cancellationToken)
    {
        await _logic.CreateSsiCertificate(data, cancellationToken).ConfigureAwait(ConfigureAwaitOptions.None);
        return NoContent();
    }

    /// <summary>
    /// Creates the Company Certificate request
    /// </summary>
    /// <param name="data">The type and document</param>
    /// <param name="cancellationToken">Cancellation Token</param>
    /// <returns>The id of the created Company certificate request</returns>
    /// <remarks>Example: POST: api/administration/companydata/companyCertificate</remarks>
    /// <response code="204">Successfully created the Company certificate request.</response>
    /// <response code="400">   
    /// </response>
    [HttpPost]
    [Consumes("multipart/form-data")]
    [Authorize(Roles = "upload_certificates")]
    [Authorize(Policy = PolicyTypes.ValidIdentity)]
    [Authorize(Policy = PolicyTypes.ValidCompany)]
    [RequestFormLimits(ValueLengthLimit = 2000000, MultipartBodyLengthLimit = 2000000)]
    [Route("companyCertificate")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    public async Task<NoContentResult> CreateCompanyCertificate([FromForm] CompanyCertificateCreationData data, CancellationToken cancellationToken)
    {
        await _logic.CreateCompanyCertificate(data, cancellationToken).ConfigureAwait(ConfigureAwaitOptions.None);
        return NoContent();
    }

    /// <summary>
    /// Gets the companyCertificates Details
    /// </summary>
    /// <returns>the companyCertificates details</returns>
    /// <remarks>Example: GET: api/administration/companydata/businessPartnerNumber}/companyCertificates</remarks>
    /// <response code="200">Returns the companyCertificates details.</response>   
    [HttpGet]
    [Authorize(Roles = "view_certificates")]
    [Authorize(Policy = PolicyTypes.ValidCompany)]
    [Route("company/{businessPartnerNumber}/companyCertificates")]
    [ProducesResponseType(typeof(IEnumerable<CompanyCertificateBpnData>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status409Conflict)]
    public IAsyncEnumerable<CompanyCertificateBpnData> GetCompanyCertificatesByBpn(string businessPartnerNumber) =>
           _logic.GetCompanyCertificatesByBpn(businessPartnerNumber);

    /// <summary>
    /// Retrieves all company certificates with respect userId.
    /// </summary>
    /// <param name="page" example="0">Optional the page of company certificate.</param>
    /// <param name="size" example="15">Amount of company certificate, default is 15.</param>
    /// <param name="sorting" example="CertificateTypeAsc">Optional Sorting of the pagination</param>
    /// <param name="certificateStatus" example="">Optional filter for company certificate status</param>
    /// <param name="certificateType" example="">Optional filter for company certificate type</param>
    /// <returns>Collection of all active company certificates.</returns>
    /// <remarks>Example: GET /api/administration/companydata/companyCertificates</remarks>
    /// <response code="200">Returns the list of all active company certificates.</response>
    [HttpGet]
    [Route("companyCertificates")]
    [Authorize(Roles = "view_certificates")]
    [Authorize(Policy = PolicyTypes.ValidIdentity)]
    [Authorize(Policy = PolicyTypes.ValidCompany)]
    [ProducesResponseType(typeof(Pagination.Response<CompanyCertificateData>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public Task<Pagination.Response<CompanyCertificateData>> GetAllCompanyCertificatesAsync([FromQuery] int page = 0, [FromQuery] int size = 15, [FromQuery] CertificateSorting? sorting = null, [FromQuery] CompanyCertificateStatusId? certificateStatus = null, [FromQuery] CompanyCertificateTypeId? certificateType = null) =>
        _logic.GetAllCompanyCertificatesAsync(page, size, sorting, certificateStatus, certificateType);

    /// <summary>
    /// Retrieves a specific company certificate document for the given documentid and companyuserid.
    /// </summary>
    /// <param name="documentId" example="4ad087bb-80a1-49d3-9ba9-da0b175cd4e3">Id of the document to get.</param>
    /// <returns>Returns the file.</returns>
    /// <remarks>Example: GET /api/administration/companydata/companyCertificates/4ad087bb-80a1-49d3-9ba9-da0b175cd4e3</remarks>
    /// <response code="200">Returns the file.</response>   
    /// <response code="404">The document was not found.</response>    
    [HttpGet]
    [Route("companyCertificates/{documentId}")]
    [Authorize(Roles = "view_certificates")]
    [Authorize(Policy = PolicyTypes.ValidCompany)]
    [Authorize(Policy = PolicyTypes.CompanyUser)]
    [ProducesResponseType(typeof(FileContentResult), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status503ServiceUnavailable)]
    public async Task<ActionResult> GetCompanyCertificateSpecificDocumentContentFileAsync([FromRoute] Guid documentId)
    {
        var (fileName, content, mediaType) = await _logic.GetCompanyCertificateDocumentByCompanyIdAsync(documentId).ConfigureAwait(ConfigureAwaitOptions.None);
        return File(content, mediaType, fileName);
    }

    /// <summary>
    /// Retrieves a specific company certificate document for the given id.
    /// </summary>
    /// <param name="documentId" example="4ad087bb-80a1-49d3-9ba9-da0b175cd4e3">Id of the document to get.</param>
    /// <returns>Returns the file.</returns>
    /// <remarks>Example: GET /api/administration/companydata/companyCertificates/documents/4ad087bb-80a1-49d3-9ba9-da0b175cd4e3</remarks>
    /// <response code="200">Returns the file.</response>
    /// <response code="403">The document which is not in status "ACTIVE".</response>
    /// <response code="404">The document was not found.</response>
    /// <response code="503">document Content is null.</response>
    [HttpGet]
    [Route("companyCertificates/documents/{documentId}")]
    [Authorize(Roles = "view_certificates")]
    [Authorize(Policy = PolicyTypes.ValidCompany)]
    [Authorize(Policy = PolicyTypes.CompanyUser)]
    [ProducesResponseType(typeof(FileContentResult), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status503ServiceUnavailable)]
    public async Task<ActionResult> GetCompanyCertificateDocumentContentFileAsync([FromRoute] Guid documentId)
    {
        var (fileName, content, mediaType) = await _logic.GetCompanyCertificateDocumentAsync(documentId).ConfigureAwait(ConfigureAwaitOptions.None);
        return File(content, mediaType, fileName);
    }

    /// <summary>
    /// Deletes the company certificate with the given id
    /// </summary>
    /// <param name="documentId" example="4ad087bb-80a1-49d3-9ba9-da0b175cd4e3"></param>
    /// <returns></returns>
    /// <remarks>Example: Delete: /api/administration/companydata/companyCertificate/document/{documentId}</remarks>
    /// <response code="200">Successfully deleted the company certificate</response>
    /// <response code="400">Incorrect document state</response>
    /// <response code="403">The user is not assigned with the Company.</response>    
    [HttpDelete]
    [Authorize(Roles = "delete_certificates")]
    [Authorize(Policy = PolicyTypes.ValidCompany)]
    [Route("companyCertificate/document/{documentId}")]
    [ProducesResponseType(typeof(int), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status409Conflict)]
    public Task<int> DeleteCompanyCertificate([FromRoute] Guid documentId) =>
        _logic.DeleteCompanyCertificateAsync(documentId);

    /// <summary>
    /// Gets all outstanding, existing and inactive credentials
    /// </summary>
    /// <remarks>Example: Get: /api/administration/companydata/credentials/</remarks>
    /// <param name="page">The page to get</param>
    /// <param name="size">Amount of entries</param>
    /// <param name="companySsiDetailStatusId">OPTIONAL: Filter for the status</param>
    /// <param name="credentialTypeId">OPTIONAL: The type of the credential that should be returned</param>
    /// <param name="companyName">OPTIONAL: Search string for the company name</param>
    /// <param name="sorting">Defines the sorting of the list</param>
    /// <response code="200">Collection of the credentials.</response>
    [HttpGet]
    [Authorize(Roles = "decision_ssicredential")]
    [Authorize(Policy = PolicyTypes.ValidIdentity)]
    [Route("credentials", Name = nameof(GetCredentials))]
    [ProducesResponseType(typeof(IEnumerable<CredentialDetailData>), StatusCodes.Status200OK)]
    public Task<Pagination.Response<CredentialDetailData>> GetCredentials(
        [FromQuery] int page = 0,
        [FromQuery] int size = 15,
        [FromQuery] CompanySsiDetailStatusId? companySsiDetailStatusId = null,
        [FromQuery] VerifiedCredentialTypeId? credentialTypeId = null,
        [FromQuery] string? companyName = null,
        [FromQuery] CompanySsiDetailSorting? sorting = null) =>
        _logic.GetCredentials(page, size, companySsiDetailStatusId, credentialTypeId, companyName, sorting);

    /// <summary>
    /// Approves the given credential and triggers the verified credential creation
    /// </summary>
    /// <remarks>Example: PUT: api/administration/companydata/credentials/{credentialId}/approval</remarks>
    /// <param name="credentialId">Id of the entry that should be approved</param>
    /// <param name="cts">Cancellation Token</param>
    /// <returns>No Content</returns>
    /// <response code="204">Successfully approved the credentials and triggered the verified credential creation.</response>
    /// <response code="404">CompanySsiDetail does not exists</response>
    /// <response code="409">
    /// Credential is in Incorrect State <br />
    /// VerifiedCredentialTypeKindId must not be null
    /// </response>
    [HttpPut]
    [Authorize(Roles = "decision_ssicredential")]
    [Authorize(Policy = PolicyTypes.CompanyUser)]
    [Route("credentials/{credentialId}/approval")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status409Conflict)]
    public async Task<NoContentResult> ApproveCredential([FromRoute] Guid credentialId, CancellationToken cts)
    {
        await _logic.ApproveCredential(credentialId, cts).ConfigureAwait(ConfigureAwaitOptions.None);
        return NoContent();
    }

    /// <summary>
    /// Rejects the given credential
    /// </summary>
    /// <remarks>Example: PUT: api/administration/companydata/credentials/{credentialId}/reject</remarks>
    /// <param name="credentialId">Id of the entry that should be rejected</param>
    /// <returns>No Content</returns>
    /// <response code="204">Successfully rejected the credential.</response>
    /// <response code="404">CompanySsiDetail does not exists</response>
    /// <response code="409">CredentialSsiDetail is in Incorrect State</response>
    [HttpPut]
    [Authorize(Roles = "decision_ssicredential")]
    [Authorize(Policy = PolicyTypes.CompanyUser)]
    [Route("credentials/{credentialId}/reject")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status409Conflict)]
    public async Task<NoContentResult> RejectCredential([FromRoute] Guid credentialId)
    {
        await _logic.RejectCredential(credentialId).ConfigureAwait(ConfigureAwaitOptions.None);
        return NoContent();
    }

    /// <summary>
    /// Rejects the given credential
    /// </summary>
    /// <remarks>Example: GET: api/administration/companydata/decentralidentity/urls</remarks>
    /// <returns>The dim service urls</returns>
    /// <response code="200">Returns the dim urls.</response>
    [HttpGet]
    [Authorize(Roles = "view_company_data")]
    [Authorize(Policy = PolicyTypes.ValidCompany)]
    [Route("decentralidentity/urls")]
    [ProducesResponseType(typeof(DimUrlsResponse), StatusCodes.Status200OK)]
    public Task<DimUrlsResponse> GetDimServiceUrls() =>
        _logic.GetDimServiceUrls();
}
