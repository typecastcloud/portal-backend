/********************************************************************************
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

using Microsoft.Extensions.Options;
using Org.Eclipse.TractusX.Portal.Backend.Framework.ErrorHandling;
using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.DBAccess;
using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.DBAccess.Models;
using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.DBAccess.Repositories;
using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.PortalEntities.Entities;
using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.PortalEntities.Enums;
using Org.Eclipse.TractusX.Portal.Backend.Provisioning.DBAccess;
using Org.Eclipse.TractusX.Portal.Backend.Provisioning.Library.ErrorHandling;
using Org.Eclipse.TractusX.Portal.Backend.Provisioning.Library.Models;
using ServiceAccountData = Org.Eclipse.TractusX.Portal.Backend.Provisioning.Library.Models.ServiceAccountData;

namespace Org.Eclipse.TractusX.Portal.Backend.Provisioning.Library.Service;

/// <inheritdoc />
public class ServiceAccountCreation : IServiceAccountCreation
{
    private readonly IProvisioningManager _provisioningManager;
    private readonly IPortalRepositories _portalRepositories;
    private readonly IProvisioningDBAccess _provisioningDbAccess;
    private readonly ServiceAccountCreationSettings _settings;

    /// <summary>
    /// Creates a new instance of <see cref="ServiceAccountCreation"/>
    /// </summary>
    /// <param name="provisioningManager">Access to the provisioning Manager and keycloak</param>
    /// <param name="portalRepositories">Access to the database</param>
    /// <param name="provisioningDbAccess">Access to the provisioning database</param>
    /// <param name="options">Options for the service account creation</param>
    public ServiceAccountCreation(
        IProvisioningManager provisioningManager,
        IPortalRepositories portalRepositories,
        IProvisioningDBAccess provisioningDbAccess,
        IOptions<ServiceAccountCreationSettings> options)
    {
        _provisioningManager = provisioningManager;
        _portalRepositories = portalRepositories;
        _provisioningDbAccess = provisioningDbAccess;
        _settings = options.Value;
    }

    /// <inheritdoc />
    async Task<(string clientId, ServiceAccountData serviceAccountData, Guid serviceAccountId, IEnumerable<UserRoleData> userRoleData)> IServiceAccountCreation.CreateServiceAccountAsync(
        ServiceAccountCreationInfo creationData,
        Guid companyId,
        IEnumerable<string> bpns,
        CompanyServiceAccountTypeId companyServiceAccountTypeId,
        bool enhanceTechnicalUserName,
        bool enabled,
        Action<CompanyServiceAccount>? setOptionalParameter)
    {
        var (name, description, iamClientAuthMethod, userRoleIds) = creationData;
        var serviceAccountsRepository = _portalRepositories.GetInstance<IServiceAccountRepository>();
        var userRolesRepository = _portalRepositories.GetInstance<IUserRolesRepository>();

        var userRoleData = await userRolesRepository
            .GetUserRoleDataUntrackedAsync(userRoleIds).ToListAsync().ConfigureAwait(false);
        if (userRoleData.Count != userRoleIds.Count())
        {
            var missingRoleIds = userRoleIds.Except(userRoleData.Select(x => x.UserRoleId));

            if (missingRoleIds.Any())
            {
                throw NotFoundException.Create(ProvisioningServiceErrors.USER_NOT_VALID_USERROLEID, new ErrorParameter[] { new("missingRoleIds", string.Join(", ", missingRoleIds)) });
            }
        }

        var clientId = await GetNextServiceAccountClientIdWithIdAsync().ConfigureAwait(ConfigureAwaitOptions.None);
        var enhancedName = enhanceTechnicalUserName ? $"{clientId}-{name}" : name;
        var serviceAccountData = await _provisioningManager.SetupCentralServiceAccountClientAsync(
            clientId,
            new ClientConfigRolesData(
                enhancedName,
                description,
                iamClientAuthMethod,
                userRoleData
                    .GroupBy(userRole =>
                        userRole.ClientClientId)
                    .ToDictionary(group =>
                            group.Key,
                        group => group.Select(userRole => userRole.UserRoleText))),
            enabled).ConfigureAwait(ConfigureAwaitOptions.None);

        if (bpns.Any())
        {
            await _provisioningManager.AddBpnAttributetoUserAsync(serviceAccountData.IamUserId, bpns).ConfigureAwait(ConfigureAwaitOptions.None);
            await _provisioningManager.AddProtocolMapperAsync(serviceAccountData.InternalClientId).ConfigureAwait(ConfigureAwaitOptions.None);
        }

        var identity = _portalRepositories.GetInstance<IUserRepository>().CreateIdentity(companyId, UserStatusId.ACTIVE, IdentityTypeId.COMPANY_SERVICE_ACCOUNT, null);

        var serviceAccount = serviceAccountsRepository.CreateCompanyServiceAccount(
            identity.Id,
            enhancedName,
            description,
            clientId,
            companyServiceAccountTypeId,
            setOptionalParameter);

        foreach (var roleData in userRoleData)
        {
            userRolesRepository.CreateIdentityAssignedRole(serviceAccount.Id, roleData.UserRoleId);
        }

        return (clientId, serviceAccountData, serviceAccount.Id, userRoleData);
    }

    private async Task<string> GetNextServiceAccountClientIdWithIdAsync()
    {
        var id = await _provisioningDbAccess.GetNextClientSequenceAsync().ConfigureAwait(ConfigureAwaitOptions.None);
        return $"{_settings.ServiceAccountClientPrefix}{id}";
    }
}
