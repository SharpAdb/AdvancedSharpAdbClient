using System.Numerics;
using Windows.UI.Composition;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

// The User Control item template is documented at https://go.microsoft.com/fwlink/?LinkId=234236

namespace AdvancedSharpAdbClient.SampleApp
{
    public sealed partial class HeaderTiles : UserControl
    {
        private SpringVector3NaturalMotionAnimation _springAnimation;

        public string Title
        {
            get { return (string)GetValue(TitleProperty); }
            set { SetValue(TitleProperty, value); }
        }

        public static readonly DependencyProperty TitleProperty =
            DependencyProperty.Register("Title", typeof(string), typeof(HeaderTiles), new PropertyMetadata(null));

        public string Source
        {
            get { return (string)GetValue(SourceProperty); }
            set { SetValue(SourceProperty, value); }
        }

        public static readonly DependencyProperty SourceProperty =
            DependencyProperty.Register("Title", typeof(string), typeof(HeaderTiles), new PropertyMetadata(null));

        public string Link
        {
            get { return (string)GetValue(LinkProperty); }
            set { SetValue(LinkProperty, value); }
        }

        public static readonly DependencyProperty LinkProperty =
            DependencyProperty.Register("Title", typeof(string), typeof(HeaderTiles), new PropertyMetadata(null));


        public HeaderTiles()
        {
            this.InitializeComponent();
        }

        private void CreateOrUpdateSpringAnimation(float finalValue)
        {
            if (_springAnimation == null)
            {
                Compositor compositor = Window.Current.Compositor;
                if (compositor != null)
                {
                    _springAnimation = compositor.CreateSpringVector3Animation();
                    _springAnimation.Target = "Scale";
                }
            }

            _springAnimation.FinalValue = new Vector3(finalValue);
        }
    }
}
