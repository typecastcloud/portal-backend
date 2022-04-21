﻿using System;
using System.ComponentModel.DataAnnotations;

namespace CatenaX.NetworkServices.PortalBackend.PortalEntities.Entities
{
    public class DocumentTemplate
    {
        private DocumentTemplate()
        {
            Documenttemplatename = null!;
            Documenttemplateversion = null!;
        }

        public DocumentTemplate(Guid id, string documenttemplatename, string documenttemplateversion, DateTime dateCreated)
        {
            Id = id;
            Documenttemplatename = documenttemplatename;
            Documenttemplateversion = documenttemplateversion;
            DateCreated = dateCreated;
        }

        [Key]
        public Guid Id { get; private set; }

        public DateTime DateCreated { get; private set; }

        public DateTime? DateLastChanged { get; set; }

        [MaxLength(255)]
        public string Documenttemplatename { get; set; }

        [MaxLength(255)]
        public string Documenttemplateversion { get; set; }

        public virtual AgreementAssignedDocumentTemplate? AgreementAssignedDocumentTemplate { get; set; }
    }
}
