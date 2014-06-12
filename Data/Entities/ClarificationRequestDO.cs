using System;
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
    public class ClarificationRequestDO : EmailDO, IClarificationRequest
    {
        #region Implementation of IClarificationRequest

        public int BookingRequestId { get; set; }
        public ClarificationStatus ClarificationStatus { get; set; }

        #endregion
    }
}
