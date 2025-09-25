using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChatterServer.Services
{
    public interface IChange
    {

        //public DateTime TimeStamp { get; }
    }

    public interface IDictionaryAddChange : IChange
    {
        public string Key { get; }
        public string Value { get; }
    }
    public interface IDictionaryRemoveChange : IChange
    {
        public string Key { get; }
    }

    public interface IStringChange : IChange
    {
        public string Value { get; }
    }
    public interface IIntChange : IChange
    {
        public int Value { get; }
    }
    public interface IBooleanChange : IChange
    {
        public bool Value { get; }
    }

    public readonly record struct OutputChange(string Value) : IStringChange
    {
        //public DateTime TimeStamp { get; } = DateTime.Now;
    }
    public readonly record struct UserNameAddChange(string Key, string Value) : IDictionaryAddChange
    {
       // public DateTime TimeStamp { get; } = DateTime.Now;
    }
    public readonly record struct UserNameRemoveChange(string Key, string Value) : IDictionaryAddChange
    {
        //public DateTime TimeStamp { get; } = DateTime.Now;
    }

    public readonly record struct StatusChange(string Value) : IStringChange
    {
        //public DateTime TimeStamp { get; } = DateTime.Now;
    }
    public readonly record struct IsRunningChange(bool Value) : IBooleanChange
    {
        //public DateTime TimeStamp { get; } = DateTime.Now;
    }
    public readonly record struct ExternalAddressChange(string Value) : IStringChange
    {
        //public DateTime TimeStamp { get; } = DateTime.Now;
    }


    public readonly record struct PortChange(string Value) : IStringChange
    {
       // public DateTime TimeStamp { get; } = DateTime.Now;
    }

    public readonly record struct ClientsConnectedChange(int Value) : IIntChange
    {
        //public DateTime TimeStamp { get; } = DateTime.Now;
    }

    public readonly record struct RunChange() : IChange
    {
        //public DateTime TimeStamp { get; } = DateTime.Now;
    }

    public readonly record struct StopChange() : IChange
    {
        //public DateTime TimeStamp { get; } = DateTime.Now;
    }
}
