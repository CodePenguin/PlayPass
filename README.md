PlayPass: Automated queuing engine for PlayLater
================================

[![Build status](https://ci.appveyor.com/api/projects/status/github/CodePenguin/PlayPass?svg=true)](https://ci.appveyor.com/project/CodePenguin/playpass/build/artifacts)

[![MIT License](https://img.shields.io/github/license/CodePenguin/PlayPass.svg)](https://github.com/CodePenguin/PlayPass/blob/master/license.txt)

Overview
--------------------------------

[PlayLater](http://playon.tv) is essentially an internet DVR created by [MediaMall Technologies, Inc](http://playon.tv).  I can't say enough about this great product.  It allows you to record many internet videos to your local machine so you can view them offline or keep them forever even if they are no longer available online. Unfortunately, the recording interface is a little bit manual at the moment so you can't automatically download all the newest episodes of your favorite show as they become available.  The original developers have indicated that they are working on a solution for a future update.

For those who want a solution right now there is PlayPass. PlayPass is an unofficial way of automatically queuing new content.  It uses the same technology that the PlayLater mobile apps use to queue the content so it does not require any modification of PlayLater.

Download the latest release
--------------------------------

All releases of PlayPass are available for download at [https://github.com/CodePenguin/PlayPass/releases](https://github.com/CodePenguin/PlayPass/releases).

How it works
--------------------------------

PlayPass works with PlayLater just like you would.  It needs to know what PlayOn folders to open and what videos to queue for recording.

### PlayLater Example

Let's say you want to queue up the latest episodes for your favorite TV show titled "Random TV Show" on the "Random TV Network".  In PlayLater you would click on the following items sequentially:

- Random TV Network
- All Current Shows
- Random TV Show
- Season 1
- Episode 1 - Awesome title!

You would have repeat this for every season and every episode individually.  That can take a long time and you'd constantly have to come back and check if the new episodes have been posted.  Once you started watching additional TV shows, this starts to get very tedious. 

### PlayPass Example

Now let's use PlayPass to remove the tedius parts so we can get back to watching.  Think of a PlayPass `pass` as a season pass for TV shows or movies.  A `pass` consists of two types of actions:

- `scan`: Looks through the current PlayOn folder looking for any **folders** that match what you would have clicked.
- `queue`: Looks through the current PlayOn folder looking for any **videos** that match what you would have clicked.

Everytime PlayPass is run in `queue` mode it will go through all the defined `passes`, `scan` for the specific folders you would have clicked manually, then tries to `queue` any videos it finds.

Based on our previous example PlayPass needs to do the following:

- `scan` for "Random TV Network"
- `scan` for "All Current Shows"
- `scan` for "Random TV Show"
- `scan` for "Season *"
- `queue` for "*"

So instead of telling it which season you would click on, we used a wild card (PlayPass supports * as a wildcard to match zero or many characters or ? to match one character.) to tell it to click on ALL seasons! And then instead of telling it a single episode to queue, we use a wild card again to tell it to queue ALL videos.  Thats it!  Every time PlayPass is run, it will automatically check for all the new episodes and start queueing them up for recording.

PlayPass uses XML config files to define the `passes` like we just described.  Lets's look at the PlayPass config file for the above example:

    <playpass>
        <passes>
            <pass description="Random TV Shows">
                <scan name="Random TV Network">
                    <scan name="All Current Shows">
                        <scan name="Random, TV Show">
                            <scan name="*">
                                <queue name="*" />
                            </scan>
                        </scan>
                    </scan>
                </scan>
            </pass>
        </passes>
    </playpass>

Each time PlayPass sees a `scan` node in the XML it will look at the `name` property and try to find any folders in the current PlayOn folder that match. The XML layout makes it kind of look like a folder tree.  In your config file you can add as many "pass" nodes as you'd like to the `passes` section.  All `pass` nodes can have as many `scan` and `queue` nodes as needed and they can be nested down as far as you need to go.

### More powerful PlayLater Example

PlayPass is pretty powerful.  You can do more in a single `pass` than just download one show.  What if you wanted to download all the movies and TV Shows in your "My Things To Watch" folder on the "Random TV Network"?  It would be really annoying if you had to tell it the name of every movie or every TV show you would ever add to the queue.  PlayPass has us covered!

    <playpass>
        <pass enabled="1" description="Random TV Network Shows">
            <scan name="Random TV Network">
                <scan name="My Things to Watch">
                    <queue name="*" />
                    <scan name="*">
                        <queue name="*" />
                        <scan name="Season *">
                            <queue name="*" />
                        </scan>
                    </scan>
                </scan>
            </scan>
        </pass>
    </playpass>


Check out the [Wiki](https://github.com/CodePenguin/PlayPass/wiki) for more examples of various services and other advanced settings.

How to use it
--------------------------------

You can compile from source or grab the latest release files and start from there.  You can modify the included `PlayPass.cfg` file or create your own.  If you don't specify which config file to use, PlayPass will just use `PlayPass.cfg`.

### Queue Mode

To test what would be queued just execute PlayPass.exe with the filename of your config file:

    PlayPass.exe MyConfig.cfg

When you are ready to run it in Queue Mode use the following:

    PlayPass.exe -queue MyConfig.cfg

When videos are queued a special `filename.playpass.skip` file is created for each video.  This makes it to files are not continually queued when you've already recorded it.  This allows you to move the final recorded file somewhere else.  By default, skip files are stored in the PlayLater media location specified in the PlayLater settings.

If you need to re-queue a file, just delete the corresponding skip file and re-run PlayPass.

### Verbose Mode

For debugging you can run it in Verbose Mode using the following:

    PlayPass.exe -verbose MyConfig.cfg

Verbose Mode prints out a lot more information to help you see what text you need to match up to in order to queue the desired items.

### Skip Mode

For automatically skipping everything found on this run, you can run in Skip Mode using the following:

    PlayPass.exe -skip MyConfig.cfg

Skip Mode creates a special filename.playpass.skip file for each file that would be queued.

### Scheduling

You can use Window's built in Task Scheduler program to execute PlayPass whenever you want.  I've got mine going off every day at midnight.  This way you are in control of when items are queued for downloading.

License
--------------------------------

PlayPass and PlaySharp are licensed under [MIT](http://opensource.org/licenses/MIT). Refer to license.txt for more information.

Disclaimer
--------------------------------

PlayPass and PlaySharp are in no way supported by or affiliated with MediaMall Technologies, Inc. PlayPass was designed by someone who really enjoys the PlayOn/PlayLater system and wanted to add something to it.
