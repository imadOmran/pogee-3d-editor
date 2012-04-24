using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using ApplicationCore.ViewModels.Editors;

namespace DoorsPlugin
{    
    public interface IDoorsViewModel : IEditorViewModel
    {
        DoorsDataService DoorService { get; }
    }
}
