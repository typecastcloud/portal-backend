/********************************************************************************
 * Copyright (c) 2021,2022 Contributors to https://github.com/lvermeulen/Keycloak.Net.git and BMW Group AG
 * Copyright (c) 2021,2022 Contributors to the CatenaX (ng) GitHub Organisation.
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

﻿using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Flurl.Http;
using Keycloak.Net.Common.Extensions;
using Keycloak.Net.Models.Common;
using Keycloak.Net.Models.Groups;
using Keycloak.Net.Models.Users;

namespace Keycloak.Net
{
    public partial class KeycloakClient
    {
        public async Task<bool> CreateGroupAsync(string realm, Group group)
        {
            var response = await (await GetBaseUrlAsync(realm).ConfigureAwait(false))
                .AppendPathSegment("/admin/realms/")
                .AppendPathSegment(realm, true)
                .AppendPathSegment("/groups")
                .PostJsonAsync(group)
                .ConfigureAwait(false);
            return response.IsSuccessStatusCode;
        }

        public async Task<IEnumerable<Group>> GetGroupHierarchyAsync(string realm, int? first = null, int? max = null, string search = null)
        {
            var queryParams = new Dictionary<string, object>
            {
                [nameof(first)] = first,
                [nameof(max)] = max,
                [nameof(search)] = search,
                ["briefRepresentation"] = false
            };

            return await (await GetBaseUrlAsync(realm).ConfigureAwait(false))
                .AppendPathSegment("/admin/realms/")
                .AppendPathSegment(realm, true)
                .AppendPathSegment("/groups")
                .SetQueryParams(queryParams)
                .GetJsonAsync<IEnumerable<Group>>()
                .ConfigureAwait(false);
        }

        public async Task<int> GetGroupsCountAsync(string realm, string search = null, bool? top = null)
        {
            var queryParams = new Dictionary<string, object>
            {
                [nameof(search)] = search,
                [nameof(top)] = top
            };

            var result = await (await GetBaseUrlAsync(realm).ConfigureAwait(false))
                .AppendPathSegment("/admin/realms/")
                .AppendPathSegment(realm, true)
                .AppendPathSegment("/groups/count")
                .SetQueryParams(queryParams)
                .GetJsonAsync()
                .ConfigureAwait(false);

            return Convert.ToInt32(DynamicExtensions.GetFirstPropertyValue(result));
        }

        public async Task<Group> GetGroupAsync(string realm, string groupId)
        {
            var result = await (await GetBaseUrlAsync(realm).ConfigureAwait(false))
                .AppendPathSegment("/admin/realms/")
                .AppendPathSegment(realm, true)
                .AppendPathSegment("/groups/")
                .AppendPathSegment(groupId, true)
                .GetJsonAsync<Group>()
                .ConfigureAwait(false);

            return result;
        }

        public async Task<bool> UpdateGroupAsync(string realm, string groupId, Group group)
        {
            var response = await (await GetBaseUrlAsync(realm).ConfigureAwait(false))
                .AppendPathSegment("/admin/realms/")
                .AppendPathSegment(realm, true)
                .AppendPathSegment("/groups/")
                .AppendPathSegment(groupId, true)
                .PutJsonAsync(group)
                .ConfigureAwait(false);
            return response.IsSuccessStatusCode;
        }

        public async Task<bool> DeleteGroupAsync(string realm, string groupId)
        {
            var response = await (await GetBaseUrlAsync(realm).ConfigureAwait(false))
                .AppendPathSegment("/admin/realms/")
                .AppendPathSegment(realm, true)
                .AppendPathSegment("/groups/")
                .AppendPathSegment(groupId, true)
                .DeleteAsync()
                .ConfigureAwait(false);
            return response.IsSuccessStatusCode;
        }

        public async Task<bool> SetOrCreateGroupChildAsync(string realm, string groupId, Group group)
        {
            var response = await (await GetBaseUrlAsync(realm).ConfigureAwait(false))
                .AppendPathSegment("/admin/realms/")
                .AppendPathSegment(realm, true)
                .AppendPathSegment("/groups/")
                .AppendPathSegment(groupId, true)
                .AppendPathSegment("/children")
                .PostJsonAsync(group)
                .ConfigureAwait(false);
            return response.IsSuccessStatusCode;
        }

        public async Task<ManagementPermission> GetGroupClientAuthorizationPermissionsInitializedAsync(string realm, string groupId) => await (await GetBaseUrlAsync(realm).ConfigureAwait(false))
            .AppendPathSegment("/admin/realms/")
            .AppendPathSegment(realm, true)
            .AppendPathSegment("/groups/")
            .AppendPathSegment(groupId, true)
            .AppendPathSegment("/management/permissions")
            .GetJsonAsync<ManagementPermission>()
            .ConfigureAwait(false);

        public async Task<ManagementPermission> SetGroupClientAuthorizationPermissionsInitializedAsync(string realm, string groupId, ManagementPermission managementPermission) =>
            await (await GetBaseUrlAsync(realm).ConfigureAwait(false))
                .AppendPathSegment("/admin/realms/")
                .AppendPathSegment(realm, true)
                .AppendPathSegment("/groups/")
                .AppendPathSegment(groupId, true)
                .AppendPathSegment("/management/permissions")
                .PutJsonAsync(managementPermission)
                .ReceiveJson<ManagementPermission>()
                .ConfigureAwait(false);

        public async Task<IEnumerable<User>> GetGroupUsersAsync(string realm, string groupId, int? first = null, int? max = null) 
        {
            var queryParams = new Dictionary<string, object>
            {
                [nameof(first)] = first,
                [nameof(max)] = max
            };

            return await (await GetBaseUrlAsync(realm).ConfigureAwait(false))
                .AppendPathSegment("/admin/realms/")
                .AppendPathSegment(realm, true)
                .AppendPathSegment("/groups/")
                .AppendPathSegment(groupId, true)
                .AppendPathSegment("/members")
                .SetQueryParams(queryParams)
                .GetJsonAsync<IEnumerable<User>>()
                .ConfigureAwait(false);
        }
    }
}
