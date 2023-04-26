using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Shell;
using System.Windows.Threading;
using System.Xml.Linq;
using System.IO;
using System.IO.Pipes;
using System.Windows.Controls.Primitives;
using System.Runtime.CompilerServices;
using System.Collections;
using System.Reflection;
using System.Security.Cryptography;
using System.Windows.Media.Animation;
using System.Windows.Media.Media3D;
using System.Data;
using System.Diagnostics;
using System.Security.Policy;
using Microsoft.VisualBasic.FileIO;
using System.Threading;
using System.Reflection.Metadata;
using Microsoft.Win32;
using System.Windows.Automation.Text;
using static System.Net.Mime.MediaTypeNames;

namespace WpfCustomControls {
    /// <summary>
    /// Follow steps 1a or 1b and then 2 to use this custom control in a XAML file.
    ///
    /// Step 1a) Using this custom control in a XAML file that exists in the current project.
    /// Add this XmlNamespace attribute to the root element of the markup file where it is 
    /// to be used:
    ///
    ///     xmlns:MyNamespace="clr-namespace:WpfCustomControlLibrary1"
    ///
    ///
    /// Step 1b) Using this custom control in a XAML file that exists in a different project.
    /// Add this XmlNamespace attribute to the root element of the markup file where it is 
    /// to be used:
    ///
    ///     xmlns:MyNamespace="clr-namespace:WpfCustomControlLibrary1;assembly=WpfCustomControlLibrary1"
    ///
    /// You will also need to add a project reference from the project where the XAML file lives
    /// to this project and Rebuild to avoid compilation errors:
    ///
    ///     Right click on the target project in the Solution Explorer and
    ///     "Add Reference"->"Projects"->[Select this project]
    ///
    ///
    /// Step 2)
    /// Go ahead and use your control in the XAML file.
    ///
    ///     <MyNamespace:CustomControl1/>
    ///
    /// </summary>


    public class DropDownMenu {

        private bool isChildOfMenu = false;
        private DropDownMenu ?parentMenu;
        private MenuOption? parentOption;
        private FrameworkElement ?parentElement;
        private Point position = new(0, 0);
        public Point Position => position;

        private Border border = new();
        public FrameworkElement UIElement => border;
        private StackPanel verticalPanel = new();
        private List<MenuOption> options = new();
        public List<MenuOption> Options => options;

        /// <summary>
        /// Create an instance
        /// </summary>
        public DropDownMenu() {
            Init();
        }
        public void Instanciate(FrameworkElement parent) {
            this.parentElement = parent;
        }
        public void Instanciate(MenuOption parentOption, DropDownMenu parentMenu) {
            isChildOfMenu = true;
            this.parentMenu = parentMenu;
            this.parentOption = parentOption;
        }
        private void Init() {
            verticalPanel.Background = Helper.Color("#2e2e2e");
            verticalPanel.Orientation = Orientation.Vertical;
            border.Child = verticalPanel;
            border.Style = System.Windows.Application.Current.Resources["ClientButtonUnfoldMenu_Style"] as Style;
        }

        public void ToggleVisibility() {
            if (UIElement.Visibility == Visibility.Visible) {
                Hide();
            }
            else if (UIElement.Visibility == Visibility.Collapsed) {
                Show();
            }
        }
        public void Show() => UIElement.Visibility = Visibility.Visible;
        public void Hide() => UIElement.Visibility = Visibility.Collapsed;
        public void HideWidthChildrenMenus() {
            UIElement.Visibility = Visibility.Collapsed;
            foreach (var option in options) {
                if (option.HasMenu) {
                    option.HideChilrenMenu();
                }
            }
        }
        public void HideChildrenMenus() {
            foreach (var option in options) {
                if (option.HasMenu) {
                    option.HideChilrenMenu();
                }
            }
        }


        public void AddOption(MenuOption option) {
            options.Add(option);
            verticalPanel.Children.Add(option.UIElement);
            UpdateOptionLayout();
        }
        public MenuOption GetOption(int index) => options[index];
        public MenuOption ?GetOption(string name) => options.Find(option => name == option.GetName);

