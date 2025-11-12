using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ForariaDomain.Models
{
    public class CreatePreferenceResponse
    {
        public string PreferenceId { get; set; }

        public string InitPoint { get; set; }

        public CreatePreferenceResponse(string PreferenceId, string InitPoint)
        {
             this.PreferenceId = PreferenceId;
             this.InitPoint = InitPoint;
        }
    }
}
