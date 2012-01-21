using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Media.Media3D;

namespace ApplicationCore.DataServices
{
    public delegate void Model3DChangedHandler(object sender, EventArgs e);

    public interface IModel3DProvider
    {
        ModelVisual3D Model3D { get; }
        Transform3D Transform3D { get; set; }
        EQEmuDisplay3D.ViewClipping ViewClipping { get; set; }

        event Model3DChangedHandler ModelChanged;
    }
}
