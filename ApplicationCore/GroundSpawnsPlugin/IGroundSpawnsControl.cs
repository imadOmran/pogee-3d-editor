using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using ApplicationCore.UserControls;

namespace GroundSpawnsPlugin
{
    interface IGroundSpawnsControl : IEditorControl
    {
        IGroundSpawnsViewModel GroundSpawnsViewModel { get; set; }
    }
}
