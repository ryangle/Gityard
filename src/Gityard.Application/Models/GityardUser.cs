using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Gityard.Application.Models
{
    public class GityardUser
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public string Email { get;set; }
        public string Password { get; set; }
        public string Avatar { get; set; }
    }
}
