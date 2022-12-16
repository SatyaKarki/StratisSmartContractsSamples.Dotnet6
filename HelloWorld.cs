using Stratis.SmartContracts;
using System;

[Deploy]
public class HelloWorld : SmartContract
{
    private string Greeting
    {
        get
        {
            return this.State.GetString("Greeting");
        }
        set
        {
            this.State.SetString("Greeting", value);
        }
    }

    public HelloWorld(ISmartContractState smartContractState)
        : base(smartContractState)
    {
        this.Greeting = "Hello World!";
    }

    public string SayHello()
    {
        return this.Greeting;
    }

}
