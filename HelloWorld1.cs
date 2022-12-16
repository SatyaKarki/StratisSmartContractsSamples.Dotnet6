using Stratis.SmartContracts;
using System;

public class HelloWorld1 : SmartContract
{
    public HelloWorld1(ISmartContractState smartContractState) : base(smartContractState)
    {
        this.Greeting = "Hello World!";
    }

    public string Greeting
    {
        get => this.State.GetString(nameof(this.Greeting));
        private set => this.State.SetString(nameof(this.Greeting), value);
    }

    public string SayHello()
    {
        return this.Greeting;
    }
}