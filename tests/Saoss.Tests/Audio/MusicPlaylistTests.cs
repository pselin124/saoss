using Saoss.Game.Audio;

namespace Saoss.Tests.Audio;

/// <summary>
/// Tests the music playlist: track selection, the start track and random picks.
/// </summary>
[TestClass]
public sealed class MusicPlaylistTests
{
    [TestMethod]
    public void SelectTracks_KeepsMp3AndOggOnly_Mp3sFirstThenByName()
    {
        var files = new[]
        {
            @"m\b.ogg", @"m\notes.txt", @"m\z.MP3", @"m\a.ogg", @"m\song.xm", @"m\c.mp3", @"m\cover.jpg", @"m\noext",
        };

        var tracks = MusicPlaylist.SelectTracks(files);

        CollectionAssert.AreEqual(new[] { @"m\c.mp3", @"m\z.MP3", @"m\a.ogg", @"m\b.ogg" }, tracks.ToArray());
    }

    [TestMethod]
    public void FirstTrack_IsTheStartTrack()
    {
        var playlist = new MusicPlaylist("start.mp3", ["a.ogg", "b.mp3"]);

        Assert.AreEqual("start.mp3", playlist.FirstTrack(new Random(1)));
    }

    [TestMethod]
    public void FirstTrack_WithoutStartTrack_IsARandomTrack()
    {
        var playlist = new MusicPlaylist(null, ["a.ogg"]);

        Assert.AreEqual("a.ogg", playlist.FirstTrack(new Random(1)));
    }

    [TestMethod]
    public void NextRandom_PicksEveryTrackAndNeverTheStartTrack()
    {
        var playlist = new MusicPlaylist("start.mp3", ["a.ogg", "b.mp3", "c.ogg"]);
        var random = new Random(7);

        var picked = Enumerable.Range(0, 200).Select(_ => playlist.NextRandom(random)).Distinct().Order().ToArray();

        CollectionAssert.AreEqual(new[] { "a.ogg", "b.mp3", "c.ogg" }, picked);
    }

    [TestMethod]
    public void EmptyPlaylist_HasNothingToPlay()
    {
        var playlist = new MusicPlaylist(null, []);

        Assert.IsNull(playlist.FirstTrack(new Random(1)));
        Assert.IsNull(playlist.NextRandom(new Random(1)));
    }

    [TestMethod]
    public void Scan_OfTheShippedMusic_FindsTheStartTrackAndTheTopLevelTracks()
    {
        var folder = Path.Combine(AppContext.BaseDirectory, "Content", "Music");

        var playlist = MusicPlaylist.Scan(folder);

        Assert.AreEqual(Path.Combine(folder, "Start", "start.mp3"), playlist.StartTrack);
        CollectionAssert.AreEqual(
            new[] { "eramaa.mp3", "love_me.mp3", "dialinthefortforyou.ogg", "electronixdanze.ogg" },
            playlist.Tracks.Select(Path.GetFileName).ToArray());
    }

    [TestMethod]
    public void Scan_OfAMissingFolder_IsEmpty()
    {
        var playlist = MusicPlaylist.Scan(Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString("N")));

        Assert.IsNull(playlist.StartTrack);
        Assert.IsEmpty(playlist.Tracks);
    }
}
