﻿using Stratis.SmartContracts;

public class Airdrop : SmartContract
{
    /// <summary>
    /// Constructor used to create a new Airdrop contract. Assigns total supply of airdrop, the
    /// contract address for the token being airdropped and the endblock that the airdrop closes on.
    /// </summary>
    /// <param name="smartContractState">The execution state for the contract.</param>
    /// <param name="tokenContractAddress">The smart contract address of the token being airdropped.</param>
    /// <param name="totalSupply">The total amount that will be airdropped, amount will be divided amongst registrants.</param>
    /// <param name="endBlock">The block that ends the sign up period and allows withdrawing, can use 0 to manually end airdrop using <see cref="CloseRegistration"/>.</param>
    public Airdrop(
        ISmartContractState smartContractState,
        Address tokenContractAddress,
        ulong totalSupply,
        ulong endBlock
    ) : base(smartContractState)
    {
        Assert(totalSupply > 0);

        TotalSupply = totalSupply;
        TokenContractAddress = tokenContractAddress;
        EndBlock = endBlock;
        Owner = Message.Sender;
    }

    private const string EnrolledStatus = "ENROLLED";
    private const string FundedStatus = "FUNDED";

    /// <summary>The contract address of the token that will be distributed. This smart contracts
    /// address must be approved to send at least the TotalSupply set.</summary>
    public Address TokenContractAddress
    {
        get => State.GetAddress(nameof(TokenContractAddress));
        private set => State.SetAddress(nameof(TokenContractAddress), value);
    }

    /// <summary>The total supply of the token that will be distributed during this airdrop.</summary>
    public ulong TotalSupply
    {
        get => State.GetUInt64(nameof(TotalSupply));
        private set => State.SetUInt64(nameof(TotalSupply), value);
    }

    /// <summary>
    /// Used to automatically close registration based on a specified block number. If the EndBlock is 0, the
    /// registration period for the airdrop must be manually ended by the owner using <see cref="CloseRegistration"/>
    /// </summary>
    public ulong EndBlock
    {
        get => State.GetUInt64(nameof(EndBlock));
        private set => State.SetUInt64(nameof(EndBlock), value);
    }

    /// <summary>Address of the owner of this contract, used to authenticate some methods. This address must
    /// authorize this smart contracts address, after deployed, to spend the <see cref="TotalSupply"/></summary>
    public Address Owner
    {
        get => State.GetAddress(nameof(Owner));
        private set => State.SetAddress(nameof(Owner), value);
    }

    public ulong NumberOfRegistrants
    {
        get => State.GetUInt64(nameof(NumberOfRegistrants));
        private set => State.SetUInt64(nameof(NumberOfRegistrants), value);
    }

    private ulong AmountToDistribute
    {
        get => State.GetUInt64(nameof(AmountToDistribute));
        set => State.SetUInt64(nameof(AmountToDistribute), value);
    }

    public string GetAccountStatus(Address address)
    {
        return State.GetString($"Status:{address}");
    }

    private void SetAccountStatus(Address address, string status)
    {
        State.SetString($"Status:{address}", status);
    }

    /// <summary>Decides whether or not new users can register.</summary>
    public bool CanRegister => EndBlock == 0 || Block.Number < EndBlock;

    /// <summary>See <see cref="AddRegistrantExecute(Address)"/></summary>
    public bool Register()
    {
        return AddRegistrantExecute(Message.Sender);
    }

    /// <summary>Allows owner of the contract to manually add a new registrant.
    /// See <see cref="AddRegistrantExecute(Address)"/></summary>
    public bool AddRegistrant(Address registrant)
    {
        return Message.Sender == Owner && AddRegistrantExecute(registrant);
    }

    /// <summary>Calculate and set the <see cref="AmountToDistribute"/></summary>
    public ulong GetAmountToDistribute()
    {
        ulong amount = State.GetUInt64(nameof(AmountToDistribute));
        if (amount == 0 && !CanRegister && NumberOfRegistrants > 0)
        {
            amount = TotalSupply / NumberOfRegistrants;
            AmountToDistribute = amount;
        }

        return amount;
    }

    /// <summary>Allows owner to close registration for the airdrop at any time.</summary>
    public bool CloseRegistration()
    {
        if (Message.Sender != Owner)
        {
            return false;
        }

        EndBlock = Block.Number;

        return true;
    }

    /// <summary>
    /// Withdraw funds after registration period has closed. Validates account status and calls the tokens contract
    /// address that is being airdropped to transfer amount to sender. On success, set senders new status and log it.
    /// The contract address must be approved at the TokenContractAddress to send at least the TotalSupply from the Owners wallet.
    /// </summary>
    public bool Withdraw()
    {
        bool invalidAccountStatus = GetAccountStatus(Message.Sender) != EnrolledStatus;

        if (invalidAccountStatus || CanRegister)
        {
            return false;
        }

        var amountToDistribute = GetAmountToDistribute();
        var transferParams = new object[] { Owner, Message.Sender, amountToDistribute };
        var result = Call(TokenContractAddress, amountToDistribute, "TransferFrom", transferParams);

        Assert(result.Success);

        SetAccountStatus(Message.Sender, FundedStatus);

        Log(new StatusLog { Registrant = Message.Sender, Status = FundedStatus });

        return true;
    }

    /// <summary>Validates and adds a new registrant. Updates the <see cref="NumberOfRegistrants"/>,
    /// account status, and logs result.</summary>
    private bool AddRegistrantExecute(Address registrant)
    {
        if (registrant == Owner)
        {
            return false;
        }

        bool validAccountStatus = string.IsNullOrWhiteSpace(GetAccountStatus(registrant));
        if (!validAccountStatus || !CanRegister || NumberOfRegistrants >= TotalSupply)
        {
            return false;
        }

        NumberOfRegistrants += 1;

        SetAccountStatus(registrant, EnrolledStatus);

        Log(new StatusLog { Registrant = registrant, Status = EnrolledStatus });

        return true;
    }

    public struct StatusLog
    {
        [Index]
        public Address Registrant;
        public string Status;
    }
}