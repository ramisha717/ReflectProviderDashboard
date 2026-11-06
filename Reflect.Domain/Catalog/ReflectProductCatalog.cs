namespace Reflect.Domain;

public static class ReflectProductCatalog
{
    public const string PlatformXR = "Reflect XR";
    public const string PlatformMobile = "Reflect Mobile";

    // Reflect Mobile Groups
    public const string MobileGroupCreativeCalm = "Creative Calm";
    public const string MobileGroupCreate = "Create";
    public const string MobileGroupLearn = "Learn";
    public const string MobileGroupReflect = "Reflect";

    public static readonly IReadOnlyList<string> MobileGroups = new[]
    {
        MobileGroupCreativeCalm,
        MobileGroupCreate,
        MobileGroupLearn,
        MobileGroupReflect
    };

    // Reflect XR Programs
    public const string XRProgramCreativeCalm = "Creative Calm";
    public const string XRProgramExploreEmotions = "Explore Emotions";
    public const string XRProgramExploreSelf = "Explore Self";
    public const string XRProgramExploreJourney = "Explore Journey";
    public const string XRProgramGettingStarted = "Getting Started";

    public static readonly IReadOnlyList<string> XRPrograms = new[]
    {
        XRProgramCreativeCalm,
        XRProgramExploreEmotions,
        XRProgramExploreSelf,
        XRProgramExploreJourney,
        XRProgramGettingStarted
    };

    // Reflect Mobile 14 modules
    public static readonly IReadOnlyList<(string Module, string Group)> MobileModules = new[]
    {
        ("Floating Bubbles", MobileGroupCreativeCalm),
        ("Finger Painting", MobileGroupCreativeCalm),
        ("Pour Paint", MobileGroupCreativeCalm),
        ("Breathing Mandala", MobileGroupCreativeCalm),
        ("Photo Coloring (AI)", MobileGroupCreativeCalm),
        ("AI Created Mandala (AI)", MobileGroupCreativeCalm),
        ("Create Your Own Prompt (AI)", MobileGroupCreate),
        ("Options Prompt Pathway (AI)", MobileGroupCreate),
        ("Select from Images Pathway (AI)", MobileGroupCreate),
        ("Creativity (AI)", MobileGroupLearn),
        ("Emotions (AI)", MobileGroupLearn),
        ("Self (AI)", MobileGroupLearn),
        ("Journey (AI)", MobileGroupLearn),
        ("Reflection Journals (AI)", MobileGroupReflect)
    };

    // Reflect XR 17 modules
    public static readonly IReadOnlyList<(string Module, string Program)> XRModules = new[]
    {
        ("Containment", XRProgramCreativeCalm),
        ("Rhythmic Relaxation", XRProgramCreativeCalm),
        ("Color Psychology", XRProgramCreativeCalm),
        ("Flow", XRProgramCreativeCalm),
        ("Emotional Expression", XRProgramExploreEmotions),
        ("Self Compassion", XRProgramExploreEmotions),
        ("Catharsis", XRProgramExploreEmotions),
        ("Portal to the Subconscious", XRProgramExploreEmotions),
        ("Present Self Outside", XRProgramExploreSelf),
        ("Present Self Inside", XRProgramExploreSelf),
        ("Personal Strengths", XRProgramExploreSelf),
        ("Future Self", XRProgramExploreSelf),
        ("Appreciation of Life", XRProgramExploreJourney),
        ("Hope Tree", XRProgramExploreJourney),
        ("New Pathways", XRProgramExploreJourney),
        ("Finding Balance", XRProgramExploreJourney),
        ("Training Tutorial", XRProgramGettingStarted)
    };

    public static string GetGroupOrProgram(string platform, string moduleName)
    {
        if (platform == PlatformMobile)
        {
            for (int i = 0; i < MobileModules.Count; i++)
            {
                if (string.Equals(MobileModules[i].Module, moduleName, StringComparison.OrdinalIgnoreCase))
                {
                    return MobileModules[i].Group;
                }
            }
        }
        else if (platform == PlatformXR)
        {
            for (int i = 0; i < XRModules.Count; i++)
            {
                if (string.Equals(XRModules[i].Module, moduleName, StringComparison.OrdinalIgnoreCase))
                {
                    return XRModules[i].Program;
                }
            }
        }

        return string.Empty;
    }
}
