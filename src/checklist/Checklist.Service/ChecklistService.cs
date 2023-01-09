/********************************************************************************
 * Copyright (c) 2021,2022 Microsoft and BMW Group AG
 * Copyright (c) 2021,2022 Contributors to the Eclipse Foundation
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

using Org.Eclipse.TractusX.Portal.Backend.Checklist.Service.Bpdm;
using Org.Eclipse.TractusX.Portal.Backend.Checklist.Service.Bpdm.Models;
using Org.Eclipse.TractusX.Portal.Backend.Framework.ErrorHandling;
using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.DBAccess;
using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.DBAccess.Repositories;
using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.PortalEntities.Enums;

namespace Org.Eclipse.TractusX.Portal.Backend.Checklist.Service;

public class ChecklistService : IChecklistService
{
    private readonly IPortalRepositories _portalRepositories;
    private readonly IBpdmService _bpdmService;

    public ChecklistService(IPortalRepositories portalRepositories, IBpdmService bpdmService)
    {
        _portalRepositories = portalRepositories;
        _bpdmService = bpdmService;
    }

    /// <inheritdoc />
    public async Task CreateInitialChecklistAsync(Guid applicationId)
    {
        var bpn =  await _portalRepositories.GetInstance<IApplicationRepository>().GetBpnForApplicationIdAsync(applicationId).ConfigureAwait(false);
        var checklistEntries = Enum.GetValues<ChecklistEntryTypeId>()
            .Select(x => 
                new ValueTuple<ChecklistEntryTypeId, ChecklistEntryStatusId>(x, GetChecklistStatus(x, bpn))
            );
        _portalRepositories.GetInstance<IApplicationChecklistRepository>()
            .CreateChecklistForApplication(applicationId, checklistEntries);
    }

    /// <inheritdoc />
    public async Task TriggerBpnDataPush(Guid applicationId, string iamUserId, CancellationToken cancellationToken)
    {
        var data = await _portalRepositories.GetInstance<ICompanyRepository>().GetBpdmDataForApplicationAsync(iamUserId, applicationId).ConfigureAwait(false);
        if (data is null)
        {
            throw new NotFoundException($"Application {applicationId} does not exists.");
        }

        if (data.ApplicationStatusId != CompanyApplicationStatusId.SUBMITTED)
        {
            throw new ArgumentException($"CompanyApplication {applicationId} is not in status SUBMITTED", nameof(applicationId));
        }

        if (!data.IsUserInCompany)
        {
            throw new ControllerArgumentException("User is not assigned to company", nameof(iamUserId));
        }

        if (string.IsNullOrWhiteSpace(data.ZipCode))
        {
            throw new ConflictException("ZipCode must not be empty");
        }

        await CheckCanRunStepAsync(applicationId, ChecklistEntryTypeId.BUSINESS_PARTNER_NUMBER, new []{ ChecklistEntryStatusId.TO_DO, ChecklistEntryStatusId.FAILED }).ConfigureAwait(false);
        var bpdmTransferData = new BpdmTransferData(data.CompanyName, data.AlphaCode2, data.ZipCode, data.City, data.Street);
        await _bpdmService.TriggerBpnDataPush(bpdmTransferData, cancellationToken).ConfigureAwait(false);
        await this.UpdateBpnStatusAsync(applicationId, ChecklistEntryStatusId.IN_PROGRESS).ConfigureAwait(false);
    }

    /// <inheritdoc />
    public async Task UpdateBpnStatusAsync(Guid applicationId, ChecklistEntryStatusId statusId)
    {
        _portalRepositories.GetInstance<IApplicationChecklistRepository>()
            .AttachAndModifyApplicationChecklist(applicationId, ChecklistEntryTypeId.BUSINESS_PARTNER_NUMBER, checklist =>
            {
                checklist.StatusId = statusId;
            });
        await _portalRepositories.SaveAsync().ConfigureAwait(false);
    }

    /// <summary>
    /// Checks whether the given step can be executed
    /// </summary>
    /// <param name="applicationId">id of the application</param>
    /// <param name="step">the step that should be executed</param>
    /// <param name="allowedStatus"></param>
    /// <exception cref="ConflictException">Exception will be thrown if the possible steps don't contain the requested step.</exception>
    private async Task CheckCanRunStepAsync(Guid applicationId, ChecklistEntryTypeId step, ChecklistEntryStatusId[] allowedStatus)
    {
        var checklistData = await _portalRepositories.GetInstance<IApplicationChecklistRepository>()
            .GetChecklistDataAsync(applicationId).ConfigureAwait(false);

        var possibleSteps = GetNextPossibleTypesWithMatchingStatus(checklistData, allowedStatus);
        if (!possibleSteps.Contains(step))
        {
            throw new ConflictException($"{ChecklistEntryTypeId.BUSINESS_PARTNER_NUMBER} is not available as next step");
        }
    }

    private static IEnumerable<ChecklistEntryTypeId> GetNextPossibleTypesWithMatchingStatus(IDictionary<ChecklistEntryTypeId, ChecklistEntryStatusId> currentStatus, ChecklistEntryStatusId[] checklistEntryStatusIds)
    {
        currentStatus.TryGetValue(ChecklistEntryTypeId.BUSINESS_PARTNER_NUMBER, out var bpnStatus);
        currentStatus.TryGetValue(ChecklistEntryTypeId.REGISTRATION_VERIFICATION, out var registrationStatus);
        currentStatus.TryGetValue(ChecklistEntryTypeId.IDENTITY_WALLET, out var identityStatus);
        currentStatus.TryGetValue(ChecklistEntryTypeId.CLEARING_HOUSE, out var clearingHouseStatus);
        currentStatus.TryGetValue(ChecklistEntryTypeId.SELF_DESCRIPTION_LP, out var selfDescriptionStatus);

        var possibleTypes = new List<ChecklistEntryTypeId>();
        if (checklistEntryStatusIds.Contains(bpnStatus))
        {
            possibleTypes.Add(ChecklistEntryTypeId.BUSINESS_PARTNER_NUMBER);
        }
        if (checklistEntryStatusIds.Contains(registrationStatus))
        {
            possibleTypes.Add(ChecklistEntryTypeId.REGISTRATION_VERIFICATION);
        }
        if (checklistEntryStatusIds.Contains(identityStatus) && bpnStatus == ChecklistEntryStatusId.DONE && registrationStatus == ChecklistEntryStatusId.DONE)
        {
            possibleTypes.Add(ChecklistEntryTypeId.IDENTITY_WALLET);
        }
        if (checklistEntryStatusIds.Contains(clearingHouseStatus) && identityStatus == ChecklistEntryStatusId.DONE)
        {
            possibleTypes.Add(ChecklistEntryTypeId.CLEARING_HOUSE);
        }
        if (checklistEntryStatusIds.Contains(selfDescriptionStatus) && clearingHouseStatus == ChecklistEntryStatusId.DONE)
        {
            possibleTypes.Add(ChecklistEntryTypeId.SELF_DESCRIPTION_LP);
        }

        return possibleTypes;
    }

    private static ChecklistEntryStatusId GetChecklistStatus(ChecklistEntryTypeId checklistEntryTypeId, string? bpn) =>
        checklistEntryTypeId switch
        {
            ChecklistEntryTypeId.BUSINESS_PARTNER_NUMBER => string.IsNullOrWhiteSpace(bpn)
                ? ChecklistEntryStatusId.TO_DO
                : ChecklistEntryStatusId.DONE,
            _ => ChecklistEntryStatusId.TO_DO
        };
}
