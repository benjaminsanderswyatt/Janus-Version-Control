﻿using System.ComponentModel.DataAnnotations;

namespace backend.Models
{
    public class TestTable
    {
        [Key]
        public int Id { get; set; }
        public string Name { get; set; }

    }

}
