using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SBPluginInterface
{
    public delegate void UserJoin( string channel, string name );
    public delegate void UserLeave( string channel, string name );
    public delegate void UserKick( string channel, string name, string by, string reason );
    public delegate void UserChange( string name, string newname );
    public delegate void UserQuit( string name, string message );
    public delegate void UserMode( string name, string channel, string mode, string by );

    public delegate void ChannelMessage( string name, string message, string channel );
    public delegate void ChannelCommand(string name, string channel, string command, string[] parameters);
    public delegate void PrivateMessage( string name, string message );
    public delegate void PrivateCommand( string name, string command, string[] parameters );

    public delegate void ServerTopicSet( string channel, string topic );
    public delegate void ServerTopicOwner( string channel, string name, string date );
    public delegate void ServerNames( string channel, string[] list );

    public interface SBPlugin
    {
        string PluginName
        {
            get;
        }
        string Version
        {
            get;
        }
        string Author
        {
            get;
        }
        string Website
        {
            get;
        }
        string Description
        {
            get;
        }

        SBPluginHost PluginHost
        {
            set;
        }

        void Dispose();
    }

    public interface SBPluginHost
    {
        void PluginResponse( string channel, string message );

        event UserJoin eventPluginUserJoined;
        event UserLeave eventPluginUserLeft;
        event UserKick eventPluginUserKicked;
        event UserChange eventPluginUserChangedNick;
        event UserQuit eventPluginUserQuit;
        event UserMode eventPluginUserModeSet;

        event ChannelMessage eventPluginChannelMessageReceived;
        event PrivateMessage eventPluginPrivateMessageReceived;

        event ChannelCommand eventPluginChannelCommandReceived;
        event PrivateCommand eventPluginPrivateCommandReceived;

        event ServerTopicSet eventPluginChannelTopicSet;
        event ServerTopicOwner eventPluginChannelTopicOwnerGet;
        event ServerNames eventPluginChannelNameListGet;

        string PluginTwitterAccessToken
        {
            get;
        }

        string PluginTwitterAccessTokenSecret
        {
            get;
        }

        string PluginTwitterConsumerKey
        {
            get;
        }
        
        string PluginTwitterConsumerKeySecret
        {
            get;
        }

        string PluginBotname
        {
            get;
        }

        string PluginBotFolder
        {
            get;
        }
    }
}
