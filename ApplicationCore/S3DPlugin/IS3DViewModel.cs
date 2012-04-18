using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using ApplicationCore.ViewModels.Editors;

namespace S3DPlugin
{
    public interface IS3DViewModel : IEditorViewModel
    {
        S3DDataService S3DService { get; }
    }
}
