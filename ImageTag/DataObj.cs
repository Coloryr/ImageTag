using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ImageTag;

public record TagGroupObj
{
    public string uuid { get; set; }
    public string name { get; set; }
}

public record TagObj
{
    public string uuid { get; set; }
    public string name { get; set; }
    public string group { get; set; }
    public string bind_group { get; set; }
    public string bind_uuid { get; set; }
}