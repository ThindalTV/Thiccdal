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
    public static string SectionName => "Login";
    public TwitchIRC? IRC { get; set; }
    public TwitchAPI? API { get; set; }
    public string? Username { get; set; }
    public string? OAuthToken { get; set; }
}

public class TwitchIRC
{
    public static string SectionName => "IRC";
    public string? Username { get; set; }
    public string? OAuthToken { get; set; }
}

public class TwitchAPI
{
    public static string SectionName => "API";
    public string? HelixClientId { get; set; }
    public string? HelixClientSecret { get; set; }
    public string? HelixOAuthToken { get; set; }

}
