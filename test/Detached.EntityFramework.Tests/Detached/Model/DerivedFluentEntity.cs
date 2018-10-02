using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace Detached.EntityFramework.Tests
{
    public class DerivedFluentEntity : FluentEntity
    {
        [ForeignKey(nameof(DerivedOwnedReference))]
        public int? DerivedOwnedReferenceId { get; set; }

        public OwnedReference DerivedOwnedReference { get; set; }
    }
}
