using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ImageTag.Views;

interface IHighlight
{
    void Highlight(TagObj obj);

    void ClearHighlight();
}
