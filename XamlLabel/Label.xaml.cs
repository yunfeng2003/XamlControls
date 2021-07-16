using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

using Microsoft.Graphics.Canvas;
using Microsoft.Graphics.Canvas.Geometry;
using Microsoft.Graphics.Canvas.Text;
using Microsoft.Graphics.Canvas.UI.Xaml;
using Windows.UI;
// The User Control item template is documented at https://go.microsoft.com/fwlink/?LinkId=234236

namespace Minicraft.XamlControls
{
    public sealed partial class Label : UserControl
    {
        private long _tokenForeground;
        private long _tokenFontSize;
        private long _tokenFontFamily;
        private long _tokenFontStyle;
        private long _tokenFontWeight;
        private long _tokenHorizontalContentAlignment;

        private bool needsResourceRecreation = true;

        public string Text
        {
            get { return ( string )GetValue( TextProperty ); }
            set { SetValue( TextProperty, value ); }
        }
        public static readonly DependencyProperty TextProperty = DependencyProperty.Register(
            nameof( Text ), typeof( string ), typeof( Label ), new PropertyMetadata( string.Empty, OnPropertyChanged ) );

        public Color TextColor
        {
            get { return ( Color )GetValue( TextColorProperty ); }
            set { SetValue( TextColorProperty, value ); }
        }
        public static readonly DependencyProperty TextColorProperty = DependencyProperty.Register(
            nameof( TextColor ), typeof( Color ), typeof( Label ), new PropertyMetadata( Colors.Black, OnPropertyChanged ) );

        public Color OutlineColor
        {
            get { return ( Color )GetValue( OutlineColorProperty ); }
            set { SetValue( OutlineColorProperty, value ); }
        }
        public static readonly DependencyProperty OutlineColorProperty = DependencyProperty.Register(
            nameof( OutlineColor ), typeof( Color ), typeof( Label ), new PropertyMetadata( Colors.Red, OnPropertyChanged ) );

        public double OutlineThickness
        {
            get { return ( double )GetValue( OutlineThicknessProperty ); }
            set { SetValue( OutlineThicknessProperty, value ); }
        }
        public static readonly DependencyProperty OutlineThicknessProperty = DependencyProperty.Register(
            nameof( OutlineThickness ), typeof( double ), typeof( Label ), new PropertyMetadata( 1.0, OnPropertyChanged ) );

        public CanvasDashStyle OutlineStyle
        {
            get { return ( CanvasDashStyle )GetValue( OutlineStyleProperty ); }
            set { SetValue( OutlineStyleProperty, value ); }
        }
        public static readonly DependencyProperty OutlineStyleProperty = DependencyProperty.Register(
            nameof( OutlineStyle ), typeof( CanvasDashStyle ), typeof( Label ), new PropertyMetadata( CanvasDashStyle.Solid, OnPropertyChanged ) );

        public CanvasWordWrapping TextWrapping
        {
            get { return ( CanvasWordWrapping )GetValue( TextWrappingProperty ); }
            set { SetValue( TextWrappingProperty, value ); }
        }
        public static readonly DependencyProperty TextWrappingProperty = DependencyProperty.Register( 
            nameof( TextWrapping ), typeof( CanvasWordWrapping ), typeof( Label ), new PropertyMetadata( CanvasWordWrapping.NoWrap, OnPropertyChanged ) );
        
        public CanvasTextDirection TextDirection
        {
            get { return ( CanvasTextDirection )GetValue( TextDirectionProperty ); }
            set { SetValue( TextDirectionProperty, value ); }
        }
        public static readonly DependencyProperty TextDirectionProperty = DependencyProperty.Register(
            nameof( TextWrapping ), typeof( CanvasTextDirection ), typeof( Label ), new PropertyMetadata( CanvasTextDirection.LeftToRightThenTopToBottom, OnPropertyChanged ) );


        CanvasGeometry textGeometry;
        CanvasTextLayout textLayout;
        CanvasStrokeStyle dashedStroke;
        public CanvasVerticalGlyphOrientation CurrentVerticalGlyphOrientation { get; set; }

