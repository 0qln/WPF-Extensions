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
using System.Runtime.InteropServices;
using System.Windows.Interop;

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
        private Window rootWindow;

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

        private string name;
        public string Name => name;

        /// <summary>
        /// Create an instance
        /// </summary>
        public DropDownMenu(string name, Window rootWindow) {
            this.rootWindow = rootWindow;
            this.name = name;
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
            verticalPanel.Background = Helper.StringToSolidColorBrush("#2e2e2e");
            verticalPanel.Orientation = Orientation.Vertical;
            border.Child = verticalPanel;
            border.Style = System.Windows.Application.Current.Resources["ClientButtonUnfoldMenu_Style"] as Style; 
        }
        public void SetCanvas(Canvas canvas) {
            canvas.Children.Add(UIElement);
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

        public void ChangeBGColor(Brush color) {
            verticalPanel.Background = color;
        }
        public void ChangeBGColorWithChildren(Brush color) {
            ChangeBGColor(color);
            foreach (var option in options) {
                if (option.HasMenu) {
                    option.ChildMenu.ChangeBGColor(color);
                }
            }
        }

        public MenuOption AddOption(string name, double height = 22) {
            return AddOption(NewOption(name, height));
        }
        public MenuOption AddOption(MenuOption option) {
            options.Add(option);
            verticalPanel.Children.Add(option.UIElement);
            UpdateOptionLayout();
            return option;
        }
        public MenuOption NewOption(string name, double height = 22) => new MenuOption(height, name, this);
        public MenuOption GetOption(int index) => options[index];
        public MenuOption ?GetOption(string name) => options.Find(option => name == option.GetName);

        public void UpdateOptionLayout() {
            double maxWidth = 0;
            var grids = verticalPanel.Children.OfType<Grid>();

            //Name
            foreach (var grid in grids) {
                grid.ColumnDefinitions[1].Width = new GridLength(1, GridUnitType.Auto);
            }
            maxWidth = grids.Max(g => Helper.GetActualColumnWidth(g, 1));
            foreach (var grid in grids) {
                grid.ColumnDefinitions[1].Width = new GridLength(maxWidth);
            }

            //Shortcut
            foreach (var grid in grids) {
                grid.ColumnDefinitions[2].Width = new GridLength(1, GridUnitType.Auto);
            }
            maxWidth = 0;
            maxWidth = grids.Max(g => Helper.GetActualColumnWidth(g, 2));
            foreach(var grid in grids) {
                grid.ColumnDefinitions[2].Width = new GridLength(maxWidth);
            }
        }
        public void UpdateMenuPosition() {
            if (!isChildOfMenu) {
                if (parentElement == null) {
                    return;
                }
                position = Helper.GetAbsolutePosition(parentElement, rootWindow);
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

            private DropDownMenu childMenu;
            public DropDownMenu ChildMenu => childMenu;
            private DropDownMenu parentMenu;
            public DropDownMenu ParentMenu => parentMenu;
            private bool hasMenu = false;
            public bool HasMenu => hasMenu;


            public MenuOption(double height, string optionName, DropDownMenu parentMenu) {
                this.parentMenu = parentMenu;
                this.name = optionName;
                this.height = height;
                childMenu = new DropDownMenu(optionName, parentMenu.rootWindow);
                arrow.Text = " ";
                icon.RenderSize = new Size(height, height);

                title.Margin = new Thickness(15, 0, 0, 0);
                title.Text = optionName;
                title.Foreground = Brushes.White;
                title.VerticalAlignment = VerticalAlignment.Top;
                title.HorizontalAlignment = HorizontalAlignment.Left;

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

            }


            public void HideChilrenMenu() => childMenu.HideWidthChildrenMenus();
            public void UpdateMenuPosition() => childMenu.UpdateMenuPositionWithChildren();

            private void Grid_MouseLeave(object sender, MouseEventArgs e) {
                grid.Background = Brushes.Transparent;
            }
            private void Grid_MouseEnter(object sender, MouseEventArgs e) {
                grid.Background = Helper.StringToSolidColorBrush("#3d3d3d");
            }

            public MenuOption AddSymbol(string path) {
                Helper.SetImageSource(icon, path);
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

                this.childMenu = menu;
                hasMenu = true;
                
                AddCommand(parentMenu.HideChildrenMenus);
                AddCommand(menu.ToggleVisibility);
                return this;
            }
            public MenuOption AddCommand(Action command) {
                grid.MouseLeftButtonUp += (s, e) => command();
                return this;
            }
        }
    }

    public class WindowHandle {
        private WindowChrome windowChrome = new();
        public WindowChrome GetWindowChrome => windowChrome;
        private Window window;

        private ApplicationButtonCollection applicationButtons;
        public ApplicationButtonCollection ApplicationButtons;
        private double clientButtonHeight = 20;
        private double height = 30;
        public double Height => height;

        private System.Windows.Controls.Image icon = new();

        private List<(Button, DropDownMenu)> clientButtons = new();

        private bool isUsingClientButtons = false;
        private StackPanel clientButtonStackPanel = new();
        private Grid mainGrid = new();
        public FrameworkElement FrameworkElement => mainGrid;
        private Brush colorWhenButtonhover = Helper.StringToSolidColorBrush("#3d3d3d");
        private Brush bgColor = Helper.StringToSolidColorBrush("#1f1f1f");
        public Brush BGColor => bgColor;

        private Canvas ?parentCanvas;

        // Handle Bar Init
        public WindowHandle(Window window) {
            this.window = window;

            // var
            WindowChrome.SetWindowChrome(window, windowChrome);
            applicationButtons = new(this, window);
            ApplicationButtons = applicationButtons;
            window.SourceInitialized += (s, e) => {
                IntPtr handle = (new WindowInteropHelper(window)).Handle;
                HwndSource.FromHwnd(handle).AddHook(new HwndSourceHook(WindowProc));
            };

            // Set up Main Grid
            mainGrid.Background = bgColor;
            mainGrid.VerticalAlignment = VerticalAlignment.Top;
            mainGrid.HorizontalAlignment = HorizontalAlignment.Stretch;
            mainGrid.Width = window.Width;
            mainGrid.Height = height;

            window.SizeChanged += (s, e) => {
                mainGrid.Width = window.ActualWidth;
            };

            var mainRow = new RowDefinition { Height = new GridLength(1, GridUnitType.Star) };
            var clientButtonColumn = new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) };
            var applicationButtonColumn = new ColumnDefinition { Width = new GridLength(applicationButtons.Width * 3) };

            mainGrid.RowDefinitions.Add(mainRow);
            mainGrid.ColumnDefinitions.Add(clientButtonColumn);
            mainGrid.ColumnDefinitions.Add(applicationButtonColumn);

            Helper.SetChildInGrid(mainGrid, clientButtonStackPanel, 0, 0);
            Helper.SetChildInGrid(mainGrid, applicationButtons.FrameworkElement, 0, 1);

            // Set up Application Buttons


            // Set up Client Button Stack Panel
            clientButtonStackPanel.Background = Brushes.Transparent;
            clientButtonStackPanel.Orientation = Orientation.Horizontal;
        }
        public WindowHandle SetParentWindow(Canvas parentCanvas) {
            parentCanvas.Children.Add(mainGrid);
            this.parentCanvas = parentCanvas;
            return this;
        }

        public WindowHandle AddIcon(string path) {
            Helper.SetImageSource(icon, path);
            icon.Margin = new Thickness(5);
            clientButtonStackPanel.Children.Insert(0, icon);
            return this;
        }
        public WindowHandle SetHeight(double height) {
            this.height = height;
            mainGrid.Height = height;
            windowChrome.CaptionHeight = height;
            
            if (applicationButtons.Height < height) {
                applicationButtons.Height = height;
            }

            return this;
        }
        public WindowHandle SetBGColor(Brush color) {
            mainGrid.Background = color;
            return this;
        }
        public WindowHandle SetColorWhenHover(Brush color) {
            colorWhenButtonhover = color;
            foreach ((Button, DropDownMenu) button in clientButtons) {
                UpdateButtonHoverColor(button.Item1);
                button.Item2.ChangeBGColorWithChildren(color);
            }
            return this;
        }
        public WindowHandle CreateClientButton(DropDownMenu dropDownMenu) {
            Button newClientButton = new() {
                Content = dropDownMenu.Name,
                Style = ClientButtonStyle()
            };
            Helper.SetWindowChromActive(newClientButton);
            parentCanvas?.Children.Add(dropDownMenu.UIElement);
            clientButtons.Add((newClientButton, dropDownMenu));
            clientButtonStackPanel.Children.Add(newClientButton);

            dropDownMenu.Instanciate(newClientButton);

            return this;
        }

        // Client Button Init
        public void ActivateAllClientButtons() {
            foreach (var button in clientButtons) {
                ActivateClientButton(button);
                button.Item2.HideWidthChildrenMenus();
                button.Item2.UpdateOptionLayout();
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
            applicationButtons.SetWindowChromeActive();
            foreach ((Button, DropDownMenu) button in clientButtons) {
                Helper.SetWindowChromActive(button.Item1);
            }
        }

        // Visual
        private void UpdateButtonHoverColor(Button button) {
            // Get the button's style
            Style newStyle = new Style(typeof(Button), button.Style);

            // Create the new Trigger
            Trigger mouseOverTrigger = new Trigger { Property = UIElement.IsMouseOverProperty, Value = true };
            mouseOverTrigger.Setters.Add(new Setter(Button.BackgroundProperty, colorWhenButtonhover));
            newStyle.Triggers.Add(mouseOverTrigger);

            // Apply the updated style to the button
            button.Style = newStyle;
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
            mouseOverTrigger.Setters.Add(new Setter(Button.BackgroundProperty, colorWhenButtonhover));
            mouseOverTrigger.Setters.Add(new Setter(Button.BorderBrushProperty, Brushes.Gray));

            userButtonTemplate.Triggers.Add(mouseOverTrigger);

            clientButtonsStyle.Setters.Add(new Setter(Button.TemplateProperty, userButtonTemplate));

            clientButtonsStyle.Seal();

            return clientButtonsStyle;
        }
        #region Fix for the Winodw maximizing glitch
        private static IntPtr WindowProc(IntPtr hwnd, int msg, IntPtr wParam, IntPtr lParam, ref bool handled) {
            switch (msg) {
                case 0x0024:
                    WmGetMinMaxInfo(hwnd, lParam);
                    handled = true;
                    break;
            }
            return (IntPtr)0;
        }
        private static void WmGetMinMaxInfo(IntPtr hwnd, IntPtr lParam) {
            MINMAXINFO mmi = (MINMAXINFO)Marshal.PtrToStructure(lParam, typeof(MINMAXINFO));
            int MONITOR_DEFAULTTONEAREST = 0x00000002;
            IntPtr monitor = MonitorFromWindow(hwnd, MONITOR_DEFAULTTONEAREST);
            if (monitor != IntPtr.Zero) {
                MONITORINFO monitorInfo = new MONITORINFO();
                GetMonitorInfo(monitor, monitorInfo);
                RECT rcWorkArea = monitorInfo.rcWork;
                RECT rcMonitorArea = monitorInfo.rcMonitor;
                mmi.ptMaxPosition.x = Math.Abs(rcWorkArea.left - rcMonitorArea.left);
                mmi.ptMaxPosition.y = Math.Abs(rcWorkArea.top - rcMonitorArea.top);
                mmi.ptMaxSize.x = Math.Abs(rcWorkArea.right - rcWorkArea.left);
                mmi.ptMaxSize.y = Math.Abs(rcWorkArea.bottom - rcWorkArea.top);
            }
            Marshal.StructureToPtr(mmi, lParam, true);
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct POINT {
            /// <summary>x coordinate of point.</summary>
            public int x;
            /// <summary>y coordinate of point.</summary>
            public int y;
            /// <summary>Construct a point of coordinates (x,y).</summary>
            public POINT(int x, int y) {
                this.x = x;
                this.y = y;
            }
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct MINMAXINFO {
            public POINT ptReserved;
            public POINT ptMaxSize;
            public POINT ptMaxPosition;
            public POINT ptMinTrackSize;
            public POINT ptMaxTrackSize;
        };

        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Auto)]
        public class MONITORINFO {
            public int cbSize = Marshal.SizeOf(typeof(MONITORINFO));
            public RECT rcMonitor = new RECT();
            public RECT rcWork = new RECT();
            public int dwFlags = 0;
        }

        [StructLayout(LayoutKind.Sequential, Pack = 0)]
        public struct RECT {
            public int left;
            public int top;
            public int right;
            public int bottom;
            public static readonly RECT Empty = new RECT();
            public int Width { get { return Math.Abs(right - left); } }
            public int Height { get { return bottom - top; } }
            public RECT(int left, int top, int right, int bottom) {
                this.left = left;
                this.top = top;
                this.right = right;
                this.bottom = bottom;
            }
            public RECT(RECT rcSrc) {
                left = rcSrc.left;
                top = rcSrc.top;
                right = rcSrc.right;
                bottom = rcSrc.bottom;
            }
            public bool IsEmpty { get { return left >= right || top >= bottom; } }
            public override string ToString() {
                if (this == Empty) { return "RECT {Empty}"; }
                return "RECT { left : " + left + " / top : " + top + " / right : " + right + " / bottom : " + bottom + " }";
            }
            public override bool Equals(object obj) {
                if (!(obj is Rect)) return false;
                return (this == (RECT)obj);
            }
            /// <summary>Return the HashCode for this struct (not garanteed to be unique)</summary>
            public override int GetHashCode() => left.GetHashCode() + top.GetHashCode() + right.GetHashCode() + bottom.GetHashCode();
            /// <summary> Determine if 2 RECT are equal (deep compare)</summary>
            public static bool operator ==(RECT rect1, RECT rect2) { return (rect1.left == rect2.left && rect1.top == rect2.top && rect1.right == rect2.right && rect1.bottom == rect2.bottom); }
            /// <summary> Determine if 2 RECT are different(deep compare)</summary>
            public static bool operator !=(RECT rect1, RECT rect2) { return !(rect1 == rect2); }
        }

        [DllImport("user32")]
        internal static extern bool GetMonitorInfo(IntPtr hMonitor, MONITORINFO lpmi);

        [DllImport("User32")]
        internal static extern IntPtr MonitorFromWindow(IntPtr handle, int flags);
        #endregion

        public class ApplicationButtonCollection {
            private Window window;

            private double height = 30;
            private double width = 40;
            public double Height {
                get { return height; } 
                set {
                    if (value <= window.Height) {
                        height = value;
                        exitButton.Height = height;
                        minimizeButton.Height = height;
                        maximizeButton.Height = height;

                        if (settingsButton != null)
                            settingsButton.Height = height;
                    }
                }
            }
            public double Width {
                get { return width; }
                set {
                    if (value <= window.Width / 3) {
                        width = value;
                        exitButton.Width = value;
                        minimizeButton.Width = value;
                        maximizeButton.Width = value;
                        if (settingsButton != null)
                            settingsButton.Width = value;
                    }
                }
            }
            public string? SettingsButtonImageSource {
                get {
                    if (settingsButton == null) return null;
                    if ((settingsButton.Content as Border)!.Child == null) return null;

                    return ((settingsButton.Content as Border)!.Child as System.Windows.Controls.Image)!.Source.ToString();
                }
                set {
                    if (settingsButton == null) return;

                    var imageContent = ((settingsButton.Content as Border)!.Child as System.Windows.Controls.Image)!;
                    imageContent.Source = new BitmapImage(new Uri(value!));
                }
            }
            public Thickness SettingsButtonImagePadding {
                get {
                    if (settingsButton == null) return new Thickness();

                    return (settingsButton.Content as Border)!.Padding;
                }
                set {
                    if (settingsButton == null) return;

                    (settingsButton.Content as Border)!.Padding = value;
                }
            }


            private Button settingsButton = new();
            private Button exitButton = new();
            private Button minimizeButton = new();
            private Button maximizeButton = new();
            public Button ExitButton => exitButton;
            public Button MinimizeButton => minimizeButton;
            public Button MaximizeButton => maximizeButton;

            private StackPanel stackPanel = new();
            public FrameworkElement FrameworkElement => stackPanel;

            private Brush colorWhenButtonHover = Helper.StringToSolidColorBrush("#3d3d3d");
            private Brush color = Brushes.Transparent;
            private Brush symbolColor = Brushes.White;
            public Brush ColorWhenButtonHover {
                get => colorWhenButtonHover;
                set {
                    colorWhenButtonHover = value;
                    UpdateColors();
                }
            }
            public Brush Color {
                get => color;
                set {
                    color = value;
                    UpdateColors();
                }
            }
            public Brush SymbolColor {
                get => symbolColor;
                set {
                    symbolColor = value;
                    UpdateColors();
                }
            }


            private WindowHandle windowHandle;


            public ApplicationButtonCollection(WindowHandle windowHandle, Window window) {
                this.window = window;
                this.windowHandle = windowHandle;


                exitButton.Style = ButtonStyle();
                exitButton.Content = "x";
                exitButton.Click += Shutdown;
                exitButton.MouseEnter += (s, e) => { exitButton.Background = colorWhenButtonHover; };
                exitButton.MouseLeave += (s, e) => { exitButton.Background = color; };
                Helper.SetWindowChromActive(exitButton);

                minimizeButton.Style = ButtonStyle();
                minimizeButton.Content = "-";
                minimizeButton.Click += Minimize;
                minimizeButton.MouseEnter += (s, e) => { minimizeButton.Background = colorWhenButtonHover; };
                minimizeButton.MouseLeave += (s, e) => { minimizeButton.Background = color; };
                Helper.SetWindowChromActive(minimizeButton);

                maximizeButton.Style = ButtonStyle();
                maximizeButton.Content = "□";
                maximizeButton.Click += Maximize;
                maximizeButton.MouseEnter += (s, e) => { maximizeButton.Background = colorWhenButtonHover; };
                maximizeButton.MouseLeave += (s, e) => { maximizeButton.Background = color; };
                Helper.SetWindowChromActive(maximizeButton);


                stackPanel.VerticalAlignment = VerticalAlignment.Center;
                stackPanel.HorizontalAlignment = HorizontalAlignment.Right;
                stackPanel.Orientation = Orientation.Horizontal;

                stackPanel.Children.Add(minimizeButton);
                stackPanel.Children.Add(maximizeButton);
                stackPanel.Children.Add(exitButton);

                UpdateColors();
                UpdateSize();
            }


            public void AddSettingsButton() {
                windowHandle.mainGrid.ColumnDefinitions[1].Width = new GridLength(width * 4);

                settingsButton.Style = ButtonStyle();
                settingsButton.Click += Settings;
                settingsButton.MouseEnter += (s, e) => { settingsButton.Background = colorWhenButtonHover; };
                settingsButton.MouseLeave += (s, e) => { settingsButton.Background = color; };

                var container = new Border {
                    HorizontalAlignment = HorizontalAlignment.Stretch,
                    VerticalAlignment = VerticalAlignment.Stretch,
                };
                settingsButton.Content = container;

                var imageContent = new System.Windows.Controls.Image { 
                    HorizontalAlignment = HorizontalAlignment.Center,
                    VerticalAlignment = VerticalAlignment.Center
                };
                container.Child = imageContent;

                Helper.SetWindowChromActive(settingsButton);
                stackPanel.Children.Insert(0, settingsButton);

                UpdateColors();
                UpdateSize();
            }

            public Style ButtonStyle() {
                // Create a new style for the button
                Style style = new Style(typeof(Button));
                style.Setters.Add(new Setter(Button.BackgroundProperty, color));
                style.Setters.Add(new Setter(Button.ForegroundProperty, symbolColor));
                style.Setters.Add(new Setter(Button.BorderBrushProperty, Brushes.Transparent));
                style.Setters.Add(new Setter(Button.HorizontalAlignmentProperty, HorizontalAlignment.Right));
                style.Setters.Add(new Setter(Button.VerticalAlignmentProperty, VerticalAlignment.Top));
                style.Setters.Add(new Setter(Button.WidthProperty, Width));
                style.Setters.Add(new Setter(Button.HeightProperty, Height));

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

                return style;
            }
            
            public void SetWindowChromeActive() {
                Helper.SetWindowChromActive(exitButton);
                Helper.SetWindowChromActive(minimizeButton);
                Helper.SetWindowChromActive(maximizeButton);
                if (settingsButton != null) {
                    Helper.SetWindowChromActive(settingsButton);
                }
            }

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
            public void OverrideSettings(Action action) {
                settingsButton.Click -= Settings;
                settingsButton.Click += (object sender, RoutedEventArgs e) => action();
            }
            private void Shutdown(object sender, RoutedEventArgs e) {
                window.Close();
            }
            private void Minimize(object sender, RoutedEventArgs e) {
                window.WindowState = WindowState.Minimized;
            }
            private void Maximize(object sender, RoutedEventArgs e) {
                if (window.WindowState == WindowState.Maximized) {
                    // Go into windowed
                    window.WindowState = WindowState.Normal;
                }
                else {
                    // Go into maximized
                    window.WindowState = WindowState.Maximized;
                }
                // Update Layout
                window.UpdateLayout();
            }
            private void Settings(object sender, RoutedEventArgs e) {

            }

            public void UpdateSize() {
                exitButton.Width = Width;
                exitButton.Height = Height;


                minimizeButton.Width = Width;
                minimizeButton.Height = Height;


                maximizeButton.Width = Width;
                maximizeButton.Height = Height;

                if (settingsButton != null) {
                    settingsButton.Width = Width;
                    settingsButton.Height = Height;
                }
            }
            public void UpdateColors() {
                exitButton.Background = color;
                exitButton.Foreground = symbolColor;
                Helper.UpdateButtonHoverColor(ExitButton, colorWhenButtonHover);


                minimizeButton.Background = color;
                minimizeButton.Foreground = symbolColor;
                Helper.UpdateButtonHoverColor(minimizeButton, colorWhenButtonHover);


                maximizeButton.Background = color;
                maximizeButton.Foreground = symbolColor;
                Helper.UpdateButtonHoverColor(maximizeButton, colorWhenButtonHover);

                if (settingsButton != null) {
                    settingsButton.Background = color;
                    settingsButton.Foreground = symbolColor;
                    Helper.UpdateButtonHoverColor(settingsButton, colorWhenButtonHover);
                }
            }
        }
    }


    public static class Helper {
        public static void UpdateButtonHoverColor(Button button, Brush colorWhenButtonhover) {
            // Get the button's style
            Style newStyle = new Style(typeof(Button), button.Style);

            // Create the new Trigger
            Trigger mouseOverTrigger = new Trigger { Property = UIElement.IsMouseOverProperty, Value = true };
            mouseOverTrigger.Setters.Add(new Setter(Button.BackgroundProperty, colorWhenButtonhover));
            newStyle.Triggers.Add(mouseOverTrigger);

            // Apply the updated style to the button
            button.Style = newStyle;
        }

        public static void SetWindowChromActive(IInputElement element) {
            WindowChrome.SetIsHitTestVisibleInChrome(element, true);
        }

        public static void SetImageSource(System.Windows.Controls.Image image, string path) {
            image.Source = new BitmapImage(new Uri(path, UriKind.RelativeOrAbsolute));
        }

        public static void SetChildInGrid(Grid grid, UIElement child, int row, int column) {
            grid.Children.Add(child);
            Grid.SetColumn(child, column);
            Grid.SetRow(child, row);
        }

        public static Point GetAbsolutePosition(FrameworkElement ?element, Window rootWindow) {
            if (element == null) return new Point(-1, -1);
            return element.TransformToAncestor(rootWindow).Transform(new Point(0,0)); 
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

        public static SolidColorBrush StringToSolidColorBrush(string colorString, double opacity = 1.0) {
            SolidColorBrush brush;

            try {
                Color color = (Color)ColorConverter.ConvertFromString(colorString);
                color.A = (byte)(255 * opacity);
                brush = new SolidColorBrush(color);
            }
            catch (FormatException) {
                // If the string cannot be converted to a color, return a transparent brush
                brush = new SolidColorBrush(System.Windows.Media.Color.FromArgb(0, 0, 0, 0));
            }

            return brush;
        }



        public static void AddRow(Grid grid, double value, GridUnitType type) {
            RowDefinition rowDefinition = new RowDefinition { Height = new GridLength(value, type) };
            grid.RowDefinitions.Add(rowDefinition);
        }
        public static void AddColumn(Grid grid, double value, GridUnitType type) {
            ColumnDefinition columnDefinition = new ColumnDefinition { Width = new GridLength(value, type) };
            grid.ColumnDefinitions.Add(columnDefinition);
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
