using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Gityard.Application.Dtos;

public record BranchDto(string FriendlyName, 
    string UpstreamBranchCanonicalName, 
    string RemoteName, 
    string CanonicalName);
