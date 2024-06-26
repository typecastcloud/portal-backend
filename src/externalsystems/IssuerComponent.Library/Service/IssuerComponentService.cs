/********************************************************************************
 * Copyright (c) 2024 Contributors to the Eclipse Foundation
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
using Org.Eclipse.TractusX.Portal.Backend.Framework.HttpClientExtensions;
using Org.Eclipse.TractusX.Portal.Backend.Framework.Token;
using Org.Eclipse.TractusX.Portal.Backend.IssuerComponent.Library.DependencyInjection;
using Org.Eclipse.TractusX.Portal.Backend.IssuerComponent.Library.Models;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;

namespace Org.Eclipse.TractusX.Portal.Backend.IssuerComponent.Library.Service;

public class IssuerComponentService(ITokenService tokenService, IHttpClientFactory httpClientFactory, IOptions<IssuerComponentSettings> options)
    : IIssuerComponentService
{
    private static readonly JsonSerializerOptions Options = new() { PropertyNamingPolicy = JsonNamingPolicy.CamelCase };
    private readonly IssuerComponentSettings _settings = options.Value;

    public async Task<bool> CreateBpnlCredential(CreateBpnCredentialRequest data, CancellationToken cancellationToken)
    {
        using var httpClient = await tokenService.GetAuthorizedClient<IssuerComponentService>(_settings, cancellationToken).ConfigureAwait(false);
        await httpClient.PostAsJsonAsync("/api/issuer/bpn", data, Options, cancellationToken)
            .CatchingIntoServiceExceptionFor("issuer-component-bpn-post", HttpAsyncResponseMessageExtension.RecoverOptions.INFRASTRUCTURE).ConfigureAwait(false);
        return true;
    }

    public async Task<bool> CreateMembershipCredential(CreateMembershipCredentialRequest data, CancellationToken cancellationToken)
    {
        using var httpClient = await tokenService.GetAuthorizedClient<IssuerComponentService>(_settings, cancellationToken).ConfigureAwait(false);
        await httpClient.PostAsJsonAsync("/api/issuer/membership", data, Options, cancellationToken)
            .CatchingIntoServiceExceptionFor("issuer-component-membership-post", HttpAsyncResponseMessageExtension.RecoverOptions.INFRASTRUCTURE).ConfigureAwait(false);
        return true;
    }

    public async Task<Guid> CreateFrameworkCredential(CreateFrameworkCredentialRequest data, string token, CancellationToken cancellationToken)
    {
        using var httpClient = httpClientFactory.CreateClient(nameof(IssuerComponentService));
        httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        var result = await httpClient.PostAsJsonAsync("/api/issuer/framework", data, Options, cancellationToken)
            .CatchingIntoServiceExceptionFor("issuer-component-framework-post", HttpAsyncResponseMessageExtension.RecoverOptions.INFRASTRUCTURE).ConfigureAwait(false);
        return await result.Content.ReadFromJsonAsync<Guid>(cancellationToken).ConfigureAwait(ConfigureAwaitOptions.None);
    }
}
