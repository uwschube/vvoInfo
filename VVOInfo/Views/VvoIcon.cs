using Avalonia;
using Avalonia.Controls.Primitives;
using System;
using System.Collections.Generic;
using System.Text;

namespace VVOInfo.Views
{
    public class VvoIcon : TemplatedControl
    {
        // Registrierung der Eigenschaft "Type" (Standard: Tram)
        public static readonly StyledProperty<VvoIconType> TypeProperty =
            AvaloniaProperty.Register<VvoIcon, VvoIconType>(nameof(Type), VvoIconType.Tram);

        public VvoIconType Type
        {
            get => GetValue(TypeProperty);
            set => SetValue(TypeProperty, value);
        }

        // Registrierung der Eigenschaft "Size" (Standard: 64 Pixel)
        public static readonly StyledProperty<double> SizeProperty =
            AvaloniaProperty.Register<VvoIcon, double>(nameof(Size), 64.0);

        public double Size
        {
            get => GetValue(SizeProperty);
            set => SetValue(SizeProperty, value);
        }
    }
}
