using Detached.DataAnnotations;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace Detached.EntityFramework.Tests
{
    public class DerivedEntity : Entity
    {
        [ForeignKey(nameof(DerivedOwnedReference))]
        public int? DerivedOwnedReferenceId { get; set; }

        [Owned]
        public OwnedReference DerivedOwnedReference { get; set; }
    }
}
