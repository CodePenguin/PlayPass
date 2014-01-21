PlayPass: Automated queuing engine for PlayLater
================================

What is it?
--------------------------------
[PlayLater](http://playon.tv) is essentially an internet DVR created by [MediaMall Technologies, Inc](http://playon.tv).  I can't say enough about this great product.  It allows you to record many internet videos to your local machine so you can view them offline or keep them forever even if they are no longer available online. Unfortunately, the recording interface is a little bit manual at the moment so you can't automatically download all the newest episodes of your favorite show as they become available.  The original developers have indicated that they are working on a solution for a future update.

For those who want a solution right now there is PlayPass. PlayPass is an unofficial way of automatically queuing new content.  It uses the same technology that the PlayLater mobile apps use to queue the content so it does not require any modification of PlayLater.

Components
--------------------------------
####PlayPass 
A console program written in C# that will automatically queue items for download based on user preferences in an XML config file.

####PlaySharp
A .NET 2.0 Assembly written in C# that handles all the communication between PlayPass and the PlayOn/PlayLater server.  It abstracts the PlayOn Server data protocols into easy to consume .NET native objects.  This could be used to create other kinds of interfaces.

How to build it
--------------------------------
Open the PlayPass.sln file in Visual Studios (Only Visual Studios 2013 has been tested but earlier versions should work also) and select "Build Solution" from the "Build" menu.

How to use it
--------------------------------
Make a copy of the PlayPass.example.cfg and modify it to your liking.  The following is an example of a config file that will automatically queue up anything in your Hulu Queue.

	<playpass>
		<settings />
		<passes>
			<pass enabled="1" description="Hulu Queue">
				<scan name="Hulu" />
				<scan name="Your Queue" />
				<scan name="Sort By Date" />
			</pass>
		</passes>
	</playpass>

You can add as many "passes" nodes as you'd like.  You can even disable one by changing the "enabled" property to "0".

To test what would be queued just execute PlayPass.exe with the filename of your config file:

    PlayPass.exe MyConfig.cfg

When you are ready to run it in Queue Mode use the following:

    PlayPass.exe -queue MyConfig.cfg

License
--------------------------------
PlayPass and PlaySharp are licensed under [MIT](http://opensource.org/licenses/MIT). Refer to license.txt for more information.

Disclaimer
--------------------------------
PlayPass and PlaySharp are in no way supported by or affiliated with MediaMall Technologies, Inc. PlayPass was designed by someone who really enjoys the PlayOn/PlayLater system and wanted to add something to it.