using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ApplicationCore.ViewModels.Editors
{
    public abstract class EditorViewModelBase : ViewModelBase, IEditorViewModel
    {
        public virtual void User3DClickAt(object sender, World3DClickEventArgs e)
        {
            switch (e.MouseButtonArgs.ChangedButton)
            {
                case System.Windows.Input.MouseButton.Left:
                    OnLeftMouseClick(sender, e);
                    break;
                case System.Windows.Input.MouseButton.Middle:
                    OnMiddleMouseClick(sender, e);
                    break;
                case System.Windows.Input.MouseButton.Right:
                    OnRightMouseClick(sender, e);
                    break;
                default:
                    break;
            }
        }
        abstract public DataServices.IDataService Service { get; }

        protected virtual void OnLeftMouseClick(object sender,World3DClickEventArgs args)
        {

        }

        protected virtual void OnRightMouseClick(object sender, World3DClickEventArgs args)
        {

        }

        protected virtual void OnMiddleMouseClick(object sender, World3DClickEventArgs args)
        {

        }
    }
}
