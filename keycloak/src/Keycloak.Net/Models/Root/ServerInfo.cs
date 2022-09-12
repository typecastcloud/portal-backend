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

﻿using System.Collections.Generic;
using Newtonsoft.Json;

namespace Keycloak.Net.Models.Root
{
    public class ServerInfo
    {
        [JsonProperty("systemInfo")]
        public SystemInfo SystemInfo { get; set; }

        [JsonProperty("memoryInfo")]
        public MemoryInfo MemoryInfo { get; set; }

        [JsonProperty("profileInfo")]
        public ProfileInfo ProfileInfo { get; set; }

        [JsonProperty("themes")]
        public Themes Themes { get; set; }

        [JsonProperty("socialProviders")]
        public List<Provider> SocialProviders { get; set; }

        [JsonProperty("identityProviders")]
        public List<Provider> IdentityProviders { get; set; }

        [JsonProperty("providers")]
        public ServerInfoProviders Providers { get; set; }

        [JsonProperty("protocolMapperTypes")]
        public ProtocolMapperTypes ProtocolMapperTypes { get; set; }

        [JsonProperty("builtinProtocolMappers")]
        public BuiltinProtocolMappers BuiltinProtocolMappers { get; set; }

        [JsonProperty("clientInstallations")]
        public ClientInstallations ClientInstallations { get; set; }

        [JsonProperty("componentTypes")]
        public ComponentTypes ComponentTypes { get; set; }

        [JsonProperty("passwordPolicies")]
        public List<PasswordPolicy> PasswordPolicies { get; set; }

        [JsonProperty("enums")]
        public Enums Enums { get; set; }
    }
}
