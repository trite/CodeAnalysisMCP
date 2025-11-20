namespace MixedAccessibility;

public class MixedClass
{
    // Public unreferenced
    public string PublicUnused { get; set; } = "";

    // Internal unreferenced
    internal string InternalUnused { get; set; } = "";

    // Private unreferenced
    private string _privateUnused = "";

    // Public used
    public string PublicUsed { get; set; } = "";

    // Private used
    private string _privateUsed = "";

    public void UsePrivate()
    {
        _privateUsed = "used";
        var x = _privateUsed;
    }
}

public class MixedConsumer
{
    public void Use()
    {
        var mixed = new MixedClass();
        mixed.PublicUsed = "test";
    }
}

internal class InternalUnusedClass
{
    public string Property { get; set; } = "";
}

public class PublicUnusedClass
{
    public string Property { get; set; } = "";
}
