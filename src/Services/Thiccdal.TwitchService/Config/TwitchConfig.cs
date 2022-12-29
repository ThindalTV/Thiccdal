using System;

namespace Thiccdal.TwitchService.Config;
public class TwitchConfig
{
    public static string SectionName => "Twitch";
    
    public TwitchLogin? Login { get; set; }
    public string[]? Channels { get; set; }
}

public class TwitchLogin
{
    public string Username { get; set; }
    public string OAuthToken { get; set; }
}
