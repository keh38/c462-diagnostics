namespace Turandot.Screen
{
    public class ScreenElements
    {
        public FixationPoint fixationPoint = new FixationPoint();
        public Counter counter = new Counter();
        public Scoreboard scoreboard = new Scoreboard();
        public ThumbSliderLayout thumbSliderLayout = new ThumbSliderLayout();
        public GrapherLayout grapherLayout = new GrapherLayout();
        public MessageLayout messageLayout = new MessageLayout();
        public ParamSliderLayout paramSliderLayout = new ParamSliderLayout();
        public ProgressBarLayout progressBarLayout = new ProgressBarLayout();
        public XboxControllerLayout xboxControllerLayout = new XboxControllerLayout();
        public string finalPrompt = "";

        public AvailableCues cues = new AvailableCues();
        public AvailableInputs inputs = new AvailableInputs();
        public ScreenElements() { }
    }
}