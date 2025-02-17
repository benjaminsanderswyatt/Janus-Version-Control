﻿using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace backend.Models
{
    public class Commit
    {
        [Key]
        public int CommitId { get; set; }

        [Required]
        [StringLength(40)]
        public string CommitHash { get; set; }

        [Required]
        public int BranchId { get; set; }

        [Required]
        [StringLength(40)]
        public string TreeHash { get; set; }

        [Required]
        [MaxLength(255)]
        public string AuthorName { get; set; }

        [Required]
        [MaxLength(255)]
        public string AuthorEmail { get; set; }

        [Required]
        public string Message { get; set; }

        [Required]
        public DateTime CommittedAt { get; set; } = DateTime.UtcNow;



        public ICollection<CommitParent> Parents { get; set; } = new List<CommitParent>();
        public ICollection<CommitParent> Children { get; set; } = new List<CommitParent>();

        [ForeignKey("BranchId")]
        public Branch Branch { get; set; }

    }

}
