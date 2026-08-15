using System.CodeDom.Compiler;
using System.Configuration;
using System.Diagnostics;
using System.Runtime.CompilerServices;

namespace CreationMaster.Properties;

[CompilerGenerated]
[GeneratedCode("Microsoft.VisualStudio.Editors.SettingsDesigner.SettingsSingleFileGenerator", "15.9.0.0")]
internal sealed class Settings : ApplicationSettingsBase
{
	private static Settings defaultInstance = (Settings)SettingsBase.Synchronized(new Settings());

	public static Settings Default => defaultInstance;

	[UserScopedSetting]
	[DebuggerNonUserCode]
	[DefaultSettingValue("")]
	public string RootDir
	{
		get
		{
			return (string)this["RootDir"];
		}
		set
		{
			this["RootDir"] = value;
		}
	}

	[UserScopedSetting]
	[DebuggerNonUserCode]
	[DefaultSettingValue("")]
	public string FifaDbFileName
	{
		get
		{
			return (string)this["FifaDbFileName"];
		}
		set
		{
			this["FifaDbFileName"] = value;
		}
	}

	[UserScopedSetting]
	[DebuggerNonUserCode]
	[DefaultSettingValue("")]
	public string FifaXmlFileName
	{
		get
		{
			return (string)this["FifaXmlFileName"];
		}
		set
		{
			this["FifaXmlFileName"] = value;
		}
	}

	[UserScopedSetting]
	[DebuggerNonUserCode]
	[DefaultSettingValue("")]
	public string LangDbFileName
	{
		get
		{
			return (string)this["LangDbFileName"];
		}
		set
		{
			this["LangDbFileName"] = value;
		}
	}

	[UserScopedSetting]
	[DebuggerNonUserCode]
	[DefaultSettingValue("")]
	public string LangXmlFileName
	{
		get
		{
			return (string)this["LangXmlFileName"];
		}
		set
		{
			this["LangXmlFileName"] = value;
		}
	}
}
