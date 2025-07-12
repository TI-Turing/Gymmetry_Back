using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FitGymApp.Domain.DTO.User.Response
{
    public class AddResponse
    {
        public Guid Id { get; set; }
        public string Token { get; set; }
    }
}
