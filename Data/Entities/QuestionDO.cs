﻿using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Data.Entities.Enumerations;
using Data.Interfaces;

namespace Data.Entities
{
    public class QuestionDO : IQuestion
    {
        #region Implementation of IQuestion

        [Key]
        public int Id { get; set; }
        public int? ClarificationRequestId { get; set; }
        public QuestionStatus Status { get; set; }
        [Required]
        public string Text { get; set; }
        public string Response { get; set; }

        [ForeignKey("ClarificationRequestId")]
        public virtual ClarificationRequestDO ClarificationRequest { get; set; }

        #endregion
    }
}