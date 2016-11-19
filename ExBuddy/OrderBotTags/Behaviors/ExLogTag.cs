
#pragma warning disable 1998

namespace ExBuddy.OrderBotTags.Behaviors
{
	using System;
	using System.Threading.Tasks;
	using System.Windows.Media;
	using System.Xml.Serialization;
	using ExBuddy.Attributes;

	[LoggerName("ExLog")]
	[Clio.XmlEngine.XmlElement("ExLog")]
	public class ExLogTag : ExProfileBehavior
	{
		[Clio.XmlEngine.XmlElement("Message")]
		public string Body { get; set; }

		[XmlIgnore]
		public Color? Color { get; set; }

		[Clio.XmlEngine.XmlAttribute("Color")]
		public string ColorString
		{
			get { return Color.ToString(); }

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

		[Clio.XmlEngine.XmlAttribute("Message")]
		public string Message { get; set; }

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

		public override string ToString()
		{
		    return "ExLogTag LineNumber:" + LineNumber;
		}

		protected override async Task<bool> Main()
		{
			if (!string.IsNullOrWhiteSpace(Message))
			{
				Logger.Info(Message);
			}
			else if (!string.IsNullOrWhiteSpace(Body))
			{
				var lines = Body.Split(new[] {Environment.NewLine}, StringSplitOptions.None);

				foreach (var line in lines)
				{
					Logger.Info(line);
				}
			}

			return isDone = true;
		}

		protected override void OnStart()
		{
			if (!string.IsNullOrWhiteSpace(Name))
			{
				Logger.Name = Name;
			}

			Logger.IncludeVersion = false;
		}
	}
}