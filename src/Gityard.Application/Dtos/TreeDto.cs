using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Gityard.Application.Dtos
{
    public record TreeDto(string Id, int Count, string Sha,string Parent);
}
