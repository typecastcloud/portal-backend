/********************************************************************************
 * Copyright (c) 2021,2022 BMW Group AG
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

using Flurl.Http;
using Flurl.Http.Configuration;

namespace CatenaX.NetworkServices.Keycloak.ErrorHandling;

public class FlurlUntrustedCertExceptionHandler
{
    public static void ConfigureExceptions(IEnumerable<string> urlsToTrust)
    {
        foreach (var urlToTrust in urlsToTrust)
        {
            FlurlHttp.ConfigureClient(urlToTrust, cli => cli.Settings.HttpClientFactory = new UntrustedCertHttpClientFactory());
        }
    }
}

public class UntrustedCertHttpClientFactory : DefaultHttpClientFactory
{
    public override HttpMessageHandler CreateMessageHandler() =>
        new HttpClientHandler { ServerCertificateCustomValidationCallback = (_,_,_,_) => true };
}