        public void UpdateOptionLayout() {
            double maxWidth = 0;
            var grids = verticalPanel.Children.OfType<Grid>();

            //Name
            maxWidth = grids.Max(g => Helper.GetActualColumnWidth(g, 1));
            foreach (var grid in grids) {
                grid.ColumnDefinitions[1].Width = new GridLength(maxWidth);
            }

            //Shortcut
            maxWidth = 0;
            maxWidth = grids.Max(g => Helper.GetActualColumnWidth(g, 2));
            foreach (var grid in grids) {
                grid.ColumnDefinitions[2].Width = new GridLength(maxWidth);
            }
        }
        public void UpdateMenuPosition() {
            if (!isChildOfMenu) {
                if (parentElement == null) {
                    return;
                }
                position = Helper.GetAbsolutePosition(parentElement);
                position.Y += parentElement.ActualHeight;
                border.RenderTransform = new TranslateTransform(position.X, position.Y);
            }
            else if (parentMenu != null && parentOption != null) {
                position = parentMenu!.Position;
                position.X += parentOption!.UIElement.ActualWidth;
                position.Y += parentMenu!.options.IndexOf(parentOption) * parentOption!.UIElement.ActualHeight;
                border.RenderTransform = new TranslateTransform(position.X, position.Y);
            }
        }
        public void UpdateMenuPositionWithChildren() {
            UpdateMenuPosition();
            foreach (var option in options) {
                if (option.HasMenu) {
                    option.UpdateMenuPosition();
                }
            }
        }


        public class MenuOption {
            private string ?name;
            public string ?GetName => name;

            private Point position;
            public Point Position => position;

            private Grid grid = new Grid();
            public FrameworkElement UIElement => grid;

            public System.Windows.Controls.Image icon = new();
            public TextBlock title = new TextBlock();
            public TextBlock keyboardShortCut = new TextBlock();
            public TextBlock arrow = new TextBlock();

            private double height;
            public double Height => height;

            private DropDownMenu menu;
            private DropDownMenu parentMenu;
            private bool hasMenu = false;
            public bool HasMenu => hasMenu;


            public MenuOption(double height, DropDownMenu parentMenu) {
                menu = new DropDownMenu();
                arrow.Text = " ";
                icon.RenderSize = new Size(height, height);
                this.height = height;

                ColumnDefinition symbolCol = new ColumnDefinition();
                symbolCol.Width = new GridLength(height, GridUnitType.Pixel);
                grid.ColumnDefinitions.Add(symbolCol);

                ColumnDefinition nameCol = new ColumnDefinition();
                nameCol.Width = new GridLength(1, GridUnitType.Auto);
                grid.ColumnDefinitions.Add(nameCol);

                ColumnDefinition shortcutCol = new ColumnDefinition();
                shortcutCol.Width = new GridLength(1, GridUnitType.Auto);
                grid.ColumnDefinitions.Add(shortcutCol);

                ColumnDefinition arrowCol = new ColumnDefinition();
                arrowCol.Width = new GridLength(height, GridUnitType.Pixel);
                grid.ColumnDefinitions.Add(arrowCol);


                Helper.SetChildInGrid(grid, icon, 0, 0);
                Helper.SetChildInGrid(grid, title, 0, 1);
                Helper.SetChildInGrid(grid, keyboardShortCut, 0, 2);
                Helper.SetChildInGrid(grid, arrow, 0, 3);

                grid.MouseEnter += Grid_MouseEnter;
                grid.MouseLeave += Grid_MouseLeave;
                this.parentMenu = parentMenu;
            }


            public void HideChilrenMenu() => menu.HideWidthChildrenMenus();
            public void UpdateMenuPosition() => menu.UpdateMenuPositionWithChildren();

            private void Grid_MouseLeave(object sender, MouseEventArgs e) {
                grid.Background = Brushes.Transparent;
            }
            private void Grid_MouseEnter(object sender, MouseEventArgs e) {
                grid.Background = Helper.Color("#3d3d3d");
            }

