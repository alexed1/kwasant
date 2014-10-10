using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Data.States
{
    public class ParticipationStatus
    {
        public const int NeedsAction = 1;
        public const int Accepted = 2;
        public const int Declined = 3;
        public const int Tentative = 4;
        public const int Delegated = 5;
    }
}
