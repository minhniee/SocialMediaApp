using System.Text;

namespace SocialMediaApp.Config;

public class EmailConfiguration
{
	public string SmtpServer { get; set; }
	public int SmtpPort { get; set; }
	public string SmtpUsername { get; set; }
	public string SmtpPassword { get; set; }
	public string FromEmail { get; set; }
	public string FromName { get; set; }
	public bool EnableSsl { get; set; }


	public override string ToString()
	{
		return GetType().GetProperties()
			.Select(info => (info.Name, Value: info.GetValue(this, null) ?? "(null)"))
			.Aggregate(
				new StringBuilder(),
				(sb, pair) => sb.AppendLine($"{pair.Name}: {pair.Value}"),
				sb => sb.ToString());
	}
}