            public MenuOption AddSymbol(string path) {
                Helper.SetImageSource(icon, path);
                return this;
            }
            public MenuOption SetName(string nameText) {
                name = nameText;

                title.Margin = new Thickness(15, 0, 0, 0);
                title.Text = nameText;
                title.Foreground = Brushes.White;
                title.VerticalAlignment = VerticalAlignment.Top;
                title.HorizontalAlignment = HorizontalAlignment.Left;

                return this;
            }
            public MenuOption SetKeyboardShortcut(string kShortcut) {
                keyboardShortCut.Margin = new Thickness(15, 0, 0, 0);
                keyboardShortCut.Text = kShortcut;
                keyboardShortCut.Foreground = Brushes.White;
                keyboardShortCut.VerticalAlignment = VerticalAlignment.Top;
                keyboardShortCut.HorizontalAlignment = HorizontalAlignment.Left;

                return this;
            }
            public MenuOption AddDropdownMenu(DropDownMenu menu) {
                arrow.Margin = new Thickness(15, 0, 0, 0);
                arrow.Text = ">";
                arrow.Foreground = Brushes.White;

                this.menu = menu;
                hasMenu = true;
                
                AddCommand(parentMenu.HideChildrenMenus);
                AddCommand(menu.ToggleVisibility);

                return this;
            }
            public MenuOption AddCommand(Action command) {
                grid.MouseLeftButtonUp += (sender, e) => command();
                return this;
            }
        }
    }

    public class WindowHandle {
        private System.Windows.Application application;

        private double applicationButtonWidth = 40;
        private double applicationButtonHeight = 30;
        private double clientButtonHeight = 20;
        private double height = 30;

        private System.Windows.Controls.Image icon = new();

        private List<(Button, DropDownMenu)> clientButtons = new();

        private Button exitButton = new();
        private Button minimizeButton = new();
        private Button maximizeButton = new();

        private bool isUsingClientButtons = false;
        private StackPanel clientButtonStackPanel = new();
        private Grid applicationButtonGrid = new();
        private Grid mainGrid = new();
        public FrameworkElement FrameworkElement => mainGrid;


        // Application Button Init
        public void OverrideShutdown(Action action) {
            exitButton.Click -= Shutdown;
            exitButton.Click += (object sender, RoutedEventArgs e) => action();
        }
        public void OverrideMinimize(Action action) {
            exitButton.Click -= Minimize;
            exitButton.Click += (object sender, RoutedEventArgs e) => action();
        }
        public void OverrideMaximize(Action action) {
            exitButton.Click -= Maximize;
            exitButton.Click += (object sender, RoutedEventArgs e) => action();
        }
        private void Shutdown(object sender, RoutedEventArgs e) {
            application.Shutdown();
        }
        private void Minimize(object sender, RoutedEventArgs e) {
            application.MainWindow.WindowState = WindowState.Minimized;
        }
        private void Maximize(object sender, RoutedEventArgs e) {
            if (application.MainWindow.WindowState == WindowState.Maximized) {
                application.MainWindow.WindowState = WindowState.Normal;
            }
            else {
                application.MainWindow.WindowState = WindowState.Maximized;
            }
        }

        // Handle Bar Init
        public WindowHandle(System.Windows.Application application) {
            // var
            this.application = application;

            // Set up Application Buttons
            exitButton.Style = ApplicationButtonStyle();
            exitButton.Content = "x";
            exitButton.Click += Shutdown;

            minimizeButton.Style = ApplicationButtonStyle();
            minimizeButton.Content = "-";
            minimizeButton.Click += Minimize;

            maximizeButton.Style = ApplicationButtonStyle();
            maximizeButton.Content = "□";
            maximizeButton.Click += Maximize;

            UpdateApplicationButtons();

            applicationButtonGrid.VerticalAlignment = VerticalAlignment.Center;
            applicationButtonGrid.HorizontalAlignment = HorizontalAlignment.Right;

            var mainRow = new RowDefinition { Height = new GridLength(1, GridUnitType.Star) };
            var minimizeColumn = new ColumnDefinition { Width = new GridLength(applicationButtonWidth) };
            var maximizeColumn = new ColumnDefinition { Width = new GridLength(applicationButtonWidth) };
            var exitColumn = new ColumnDefinition { Width = new GridLength(applicationButtonWidth) };

            applicationButtonGrid.RowDefinitions.Add(mainRow);
            applicationButtonGrid.ColumnDefinitions.Add(minimizeColumn);
            applicationButtonGrid.ColumnDefinitions.Add(maximizeColumn);
            applicationButtonGrid.ColumnDefinitions.Add(exitColumn);

            Helper.SetChildInGrid(applicationButtonGrid, minimizeButton, 0, 0);
            Helper.SetChildInGrid(applicationButtonGrid, maximizeButton, 0, 1);
            Helper.SetChildInGrid(applicationButtonGrid, exitButton, 0, 2);


            // Set up Main Grid
            mainGrid.Background = Helper.Color("#1f1f1f");
            mainGrid.VerticalAlignment = VerticalAlignment.Stretch;
            mainGrid.HorizontalAlignment = HorizontalAlignment.Stretch;

            mainRow = new RowDefinition { Height = new GridLength(1, GridUnitType.Star) };
            var clientButtonColumn = new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) };
            var applicationButtonColumn = new ColumnDefinition { Width = new GridLength(applicationButtonWidth * 3) };

