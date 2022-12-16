﻿using Stratis.SmartContracts;

/// <summary>
/// Implementation of a single-use auction contract.
/// DISCLAIMER: For demonstration purposes only. We would recommend significant testing
/// before using the following code in your own applications.
/// </summary>
[Deploy]
public class Auction : SmartContract
{
    public Address Owner
    {
        get
        {
            return this.State.GetAddress("Owner");
        }
        private set
        {
            this.State.SetAddress("Owner", value);
        }
    }

    public ulong EndBlock
    {
        get
        {
            return this.State.GetUInt64("EndBlock");
        }
        private set
        {
            this.State.SetUInt64("EndBlock", value);
        }
    }

    public Address HighestBidder
    {
        get
        {
            return this.State.GetAddress("HighestBidder");
        }
        private set
        {
            this.State.SetAddress("HighestBidder", value);
        }
    }

    public ulong HighestBid
    {
        get
        {
            return this.State.GetUInt64("HighestBid");
        }
        private set
        {
            this.State.SetUInt64("HighestBid", value);
        }
    }

    public bool HasEnded
    {
        get
        {
            return this.State.GetBool("HasEnded");
        }
        private set
        {
            this.State.SetBool("HasEnded", value);
        }
    }

    public ulong GetBalance(Address address)
    {
        return this.State.GetUInt64($"Balances[{address}]");
    }

    private void SetBalance(Address address, ulong balance)
    {
        this.State.SetUInt64($"Balances[{address}]", balance);
    }

    public Auction(ISmartContractState smartContractState, ulong durationBlocks)
        : base(smartContractState)
    {
        this.Owner = this.Message.Sender;
        this.EndBlock = this.Block.Number + durationBlocks;
        this.HasEnded = false;

        Log(new Created { duration = durationBlocks, sender = Message.Sender.ToString() });
    }

    public void Bid()
    {
        Assert(this.Block.Number < this.EndBlock);
        Assert(this.Message.Value > this.HighestBid);
        if (this.HighestBid > 0)
        {
            ulong currentBalance = GetBalance(this.Message.Sender);
            SetBalance(this.Message.Sender, this.HighestBid + currentBalance);
        }
        this.HighestBidder = this.Message.Sender;
        this.HighestBid = this.Message.Value;
    }

    public bool Withdraw()
    {
        ulong amount = GetBalance(this.Message.Sender);
        Assert(amount > 0);
        SetBalance(this.Message.Sender, 0);
        ITransferResult transferResult = Transfer(this.Message.Sender, amount);
        if (!transferResult.Success)
            this.SetBalance(this.Message.Sender, amount);
        return transferResult.Success;
    }

    public void AuctionEnd()
    {
        Assert(this.Block.Number >= this.EndBlock);
        Assert(!this.HasEnded);
        this.HasEnded = true;
        Transfer(this.Owner, this.HighestBid);
    }

    public struct Created
    {
        [Index]
        public ulong duration;
        public string sender;
    }
}