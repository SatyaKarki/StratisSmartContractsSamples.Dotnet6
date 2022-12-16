using Stratis.SmartContracts;

public class BasicProvenance : SmartContract
{
    public BasicProvenance(ISmartContractState smartContractState, Address supplyChainOwner, Address supplyChainObserver)
    : base(smartContractState)
    {
        this.InitiatingCounterParty = Message.Sender;
        this.CounterParty = Message.Sender;
        this.SupplyChainOwner = supplyChainOwner;
        this.SupplyChainObserver = supplyChainObserver;
        Status = (uint)StateType.Created;
    }

    public enum StateType : uint
    {
        Created = 0,
        InTransit = 1,
        Completed = 2
    }

    public uint Status
    {
        get => this.State.GetUInt32(nameof(this.Status));
        private set => this.State.SetUInt32(nameof(this.Status), value);
    }

    public Address InitiatingCounterParty
    {
        get => this.State.GetAddress(nameof(this.InitiatingCounterParty));
        private set => this.State.SetAddress(nameof(this.InitiatingCounterParty), value);
    }

    public Address CounterParty
    {
        get => this.State.GetAddress(nameof(this.CounterParty));
        private set => this.State.SetAddress(nameof(this.CounterParty), value);
    }

    public Address PreviousCounterParty
    {
        get => this.State.GetAddress(nameof(this.PreviousCounterParty));
        private set => this.State.SetAddress(nameof(this.PreviousCounterParty), value);
    }

    public Address SupplyChainOwner
    {
        get => this.State.GetAddress(nameof(this.SupplyChainOwner));
        private set => this.State.SetAddress(nameof(this.SupplyChainOwner), value);
    }
    public Address SupplyChainObserver
    {
        get => this.State.GetAddress(nameof(this.SupplyChainObserver));
        private set => this.State.SetAddress(nameof(this.SupplyChainObserver), value);
    }

    public void TransferResponsibility(Address newCounterParty)
    {
        Assert(this.CounterParty == Message.Sender && this.Status != (uint)StateType.Completed);

        if (this.Status == (uint)StateType.Created)
        {
            Status   = (uint)StateType.InTransit;
        }

        PreviousCounterParty = CounterParty;
        CounterParty = newCounterParty;
    }

    public void Complete()
    {
        Assert(this.SupplyChainOwner == Message.Sender && Status != (uint)StateType.Completed);

        Status = (uint)StateType.Completed;
        PreviousCounterParty = CounterParty;
        CounterParty = Address.Zero;
    }
}