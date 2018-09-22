using Detached.DataAnnotations;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Detached.EntityFramework.Tests
{
    public class FluentEntity
    {
        [Key]
        public int Id { get; set; }

        public string Name { get; set; }

        [ForeignKey(nameof(OwnedReference))]
        public int? OwnedReferenceId { get; set; }
        
        public OwnedReference OwnedReference { get; set; }

        [ForeignKey(nameof(AssociatedReference))]
        public int? AssociatedReferenceId { get; set; }
        
        public AssociatedReference AssociatedReference { get; set; }
        
        public IList<OwnedListItem> OwnedList { get; set; }
        
        public IList<AssociatedListItem> AssociatedList { get; set; }
    }
}
