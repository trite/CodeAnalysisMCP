namespace WithUnreferencedCode;

public class UsedClass
{
    public string UsedMethod()
    {
        return "This method is used";
    }

    public int UsedProperty { get; set; }
}

public class Consumer
{
    public void UseOtherClass()
    {
        var used = new UsedClass();
        var result = used.UsedMethod();
        used.UsedProperty = 42;
    }
}
