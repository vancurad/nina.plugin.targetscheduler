using System;
using System.Windows.Markup;

namespace NINA.Plugin.TargetScheduler.Controls.DatabaseManager {

    public class EnumBindingSourceExtension : MarkupExtension {
        public Type EnumType { get; private set; }

        public EnumBindingSourceExtension(Type enumType) {
            if (enumType is null || !enumType.IsEnum) {
                throw new Exception("enumType must be an Enum");
            }

            EnumType = enumType;
        }

        public override object ProvideValue(IServiceProvider serviceProvider) {
            return Enum.GetValues(EnumType);
        }
    }
}