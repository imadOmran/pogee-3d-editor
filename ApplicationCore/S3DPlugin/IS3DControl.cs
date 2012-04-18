using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using ApplicationCore.UserControls;

namespace S3DPlugin
{
    public interface IS3DControl : IEditorControl
    {
        IS3DViewModel S3DViewModel { get; set; }
    }
}
