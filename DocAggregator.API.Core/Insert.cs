using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DocAggregator.API.Core
{
    public enum InsertKind
    {
        Unknown,
        PlainText,
        CheckMark,
    }

    public class Insert
    {
        private bool? replacedCheckmark;

        public InsertKind Kind { get; private set; }
        public string ReplacedText { get; set; }
        public bool? ReplacedCheckmark
        {
            get
            {
                // TODEL:
                Console.WriteLine($"{GetType()}.{nameof(ReplacedCheckmark)} returns \"{replacedCheckmark}\".");
                return replacedCheckmark;
            }
            set
            {
                // TODEL:
                Console.WriteLine($"{GetType()}.{nameof(ReplacedCheckmark)} has changed from \"{replacedCheckmark}\" to \"{value}\".");
                replacedCheckmark = value;
            }
        }
        public string OriginalMask { get; set; }
        public object AssociatedChunk { get; set; }

        public Insert(string mask, InsertKind kind = InsertKind.PlainText)
        {
            OriginalMask = mask;
            Kind = kind;
            // TODEL:
            Console.WriteLine($"{GetType()} created with arguments (\"{mask}\", {kind}).");
        }

        // override object.Equals
        public override bool Equals(object obj)
        {
            //       
            // See the full list of guidelines at
            //   http://go.microsoft.com/fwlink/?LinkID=85237  
            // and also the guidance for operator== at
            //   http://go.microsoft.com/fwlink/?LinkId=85238
            //

            if (obj == null || GetType() != obj.GetType())
            {
                return false;
            }

            Insert other = obj as Insert;
            if (Kind != other.Kind)
            {
                return false;
            }
            return OriginalMask.Equals(other.OriginalMask);
        }

        // override object.GetHashCode
        public override int GetHashCode()
        {
            return Kind.GetHashCode() ^ OriginalMask.GetHashCode();
        }
    }
}
