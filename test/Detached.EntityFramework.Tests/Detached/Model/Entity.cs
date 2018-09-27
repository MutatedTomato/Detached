using Detached.DataAnnotations;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Detached.EntityFramework.Tests
{
    public class Entity
    {
        [Key]
        public int Id { get; set; }

        public string Name { get; set; }

        [ForeignKey(nameof(OwnedReference))]
        public int? OwnedReferenceId { get; set; }

        [Owned]
        public OwnedReference OwnedReference { get; set; }

        [Owned]
        [ForeignKey("OwnedReferenceWithShadowKeyId")]
        public OwnedReference OwnedReferenceWithShadowKey { get; set; }

        [ForeignKey(nameof(AssociatedReference))]
        public int? AssociatedReferenceId { get; set; }

        [Associated]
        public AssociatedReference AssociatedReference { get; set; }

        [Associated]
        [ForeignKey("AssociatedReferenceWithShadowKeyId")]
        public AssociatedReference AssociatedReferenceWithShadowKey { get; set; }

        [Owned]
        public IList<OwnedListItem> OwnedList { get; set; }

        [Associated]
        public IList<AssociatedListItem> AssociatedList { get; set; }
    }
}
