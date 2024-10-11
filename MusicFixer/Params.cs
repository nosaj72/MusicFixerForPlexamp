using PowerArgs;

namespace MusicFixer;

public class Params
{
    [ArgRequired(PromptIfMissing = true)]
    [ArgDescription("Directory to scan")]
    [ArgShortcut("p")]
    public required string Path { get; set; }

    [ArgDescription("Fix Tags")]
    [ArgShortcut("f")]
    public bool fix { get; set; }
}