using AUnit;

#pragma warning disable CA1822 // Mark members as static

//Console.WriteLine("Hello world!");

class MyTests
{
    [Test("my tag")]
    public void AlwaysSucceed()
    {
        Assert.Succeed("This test always succeeds.");
    }

    [Test]
    [ExpectedStatus(TestStatus.Failed)]
    public void AlwaysFail()
    {
        Assert.Fail("This test always fails.");
    }

    [Test]
    [ExpectedStatus(TestStatus.Failed)]
    public void PrintAdditionalInfo()
    {
        Console.WriteLine("printing to console ...");
        throw new Exception("... and logging exceptions");
    }
}