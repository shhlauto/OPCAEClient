namespace OPCAE.OPCAE
{
    using System;
    public class OnEventStruct
    {
        public short ChangeMask;
        public short NewState;
        public string Source;
        public long Time;
        public string Message;
        public int EventType;
        public int EventCategory;
        public int Severity;
        public string ConditionName;
        public string SubconditionName;
        public short Quality;
        public bool AckRequired;
        public long ActiveTime;
        public int Cookie;
        public int NumEventAttrs;
        public object[] EventAttributes;
        public string ActorID;
    }
}

