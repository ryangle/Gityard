using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Gityard.Application.Dtos
{
    public record CommitDto(string Id, string Message, string Author, string Sha,DateTimeOffset When);
}
