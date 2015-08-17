namespace ExBuddy.OrderBotTags.Craft
{
    using System;
    using System.ComponentModel;
    using System.Windows.Media;

    using Buddy.Coroutines;

    using Clio.XmlEngine;

    using ff14bot.Helpers;
    using ff14bot.NeoProfiles;
    using ff14bot.RemoteWindows;

    using TreeSharp;

    using Action = TreeSharp.Action;

    [XmlElement("Collect")]
    public class Collect : ProfileBehavior
    {
        private bool isDone;

        public override bool IsDone
        {
            get
            {
                return isDone;
            }
        }

        [DefaultValue(1300)]
        [XmlAttribute("Value")]
        public int Value { get; set; }

        public Version Version
        {
            get
            {
                return new Version(3, 0, 5);
            }
        }

        protected override void OnResetCachedDone()
        {
            this.isDone = false;
        }

        protected override Composite CreateBehavior()
        {
            return
                new PrioritySelector(
                    new WaitContinue(10,
                        ret => SelectYesNoItem.IsOpen,
                        new Sequence(
                            new Sleep(2, 3),
                            new Action(
                                r =>
                                    {
                                        uint value;
                                        value = SelectYesNoItem.CollectabilityValue;

                                        if (value == 0)
                                        {
                                            new Sleep(2, 3);
                                        }

                                        value = SelectYesNoItem.CollectabilityValue;
                                        Log(
                                            string.Format(
                                                "Collectible created with value: {0} required: {1}",
                                                value,
                                                Value));
                                        if (value >= Value || value == 0)
                                        {
                                            Log("Collecting Collectible -> Value: " + value, Colors.Green);
                                            SelectYesNoItem.Yes();
                                        }
                                        else
                                        {
                                            Log("Declining Collectible -> Value: " + value, Colors.Red);
                                            SelectYesNoItem.No();
                                        }
                                    }))));
        }

        protected void Log(string message, Color color)
        {
            Logging.Write(color, string.Format("[Collect v{0}] {1}", this.Version, message));
        }

        protected void Log(string message)
        {
            Log(message, Colors.Gold);
        }
    }
}