        public Label()
        {
            InitializeComponent();

            _tokenFontSize = RegisterPropertyChangedCallback( FontSizeProperty, OnInternalPropertyChanged );
            _tokenFontStyle = RegisterPropertyChangedCallback( FontStyleProperty, OnInternalPropertyChanged );
            _tokenForeground = RegisterPropertyChangedCallback( ForegroundProperty, OnInternalPropertyChanged );
            _tokenFontFamily = RegisterPropertyChangedCallback( FontFamilyProperty, OnInternalPropertyChanged );
            _tokenFontWeight = RegisterPropertyChangedCallback( FontWeightProperty, OnInternalPropertyChanged );
            _tokenHorizontalContentAlignment = RegisterPropertyChangedCallback( HorizontalContentAlignmentProperty, OnInternalPropertyChanged );

            CurrentVerticalGlyphOrientation = CanvasVerticalGlyphOrientation.Default;
            dashedStroke = new CanvasStrokeStyle()
            {
                DashStyle = CanvasDashStyle.Solid
            };
        }
        private CanvasTextLayout CreateTextLayout( ICanvasResourceCreator resourceCreator, float canvasWidth, float canvasHeight )
        {
            var textFormat = new CanvasTextFormat();
            textFormat.FontSize = ( float )FontSize;
            textFormat.FontFamily = FontFamily.Source;
            textFormat.FontStyle = FontStyle;
            textFormat.FontWeight = FontWeight;
            textFormat.WordWrapping = TextWrapping;
            textFormat.Direction = TextDirection;
            textFormat.HorizontalAlignment = GetCanvasHorizontalAlignemnt();

            var textLayout = new CanvasTextLayout( resourceCreator, Text, textFormat, canvasWidth, canvasHeight );
            textLayout.TrimmingSign = CanvasTrimmingSign.Ellipsis;
            textLayout.TrimmingGranularity = CanvasTextTrimmingGranularity.Character;
            textLayout.VerticalGlyphOrientation = CurrentVerticalGlyphOrientation;

            return textLayout;
        }
        void EnsureResources( ICanvasResourceCreatorWithDpi resourceCreator, Size targetSize )
        {
            if( !needsResourceRecreation ) return;

            if( textLayout != null )
            {
                textLayout.Dispose();
                textGeometry.Dispose();
            }

            textLayout = CreateTextLayout( resourceCreator, ( float )targetSize.Width, ( float )targetSize.Height );
            textGeometry = CanvasGeometry.CreateText( textLayout );

            dashedStroke.DashStyle = OutlineStyle;

            needsResourceRecreation = false;
        }

        private void control_Unloaded( object sender, RoutedEventArgs e )
        {
            canvas.RemoveFromVisualTree();
            canvas = null;

            UnregisterPropertyChangedCallback( FontSizeProperty, _tokenFontSize );
            UnregisterPropertyChangedCallback( FontStyleProperty, _tokenFontStyle );
            UnregisterPropertyChangedCallback( ForegroundProperty, _tokenForeground );
            UnregisterPropertyChangedCallback( FontFamilyProperty, _tokenFontFamily );
            UnregisterPropertyChangedCallback( FontWeightProperty, _tokenFontWeight );
            UnregisterPropertyChangedCallback( HorizontalContentAlignmentProperty, _tokenHorizontalContentAlignment );
        }

        private void Canvas_Draw( CanvasControl sender, CanvasDrawEventArgs args )
        {
            EnsureResources( sender, sender.Size );

            args.DrawingSession.DrawGeometry( textGeometry, OutlineColor, ( float )OutlineThickness, dashedStroke );
            args.DrawingSession.DrawTextLayout( textLayout, 0, 0, TextColor );
        }
        private void Canvas_CreateResources( CanvasControl sender, object args )
        {
            needsResourceRecreation = true;
            canvas.Invalidate();
        }

        private void Canvas_SizeChanged( object sender, SizeChangedEventArgs e )
        {
            needsResourceRecreation = true;
            canvas.Invalidate();
        }
        private void OnInternalPropertyChanged( DependencyObject sender, DependencyProperty dp )
        {
            needsResourceRecreation = true;
            canvas.Invalidate();
        }
        private static void OnPropertyChanged( DependencyObject d, DependencyPropertyChangedEventArgs e )
        {
            if( d is Label instance )
            {
                instance.needsResourceRecreation = true;
                instance.canvas.Invalidate();
            }
        }
        private CanvasHorizontalAlignment GetCanvasHorizontalAlignemnt()
        {
            switch( HorizontalContentAlignment )
            {
            case HorizontalAlignment.Center:
            return CanvasHorizontalAlignment.Center;
            case HorizontalAlignment.Left:
            return CanvasHorizontalAlignment.Left;
            case HorizontalAlignment.Right:
            return CanvasHorizontalAlignment.Right;
            default:
            return CanvasHorizontalAlignment.Left;
            }
        }
    }
}
