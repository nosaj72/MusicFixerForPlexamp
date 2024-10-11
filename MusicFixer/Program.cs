using ATL;
using MusicFixer;
using PowerArgs;


Console.Title = "MusicFixer";
Console.WriteLine("MusicFixer: Plexamp Release Type Fixer");
var parsed = Args.Parse<Params>(args);

var attr = File.GetAttributes(parsed.Path);

if (attr.HasFlag(FileAttributes.Directory))
{
    await ReadDirectory(parsed.Path, parsed.fix);
}
else
{
    var dir = Directory.GetParent(parsed.Path);
    if (dir != null) await ReadDirectory(dir.FullName, parsed.fix);
}

Console.WriteLine("Complete");
await Task.Delay(5000);
return;

async Task ReadDirectory(string directory, bool autoFix)
{
    Console.WriteLine($"Checking directory {directory}");
    var directories = Directory.GetDirectories(directory);

    foreach (var child in directories) await ReadDirectory(child, autoFix);

    var files = Directory.GetFiles(directory, "*.*");

    var type = "";

    foreach (var alacFile in files.Where(x => x.EndsWith(".m4a", StringComparison.OrdinalIgnoreCase) || x.EndsWith(".flac", StringComparison.OrdinalIgnoreCase)))
    {
        var theTrack = new Track(alacFile);

        var releaseType = "";
        if (theTrack.AdditionalFields.TryGetValue("RELEASETYPE", out var value)) releaseType = value;

        if (type == "")
        {
            Console.WriteLine($"{theTrack.Artist} - {theTrack.Album}");

            if (releaseType != "") Console.WriteLine($"Current Release Type: {releaseType}");

            if (autoFix)
            {
                type = "fix";
            }
            else
            {
                Console.WriteLine(
                "1: Album, 2: Live, 3: Compilation, 4: Single, 5: EP, 6: Remix, 7: Soundtrack, 0: No Change, C: Clear, Enter: Fix");
                var key = Console.ReadKey();
                Console.WriteLine();
                if (key.Key == ConsoleKey.Escape) break;
                if (key.KeyChar == 'x') break;
                type = key.KeyChar switch
                {
                    '1' => "album",
                    '2' => "album;live",
                    '3' => "album;compilation",
                    '4' => "single",
                    '5' => "ep",
                    '6' => "album;remix",
                    '7' => "album;soundtrack",
                    '0' => "keep",
                    'c' => "clear",
                    _ => "fix"
                };
            }

            
        }

        switch (type)
        {
            case "keep":
            case "fix" when releaseType == "":
                continue;
            case "fix":
            {
                type = releaseType;
                if (releaseType == "live") type = "album;live";
                if (releaseType == "compilation") type = "album;compilation";
                if (releaseType == "remix") type = "album;remix";
                if (releaseType == "soundtrack") type = "album;soundtrack";
                break;
            }
        }

        if (type == "clear")
        {
            Console.WriteLine($"Updating {theTrack.TrackNumber} - {theTrack.Title}");
            theTrack.AdditionalFields.Remove("RELEASETYPE");
            await theTrack.SaveAsync();
            continue;
        }

        if (type == releaseType) continue;
        Console.WriteLine($"Updating {theTrack.TrackNumber} - {theTrack.Title} to {type}");
        if (!theTrack.AdditionalFields.TryAdd("RELEASETYPE", releaseType))
            theTrack.AdditionalFields["RELEASETYPE"] = type;

        await theTrack.SaveAsync();
    }
}