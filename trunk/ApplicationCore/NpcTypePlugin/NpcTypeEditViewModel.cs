using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using ApplicationCore.ViewModels;
using ApplicationCore.ViewModels.Editors;

namespace NpcTypePlugin
{
    public class NpcTypeEditViewModel : ViewModelBase, IEditorViewModel
    {
        public NpcTypeEditViewModel()
        {

        }

        public void User3DClickAt(object sender, World3DClickEventArgs e)
        {
        }

        public ApplicationCore.DataServices.IDataService Service
        {
            get { throw new NotImplementedException(); }
        }
    }
}
