using Avalonia.Controls;
using System;
using Avalonia.Input;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using VVOInfo.Models;
using VVOInfo.Services;

namespace VVOInfo.Views;

public partial class MainWindow : Window
{

    public MainWindow()
    {
        InitializeComponent();
        if (OperatingSystem.IsLinux())
        {
            WindowState = WindowState.FullScreen;

            this.Cursor = new Cursor(StandardCursorType.None);
        }

        
        //        greetingButton.Content = "Goodbye Cruel World!";
        //       CancelTextBlock.Text = "test!";
        //XXc.Visibility = Avalonia.Visibility.Collapsed;
        //XXc.visible = false;
        ttt(1);
    }

    private void ttt(int i)
    {
        if (i != 0)
            return;

        Viewbox viewbox = new Viewbox();
        viewbox.IsVisible = false;
        TextBlock textblock = new TextBlock();
        textblock.IsVisible = false;
        textblock.Background = Avalonia.Media.Brushes.Red;
        textblock.Height = 100;
        ColumnDefinition columnDefinition = new ColumnDefinition();
        GridLength gridLength = columnDefinition.Width;
        Grid grid = new Grid();
        grid.ShowGridLines = true;
       // grid.ColumnDefinitions

            

    }


}