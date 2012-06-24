using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using ApplicationCore.UserControls;

namespace SpawnExtractorPlugin
{
    public interface ISpawnExtractorControl : IEditorControl
    {
        SpawnExtractorTabViewModel ExtractorViewModel { get; set; }
    }
}
