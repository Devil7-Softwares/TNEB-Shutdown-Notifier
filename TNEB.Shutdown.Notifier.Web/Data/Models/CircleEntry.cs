﻿using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using TNEB.Shutdown.Scrapper;

namespace TNEB.Shutdown.Notifier.Web.Data.Models
{
    [Index(nameof(Value), IsUnique = true)]
    public class CircleEntry : ICircle
    {
        [Key]
        [Required]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public Guid Id { get; set; }

        [Required]
        public string Value { get; set; }

        [Required]
        public string Name { get; set; }

        public CircleEntry()
        {
            Id = Guid.NewGuid();
            Name = string.Empty;
            Value = string.Empty;
        }

        public CircleEntry(string name, string value)
        {
            Id = Guid.NewGuid();
            Name = name;
            Value = value;
        }
    }
}
