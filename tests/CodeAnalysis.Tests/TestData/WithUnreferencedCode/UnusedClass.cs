namespace WithUnreferencedCode;

// This entire class is unreferenced
public class UnusedClass
{
    public string UnusedMethod()
    {
        return "This method is never called";
    }

    public int UnusedProperty { get; set; }

    private void PrivateUnusedMethod()
    {
        // This is also unreferenced
    }
}