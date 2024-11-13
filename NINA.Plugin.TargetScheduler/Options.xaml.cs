using System.ComponentModel.Composition;
using System.Windows;

namespace NINA.Plugin.TargetScheduler {

    [Export(typeof(ResourceDictionary))]
    public partial class Options : ResourceDictionary {

        public Options() {
            InitializeComponent();
        }
    }
}