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

    [Test]
    public void Time(TestEnv env)
    {
        var info1 = env.Time("test 1", 1, 2, () =>
        {
            Task.Delay(110).Wait();
        });

        var info2 = env.Time("test 2", 10, 20, () =>
        {
            Task.Delay(11).Wait();
        });

        var info3 = env.Time("test 3", 1, 2, () =>
        {
            Task.Delay(110).Wait();
        });

    }
}