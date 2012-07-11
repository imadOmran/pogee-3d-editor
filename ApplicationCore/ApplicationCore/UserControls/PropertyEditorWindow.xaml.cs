using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using System.Reflection;

namespace ApplicationCore.UserControls
{
    /// <summary>
    /// Interaction logic for PropertyEditorWindow.xaml
    /// </summary>
    public partial class PropertyEditorWindow : Window
    {
        public PropertyEditorWindow(object item)
        {
            InitializeComponent();

            PropertyEditorWindowViewModel vm = DataContext as PropertyEditorWindowViewModel;
            vm.Item = item;                        
        }
                
        private IEnumerable<object> _items = null;
        private Dictionary<PropertyInfo,object> _itemCopy = new Dictionary<PropertyInfo,object>();

        public PropertyEditorWindow(IEnumerable<object> items)
        {
            if (items == null) throw new NullReferenceException();
            _items = items;
            InitializeComponent();

            this.Closing += new System.ComponentModel.CancelEventHandler(PropertyEditorWindow_Closing);

            PropertyEditorWindowViewModel vm = DataContext as PropertyEditorWindowViewModel;
            if (items.Count() > 0)
            {
                vm.Item = items.ElementAt(0);
                var propertyInfos = vm.Item.GetType().GetProperties().Where(x => { return x.CanWrite; });
                foreach (var p in propertyInfos)
                {
                    _itemCopy[p] = p.GetValue(vm.Item, null);
                }
            }            
        }

        void PropertyEditorWindow_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            PropertyEditorWindowViewModel vm = DataContext as PropertyEditorWindowViewModel;

            if (_items != null && vm.Item != null && _items.Count() > 1)
            {

                //get the properties that have changed on the source object - these changes need propagated down
                List<PropertyInfo> changedProperties = new List<PropertyInfo>();
                foreach (KeyValuePair<PropertyInfo, object> entry in _itemCopy)
                {
                    var val = entry.Key.GetValue(vm.Item, null);

                    if (!Object.Equals(val, entry.Value))
                    {
                        changedProperties.Add(entry.Key);
                    }
                }

                if (changedProperties.Count == 0) return;

                foreach (var item in _items.Where(x => { return x != vm.Item; }))
                {
                    foreach (var p in changedProperties)
                    {
                        p.SetValue(item, p.GetValue(vm.Item, null), null);
                    }
                }
            }
        }
        
        public PropertyEditorWindow()
        {
            InitializeComponent();
            PropertyEditorWindowViewModel vm = DataContext as PropertyEditorWindowViewModel;
            vm.Item = new object();
        }
    }
}
