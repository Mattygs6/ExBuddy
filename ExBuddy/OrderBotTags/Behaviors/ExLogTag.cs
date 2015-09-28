namespace ExBuddy.OrderBotTags.Behaviors
{
	using System;
	using System.Threading.Tasks;
	using System.Windows.Media;

	using Clio.XmlEngine;

	using ExBuddy.Attributes;

	[LoggerName("ExLog")]
	[XmlElement("ExLog")]
	public class ExLogTag : ExProfileBehavior
	{
		protected override Color Info
		{
			get
			{
				if (Color.HasValue)
				{
					return Color.Value;
				}

				return base.Info;
			}
		}

		protected override void OnStart()
		{
			if (!string.IsNullOrWhiteSpace(Name))
			{
				Logger.Name = Name;	
			}

			Logger.IncludeVersion = false;
		}

		protected override async Task<bool> Main()
		{
			if (!string.IsNullOrWhiteSpace(Message))
			{
				Logger.Info(Message);
			}
			else if (!string.IsNullOrWhiteSpace(Body))
			{
				var lines = Body.Split(new [] { Environment.NewLine }, StringSplitOptions.None);

				foreach (var line in lines)
				{
					Logger.Info(line);
				}
			}

			return isDone = true;
		}

		[XmlElement("Message")]
		public string Body { get; set; }

		[XmlAttribute("Message")]
		public string Message { get; set; }

		[System.Xml.Serialization.XmlIgnore]
		public Color? Color { get; set; }

		[XmlAttribute("Color")]
		public string ColorString
		{
			get
			{
				return Color.ToString();
			}

			set
			{
				try
				{
					Color = ColorConverter.ConvertFromString(value) as Color?;
				}
				catch (Exception ex)
				{
					Logger.Error(ex.Message + " - Using default color, are you missing the '#'?");
				}
				
			}
		}

		public override string ToString()
		{
			return "ExLogTag";
		}
	}
}
