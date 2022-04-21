﻿using CatenaX.NetworkServices.PortalBackend.PortalEntities.Enums;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace CatenaX.NetworkServices.PortalBackend.PortalEntities.Entities
{
    public class Agreement
    {
        private Agreement()
        {
            Name = null!;
            Consents = new HashSet<Consent>();
            AgreementAssignedCompanyRoles = new HashSet<AgreementAssignedCompanyRole>();
            AgreementAssignedDocumentTemplates = new HashSet<AgreementAssignedDocumentTemplate>();
        }

        public Agreement(Guid id, AgreementCategoryId agreementCategoryId, string name, DateTime dateCreated) : this()
        {
            Id = id;
            AgreementCategoryId = agreementCategoryId;
            Name = name;
            DateCreated = dateCreated;
        }

        public AgreementCategoryId AgreementCategoryId { get; private set; }

        [Key]
        public Guid Id { get; private set; }

        public DateTime DateCreated { get; private set; }

        public DateTime? DateLastChanged { get; set; }

        [MaxLength(255)]
        public string? AgreementType { get; set; }

        [MaxLength(255)]
        public string Name { get; set; }

        public Guid? AppId { get; set; }

        public Guid IssuerCompanyId { get; set; }

        public Guid? UseCaseId { get; set; }

        public virtual AgreementCategory? AgreementCategory { get; set; }
        public virtual App? App { get; set; }
        public virtual Company? IssuerCompany { get; set; }
        public virtual UseCase? UseCase { get; set; }
        public virtual ICollection<Consent> Consents { get; private set; }
        public virtual ICollection<AgreementAssignedCompanyRole> AgreementAssignedCompanyRoles { get; private set; }
        public virtual ICollection<AgreementAssignedDocumentTemplate> AgreementAssignedDocumentTemplates { get; private set; }
    }
}
