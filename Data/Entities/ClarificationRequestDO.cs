using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Data.Interfaces;
using Data.States.Templates;

namespace Data.Entities
{
    public class ClarificationRequestDO : EmailDO, IClarificationRequest
    {


        public ClarificationRequestDO()
        {
        }

        #region Implementation of IClarificationRequest


        [Required, ForeignKey("ClarificationRequestStateTemplate")]
        public int ClarificationRequestState { get; set; }
        public _ClarificationRequestStateTemplate ClarificationRequestStateTemplate { get; set; }


        public int NegotiationId { get; set; }
        [ForeignKey("NegotiationId")]
        public virtual NegotiationDO Negotiation { get; set; }

        //should add a setter override that prevents more than one recipient being added to the To of a CR.

        #endregion
    }
}