            mainGrid.RowDefinitions.Add(mainRow);
            mainGrid.ColumnDefinitions.Add(clientButtonColumn);
            mainGrid.ColumnDefinitions.Add(applicationButtonColumn);

            Helper.SetChildInGrid(mainGrid, clientButtonStackPanel, 0, 0);
            Helper.SetChildInGrid(mainGrid, applicationButtonGrid, 0, 1);
            // Set up Client Button Stack Panel
            clientButtonStackPanel.Background = Brushes.Transparent;
            clientButtonStackPanel.Orientation = Orientation.Horizontal;
        }
        public WindowHandle SetParentWindow(Grid parentGrid) {
            Helper.SetChildInGrid(parentGrid, FrameworkElement, 0, 0);
            return this;
        }
        public WindowHandle SetParentWindow(Canvas canvas) {
            canvas.Children.Add(mainGrid);
            return this;
        }

        public WindowHandle SetHeight(double height) {
            this.height = height;
            this.mainGrid.Height = height;
            return this;
        }
        public WindowHandle AddIcon(string path) {
            Helper.SetImageSource(icon, path);
            icon.Margin = new Thickness(5);
            clientButtonStackPanel.Children.Insert(0, icon);
            return this;
        }
        public WindowHandle SetApplicationButtonDimensions(double systemButtonWidth, double systemButtonHeight) {
            this.applicationButtonWidth = systemButtonWidth;
            this.applicationButtonHeight = systemButtonHeight;
            UpdateApplicationButtons();
            return this;
        }
        public WindowHandle SetApplicationButtonColor(Brush color) {
            exitButton.Foreground = color;
            minimizeButton.Foreground = color;
            maximizeButton.Foreground = color;
            return this;
        }
        public WindowHandle CreateClientButton(string name, DropDownMenu dropDownMenu) {
            Button newClientButton = new() {
                Content = name,
                Style = ClientButtonStyle()
            };

            clientButtons.Add((newClientButton, dropDownMenu));
            clientButtonStackPanel.Children.Add(newClientButton);
            return this;
        }

        // Client Button Init
        public void ActivateAllClientButtons() {
            foreach (var button in clientButtons) {
                ActivateClientButton(button);
                button.Item2.HideWidthChildrenMenus();
            }
        }
        private void ActivateClientButton((Button, DropDownMenu) button) {
            button.Item1.Loaded += (object sender, RoutedEventArgs e) => button.Item2.UpdateOptionLayout();
            button.Item1.Loaded += (object sender, RoutedEventArgs e) => button.Item2.UpdateMenuPositionWithChildren();
            button.Item1.Click += (object sender, RoutedEventArgs e) => ToggleMenu(button.Item2);
            button.Item1.MouseEnter += (object sender, MouseEventArgs e) => { 
                if (isUsingClientButtons) {
                    HideAllMenus();
                    button.Item2.Show();
                }
            };
        }

        private void ActivateClientButton((Button, DropDownMenu) button, Action action) {
            ActivateClientButton(button);
            button.Item1.Click += (object sender, RoutedEventArgs e) => action();
        }
        public void ActivateButtonFunction(string name) {
            var button = GetClientButton(name);
            if (button.Item1 == null || button.Item2 == null) {
                return;
            }
            ActivateClientButton(button);
        }
        public void SetButtonFunction(string name, Action action) {
            var button = GetClientButton(name);
            if (button.Item1 == null || button.Item2 == null) {
                return;
            }
            ActivateClientButton(button, action);
        }

        // Client Button Management
        public (Button, DropDownMenu) GetClientButton(string name) => clientButtons.Find(x => x.Item1.Content.ToString() == name);
        public Button ?GetClientButtonButton(string name) => clientButtons.Find(x => x.Item1.Content.ToString() == name).Item1;
        public void HideAllMenus() => clientButtons.ForEach(x => x.Item2.HideWidthChildrenMenus());
        public void ToggleMenu(DropDownMenu element) {
            if (isUsingClientButtons) {
                HideAllMenus();
                isUsingClientButtons = false;
            }
            else {
                isUsingClientButtons = true;
                element.Show();
            }
        }

        // Window Chrome
        public void SetWindowChromActiveAll() {
            SetWindowChromActive(exitButton);
            SetWindowChromActive(minimizeButton);
            SetWindowChromActive(maximizeButton);
            foreach ((Button, DropDownMenu) button in clientButtons) {
                SetWindowChromActive(button.Item1);
            }
        }
        public void SetWindowChromActive(IInputElement element) {
            WindowChrome.SetIsHitTestVisibleInChrome(element, true);
        }

        // Visual
        private void UpdateApplicationButtons() {
            exitButton.Width = applicationButtonWidth;
            exitButton.Height =  applicationButtonHeight;

            minimizeButton.Width = applicationButtonWidth;
            minimizeButton.Height = applicationButtonHeight;


            maximizeButton.Width = applicationButtonWidth;
            maximizeButton.Height = applicationButtonHeight;
        }
        public Style ClientButtonStyle() {
            Style clientButtonsStyle = new Style(typeof(Button));

            clientButtonsStyle.Setters.Add(new Setter(Button.MarginProperty, new Thickness(10, 0, 0, 0)));
            clientButtonsStyle.Setters.Add(new Setter(Button.BackgroundProperty, Brushes.Transparent));
            clientButtonsStyle.Setters.Add(new Setter(Button.ForegroundProperty, Brushes.White));
            clientButtonsStyle.Setters.Add(new Setter(Button.BorderBrushProperty, Brushes.Transparent));
            clientButtonsStyle.Setters.Add(new Setter(Button.BorderThicknessProperty, new Thickness(1)));
            clientButtonsStyle.Setters.Add(new Setter(Button.HorizontalAlignmentProperty, HorizontalAlignment.Left));
            clientButtonsStyle.Setters.Add(new Setter(Button.VerticalAlignmentProperty, VerticalAlignment.Center));
            clientButtonsStyle.Setters.Add(new Setter(Button.HeightProperty, clientButtonHeight));

            ControlTemplate userButtonTemplate = new ControlTemplate(typeof(Button));
            FrameworkElementFactory borderFactory = new FrameworkElementFactory(typeof(Border));
            borderFactory.SetValue(Border.BackgroundProperty, new TemplateBindingExtension(Button.BackgroundProperty));
            borderFactory.SetValue(Border.BorderBrushProperty, new TemplateBindingExtension(Button.BorderBrushProperty));
            borderFactory.SetValue(Border.BorderThicknessProperty, new TemplateBindingExtension(Button.BorderThicknessProperty));

            FrameworkElementFactory contentPresenterFactory = new FrameworkElementFactory(typeof(ContentPresenter));
            contentPresenterFactory.SetValue(ContentPresenter.HorizontalAlignmentProperty, HorizontalAlignment.Center);
            contentPresenterFactory.SetValue(ContentPresenter.VerticalAlignmentProperty, VerticalAlignment.Center);

            borderFactory.AppendChild(contentPresenterFactory);

            userButtonTemplate.VisualTree = borderFactory;

            Trigger mouseOverTrigger = new Trigger { Property = Button.IsMouseOverProperty, Value = true };
            mouseOverTrigger.Setters.Add(new Setter(Button.BackgroundProperty, new SolidColorBrush(Color.FromRgb(0x3d, 0x3d, 0x3d))));
            mouseOverTrigger.Setters.Add(new Setter(Button.BorderBrushProperty, Brushes.Gray));

            userButtonTemplate.Triggers.Add(mouseOverTrigger);

            clientButtonsStyle.Setters.Add(new Setter(Button.TemplateProperty, userButtonTemplate));

            clientButtonsStyle.Seal();

            return clientButtonsStyle;
        }
        public Style ApplicationButtonStyle() {
            // Create a new style for the button
            Style style = new Style(typeof(Button));
            style.Setters.Add(new Setter(Button.BackgroundProperty, Brushes.Transparent));
            style.Setters.Add(new Setter(Button.ForegroundProperty, Brushes.White));
            style.Setters.Add(new Setter(Button.BorderBrushProperty, Brushes.Transparent));
            style.Setters.Add(new Setter(Button.HorizontalAlignmentProperty, HorizontalAlignment.Right));
            style.Setters.Add(new Setter(Button.VerticalAlignmentProperty, VerticalAlignment.Top));
            style.Setters.Add(new Setter(Button.WidthProperty, applicationButtonWidth));
            style.Setters.Add(new Setter(Button.HeightProperty, applicationButtonHeight));

            // Set the control template of the button
            ControlTemplate template = new ControlTemplate(typeof(Button));
            FrameworkElementFactory border = new FrameworkElementFactory(typeof(Border));
            border.SetBinding(Button.BackgroundProperty, new Binding("Background") { RelativeSource = new RelativeSource(RelativeSourceMode.TemplatedParent) });
            border.SetBinding(Button.BorderBrushProperty, new Binding("BorderBrush") { RelativeSource = new RelativeSource(RelativeSourceMode.TemplatedParent) });
            border.SetBinding(Button.BorderThicknessProperty, new Binding("BorderThickness") { RelativeSource = new RelativeSource(RelativeSourceMode.TemplatedParent) });
            FrameworkElementFactory contentPresenter = new FrameworkElementFactory(typeof(ContentPresenter));
            contentPresenter.SetValue(Button.HorizontalAlignmentProperty, HorizontalAlignment.Center);
            contentPresenter.SetValue(Button.VerticalAlignmentProperty, VerticalAlignment.Center);
            border.AppendChild(contentPresenter);
            template.VisualTree = border;
            style.Setters.Add(new Setter(Button.TemplateProperty, template));

            // Add a trigger to change the background color when the mouse is over the button
            Trigger mouseOverTrigger = new Trigger { Property = UIElement.IsMouseOverProperty, Value = true };
            mouseOverTrigger.Setters.Add(new Setter(Button.BackgroundProperty, Helper.Color("#3d3d3d")));
            style.Triggers.Add(mouseOverTrigger);

            return style;
        }
    }


    public static class Helper {
        public static void SetImageSource(System.Windows.Controls.Image image, string path) {
            image.Source = new BitmapImage(new Uri(path, UriKind.RelativeOrAbsolute));
        }

        public static void SetChildInGrid(Grid grid, UIElement child, int row, int column) {
            grid.Children.Add(child);
            Grid.SetColumn(child, column);
            Grid.SetRow(child, row);
        }

        public static Point GetAbsolutePosition(FrameworkElement ?element) {
            if (element == null) return new Point(-1, -1);
            return element.TransformToAncestor(System.Windows.Application.Current.MainWindow).Transform(new Point(0,0));
        }

        public static double GetActualColumnWidth(Grid grid, int columnIndex) {
            grid.Measure(new Size(double.PositiveInfinity, double.PositiveInfinity));
            grid.Arrange(new Rect(0, 0, grid.DesiredSize.Width, grid.DesiredSize.Height));
            var columnWidth = grid.ColumnDefinitions[columnIndex].ActualWidth;
            return columnWidth;
        }

        public static double GetActualRowHeight(Grid grid, int rowIndex) {
            grid.Measure(new Size(double.PositiveInfinity, double.PositiveInfinity));
            grid.Arrange(new Rect(0, 0, grid.DesiredSize.Width, grid.DesiredSize.Height));
            var rowHeight = grid.RowDefinitions[rowIndex].ActualHeight;
            return rowHeight;
        }

        public static SolidColorBrush Color(string hex) {
            return new SolidColorBrush((Color)ColorConverter.ConvertFromString(hex));
        }
    }


    public enum ProgramRunType {
        DebugLibrary,
        StandAlone
    }


    public enum DockDirection {
        Right,
        Bottom,
        Left,
        Top
    }
}
