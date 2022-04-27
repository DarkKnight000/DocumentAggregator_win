using System;

namespace DocAggregator.API.Core.Models
{
    public enum ClaimFieldSource
    {
        Claim,
        Additional,
        AccessRight,
    }

    public class ClaimField
    {
        public int? NumeralID { get; init; }
        public string VerbousID { get; init; }
        public string Category { get; init; }
        public string Attribute { get; init; }
        public string Value { get; init; }
        public bool ToBoolean() => bool.TryParse(Value, out bool result) & result;
    }

    public class AccessRightField
    {
        public int NumeralID { get; init; }
        public string Name { get; init; }
        public AccessRightStatus Status { get; init; }
        public bool IsAllowed => Status.HasFlag(AccessRightStatus.Allowed);
        public bool IsDenied => Status.HasFlag(AccessRightStatus.Denied);
    }
}
