using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using ApplicationCore.UserControls;

namespace DoorsPlugin
{
    public interface IDoorsControl : IEditorControl
    {
        IDoorsViewModel DoorsViewModel { get; set; }
    }
}
